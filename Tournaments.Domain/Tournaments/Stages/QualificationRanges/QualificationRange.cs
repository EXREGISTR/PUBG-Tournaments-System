using Domain.Common;

namespace Domain.Tournaments.Stages.QualificationRanges;

public enum QualificationRangeColor {
    Green, Red, Yellow, Orange, Blue, Purple
}

public class QualificationRange {
    public Guid Id { get; private init; }
    public Guid NextStageId { get; internal set; }
    public int FromPlace { get; internal set; }
    public int ToPlace { get; internal set; }
    public QualificationRangeColor Color { get; internal set; }
    public Guid StageId { get; private init; }

    public TournamentStageBase NextStage { get; private set; } = null!;
    
    private QualificationRange() { }

    public QualificationRange(
        Guid nextStageId, 
        int fromPlace, int toPlace,
        QualificationRangeColor color) {
        if (fromPlace > toPlace) {
            (fromPlace, toPlace) = (toPlace, fromPlace);
        }
        
        EnsureValid(fromPlace, toPlace);

        Id = Guid.CreateVersion7();
        NextStageId = nextStageId;
        FromPlace = fromPlace;
        ToPlace = toPlace;
        Color = color;
    }

    internal static void EnsureValid(int fromPlace, int toPlace) {
        var errors = new Dictionary<string, ICollection<string>>();
        
        if (fromPlace <= 0) {
            errors.Add(nameof(FromPlace), ["Should be greater than zero"]);
        }

        if (toPlace <= 0) {
            errors.Add(nameof(ToPlace), ["Should be greater than zero"]);
        }

        if (errors.Count > 0) throw ExceptionFactories.StageQualificationRange.Validation(errors);
    }
}