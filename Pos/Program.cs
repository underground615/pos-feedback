using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Pos.Application;
using Pos.Domain;
using Pos.Infrastructure;
using Pos.ConsoleApp;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

var services = new ServiceCollection();
services.AddSingleton<ICourseRepository, InMemoryCourseRepository>();
services.AddSingleton<IResultExporter, CsvResultExporter>();
services.AddSingleton<FinalScoreService>();
services.AddSingleton<WeightedScoreStrategy>();
services.AddSingleton<CompleteScoreStrategy>();
services.AddSingleton<PosFacade>();
services.AddTransient<ConsoleMenu>();
using var provider = services.BuildServiceProvider();
var menu = provider.GetRequiredService<ConsoleMenu>();

if (args.Contains("--demo")) {
    try {
        menu.RunDemo();
    }
    catch (IOException exception) {
        Console.WriteLine($"Ошибка экспорта: {exception.Message}");
        Environment.ExitCode = 1;
    }
    catch (UnauthorizedAccessException exception) {
        Console.WriteLine($"Нет доступа: {exception.Message}");
        Environment.ExitCode = 1;
    }
}
else {
    menu.Run();
}

