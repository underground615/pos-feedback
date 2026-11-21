using Pos.Domain;
using Xunit;

namespace Pos.Tests;

public sealed class DomainTests {
    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void InvalidScoreDoesNotReplaceEvaluation(int value) {
        var course = new Course();
        var student = course.AddStudent("Анна");
        var criterion = course.AddCriterion("Экзамен", 10, 1);
        course.SetEvaluation(student.Id, criterion.Id, 7);
        Assert.Throws<ArgumentOutOfRangeException>(() => course.SetEvaluation(student.Id, criterion.Id, value));
        Assert.Equal(7m, Assert.Single(course.Evaluations).Score.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void ScoreBoundaries(int value) {
        var course = new Course();
        var student = course.AddStudent("Анна");
        var criterion = course.AddCriterion("Экзамен", 10, 1);
        course.SetEvaluation(student.Id, criterion.Id, value);
        Assert.Equal((decimal)value, Assert.Single(course.Evaluations).Score.Value);
    }

    [Fact]
    public void UnknownReferencesAreRejected() {
        var course = new Course();
        var student = course.AddStudent("Анна");
        var criterion = course.AddCriterion("Экзамен", 10, 1);
        Assert.Throws<InvalidOperationException>(() => course.SetEvaluation(Guid.NewGuid(), criterion.Id, 5));
        Assert.Throws<InvalidOperationException>(() => course.SetEvaluation(student.Id, Guid.NewGuid(), 5));
        Assert.Empty(course.Evaluations);
    }

    [Fact]
    public void ReplaceAndRemoveEvaluation() {
        var course = new Course();
        var student = course.AddStudent("Анна");
        var criterion = course.AddCriterion("Экзамен", 10, 1);
        course.SetEvaluation(student.Id, criterion.Id, 3);
        course.SetEvaluation(student.Id, criterion.Id, 9);
        Assert.Equal(9m, Assert.Single(course.Evaluations).Score.Value);
        Assert.True(course.RemoveEvaluation(student.Id, criterion.Id));
        Assert.False(course.RemoveEvaluation(student.Id, criterion.Id));
        Assert.Empty(course.Evaluations);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void EmptyNamesAreRejected(string name) {
        Assert.Throws<ArgumentException>(() => new Student(name));
        Assert.Throws<ArgumentException>(() => new Criterion(name, 10, 1));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(10, 0)]
    [InlineData(10, -1)]
    public void InvalidCriterion(int maximum, int weight) {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Criterion("Тест", maximum, weight));
    }

    [Fact]
    public void EntitiesHaveIdentityAndScoreHasValueEquality() {
        var first = new Student(" Анна ");
        var second = new Student("Анна");
        Assert.Equal("Анна", first.Name);
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(new Score(5), new Score(5));
        Assert.Equal("Тест", new Criterion(" Тест ", 10, 1).Name);
    }

    [Fact]
    public void AggregateCollectionsCannotBeMutated() {
        var course = new Course();
        Assert.Throws<NotSupportedException>(() => ((IList<Student>)course.Students).Add(new Student("Анна")));
    }

    [Fact]
    public void WeightedScoreNormalizesDifferentMaximumsAndWeights() {
        var course = new Course();
        var student = course.AddStudent("Анна");
        var homework = course.AddCriterion("ДЗ", 20, 2);
        var exam = course.AddCriterion("Экзамен", 100, 3);
        course.SetEvaluation(student.Id, homework.Id, 16);
        course.SetEvaluation(student.Id, exam.Id, 60);
        var service = new FinalScoreService();
        Assert.Equal(6.8m, service.Calculate(course, student.Id, new WeightedScoreStrategy()));
        Assert.Equal(6.8m, service.Calculate(course, student.Id, new CompleteScoreStrategy()));
        course.SetEvaluation(student.Id, exam.Id, 100);
        Assert.Equal(9.2m, service.Calculate(course, student.Id, new WeightedScoreStrategy()));
    }

    [Fact]
    public void MissingScoresFollowSelectedPolicy() {
        var course = new Course();
        var student = course.AddStudent("Анна");
        var service = new FinalScoreService();
        var weighted = new WeightedScoreStrategy();
        var complete = new CompleteScoreStrategy();
        Assert.Null(service.Calculate(course, student.Id, weighted));
        Assert.Null(service.Calculate(course, student.Id, complete));
        var criterion = course.AddCriterion("ДЗ", 10, 1);
        Assert.Equal(0m, service.Calculate(course, student.Id, weighted));
        Assert.Null(service.Calculate(course, student.Id, complete));
        course.SetEvaluation(student.Id, criterion.Id, 10);
        Assert.Equal(10m, service.Calculate(course, student.Id, complete));
        course.AddCriterion("Экзамен", 10, 1);
        Assert.Equal(5m, service.Calculate(course, student.Id, weighted));
        Assert.Null(service.Calculate(course, student.Id, complete));
    }

    [Fact]
    public void ResultIsRoundedAndStudentMustExist() {
        var course = new Course();
        var student = course.AddStudent("Анна");
        var criterion = course.AddCriterion("ДЗ", 3, 1);
        course.SetEvaluation(student.Id, criterion.Id, 1);
        var service = new FinalScoreService();
        Assert.Equal(3.33m, service.Calculate(course, student.Id, new WeightedScoreStrategy()));
        Assert.Throws<InvalidOperationException>(() => service.Calculate(course, Guid.NewGuid(), new WeightedScoreStrategy()));
        Assert.Throws<InvalidOperationException>(() => service.Calculate(course, Guid.NewGuid(), new CompleteScoreStrategy()));
    }
}
