namespace Pos.Domain;

public sealed class CompleteScoreStrategy : IFinalScoreStrategy {
    private readonly WeightedScoreStrategy weighted = new();

    public decimal? Calculate(Course course, Guid studentId) {
        foreach (var criterion in course.Criteria) {
            if (course.GetEvaluation(studentId, criterion.Id) == null) {
                return null;
            }
        }
        return weighted.Calculate(course, studentId);
    }
}

