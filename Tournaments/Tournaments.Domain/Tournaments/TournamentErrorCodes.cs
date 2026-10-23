namespace Domain.Tournaments;

internal static class TournamentErrorCodes {
    public const string IsNotPublished = nameof(IsNotPublished);
    public const string IsNotDraft = nameof(IsNotDraft);
    public const string AlreadyWasStarted = nameof(AlreadyWasStarted);
    public const string IsNotStarted = nameof(IsNotStarted);
    public const string CancellationDeadlinePassed = nameof(CancellationDeadlinePassed);
    public const string IsNotSupportRegistration = nameof(IsNotSupportRegistration);
    public const string IsNotOpenedRegistration = nameof(IsNotOpenedRegistration);
    public const string FrequentEditing = nameof(FrequentEditing);
    public const string IsNotSupportInvitingTeam = nameof(IsNotSupportInvitingTeam);
    public const string TeamWithThisTagAlreadyExists = nameof(TeamWithThisTagAlreadyExists);
    public const string IsSupportRegistration = nameof(IsSupportRegistration);
}