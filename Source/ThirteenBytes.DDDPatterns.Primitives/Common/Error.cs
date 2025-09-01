namespace ThirteenBytes.DDDPatterns.Primitives.Common
{
    public record Error(string Code, string Description)
    {
        public static Error NotFound(string domainModelName) => new($"{domainModelName}.NotFound", $"The specified {domainModelName} was not found.");
        public static Error Exists(string domainModelName) => new($"{domainModelName}.Exists", $"The specified {domainModelName} already exists.");
        public static Error InvalidInput(string description) => new("InvalidInput", description);
        public static Error Unauthorized(string description) => new("Unauthorized", description);
        public static Error InternalError(string description) => new("InternalError", description);
        public static Error Validation(string description) => new("Validation", description);
    }
}
