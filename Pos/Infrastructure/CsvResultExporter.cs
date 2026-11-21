using System.Globalization;
using System.Text;
using Pos.Application;

namespace Pos.Infrastructure;

public sealed class CsvResultExporter : IResultExporter {
    public void Export(IReadOnlyList<StudentResult> results, string path) {
        using var writer = new StreamWriter(path, false, new UTF8Encoding(true));
        writer.WriteLine("StudentId;StudentName;Score");
        foreach (var result in results) {
            var name = "\"" + result.StudentName.Replace("\"", "\"\"") + "\"";
            var score = result.Score?.ToString(CultureInfo.InvariantCulture) ?? "";
            writer.WriteLine($"{result.StudentId};{name};{score}");
        }
    }
}

