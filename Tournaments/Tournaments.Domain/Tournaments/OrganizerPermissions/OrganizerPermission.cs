namespace Domain.Tournaments.OrganizerPermissions;

public class OrganizerPermission {
    public Guid UserId { get; private init; }
    public Guid TournamentId { get; private init; }
    public OrganizerPermissionType Type { get; private init; }
    
    private OrganizerPermission() { }

    internal OrganizerPermission(Guid userId, OrganizerPermissionType type) {
        UserId = userId;
        Type = type;
    }
    
    internal static IEnumerable<OrganizerPermission> ForTournamentCreator(Guid userId) =>
        Enum.GetValues<OrganizerPermissionType>()
            .Select(type => new OrganizerPermission(userId, type));
}