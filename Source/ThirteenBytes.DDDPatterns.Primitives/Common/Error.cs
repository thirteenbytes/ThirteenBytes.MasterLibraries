namespace ThirteenBytes.DDDPatterns.Primitives.Common
{
    /// <summary>
    /// Represents an error with a code and description, used throughout the DDD patterns library
    /// for consistent error handling and reporting.
    /// </summary>
    /// <param name="Code">The error code that identifies the type of error.</param>
    /// <param name="Description">A human-readable description of the error.</param>
    public record Error(string Code, string Description)
    {
        /// <summary>
        /// Creates a "not found" error for the specified domain model.
        /// </summary>
        /// <param name="domainModelName">The name of the domain model that was not found.</param>
        /// <returns>An Error instance with a standardized "not found" message.</returns>
        public static Error NotFound(string domainModelName) => new($"{domainModelName}.NotFound", $"The specified {domainModelName} was not found.");

        /// <summary>
        /// Creates an "already exists" error for the specified domain model.
        /// </summary>
        /// <param name="domainModelName">The name of the domain model that already exists.</param>
        /// <returns>An Error instance with a standardized "already exists" message.</returns>
        public static Error Exists(string domainModelName) => new($"{domainModelName}.Exists", $"The specified {domainModelName} already exists.");

        /// <summary>
        /// Creates an "invalid input" error with a custom description.
        /// </summary>
        /// <param name="description">A description of what input was invalid.</param>
        /// <returns>An Error instance with an "InvalidInput" code.</returns>
        public static Error InvalidInput(string description) => new("InvalidInput", description);

        /// <summary>
        /// Creates an "unauthorized" error with a custom description.
        /// </summary>
        /// <param name="description">A description of the authorization failure.</param>
        /// <returns>An Error instance with an "Unauthorized" code.</returns>
        public static Error Unauthorized(string description) => new("Unauthorized", description);

        /// <summary>
        /// Creates an "internal error" for unexpected system failures.
        /// </summary>
        /// <param name="description">A description of the internal error.</param>
        /// <returns>An Error instance with an "InternalError" code.</returns>
        public static Error InternalError(string description) => new("InternalError", description);

        /// <summary>
        /// Creates a "validation" error for business rule violations.
        /// </summary>
        /// <param name="description">A description of the validation failure.</param>
        /// <returns>An Error instance with a "Validation" code.</returns>
        public static Error Validation(string description) => new("Validation", description);
    }
}
