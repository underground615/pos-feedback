namespace Pos.Application;

public interface IResultExporter {
    void Export(IReadOnlyList<StudentResult> results, string path);
}

