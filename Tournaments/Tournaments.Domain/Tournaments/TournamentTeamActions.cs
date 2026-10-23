using Domain.Common;
using Domain.Tournaments.Enums;
using Domain.Tournaments.OrganizerPermissions;
using Domain.Tournaments.Teams;

namespace Domain.Tournaments;

public partial class Tournament {
    public void RegisterTeam(TournamentTeam team) {
        if (DeletedAt != null) return;
        
        if (Rules.IsNotSupportRegistration()) {
            throw ExceptionFactories.Tournament.Conflict(
                "IsNotSupportRegistration",
                "You can't register team on tournament without open registration");
        }
            
        if (State != TournamentState.RegistrationOpened) {
            throw ExceptionFactories.Tournament.Conflict(
                "RegistrationIsNotOpened",
                "Registration is not opened");
        }
        
        if (teams.Count == Rules.RegistrationInfo!.MaxTeamsCount) {
            throw ExceptionFactories.Tournament.Conflict("NoMorePlaces", "No more places");
        }
        
        var existsByTag = teams.Any(x => x.Tag == team.Tag);
        
        if (existsByTag) {
            throw ExceptionFactories.Tournament.Conflict(
                TournamentErrorCodes.TeamWithThisTagAlreadyExists,
                "Team with this tag already exists");
        }
        
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

    public void InviteTeam(TournamentTeam team, Guid? organizerId) {
        if (DeletedAt != null) return;
        
        EnsureHasPermissionIfNotNull(organizerId, OrganizerPermissionType.InviteTeam);
        
        if (State != TournamentState.Published) {
            throw ExceptionFactories.Tournament.Conflict(
                TournamentErrorCodes.IsNotPublished, "You can invite team only when Published state",
                ("CurrentState", State));
        }
        
        const int requiredTeamSize = 4;
        
        if (Rules.TeamSize.Value != requiredTeamSize) {
            throw ExceptionFactories.Tournament.Conflict(
                "IncorrectPlayersNumber",
                $"Inviting is access only for tournament where team size - {requiredTeamSize}",
                ("RequiredTeamSize", requiredTeamSize));
        }
            
        if (Rules.RegistrationInfo != null) {
            throw ExceptionFactories.Tournament.Conflict(
                TournamentErrorCodes.IsNotSupportInvitingTeam,
                "You cannot invite a team to a tournament with open registration");
        }
        
        var existsTeamOnTournament = teams.Any(x => x.OriginalTeamId == team.OriginalTeamId);

        if (existsTeamOnTournament) {
            throw ExceptionFactories.Tournament.Conflict(
                "TeamAlreadyExists",
                "Team already exists");
        }
        
        if (Rules.IsRegional() && team.Region != Rules.Region) {
            throw ExceptionFactories.Tournament.Conflict(
                "TeamFromAnotherRegion",
                "You trying to invite team from another region on the regional tournament");
        }
        
        var existsByTag = teams.Any(x => x.Tag == team.Tag);

        if (existsByTag) {
            throw ExceptionFactories.Tournament.Conflict(
                TournamentErrorCodes.TeamWithThisTagAlreadyExists,
                "Team with this tag already exists. Please pass an overridden tag");
        }
        
        teams.Add(team);
    }
}