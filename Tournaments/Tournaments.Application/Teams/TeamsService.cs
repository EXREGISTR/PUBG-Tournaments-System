using Application.Common;
using Application.Common.Repositories;
using Application.Common.Repositories.Tournaments;
using Application.Teams.DTOs;
using Domain.Common;
using Domain.Common.Extensions;
using Domain.Teams;
using Domain.Teams.Invitations;
using Domain.Teams.Members;

namespace Application.Teams;

public class TeamsService(
    IUserContext userContext,
    IUsersClient usersClient,
    ITournamentsQueryRepository tournamentsRepository,
    ITeamsRepository teamsRepository, 
    IUnitOfWork unitOfWork) {
    public async Task Create(TeamEditModel editModel, CancellationToken token = default) {
        Guid creatorId = userContext.GetUserId();

        var team = new Team(editModel.Tag, editModel.Title, editModel.LogotypeUrl, editModel.Region, creatorId);
        
        teamsRepository.Insert(team);
        
        await unitOfWork.SaveChanges(token);
    }

    public async Task Edit(Guid teamId, TeamEditModel editModel, CancellationToken token = default) {
        Team team = await GetTeamNoIncludes(teamId, token);
        
        Guid initiatorId = userContext.GetUserId();
        
        team.Edit(editModel.Tag, editModel.Title, editModel.LogotypeUrl, editModel.Region, initiatorId);
        
        await unitOfWork.SaveChanges(token);
    }

    public async Task Delete(Guid teamId, CancellationToken token = default) {
        Team team = await GetTeamNoIncludes(teamId, token);
        
        Guid initiatorId = userContext.GetUserId();
        
        if (team.CreatorId != initiatorId) {
            throw ExceptionFactories.Team.Unauthorized(
                "OnlyCreatorCanDelete", 
                "Only creator can delete", 
                new { team.CreatorId });
        }
        
        var tournamentsWhereTeamParticipatingNowOrWillParticipateInFuture = await tournamentsRepository
            .GetTournamentIdsWithParticipatingTeamLaterThanNow(teamId, token);

        if (tournamentsWhereTeamParticipatingNowOrWillParticipateInFuture.Count != 0) {
            throw ExceptionFactories.Team.Conflict(
                "ImpossibleToDelete", 
                "You can't delete team. Exists tournaments where this team is participating. " +
                "Please unregister this team or contact with organizers if you were invited",
                new { TournamentIds = tournamentsWhereTeamParticipatingNowOrWillParticipateInFuture });
        }
        
        teamsRepository.Delete(team);

        await unitOfWork.SaveChanges(token);
    }

    public async Task<TeamView> GetTeam(Guid teamId, CancellationToken token = default) {
        var team = await GetTeamIncludeMembers(teamId, token);
        
        var members = team.Members
            .Select(x => new TeamMemberView(x.Id, x.Nickname, x.AvatarUrl, x.Role))
            .ToArray();
        
        return new TeamView(teamId, team.Tag, team.Title, team.LogotypeUrl, team.Region, members);
    }

    public async Task InvitePeople(Guid teamId, Guid userId, TeamMemberRole role, CancellationToken token = default) {
        var existsTargetUser = await usersClient.Exists(userId, token);

        if (!existsTargetUser) {
            throw ExceptionFactories.Team.Conflict("UserNotFound", "You can't invite unknown user");
        }
        
        var currentTeamIdAtUser = await teamsRepository.GetTeamIdWithSameTeamMemberId(userId, token);
        
        if (currentTeamIdAtUser != null && currentTeamIdAtUser != teamId) {
            throw ExceptionFactories.Team.Conflict(
                "UserInOtherTeam", 
                "This user in other team", 
                new { CurrentTeamId = currentTeamIdAtUser });
        }
        
        Guid initiatorId = userContext.GetUserId();
        Team team = await GetTeamIncludeInvitations(teamId, token);
        
        team.InvitePeople(initiatorId, userId, role);
        
        await unitOfWork.SaveChanges(token);
    }
    
    public async Task<IReadOnlyCollection<InvitationForTeamView>> GetInvites(
        Guid teamId,
        CancellationToken token = default) {
        var exists = await teamsRepository.Exists(teamId, token);

        if (!exists) {
            throw ExceptionFactories.Team.NotFound();
        }
        
        var invites = await teamsRepository
            .GetInvitations(invitation => invitation.TeamId == teamId, token);

        return invites
            .Select(InvitationToView)
            .ToArray();
    }
    
    public async Task<IReadOnlyCollection<InvitationForTeamView>> GetInvitesForMe(
        CancellationToken token = default) {
        var userId = userContext.GetUserId();
        var invites = await teamsRepository
            .GetInvitations(invitation => invitation.ReceiverId == userId, token);

        return invites
            .Select(InvitationToView)
            .ToArray();
    }
    
    public async Task AcceptInvitation(Guid teamId, string nickname, string? avatarUrl, CancellationToken token = default) {
        Team team = await GetTeamIncludeInvitations(teamId, token);
        
        Guid userId = userContext.GetUserId();
        team.AcceptInvitation(userId, nickname, avatarUrl);
        
        await unitOfWork.SaveChanges(token);
    }

    public async Task EditMember(Guid teamId, Guid teamMemberId, TeamMemberEditModel editModel, CancellationToken token = default) {
        Team team = await GetTeamIncludeMembers(teamId, token);
        
        Guid initiatorId = userContext.GetUserId();
        
        team.EditMember(teamMemberId, editModel.Nickname, editModel.AvatarUrl, editModel.Role, initiatorId);
        
        await unitOfWork.SaveChanges(token);
    }
    
    public async Task DeleteMember(Guid teamId, Guid teamMemberId, CancellationToken token = default) {
        Team team = await GetTeamIncludeMembers(teamId, token);
        
        Guid initiatorId = userContext.GetUserId();
        team.DeleteMember(teamMemberId, initiatorId);
        
        await unitOfWork.SaveChanges(token);
    }

    private async Task<Team> GetTeamNoIncludes(Guid teamId, CancellationToken token) {
        var team = await teamsRepository.GetById(teamId, token);

        return team ?? throw ExceptionFactories.Team.NotFound();
    }
    
    private async Task<Team> GetTeamIncludeMembers(Guid teamId, CancellationToken token) {
        var team = await teamsRepository.GetByIdIncludeMembers(teamId, token);

        return team ?? throw ExceptionFactories.Team.NotFound();
    }
    
    private async Task<Team> GetTeamIncludeInvitations(Guid teamId, CancellationToken token) {
        var team = await teamsRepository.GetByIdIncludeInvitations(teamId, token);

        return team ?? throw ExceptionFactories.Team.NotFound();
    }
    
    private static InvitationForTeamView InvitationToView(InvitationForTeam invitation) =>
        new InvitationForTeamView(
            invitation.TeamId,
            new Sender(invitation.Sender.Id, invitation.Sender.Nickname, invitation.Sender.AvatarUrl),
            invitation.ReceiverId,
            invitation.Role);
}