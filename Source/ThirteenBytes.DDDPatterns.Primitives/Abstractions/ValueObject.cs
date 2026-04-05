using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions
{
    /// <summary>
    /// Base class for value objects that wrap a <b>single</b> primitive value with validation.
    /// Provides a strongly-typed wrapper around primitive values with explicit conversion operators.
    /// Implements value-based equality semantics automatically.
    /// </summary>
    /// <remarks>
    /// Use this base class when the value object represents exactly one underlying primitive (e.g. a string ID,
    /// a monetary amount, or an email address). Equality is derived solely from <typeparamref name="TValue"/>.
    /// A <c>From</c> helper is provided for trusted-source reconstitution (e.g. EF Core
    /// <c>IEntityTypeConfiguration</c>) that bypasses domain validation.
    /// For value objects composed of <b>multiple</b> components, use <see cref="ValueObject"/> instead.
    /// </remarks>
    /// <typeparam name="TValue">The type of the underlying primitive value.</typeparam>
    /// <typeparam name="TSelf">The concrete value object type (self-referencing generic pattern).</typeparam>
    public abstract class ValueObject<TValue, TSelf> : IEquatable<ValueObject<TValue, TSelf>>
        where TSelf : ValueObject<TValue, TSelf>
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
        /// Determines whether the specified object is equal to the current value object.
        /// Value objects are equal if their wrapped values are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current value object.</param>
        /// <returns>true if the specified object is equal to the current value object; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ValueObject<TValue, TSelf>)obj);
        }

        /// <summary>
        /// Determines whether the specified value object is equal to the current value object.
        /// Value objects are equal if their wrapped values are equal.
        /// </summary>
        /// <param name="other">The value object to compare with the current value object.</param>
        /// <returns>true if the specified value object is equal to the current value object; otherwise, false.</returns>
        public bool Equals(ValueObject<TValue, TSelf>? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != GetType()) return false;
            return EqualityComparer<TValue>.Default.Equals(Value, other.Value);
        }

        /// <summary>
        /// Returns the hash code for this value object based on its wrapped value.
        /// </summary>
        /// <returns>A hash code for the current value object.</returns>
        public override int GetHashCode() =>
            Value is null ? 0 : EqualityComparer<TValue>.Default.GetHashCode(Value);

        /// <summary>
        /// Returns a string representation of the underlying value.
        /// </summary>
        /// <returns>A string representation of the value, or empty string if value is null.</returns>
        public override string ToString() =>
            Value?.ToString() ?? string.Empty;

        /// <summary>
        /// Determines whether two value object instances are equal.
        /// </summary>
        /// <param name="left">The first value object to compare.</param>
        /// <param name="right">The second value object to compare.</param>
        /// <returns>true if the value objects are equal; otherwise, false.</returns>
        public static bool operator ==(ValueObject<TValue, TSelf>? left, ValueObject<TValue, TSelf>? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two value object instances are not equal.
        /// </summary>
        /// <param name="left">The first value object to compare.</param>
        /// <param name="right">The second value object to compare.</param>
        /// <returns>true if the value objects are not equal; otherwise, false.</returns>
        public static bool operator !=(ValueObject<TValue, TSelf>? left, ValueObject<TValue, TSelf>? right) =>
            !(left == right);

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

        /// <summary>
        /// Creates a value object instance directly from a trusted value, bypassing validation.
        /// Intended for infrastructure concerns such as EF Core <c>IEntityTypeConfiguration</c>
        /// where values are read from a trusted source (e.g., the database) and are already known to be valid.
        /// </summary>
        /// <param name="value">The trusted value to wrap.</param>
        /// <param name="creator">Function that creates the value object instance from the value.</param>
        /// <returns>A new instance of <typeparamref name="TSelf"/> wrapping the provided value.</returns>
        protected static TSelf From(TValue value, Func<TValue, TSelf> creator) =>
            creator(value);
    }

    /// <summary>
    /// Base class for value objects in Domain-Driven Design that are composed of
    /// <b>multiple</b> components.
    /// Value objects are equality-comparable by their components rather than identity.
    /// They are immutable and represent domain concepts defined by their collective attributes.
    /// </summary>
    /// <remarks>
    /// Use this base class when the value object encapsulates more than one underlying value
    /// (e.g. a <c>Money</c> type with <c>Amount</c> and <c>Currency</c>, or an <c>Address</c>).
    /// Equality is driven by <see cref="GetEqualityComponents"/>, which every concrete type must implement.
    /// <para>
    /// A <c>From</c> helper is intentionally absent: because this class has no fixed <c>TValue</c>,
    /// the concrete type owns full knowledge of how to reconstitute itself from a trusted source
    /// (e.g. EF Core <c>IEntityTypeConfiguration</c>). Each derived class should expose its own
    /// static <c>From</c> factory if needed.
    /// </para>
    /// For value objects that wrap a <b>single</b> primitive, use <see cref="ValueObject{TValue, TSelf}"/> instead.
    /// </remarks>
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
