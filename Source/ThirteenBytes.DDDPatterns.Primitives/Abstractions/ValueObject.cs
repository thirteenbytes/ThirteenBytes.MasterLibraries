using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions
{
    public abstract class ValueObject<TValue, TSelf>
    {
        public TValue Value { get; }

        protected ValueObject(TValue value) =>
            Value = value!;

        public static explicit operator TValue(ValueObject<TValue, TSelf> valueObject) =>
            valueObject.Value;

        public override string ToString() =>
            Value?.ToString() ?? string.Empty;

        protected static Result<TSelf> WithValidation(
            TValue input,
            Func<TValue, List<Error>> validate,
            Func<TValue, TSelf> creator)
        {
            var errors = validate(input);
            return errors.Any() ? errors : creator(input);
        }
    }

    /// <summary>
    /// Base class for Value Objects in Domain-Driven Design.
    /// Value Objects are equality-comparable by their properties rather than identity.
    /// </summary>
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != GetType()) return false;
            return Equals((ValueObject)obj);
        }
        public bool Equals(ValueObject? other)
        {
            if (other is null || other.GetType() != GetType()) return false;
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public override int GetHashCode()
        {
            // Prefer HashCode to reduce collisions
            var hash = new HashCode();
            foreach (var component in GetEqualityComponents())
                hash.Add(component);
            return hash.ToHashCode();
        }
        public override string ToString()
        {
            return $"{GetType().Name}[{string.Join(", ", GetEqualityComponents().Select(x => x?.ToString() ?? "null"))}]";
        }

        public static bool operator ==(ValueObject? left, ValueObject? right) =>
            EqualOperator(left, right);

        public static bool operator !=(ValueObject? left, ValueObject? right) =>
            NotEqualOperator(left, right);
        protected abstract IEnumerable<object?> GetEqualityComponents();

        // Optional: shared validation helper for composite VOs
        protected static Result<T> WithValidation<T>(
            Func<List<Error>> validate,
            Func<T> creator)
        {
            var errors = validate();
            return errors.Any() ? errors : creator();
        }

        // Optional: provide a reusable equality helper for derived operator== implementations
        protected static bool EqualOperator(ValueObject? left, ValueObject? right) =>
            left is null ^ right is null ? false : (left?.Equals(right!) ?? true);

        protected static bool NotEqualOperator(ValueObject? left, ValueObject? right) =>
            !EqualOperator(left, right);
    }
}
