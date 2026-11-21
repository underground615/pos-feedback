namespace Pos.Domain;

public sealed class Course {
    private readonly List<Student> students = new();
    private readonly List<Criterion> criteria = new();
    private readonly List<Evaluation> evaluations = new();
    private readonly EvaluationFactory factory = new();

    public IReadOnlyList<Student> Students => students.AsReadOnly();
    public IReadOnlyList<Criterion> Criteria => criteria.AsReadOnly();
    public IReadOnlyList<Evaluation> Evaluations => evaluations.AsReadOnly();

    public Student AddStudent(string name) {
        var student = new Student(name);
        students.Add(student);
        return student;
    }

    public Criterion AddCriterion(string name, decimal maxScore, decimal weight) {
        var criterion = new Criterion(name, maxScore, weight);
        criteria.Add(criterion);
        return criterion;
    }

    public Student GetStudent(Guid id) {
        foreach (var student in students) {
            if (student.Id == id) {
                return student;
            }
        }
        throw new InvalidOperationException("Студент не найден.");
    }

    public Criterion GetCriterion(Guid id) {
        foreach (var criterion in criteria) {
            if (criterion.Id == id) {
                return criterion;
            }
        }
        throw new InvalidOperationException("Критерий не найден в рубрикаторе.");
    }

    public Evaluation? GetEvaluation(Guid studentId, Guid criterionId) {
        foreach (var evaluation in evaluations) {
            if (evaluation.StudentId == studentId && evaluation.CriterionId == criterionId) {
                return evaluation;
            }
        }
        return null;
    }

    public void SetEvaluation(Guid studentId, Guid criterionId, decimal value) {
        var evaluation = factory.Create(this, studentId, criterionId, value);
        var previous = GetEvaluation(studentId, criterionId);
        if (previous != null) {
            evaluations.Remove(previous);
        }
        evaluations.Add(evaluation);
    }

    public bool RemoveEvaluation(Guid studentId, Guid criterionId) {
        GetStudent(studentId);
        GetCriterion(criterionId);
        var evaluation = GetEvaluation(studentId, criterionId);
        return evaluation != null && evaluations.Remove(evaluation);
    }
}

