using Pos.Application;
using Pos.Domain;

namespace Pos.Application;

public sealed class PosFacade {
    private readonly ICourseRepository repository;
    private readonly IResultExporter exporter;
    private readonly FinalScoreService scoreService;

    public PosFacade(ICourseRepository repository, IResultExporter exporter, FinalScoreService scoreService) {
        this.repository = repository;
        this.exporter = exporter;
        this.scoreService = scoreService;
    }

    public Student AddStudent(string name) {
        var course = repository.Get();
        var student = course.AddStudent(name);
        repository.Save(course);
        return student;
    }

    public Criterion AddCriterion(string name, decimal maxScore, decimal weight) {
        var course = repository.Get();
        var criterion = course.AddCriterion(name, maxScore, weight);
        repository.Save(course);
        return criterion;
    }

    public IReadOnlyList<Student> GetStudents() {
        return repository.Get().Students;
    }

    public IReadOnlyList<Criterion> GetCriteria() {
        return repository.Get().Criteria;
    }

    public IReadOnlyList<Evaluation> GetEvaluations() {
        return repository.Get().Evaluations;
    }

    public void SetEvaluation(Guid studentId, Guid criterionId, decimal value) {
        var course = repository.Get();
        course.SetEvaluation(studentId, criterionId, value);
        repository.Save(course);
    }

    public bool RemoveEvaluation(Guid studentId, Guid criterionId) {
        var course = repository.Get();
        var removed = course.RemoveEvaluation(studentId, criterionId);
        repository.Save(course);
        return removed;
    }

    public List<StudentResult> GetResults(IFinalScoreStrategy strategy) {
        var course = repository.Get();
        var results = new List<StudentResult>();
        foreach (var student in course.Students) {
            results.Add(new StudentResult(student.Id, student.Name,
                scoreService.Calculate(course, student.Id, strategy)));
        }
        return results;
    }

    public void ExportResults(IFinalScoreStrategy strategy, string path) {
        exporter.Export(GetResults(strategy), path);
    }
}

