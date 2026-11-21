using Pos.Application;
using Pos.Domain;

namespace Pos.Infrastructure;

public sealed class InMemoryCourseRepository : ICourseRepository {
    private Course course = new();

    public Course Get() {
        return course;
    }

    public void Save(Course course) {
        ArgumentNullException.ThrowIfNull(course);
        this.course = course;
    }
}

