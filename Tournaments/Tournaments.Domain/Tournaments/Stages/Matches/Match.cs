using Domain.Maps;

namespace Domain.Tournaments.Stages.Matches;

public class Match {
    public Guid Id { get; private init; }
    public int MapId { get; internal set; }
    public MatchResults Results { get; private set; } = null!;
    public Guid StageId { get; private init; }
    public DateTime LoadedAt { get; private init; }
    
    public Map Map { get; private set; } = null!;
    
    private Match() { }

    public Match(int mapId, MatchResults results, DateTime utcNow) {
        Id = Guid.CreateVersion7();
        MapId = mapId;
        Results = results;
        LoadedAt = utcNow;
    }
}