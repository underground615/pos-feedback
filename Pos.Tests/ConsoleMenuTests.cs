using Pos.Application;
using Pos.ConsoleApp;
using Pos.Domain;
using Pos.Infrastructure;
using Xunit;

namespace Pos.Tests;

[CollectionDefinition("Console", DisableParallelization = true)]
public sealed class ConsoleCollection {
}

[Collection("Console")]
public sealed class ConsoleMenuTests {
    [Fact]
    public void MenuAddsDataAndShowsResult() {
        var exporter = new FakeExporter();
        var facade = new PosFacade(new InMemoryCourseRepository(), exporter, new FinalScoreService());
        var menu = new ConsoleMenu(facade, new WeightedScoreStrategy(), new CompleteScoreStrategy());
        var input = "1\nАнна\n2\nДЗ\n10\n1\n3\n4\n1\n1\n8\n7\n0\n";
        var originalInput = Console.In;
        var originalOutput = Console.Out;
        using var reader = new StringReader(input);
        using var writer = new StringWriter();
        try {
            Console.SetIn(reader);
            Console.SetOut(writer);
            menu.Run();
        }
        finally {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
        }
        Assert.Single(facade.GetStudents());
        Assert.Single(facade.GetCriteria());
        Assert.Equal(8m, Assert.Single(facade.GetEvaluations()).Score.Value);
        Assert.Contains("Анна: 8", writer.ToString());
    }

    private sealed class FakeExporter : IResultExporter {
        public IReadOnlyList<StudentResult> Results { get; private set; } = new List<StudentResult>();

        public void Export(IReadOnlyList<StudentResult> results, string path) {
            Results = results;
        }
    }
}
