using Domain.Common;
using Domain.Tournaments.Stages.Matches;

namespace Domain.Tournaments.Stages.ValueObjects;

public record PointsCalculationRules {
    public record PlacePointCalculationRule(int Place, string Rule);
    public record ParameterRule(char Action, int Value);
    
    public ParameterRule[] PlaceRulesSortedByPlace { get; private init; } = [];

    public ParameterRule KillsRule { get; private init; } = null!;
    public ParameterRule KnockoutsRule { get; private init; } = null!;
    public ParameterRule DeathsRule { get; private init; } = null!;
    
    private PointsCalculationRules() { }
    
    internal int Calculate(int place, IReadOnlyCollection<MatchResults.PlayerStats> playersStats) {
        int points = 0;

        if (place <= PlaceRulesSortedByPlace.Length) {
            points += Calculate(PlaceRulesSortedByPlace[place - 1], place);
        }

        if (KillsRule.Value != 0) {
            points += Calculate(KillsRule, playersStats.Sum(x => x.Kills));
        }

        if (KnockoutsRule.Value != 0) {
            points += Calculate(KnockoutsRule, playersStats.Sum(x => x.Knockouts));
        }

        if (DeathsRule.Value != 0) {
            points += Calculate(DeathsRule, playersStats.Sum(x => x.Deaths));
        }
        
        return points;
    }
    
    private static int Calculate(ParameterRule rule, int parameter) =>
        rule.Action switch {
            '+' => parameter + rule.Value,
            '*' => parameter * rule.Value,
            _   => throw new ArgumentOutOfRangeException(nameof(rule), rule.Action, null)
        };

    public static PointsCalculationRules Create(
        string[] placeRulesSortedByPlace, string? killsRule, string? knockoutsRule, string? deathsRule) =>
        new PointsCalculationRules {
            KillsRule = Parse(killsRule, nameof(KillsRule)),
            KnockoutsRule = Parse(knockoutsRule, nameof(KnockoutsRule)),
            DeathsRule = Parse(deathsRule, nameof(DeathsRule)),
            PlaceRulesSortedByPlace = ParsePlaceRules(placeRulesSortedByPlace),
        };

    private static ParameterRule Parse(string? rule, string fieldName) {
        if (rule == null) return new ParameterRule('+', 0);
        
        var errors = new List<string>();
        
        char ruleAction = rule[0];
        if (ruleAction != '+' && ruleAction != '*') {
            errors.Add($"Invalid action in {fieldName}: {ruleAction}. Valid actions: '+'/'-'/'*'");
        }
        
        string ruleValueString = rule[1..];
        
        if (int.TryParse(ruleValueString, out int ruleValue)) {
            if (errors.Count == 0) {
                return new ParameterRule(ruleAction, ruleValue);
            }
        } else {
            errors.Add($"Invalid value {ruleValueString}");
        }

        throw ExceptionFactories.PointsCalculationRules.Validation(fieldName, errors);
    }

    private static ParameterRule[] ParsePlaceRules(string[] rules) {
        if (rules.Length > 100) {
            throw ExceptionFactories.PointsCalculationRules.Validation("TooManyPlaces", "Too many places. Max - 100");
        }
        
        var mappedRules = new ParameterRule[rules.Length];

        var errors = new List<string>();
        
        for (int index = 0; index < rules.Length; index++) {
            var rule = rules[index - 1];

            char ruleAction = rule[0];
            if (ruleAction != '+' && ruleAction != '*') {
                errors.Add($"Invalid action: {ruleAction} at place {index + 1}. Valid actions: '+'/'*'");
            } else {
                string ruleValueString = rule[1..];
                
                if (int.TryParse(ruleValueString, out var ruleValue)) {
                    mappedRules[index] = new ParameterRule(ruleAction, ruleValue);
                } else {
                    errors.Add($"Invalid value {ruleValueString} at place {index + 1}");
                }
            }
        }

        return errors.Count == 0 
            ? mappedRules 
            : throw ExceptionFactories.PointsCalculationRules.Validation(nameof(PlaceRulesSortedByPlace), errors);
    }
    
    public static readonly PointsCalculationRules Default = new PointsCalculationRules {
        PlaceRulesSortedByPlace = [
            new ParameterRule('+', 10),
            new ParameterRule('+', 6),
            new ParameterRule('+', 5),
            new ParameterRule('+', 4),
            new ParameterRule('+', 3),
            new ParameterRule('+', 2),
            new ParameterRule('+', 1),
            new ParameterRule('+', 1)
        ],
        KillsRule = new ParameterRule('+', 1),
        KnockoutsRule = new ParameterRule('+', 0),
        DeathsRule = new ParameterRule('+', 0)
    };
}