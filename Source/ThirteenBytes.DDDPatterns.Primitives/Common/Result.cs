namespace ThirteenBytes.DDDPatterns.Primitives.Common
{
    /// <summary>
    /// Represents the result of an operation that can either succeed or fail, following the Result pattern.
    /// This class provides a way to handle errors without throwing exceptions.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; init; }

        /// <summary>
        /// Gets a value indicating whether the operation failed.
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Gets the collection of errors that occurred during the operation.
        /// Empty when IsSuccess is true.
        /// </summary>
        public IReadOnlyList<Error> Errors { get; init; }

        /// <summary>
        /// Initializes a new instance of the Result class.
        /// </summary>
        /// <param name="isSuccess">Whether the operation was successful.</param>
        /// <param name="errors">The collection of errors that occurred.</param>
        protected Result(bool isSuccess, IReadOnlyList<Error> errors)
        {
            IsSuccess = isSuccess;
            Errors = errors;
        }

        /// <summary>
        /// Creates a successful result with no errors.
        /// </summary>
        /// <returns>A successful Result instance.</returns>
        public static Result Success() => new(true, Array.Empty<Error>());

        /// <summary>
        /// Creates a failed result with a single error.
        /// </summary>
        /// <param name="error">The error that occurred.</param>
        /// <returns>A failed Result instance containing the error.</returns>
        public static Result Failure(Error error) => new(false, new[] { error });

        /// <summary>
        /// Creates a failed result with multiple errors.
        /// </summary>
        /// <param name="errors">The collection of errors that occurred.</param>
        /// <returns>A failed Result instance containing the errors.</returns>
        public static Result Failure(IReadOnlyList<Error> errors) => new(false, errors);

        /// <summary>
        /// Combines multiple results into a single result. If any result is a failure,
        /// the combined result will be a failure containing all errors.
        /// </summary>
        /// <param name="results">The results to combine.</param>
        /// <returns>A combined Result instance.</returns>
        public static Result Combine(params Result[] results)
        {
            var errors = results
                .Where(r => r.IsFailure)
                .SelectMany(r => r.Errors)
                .ToList();

            return errors.Any() ? Failure(errors) : Success();
        }

        /// <summary>
        /// Implicitly converts an Error to a failed Result.
        /// </summary>
        /// <param name="error">The error to convert.</param>
        public static implicit operator Result(Error error) => Failure(error);

        /// <summary>
        /// Implicitly converts an array of Errors to a failed Result.
        /// </summary>
        /// <param name="errors">The errors to convert.</param>
        public static implicit operator Result(Error[] errors) => Failure(errors);

        /// <summary>
        /// Implicitly converts a list of Errors to a failed Result.
        /// </summary>
        /// <param name="errors">The errors to convert.</param>
        public static implicit operator Result(List<Error> errors) => Failure(errors);
    }

    /// <summary>
    /// Represents the result of an operation that can either succeed with a value or fail with errors.
    /// This is a generic version of Result that includes a value when successful.
    /// </summary>
    /// <typeparam name="T">The type of the value returned on success.</typeparam>
    public class Result<T> : Result
    {
        /// <summary>
        /// Gets the value returned by a successful operation.
        /// Will be default(T) when IsFailure is true.
        /// </summary>
        public T? Value { get; }

        /// <summary>
        /// Initializes a new successful Result with a value.
        /// </summary>
        /// <param name="value">The value to return.</param>
        private Result(T value) : base(true, Array.Empty<Error>())
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new failed Result with errors.
        /// </summary>
        /// <param name="errors">The collection of errors that occurred.</param>
        private Result(IReadOnlyList<Error> errors) : base(false, errors)
        {
            Value = default;
        }

        /// <summary>
        /// Creates a successful result with a value.
        /// </summary>
        /// <param name="value">The value to return.</param>
        /// <returns>A successful Result instance containing the value.</returns>
        public static Result<T> Success(T value) => new(value);

        /// <summary>
        /// Creates a failed result with a single error.
        /// </summary>
        /// <param name="error">The error that occurred.</param>
        /// <returns>A failed Result instance containing the error.</returns>
        public static new Result<T> Failure(Error error) => new(new[] { error });

        /// <summary>
        /// Creates a failed result with multiple errors.
        /// </summary>
        /// <param name="errors">The collection of errors that occurred.</param>
        /// <returns>A failed Result instance containing the errors.</returns>
        public static new Result<T> Failure(IReadOnlyList<Error> errors) => new(errors);

        /// <summary>
        /// Implicitly converts a value to a successful Result.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static implicit operator Result<T>(T value) => Success(value);

        /// <summary>
        /// Implicitly converts an Error to a failed Result.
        /// </summary>
        /// <param name="error">The error to convert.</param>
        public static implicit operator Result<T>(Error error) => Failure(error);

        /// <summary>
        /// Implicitly converts a list of Errors to a failed Result.
        /// </summary>
        /// <param name="errors">The errors to convert.</param>
        public static implicit operator Result<T>(List<Error> errors) => Failure(errors);

        /// <summary>
        /// Implicitly converts an array of Errors to a failed Result.
        /// </summary>
        /// <param name="errors">The errors to convert.</param>
        public static implicit operator Result<T>(Error[] errors) => Failure(errors);

        /// <summary>
        /// Gets the value or throws an InvalidOperationException if the result is a failure.
        /// </summary>
        /// <returns>The value if the result is successful.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the result is a failure.</exception>
        public T ValueOrThrow()
        {
            if (IsFailure)
                throw new InvalidOperationException("Cannot access Value when result is failure.");
            return Value!;
        }

        /// <summary>
        /// Matches the result against success and failure cases, returning a value of type TResult.
        /// </summary>
        /// <typeparam name="TResult">The type of the result to return.</typeparam>
        /// <param name="onSuccess">Function to execute when the result is successful.</param>
        /// <param name="onFailure">Function to execute when the result is a failure.</param>
        /// <returns>The result of executing either onSuccess or onFailure.</returns>
        public TResult Match<TResult>(
            Func<T, TResult> onSuccess,
            Func<IReadOnlyList<Error>, TResult> onFailure)
        {
            return IsSuccess ? onSuccess(Value!) : onFailure(Errors);
        }
    }
}
