using Domain.Common;
using Domain.Tournaments.Enums;
using Domain.Tournaments.Events;
using Domain.Tournaments.OrganizerPermissions;

namespace Domain.Tournaments;

// Possible transitions:
// Draft -> Published -> RegistrationOpened -> RegistrationClosed -> InProcess -> Completed
// Draft -> Published -> InProcess -> Completed

// Draft -> Published -> Cancelled
// Draft -> Published -> RegistrationOpened -> Cancelled
// Draft -> Published -> RegistrationOpened -> RegistrationClosed -> Cancelled

// Draft -> Published -> Deleted
// Draft -> Published -> Registration -> Deleted

// Draft -> Deleted -> Draft
public partial class Tournament {
    public void Publish(Guid organizerId, DateTime utcNow) {
        if (DeletedAt != null) return;
        
        EnsureHasPermission(organizerId, OrganizerPermissionType.Publish);
        
        if (State == TournamentState.Published) return;

        if (State != TournamentState.Draft) {
            throw ExceptionFactories.Tournament.Conflict(
                TournamentErrorCodes.IsNotDraft, 
                "You can publish tournament only in draft state");
        }
        
        State = TournamentState.Published;
        PublishedAt = utcNow;
        PushEvent(new PublishedDomainEvent(Id));
    }
    
    public void OpenRegistration() {
        if (DeletedAt != null) return;
        
        EnsureSupportRegistration();
        
        if (State is TournamentState.RegistrationClosed or TournamentState.Cancelled) return;

        if (State != TournamentState.Published) {
            throw ExceptionFactories.Tournament.Conflict(
                TournamentErrorCodes.IsNotPublished, 
                "You can start registration only after publication");
        }
        
        State = TournamentState.RegistrationOpened;
        PushEvent(new RegistrationStartedDomainEvent(Id));
    }

    /// <returns>True if teams count greater than or equals min participants count (property from <see cref="ValueObjects.RegistrationInformation"/>) </returns>
    public bool CloseRegistration() {
        if (DeletedAt != null) return false;
        
        EnsureSupportRegistration();
        
        if (State is TournamentState.RegistrationClosed or TournamentState.Cancelled) return false;

        if (State != TournamentState.RegistrationOpened) {
            throw ExceptionFactories.Tournament.Conflict(
                TournamentErrorCodes.IsNotOpenedRegistration, 
                "Registration is not opened");
        }

        if (teams.Count < Rules.RegistrationInfo!.MinTeamsCount) {
            const string reason = "Tournament was canceled because not enough teams to start";
            CancelInternal(reason);
            return false;
        }
        
        State = TournamentState.RegistrationClosed;
        PushEvent(new RegistrationClosedDomainEvent(Id));
        return true;
    }
    
    public void Delete(Guid? organizerId, DateTime utcNow) {
        if (DeletedAt != null) return;
        
        EnsureHasPermissionIfNotNull(organizerId, OrganizerPermissionType.Delete);
        
        if (State >= TournamentState.InProcess) {
            throw ExceptionFactories.Tournament.Conflict(
                TournamentErrorCodes.AlreadyWasStarted,
                "You can delete tournament only before it start");
        }

        State = TournamentState.Draft;
        DeletedAt = utcNow;
        PushEvent(new DeletedDomainEvent(Id));
    }

    public void Recover(Guid organizerId) {
        if (DeletedAt == null) return;
        
        EnsureHasPermission(organizerId, OrganizerPermissionType.Recover);
        
        DeletedAt = null;
        PushEvent(new RecoveredDomainEvent(Id));
    }
    
    public void Start() {
        if (DeletedAt != null || State == TournamentState.InProcess) return;

        if (State == TournamentState.Draft) {
            throw ExceptionFactories.Tournament.Conflict(
                TournamentErrorCodes.IsNotPublished, 
                "You can start tournament only after publication");
        }

        if (Rules.IsNotSupportRegistration()) {
            if (State != TournamentState.RegistrationClosed) {
                throw ExceptionFactories.Tournament.Conflict(
                    TournamentErrorCodes.AlreadyWasStarted, 
                    "You can start tournament only after registration");
            }
        } else if (State > TournamentState.Published) {
            throw ExceptionFactories.Tournament.Conflict(
                TournamentErrorCodes.AlreadyWasStarted, 
                "You can't start tournament that has started");
        }
        
        State = TournamentState.InProcess;
        PushEvent(new StartedDomainEvent(Id));
    }

    public void Complete() {
        if (State == TournamentState.Completed) return;

        if (State != TournamentState.InProcess) {
            throw ExceptionFactories.Tournament.Conflict(
                TournamentErrorCodes.IsNotStarted, 
                "Tournament is not started");
        }

        State = TournamentState.Completed;
        PushEvent(new CompletedDomainEvent(Id));
    }
    
    public void Cancel(Guid organizerId, string reason, DateTime utcNow) {       
        EnsureHasPermission(organizerId, OrganizerPermissionType.Publish);

        const int minHoursFromStartDateToNowForCancellation = 6;
        
        if (State == TournamentState.Cancelled) return;
        
        if (State is TournamentState.RegistrationOpened or TournamentState.RegistrationClosed 
            && Rules.StartDate > utcNow.AddHours(minHoursFromStartDateToNowForCancellation)) {
            throw ExceptionFactories.Tournament.Conflict(
                TournamentErrorCodes.CancellationDeadlinePassed, 
                $"You can only cancel this tournament while it is in registration phase and more than {minHoursFromStartDateToNowForCancellation} hours before start");
        }

        if (State >= TournamentState.InProcess) {
            throw ExceptionFactories.Tournament.Conflict(
                TournamentErrorCodes.AlreadyWasStarted,
                "You can't cancel tournament that has started");
        }
        
        CancelInternal(reason);
    }
    
    private void CancelInternal(string reason) {
        State = TournamentState.Cancelled;
        PushEvent(new CancelledDomainEvent(Id, reason));
    }
}