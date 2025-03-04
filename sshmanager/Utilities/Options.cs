using System.Diagnostics.CodeAnalysis;

namespace sshmanager.Utilities;
public class Option
{
    public static Option None { get; } = new();
    public static Option<T> Some<T>(T value) => new(value);
    public static Option<T> FromNull<T>(T? value) => value is null ? new() : new(value);
}

public class Option<T>
{
    private readonly T value;

    public Option(T value) {
        this.value = value;
        HasValue = true;
    }

    public Option() {
        value = default!;
        HasValue = false;
    }

    public bool HasValue { get; }

    public T Value => HasValue ? value : throw new InvalidOperationException("Option does not have a value.");

    public static Option<T> None => new();

    public static Option<T> Some(T value) => new(value);

    public T Unwrap() => Value;
    public T UnwrapOrDefault(T default_value) => HasValue ? Value : default_value;
    public bool TryUnwrap([NotNullWhen(true)] out T? value) {
        value = HasValue ? Value : default;
        return HasValue;
    }

    public TResult Map<TResult>(Func<T, TResult> has_value, Func<TResult> no_value) => HasValue ? has_value(Value) : no_value();
    public void Map(Action<T> has_value, Action no_value) { if (HasValue) { has_value(Value); } else { no_value(); } }

    //Overrides
    public static bool operator ==(Option<T> obj1, Option<T> obj2) {
        if (obj1.HasValue != obj2.HasValue) {
            return false;
        }

        if (!obj1.HasValue && !obj2.HasValue) {
            return true;
        }

        return obj1.Value!.Equals(obj2.Value);
    }

    public static bool operator !=(Option<T> obj1, Option<T> obj2) {
        if (obj1.HasValue != obj2.HasValue) {
            return true;
        }

        if (!obj1.HasValue && !obj2.HasValue) {
            return false;
        }

        return !(obj1.Value!.Equals(obj2.Value));
    }

    public override bool Equals(object? obj) {
        if (obj == null || GetType() != obj.GetType()) {
            return false;
        }

        Option<T> other = (Option<T>)obj;

        if (HasValue != other.HasValue) {
            return false;
        }

        if (!HasValue) {
            return true;
        }

        return Value!.Equals(other.Value);
    }

    public override int GetHashCode() => HashCode.Combine(value, HasValue);

    public static implicit operator Option<T>(T value) => new(value);

    public static implicit operator T(Option<T> value) => value.Value;

    public static implicit operator Option<T>(Option _) => new();

}