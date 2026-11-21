namespace Pos.Domain;

public sealed class FinalScoreService {
    public decimal? Calculate(Course course, Guid studentId, IFinalScoreStrategy strategy) {
        ArgumentNullException.ThrowIfNull(course);
        ArgumentNullException.ThrowIfNull(strategy);
        course.GetStudent(studentId);
        var result = strategy.Calculate(course, studentId);
        return result.HasValue ? Math.Round(result.Value, 2, MidpointRounding.AwayFromZero) : null;
    }
}

