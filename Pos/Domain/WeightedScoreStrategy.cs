namespace Pos.Domain;

public sealed class WeightedScoreStrategy : IFinalScoreStrategy {
    public decimal? Calculate(Course course, Guid studentId) {
        if (course.Criteria.Count == 0) {
            return null;
        }
        decimal total = 0;
        decimal totalWeight = 0;
        foreach (var criterion in course.Criteria) {
            var evaluation = course.GetEvaluation(studentId, criterion.Id);
            total += (evaluation?.Score.Value ?? 0) / criterion.MaxScore * criterion.Weight;
            totalWeight += criterion.Weight;
        }
        return 10 * total / totalWeight;
    }
}

