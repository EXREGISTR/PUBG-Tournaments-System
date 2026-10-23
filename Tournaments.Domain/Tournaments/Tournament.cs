using DDD.SDK.Domain;
using Domain.Common;
using Domain.Tournaments.Enums;
using Domain.Tournaments.Events;
using Domain.Tournaments.OrganizerPermissions;
using Domain.Tournaments.Stages;
using Domain.Tournaments.Teams;
using Domain.Tournaments.ValueObjects;

namespace Domain.Tournaments;

public partial class Tournament : AggregateRoot {
    private readonly List<TournamentTeam> teams = [];
    private readonly List<TournamentStageBase> stages = [];
    private readonly List<OrganizerPermission> organizerPermissions = [];
    
    public Guid Id { get; private init; }
    public TournamentInformation Information { get; private set; } = null!;
    public TournamentRules Rules { get; private set; } = null!;
    public TournamentState State { get; private set; }
    
    public DateTime CreatedAt { get; private init; }
    public DateTime? LastEditedAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    
    public IReadOnlyCollection<TournamentTeam> Teams => teams;
    public IReadOnlyCollection<TournamentStageBase> Stages => stages;
    public IReadOnlyCollection<OrganizerPermission> OrganizerPermissions => organizerPermissions;
    
    private Tournament() { }
    
    public Tournament(
        TournamentInformation information,
        TournamentRules rules,
        Guid creatorId,
        DateTime utcNow) {
        Id = Guid.CreateVersion7();
        Information = information;
        Rules = rules;
        
        State = TournamentState.Draft;
        CreatedAt = utcNow;
        
        organizerPermissions.AddRange(OrganizerPermission.ForTournamentCreator(creatorId));
        
        PushEvent(new CreatedDomainEvent(Id));
    }
    
    public void EditRules(TournamentRules rules, Guid organizerId, DateTime utcNow) {
        if (DeletedAt != null) return;
        
        EnsureHasPermission(organizerId, OrganizerPermissionType.EditRules);
        EnsureDraftAndCanEdit(utcNow);
        
        Rules = rules;
        LastEditedAt = utcNow;
    }

    public void EditContent(TournamentInformation information, Guid organizerId, DateTime utcNow) {
        if (DeletedAt != null) return;
        
        EnsureHasPermission(organizerId, OrganizerPermissionType.EditContent);
        EnsureCanEdit(utcNow);
        
        if (State >= TournamentState.Published && Information.Description != information.Description) {
            PushEvent(new DescriptionEditedDomainEvent(Id));
        }
        
        Information = information;
        LastEditedAt = utcNow;
    }
    
    private void EnsureSupportRegistration() {
        if (Rules.IsNotSupportRegistration()) {
            throw ExceptionFactories.Tournament.Conflict(
                TournamentErrorCodes.IsNotSupportRegistration,
                "Tournament is not support registration");
        }
    }

    private void EnsureIsNotSupportRegistration() {
        if (Rules.IsSupportRegistration()) {
            throw ExceptionFactories.Tournament.Conflict(
                TournamentErrorCodes.IsSupportRegistration,
                "Tournament is not support registration");
        }
    }
    
    private void EnsureDraft() => EnsureState(TournamentState.Draft);
    private void EnsureInProcess() => EnsureState(TournamentState.InProcess);

    private void EnsureInclusiveBetweenStates(TournamentState left, TournamentState right) {
        if (left > right) {
            (left, right) = (right, left);
        }
        
        if (State >= left && State <= right) return;
        
        throw ExceptionFactories.Tournament.Conflict(
            "InvalidState", $"Should be inclusive between {left} and {right}");
    }
    
    private void EnsureState(TournamentState expectedState) {
        if (State != expectedState) {
            throw ExceptionFactories.Tournament.Conflict(
                $"IsNot{expectedState}", $"Is not {expectedState} state");
        }
    }

    private void EnsureHasPermissionIfNotNull(Guid? organizerId, OrganizerPermissionType permissionType) {
        if (organizerId == null) return;
        
        EnsureHasPermission(organizerId.Value, permissionType);
    }
    
    private void EnsureHasPermission(Guid organizerId, OrganizerPermissionType permissionType) {
        var hasPermission = organizerPermissions.Any(x => x.UserId == organizerId && x.Type == permissionType);
        
        if (!hasPermission) {
            throw ExceptionFactories.Tournament.Unauthorized(
                "NoPermission", 
                "You don't have permission to do that");
        }
    }

    private void EnsureDraftAndCanEdit(DateTime utcNow) {
        EnsureDraft();
        EnsureCanEdit(utcNow);
    }
    
    private void EnsureCanEdit(DateTime utcNow) { 
        const int delayBeforeEditInMinutes = 1;
        
        if (LastEditedAt == null 
            || LastEditedAt.Value.AddMinutes(delayBeforeEditInMinutes) >= utcNow) return;

        throw ExceptionFactories.Tournament.Conflict(
            TournamentErrorCodes.FrequentEditing, 
            $"You are editing too frequently. The acceptable editing frequency is {delayBeforeEditInMinutes} minutes per time");
    }
}