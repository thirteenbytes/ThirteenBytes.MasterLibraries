namespace ThirteenBytes.DDDPatterns.Primitives.Common
{
    public class Result
    {
        public bool IsSuccess { get; init; }
        public bool IsFailure => !IsSuccess;
        public IReadOnlyList<Error> Errors { get; init; }

        protected Result(bool isSuccess, IReadOnlyList<Error> errors)
        {
            IsSuccess = isSuccess;
            Errors = errors;
        }

        public static Result Success() => new(true, Array.Empty<Error>());

        public static Result Failure(Error error) => new(false, new[] { error });

        public static Result Failure(IReadOnlyList<Error> errors) => new(false, errors);

        public static Result Combine(params Result[] results)
        {
            var errors = results
                .Where(r => r.IsFailure)
                .SelectMany(r => r.Errors)
                .ToList();

            return errors.Any() ? Failure(errors) : Success();
        }

        public static implicit operator Result(Error error) => Failure(error);

        public static implicit operator Result(Error[] errors) => Failure(errors);

        public static implicit operator Result(List<Error> errors) => Failure(errors);
    }

    public class Result<T> : Result
    {
        public T? Value { get; }

        private Result(T value) : base(true, Array.Empty<Error>())
        {
            Value = value;
        }

        private Result(IReadOnlyList<Error> errors) : base(false, errors)
        {
            Value = default;
        }

        public static Result<T> Success(T value) => new(value);

        public static new Result<T> Failure(Error error) => new(new[] { error });

        public static new Result<T> Failure(IReadOnlyList<Error> errors) => new(errors);

        public static implicit operator Result<T>(T value) => Success(value);

        public static implicit operator Result<T>(Error error) => Failure(error);

        public static implicit operator Result<T>(List<Error> errors) => Failure(errors);

        public static implicit operator Result<T>(Error[] errors) => Failure(errors);

        public T ValueOrThrow()
        {
            if (IsFailure)
                throw new InvalidOperationException("Cannot access Value when result is failure.");
            return Value!;
        }

        public TResult Match<TResult>(
            Func<T, TResult> onSuccess,
            Func<IReadOnlyList<Error>, TResult> onFailure)
        {
            return IsSuccess ? onSuccess(Value!) : onFailure(Errors);
        }
    }
}
