namespace Pos.Domain;

public readonly record struct Score {
    public decimal Value {
        get;
    }

    public Score(decimal value) {
        if (value < 0) {
            throw new ArgumentOutOfRangeException(nameof(value), "Балл не может быть отрицательным.");
        }
        Value = value;
    }
}

