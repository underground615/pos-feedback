namespace Pos.Domain;

public sealed class Student {
    public Guid Id { get; }
    public string Name { get; }

    public Student(string name) {
        if (string.IsNullOrWhiteSpace(name)) {
            throw new ArgumentException("Имя студента не может быть пустым.", nameof(name));
        }
        Id = Guid.NewGuid();
        Name = name.Trim();
    }
}

