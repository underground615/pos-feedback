using Pos.Application;
using Pos.Domain;
using Pos.Infrastructure;
using Xunit;

namespace Pos.Tests;

public sealed class ApplicationTests {
    [Fact]
    public void FacadeRunsScenarioAndExportsCurrentResults() {
        var repository = new InMemoryCourseRepository();
        var exporter = new FakeExporter();
        var facade = new PosFacade(repository, exporter, new FinalScoreService());
        var student = facade.AddStudent("Анна");
        var criterion = facade.AddCriterion("ДЗ", 10, 1);
        facade.SetEvaluation(student.Id, criterion.Id, 8);
        Assert.Single(facade.GetStudents());
        Assert.Single(facade.GetCriteria());
        Assert.Equal(8m, Assert.Single(facade.GetEvaluations()).Score.Value);
        facade.ExportResults(new WeightedScoreStrategy(), "report.csv");
        Assert.Equal("report.csv", exporter.Path);
        Assert.Equal(8m, Assert.Single(exporter.Results).Score);
        Assert.True(facade.RemoveEvaluation(student.Id, criterion.Id));
        Assert.Equal(0m, Assert.Single(facade.GetResults(new WeightedScoreStrategy())).Score);
        Assert.Null(Assert.Single(facade.GetResults(new CompleteScoreStrategy())).Score);
    }

    [Fact]
    public void RepositorySavesAggregate() {
        var repository = new InMemoryCourseRepository();
        var course = new Course();
        course.AddStudent("Анна");
        repository.Save(course);
        Assert.Same(course, repository.Get());
        Assert.Throws<ArgumentNullException>(() => repository.Save(null!));
    }

    [Fact]
    public void CsvEscapesNamesAndUsesInvariantDecimals() {
        var path = System.IO.Path.GetTempFileName();
        var previousCulture = System.Globalization.CultureInfo.CurrentCulture;
        try {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("ru-RU");
            var id = Guid.NewGuid();
            var results = new List<StudentResult> {
                new(id, "Анна;\"А\"\nБ", 6.8m),
                new(Guid.Empty, "Иван", null)
            };
            new CsvResultExporter().Export(results, path);
            var text = File.ReadAllText(path);
            Assert.StartsWith("StudentId;StudentName;Score", text);
            Assert.Contains($"{id};\"Анна;\"\"А\"\"\nБ\";6.8", text);
            Assert.Contains($"{Guid.Empty};\"Иван\";", text);
            new CsvResultExporter().Export(new List<StudentResult>(), path);
            Assert.Single(File.ReadAllLines(path));
        }
        finally {
            System.Globalization.CultureInfo.CurrentCulture = previousCulture;
            File.Delete(path);
        }
    }

    private sealed class FakeExporter : IResultExporter {
        public string? Path { get; private set; }
        public IReadOnlyList<StudentResult> Results { get; private set; } = new List<StudentResult>();

        public void Export(IReadOnlyList<StudentResult> results, string path) {
            Path = path;
            Results = results;
        }
    }
}
