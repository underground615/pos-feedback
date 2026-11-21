namespace Pos.Domain;

public sealed class Evaluation {
    public Guid StudentId { get; }
    public Guid CriterionId { get; }
    public Score Score { get; }

    internal Evaluation(Guid studentId, Guid criterionId, Score score) {
        StudentId = studentId;
        CriterionId = criterionId;
        Score = score;
    }
}

