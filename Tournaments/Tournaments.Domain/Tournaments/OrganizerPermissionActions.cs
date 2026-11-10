using Domain.Common;
using Domain.Tournaments.OrganizerPermissions;

namespace Domain.Tournaments;

public partial class Tournament {
    public void AddPermissions(Guid assignerId, Guid targetOrganizerId, IReadOnlyCollection<OrganizerPermissionType> permissionTypes) {
        if (assignerId == targetOrganizerId) {
            throw ExceptionFactories.Tournament.Conflict("InitiatorAndTargetOrganizerIsSame", "You can't execute this action with self");
        }
        
        EnsureHasPermission(assignerId, OrganizerPermissionType.PermissionsManagement);
        
        var availablePermissionsFromPassedAtAssigner = permissionTypes
            .Where(type => organizerPermissions.Exists(x => x.UserId == assignerId && x.Type == type));
        
        var newPermissions = availablePermissionsFromPassedAtAssigner
            .Where(type => !organizerPermissions.Exists(x => x.UserId == targetOrganizerId && x.Type == type))
            .Select(type => new OrganizerPermission(targetOrganizerId, type));
        
        organizerPermissions.AddRange(newPermissions);
    }

    public void RemovePermissions(Guid initiatorId, Guid targetOrganizerId, IReadOnlyCollection<OrganizerPermissionType> permissionTypes) {
        if (initiatorId == targetOrganizerId) {
            throw ExceptionFactories.Tournament.Conflict("InitiatorAndTargetOrganizerIsSame", "You can't execute this action with self");
        }
        
        EnsureHasPermission(initiatorId, OrganizerPermissionType.PermissionsManagement);
        
        var availablePermissionsToRemoveFromPassedAtAssigner = permissionTypes
            .Where(type => organizerPermissions.Exists(x => x.UserId == initiatorId && x.Type == type))
            .ToArray();

        organizerPermissions.RemoveAll(x => 
            x.UserId == targetOrganizerId && availablePermissionsToRemoveFromPassedAtAssigner.Contains(x.Type));
    }
}