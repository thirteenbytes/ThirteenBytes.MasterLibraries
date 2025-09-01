using System.Collections.Generic;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions;
using ThirteenBytes.DDDPatterns.Primitives.Common;
using FinanceExample.Domain;

namespace FinanceExample.Domain.Common
{
    public class FullName : ValueObject
    {
        public string FirstName { get; }
        public string LastName { get; }

        private FullName(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public static Result<FullName> Create(string firstName, string lastName)
        {
            return WithValidation(
                () => ValidateFullName(firstName, lastName),
                () => new FullName(firstName, lastName));
        }

        private static List<Error> ValidateFullName(string firstName, string lastName)
        {
            var errors = new List<Error>();
            
            if (string.IsNullOrWhiteSpace(firstName))
            {
                errors.Add(Error.Validation("First name cannot be empty"));
            }
            else if (firstName.Length < Constants.FirstName.MinimumLength)
            {
                errors.Add(Error.Validation($"First name must be at least {Constants.FirstName.MinimumLength} characters"));
            }
            else if (firstName.Length > Constants.FirstName.MaximumLength)
            {
                errors.Add(Error.Validation($"First name must not exceed {Constants.FirstName.MinimumLength} characters"));
            }
                
            if (string.IsNullOrWhiteSpace(lastName))
            {
                errors.Add(Error.Validation("Last name cannot be empty"));
            }
            else if (lastName.Length < Constants.LastName.MinimumLength)
            {
                errors.Add(Error.Validation($"Last name must be at least {Constants.LastName.MinimumLength} characters"));
            }
            else if (lastName.Length > Constants.LastName.MaximumLength)
            {
                errors.Add(Error.Validation($"Last name must not exceed {Constants.LastName.MaximumLength} characters"));
            }
                
            return errors;
        }

        // Returns the full formatted name
        public string FullNameFormatted => $"{FirstName} {LastName}";

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return FirstName;
            yield return LastName;
        }
    }
}