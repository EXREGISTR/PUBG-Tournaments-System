namespace Domain.Tournaments.Enums;

public enum TournamentState {
    Draft = 0,
    Published = 5,
    RegistrationOpened = 10,
    RegistrationClosed = 11,
    InProcess = 15,
    Completed = 20,
    Cancelled = 25
}