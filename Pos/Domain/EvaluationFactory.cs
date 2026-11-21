namespace Pos.Domain;

public sealed class EvaluationFactory {
    public Evaluation Create(Course course, Guid studentId, Guid criterionId, decimal value) {
        ArgumentNullException.ThrowIfNull(course);
        course.GetStudent(studentId);
        var criterion = course.GetCriterion(criterionId);
        var score = new Score(value);
        if (score.Value > criterion.MaxScore) {
            throw new ArgumentOutOfRangeException(nameof(value), "Балл превышает максимум критерия.");
        }
        return new Evaluation(studentId, criterionId, score);
    }
}

