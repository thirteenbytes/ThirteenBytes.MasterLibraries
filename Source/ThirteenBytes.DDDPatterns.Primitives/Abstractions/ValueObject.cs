using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions
{
    /// <summary>
    /// Base class for value objects that wrap a single value with validation.
    /// Provides a strongly-typed wrapper around primitive values with explicit conversion operators.
    /// </summary>
    /// <typeparam name="TValue">The type of the underlying value.</typeparam>
    /// <typeparam name="TSelf">The concrete value object type (self-referencing generic pattern).</typeparam>
    public abstract class ValueObject<TValue, TSelf>
    {
        /// <summary>
        /// Gets the underlying value wrapped by this value object.
        /// </summary>
        public TValue Value { get; }

        /// <summary>
        /// Initializes a new instance of the value object with the specified value.
        /// </summary>
        /// <param name="value">The value to wrap. Cannot be null.</param>
        protected ValueObject(TValue value) =>
            Value = value!;

        /// <summary>
        /// Explicitly converts a value object to its underlying value type.
        /// </summary>
        /// <param name="valueObject">The value object to convert.</param>
        /// <returns>The underlying value.</returns>
        public static explicit operator TValue(ValueObject<TValue, TSelf> valueObject) =>
            valueObject.Value;

        /// <summary>
        /// Returns a string representation of the underlying value.
        /// </summary>
        /// <returns>A string representation of the value, or empty string if value is null.</returns>
        public override string ToString() =>
            Value?.ToString() ?? string.Empty;

        /// <summary>
        /// Helper method for creating value objects with validation.
        /// Validates the input and returns either a successful result with the created value object,
        /// or a failed result with validation errors.
        /// </summary>
        /// <param name="input">The input value to validate and wrap.</param>
        /// <param name="validate">Function that returns a list of validation errors for the input.</param>
        /// <param name="creator">Function that creates the value object instance from valid input.</param>
        /// <returns>A Result containing either the created value object or validation errors.</returns>
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
    /// Base class for value objects in Domain-Driven Design.
    /// Value objects are equality-comparable by their properties rather than identity.
    /// They are immutable and represent domain concepts that are defined by their attributes.
    /// </summary>
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        /// <summary>
        /// Determines whether the specified object is equal to the current value object.
        /// Value objects are equal if all their components are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current value object.</param>
        /// <returns>true if the specified object is equal to the current value object; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != GetType()) return false;
            return Equals((ValueObject)obj);
        }

        /// <summary>
        /// Determines whether the specified value object is equal to the current value object.
        /// Value objects are equal if all their components are equal.
        /// </summary>
        /// <param name="other">The value object to compare with the current value object.</param>
        /// <returns>true if the specified value object is equal to the current value object; otherwise, false.</returns>
        public bool Equals(ValueObject? other)
        {
            if (other is null || other.GetType() != GetType()) return false;
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        /// <summary>
        /// Returns the hash code for this value object based on its equality components.
        /// Uses System.HashCode to reduce hash collisions.
        /// </summary>
        /// <returns>A hash code for the current value object.</returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var component in GetEqualityComponents())
                hash.Add(component);
            return hash.ToHashCode();
        }

        /// <summary>
        /// Returns a string representation of the value object showing its type and component values.
        /// </summary>
        /// <returns>A string representation of the value object.</returns>
        public override string ToString()
        {
            return $"{GetType().Name}[{string.Join(", ", GetEqualityComponents().Select(x => x?.ToString() ?? "null"))}]";
        }

        /// <summary>
        /// Determines whether two value object instances are equal.
        /// </summary>
        /// <param name="left">The first value object to compare.</param>
        /// <param name="right">The second value object to compare.</param>
        /// <returns>true if the value objects are equal; otherwise, false.</returns>
        public static bool operator ==(ValueObject? left, ValueObject? right) =>
            EqualOperator(left, right);

        /// <summary>
        /// Determines whether two value object instances are not equal.
        /// </summary>
        /// <param name="left">The first value object to compare.</param>
        /// <param name="right">The second value object to compare.</param>
        /// <returns>true if the value objects are not equal; otherwise, false.</returns>
        public static bool operator !=(ValueObject? left, ValueObject? right) =>
            NotEqualOperator(left, right);

        /// <summary>
        /// Returns the components that define equality for this value object.
        /// All components returned by this method will be used for equality comparison and hash code generation.
        /// </summary>
        /// <returns>An enumerable of objects that define the value object's equality.</returns>
        protected abstract IEnumerable<object?> GetEqualityComponents();

        /// <summary>
        /// Helper method for creating value objects with validation.
        /// Validates the input and returns either a successful result with the created value object,
        /// or a failed result with validation errors.
        /// </summary>
        /// <typeparam name="T">The type of the value object to create.</typeparam>
        /// <param name="validate">Function that returns a list of validation errors.</param>
        /// <param name="creator">Function that creates the value object instance.</param>
        /// <returns>A Result containing either the created value object or validation errors.</returns>
        protected static Result<T> WithValidation<T>(
            Func<List<Error>> validate,
            Func<T> creator)
        {
            var errors = validate();
            return errors.Any() ? errors : creator();
        }

        /// <summary>
        /// Provides a reusable equality helper for derived operator== implementations.
        /// </summary>
        /// <param name="left">The first value object to compare.</param>
        /// <param name="right">The second value object to compare.</param>
        /// <returns>true if the value objects are equal; otherwise, false.</returns>
        protected static bool EqualOperator(ValueObject? left, ValueObject? right) =>
            left is null ^ right is null ? false : (left?.Equals(right!) ?? true);

        /// <summary>
        /// Provides a reusable inequality helper for derived operator!= implementations.
        /// </summary>
        /// <param name="left">The first value object to compare.</param>
        /// <param name="right">The second value object to compare.</param>
        /// <returns>true if the value objects are not equal; otherwise, false.</returns>
        protected static bool NotEqualOperator(ValueObject? left, ValueObject? right) =>
            !EqualOperator(left, right);
    }
}
