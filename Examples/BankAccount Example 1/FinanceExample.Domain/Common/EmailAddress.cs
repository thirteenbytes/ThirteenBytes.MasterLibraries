using System.Text.RegularExpressions;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions;
using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace FinanceExample.Domain.Common
{
    public class EmailAddress : ValueObject<string, EmailAddress>
    {
        private EmailAddress(string value) : base(value)
        {
        }

        public static Result<EmailAddress> Create(string emailAddress)
        {
            return WithValidation(
                emailAddress,
                Validate,
                email => new EmailAddress(email));
        }

        private static List<Error> Validate(string email)
        {
            var errors = new List<Error>();

            if (string.IsNullOrWhiteSpace(email))
            {
                errors.Add(Error.Validation("Email address cannot be empty"));
                return errors;
            }

            if (email.Length > 256)
            {
                errors.Add(Error.Validation("Email address is too long (maximum 256 characters)"));
            }

            // RFC 5322 compliant email validation
            if (!IsValidEmail(email))
            {
                errors.Add(Error.Validation("Invalid email format"));
            }

            return errors;
        }

        private static bool IsValidEmail(string email)
        {
            // Simplified but effective email validation
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                if (!regex.IsMatch(email))
                    return false;

                // Check for basic structural requirements
                var parts = email.Split('@');
                if (parts.Length != 2)
                    return false;

                var domainParts = parts[1].Split('.');
                if (domainParts.Length < 2)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}