using Domain.Maps;

namespace Domain.Tournaments.Stages.Matches;

public class Match {
    public Guid Id { get; private init; }
    public int MapId { get; internal set; }
    public MatchResults Results { get; private set; } = null!;
    
    public int[]? ParticipatedGroups { get; private set; }
    public Guid StageId { get; private init; }
    public DateTime CreatedAt { get; private init; }
    
    public Map Map { get; private set; } = null!;
    
    private Match() { }

    public Match(int mapId, MatchResults results, IEnumerable<int>? participatedGroups, DateTime utcNow) {
        Id = Guid.CreateVersion7();
        MapId = mapId;
        Results = results;
        ParticipatedGroups = participatedGroups?.ToArray();
        CreatedAt = utcNow;
    }
}