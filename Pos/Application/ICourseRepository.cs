using Pos.Domain;

namespace Pos.Application;

public interface ICourseRepository {
    Course Get();
    void Save(Course course);
}

