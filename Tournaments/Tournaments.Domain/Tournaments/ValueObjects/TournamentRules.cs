using Domain.Common;
using Domain.Tournaments.Enums;

namespace Domain.Tournaments.ValueObjects;

public record TournamentRules {
    public TournamentTeamSize TeamSize { get; private init; } = null!;
    public Region Region { get; private init; }
    public RegistrationInformation? RegistrationInfo { get; private init; }
    public TournamentTier? Tier { get; private init; }
    public DateTime StartDate { get; private init; }
    public DateTime EndDate { get; private init; }
    
    private TournamentRules() { }
    
    public TournamentRules(
        TournamentTeamSize teamSize,
        Region region,
        TournamentTier? tier,
        DateTime startDate,
        int daysDuration,
        RegistrationInformation? registrationInformation,
        DateTime utcNow) {
        EnsureValid(startDate, region, tier, utcNow);
        
        TeamSize = teamSize;
        Region = region;
        RegistrationInfo = registrationInformation;
    }

    private static void EnsureValid(DateTime startDate, Region region, TournamentTier? tier, DateTime utcNow) {
        var errors = new List<(string FieldName, ICollection<string> Error)>(); 
        
        if (region != Region.Global && tier == TournamentTier.S) {
            errors.Add((nameof(Tier), ["Tier can't be S when tournament is not global"]));
        }
        
        if ((startDate - utcNow).Duration().Hours < 8) {
            errors.Add((nameof(StartDate), ["Start date should be later 8 hours from now"]));
        }
        
        if (errors.Count > 0) throw ExceptionFactories.TournamentRules.Validation(errors);
    }

    public bool IsRegional() => Region != Region.Global;
    public bool IsNotSupportRegistration() => !IsSupportRegistration();
    public bool IsSupportRegistration() => RegistrationInfo != null;

    public static TournamentRules CreateDefault() {
        return new TournamentRules {
            TeamSize = TournamentTeamSize.FourManSquads,
            Region = Region.Cis,
            RegistrationInfo = null,
            Tier = TournamentTier.A
        };
    }
}