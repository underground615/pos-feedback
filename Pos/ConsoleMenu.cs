using System.Globalization;
using Pos.Application;
using Pos.Domain;

namespace Pos.ConsoleApp;

public sealed class ConsoleMenu {
    private readonly PosFacade facade;
    private readonly WeightedScoreStrategy weighted;
    private readonly CompleteScoreStrategy complete;
    private IFinalScoreStrategy strategy;

    public ConsoleMenu(PosFacade facade, WeightedScoreStrategy weighted, CompleteScoreStrategy complete) {
        this.facade = facade;
        this.weighted = weighted;
        this.complete = complete;
        strategy = weighted;
    }

    public void Run() {
        Console.WriteLine("ПОС. Данные хранятся в памяти до выхода.");
        while (true) {
            Console.WriteLine("\n1. Добавить студента\n2. Добавить критерий\n3. Списки студентов и критериев" +
                "\n4. Выставить или исправить оценку\n5. Посмотреть оценки\n6. Отменить оценку" +
                "\n7. Итоги\n8. Экспорт CSV\n9. Политика пропусков\n0. Выход");
            Console.Write("> ");
            var command = Console.ReadLine();
            if (command == null || command == "0") {
                return;
            }
            try {
                Execute(command.Trim());
            }
            catch (EndOfStreamException) {
                return;
            }
            catch (ArgumentException exception) {
                Console.WriteLine($"Ошибка: {exception.Message}");
            }
            catch (InvalidOperationException exception) {
                Console.WriteLine($"Ошибка: {exception.Message}");
            }
            catch (IOException exception) {
                Console.WriteLine($"Ошибка файла: {exception.Message}");
            }
            catch (UnauthorizedAccessException exception) {
                Console.WriteLine($"Нет доступа к файлу: {exception.Message}");
            }
        }
    }

    private void Execute(string command) {
        switch (command) {
            case "1":
                facade.AddStudent(Read("Имя студента"));
                Console.WriteLine("Студент добавлен.");
                break;
            case "2":
                facade.AddCriterion(Read("Название критерия"), ReadDecimal("Максимум"), ReadDecimal("Вес"));
                Console.WriteLine("Критерий добавлен.");
                break;
            case "3":
                PrintLists();
                break;
            case "4":
                PrintLists();
                facade.SetEvaluation(ReadStudent(), ReadCriterion(), ReadDecimal("Балл"));
                Console.WriteLine("Оценка сохранена.");
                break;
            case "5":
                PrintEvaluations();
                break;
            case "6":
                PrintLists();
                var removed = facade.RemoveEvaluation(ReadStudent(), ReadCriterion());
                Console.WriteLine(removed ? "Оценка отменена." : "Оценки не было.");
                break;
            case "7":
                PrintResults();
                break;
            case "8":
                facade.ExportResults(strategy, Read("Путь CSV (файл будет перезаписан)"));
                Console.WriteLine("Итоги экспортированы.");
                break;
            case "9":
                var policy = Read("1 - пропуски как нули; 2 - нужны все оценки");
                if (policy != "1" && policy != "2") {
                    throw new ArgumentException("Введите 1 или 2.");
                }
                strategy = policy == "1" ? weighted : complete;
                Console.WriteLine("Политика изменена.");
                break;
            default:
                Console.WriteLine("Неизвестная команда.");
                break;
        }
    }

    private void PrintLists() {
        Console.WriteLine("Студенты:");
        var students = facade.GetStudents();
        for (var i = 0; i < students.Count; i++) {
            Console.WriteLine($"{i + 1}. {students[i].Name}");
        }
        Console.WriteLine("Критерии:");
        var criteria = facade.GetCriteria();
        for (var i = 0; i < criteria.Count; i++) {
            var criterion = criteria[i];
            Console.WriteLine($"{i + 1}. {criterion.Name}: максимум {criterion.MaxScore}, вес {criterion.Weight}");
        }
    }

    private void PrintEvaluations() {
        var evaluations = facade.GetEvaluations();
        if (evaluations.Count == 0) {
            Console.WriteLine("Оценок пока нет.");
        }
        foreach (var evaluation in evaluations) {
            var student = facade.GetStudents().First(item => item.Id == evaluation.StudentId);
            var criterion = facade.GetCriteria().First(item => item.Id == evaluation.CriterionId);
            Console.WriteLine($"{student.Name} / {criterion.Name}: {evaluation.Score.Value} из {criterion.MaxScore}");
        }
    }

    private void PrintResults() {
        Console.WriteLine(strategy == weighted ? "Пропуски считаются нулями." : "Для итога нужны все оценки.");
        var results = facade.GetResults(strategy);
        if (results.Count == 0) {
            Console.WriteLine("Студентов пока нет.");
        }
        foreach (var result in results) {
            var score = result.Score?.ToString("0.##", CultureInfo.InvariantCulture) ?? "нет итога";
            Console.WriteLine($"{result.StudentName}: {score}");
        }
    }

    private Guid ReadStudent() {
        var students = facade.GetStudents();
        return students[ReadIndex("Номер студента", students.Count)].Id;
    }

    private Guid ReadCriterion() {
        var criteria = facade.GetCriteria();
        return criteria[ReadIndex("Номер критерия", criteria.Count)].Id;
    }

    private static int ReadIndex(string prompt, int count) {
        if (!int.TryParse(Read(prompt), out var index) || index < 1 || index > count) {
            throw new ArgumentException("Нет элемента с таким номером.");
        }
        return index - 1;
    }

    private static string Read(string prompt) {
        Console.Write($"{prompt}: ");
        var value = Console.ReadLine();
        if (value == null) {
            throw new EndOfStreamException();
        }
        return value.Trim();
    }

    private static decimal ReadDecimal(string prompt) {
        var value = Read(prompt).Replace(',', '.');
        if (!decimal.TryParse(value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out var number)) {
            throw new ArgumentException("Введите число, например 2,5.");
        }
        return number;
    }

    public void RunDemo() {
        Console.WriteLine("ПОС: демонстрация");
        var anna = facade.AddStudent("Анна");
        var ivan = facade.AddStudent("Иван");
        var homework = facade.AddCriterion("Домашка", 10, 0.4m);
        var exam = facade.AddCriterion("Экзамен", 10, 0.6m);
        facade.SetEvaluation(anna.Id, homework.Id, 8);
        facade.SetEvaluation(anna.Id, exam.Id, 6);
        facade.SetEvaluation(ivan.Id, homework.Id, 10);
        PrintLists();
        PrintEvaluations();
        PrintResults();
        strategy = complete;
        PrintResults();
        strategy = weighted;
        facade.ExportResults(strategy, "results.csv");
        Console.WriteLine("Экспорт: results.csv");
    }
}

