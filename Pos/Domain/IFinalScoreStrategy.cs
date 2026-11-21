namespace Pos.Domain;

public interface IFinalScoreStrategy {
    // Курс и существование студента проверяет FinalScoreService перед расчётом.
    decimal? Calculate(Course course, Guid studentId);
}

