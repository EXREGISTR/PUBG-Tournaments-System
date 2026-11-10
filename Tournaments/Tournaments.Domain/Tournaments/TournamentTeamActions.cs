using Domain.Common;
using Domain.Common.Extensions;
using Domain.Tournaments.OrganizerPermissions;
using Domain.Tournaments.Teams;
using Domain.Tournaments.Teams.Members;

namespace Domain.Tournaments;

public partial class Tournament {
    public void RegisterTeam(TournamentTeam team) {
        EnsureSupportRegistration();
        EnsureState(TournamentState.RegistrationOpened);
        
        if (teams.Count == Rules.RegistrationInfo!.MaxTeamsCount) {
            throw ExceptionFactories.Tournament.Conflict("NoMorePlaces", "No more places");
        }
        
        EnsureNotExistsTeamByOriginalId(team.OriginalTeamId);
        EnsureNoTeamWithSameTag(team.Tag);
        
        var allNicknames = 
            teams
                .SelectMany(x => x.Members)
                .Select(x => x.Nickname)
                .ToHashSet();
        
        var duplicatedNicknames = 
            team.Members
                .Select(player => player.Nickname)
                .Where(allNicknames.Contains)
                .ToList();
        
        if (duplicatedNicknames.Count > 0) {
            var duplicatedNicknamesString = string.Join(',', duplicatedNicknames);
            
            throw ExceptionFactories.Tournament.Conflict(
                "FoundDuplicatedNicknames", 
                $"Duplicated nicknames in other teams: {duplicatedNicknamesString}",
                ("DuplicatedNicknames", duplicatedNicknamesString));
        }
        
        teams.Add(team);
    }

    public void UnregisterTeam(Guid tournamentTeamId, Guid initiatorId) {
        EnsureSupportRegistration();
        EnsureState(TournamentState.RegistrationOpened);

        var index = GetTeamIndex(tournamentTeamId);

        var team = teams[index];
        
        if (team.RegistratorId == null) {
            throw ExceptionFactories.TournamentTeam.Conflict(
                "CanNotUnregisterInvitedTeam", 
                "You can't unregister invited team");
        }
        
        if (team.RegistratorId.Value != initiatorId) {
            throw ExceptionFactories.Team.Unauthorized(
                "OnlyRegistratorCanUnregister", 
                "Only registrator can unregister", 
                new { team.RegistratorId });
        }
        
        teams.RemoveAt(index);
    }

    public void InviteTeam(TournamentTeam team, Guid organizerId) {
        EnsureHasPermission(organizerId, OrganizerPermissionType.TeamsManagement);
        
        if (Rules.IsSupportRegistration()) {
            EnsureInclusiveBetweenStates(TournamentState.Published, TournamentState.RegistrationClosed);
        } else {
            EnsureState(TournamentState.Published);
        }
        
        const int requiredTeamSize = 4;
        
        if (Rules.TeamSize.Value != requiredTeamSize) {
            throw ExceptionFactories.Tournament.Conflict(
                "IncorrectPlayersNumber",
                $"Inviting is access only for tournament where team size - {requiredTeamSize}",
                ("RequiredTeamSize", requiredTeamSize));
        }
        
        EnsureNotExistsTeamByOriginalId(team.OriginalTeamId);
        
        if (Rules.IsRegional() && team.Region != Rules.Region) {
            throw ExceptionFactories.Tournament.Conflict(
                "TeamFromAnotherRegion",
                "You trying to invite team from another region on the regional tournament");
        }
        
        EnsureNoTeamWithSameTag(team.Tag);
        
        teams.Add(team);
    }

    public void DeleteTeam(Guid tournamentTeamId, Guid organizerId) {
        EnsureHasPermission(organizerId, OrganizerPermissionType.TeamsManagement);
        
        if (Rules.IsSupportRegistration()) {
            EnsureInclusiveBetweenStates(TournamentState.Published, TournamentState.RegistrationClosed);
        } else {
            EnsureState(TournamentState.Published);
        }
        
        teams.RemoveAll(x => x.Id == tournamentTeamId);
    }

    private void EnsureNotExistsTeamByOriginalId(Guid teamId) {
        var existsTeamOnTournament = teams.Any(x => x.OriginalTeamId == teamId);

        if (existsTeamOnTournament) {
            throw ExceptionFactories.Tournament.Conflict(
                "TeamAlreadyExists",
                "Team already exists");
        }
    }

    public void EditTeamMember(
        Guid tournamentTeamId, Guid tournamentTeamMemberId,
        string nickname, string? avatarUrl,
        TournamentTeamMemberRole role, Guid initiatorId) {
        EnsureInclusiveBetweenStates(TournamentState.Published, TournamentState.Completed);
        
        GetTeam(tournamentTeamId)
            .EditMember(tournamentTeamMemberId, nickname, avatarUrl, role, initiatorId);
    }

    public void EditTeam(Guid tournamentTeamId, string tag, string title, string? logotypeUrl, Region region, Guid initiatorId) {
        EnsureInclusiveBetweenStates(TournamentState.Published, TournamentState.Completed);
        EnsureNoTeamWithSameTag(tag);
        
        GetTeam(tournamentTeamId)
            .Edit(tag, title, logotypeUrl, region, initiatorId);
    }

    public void DisqualifyTeam(Guid tournamentTeamId, Guid organizerId) {
        EnsureHasPermission(organizerId, OrganizerPermissionType.TeamsManagement);
        EnsureInclusiveBetweenStates(
            Rules.IsSupportRegistration() ? TournamentState.RegistrationOpened : TournamentState.Published, 
            TournamentState.Completed);
        
        GetTeam(tournamentTeamId)
            .Disqualify();
    }

    private void EnsureNoTeamWithSameTag(string tag) {
        var existsByTag = teams.Any(x => x.Tag == tag);
        
        if (existsByTag) {
            throw ExceptionFactories.Tournament.Conflict(
                TournamentErrorCodes.TeamWithThisTagAlreadyExists,
                "Team with this tag already exists");
        }
    }

    private int GetTeamIndex(Guid tournamentTeamId) {
        var index = teams.FindIndex(x => x.Id == tournamentTeamId);

        return index != -1 ? index : throw ExceptionFactories.TournamentTeam.NotFound();
    }

    private TournamentTeam GetTeam(Guid tournamentTeamId) {
        var index = GetTeamIndex(tournamentTeamId);
        return teams[index];
    }
}