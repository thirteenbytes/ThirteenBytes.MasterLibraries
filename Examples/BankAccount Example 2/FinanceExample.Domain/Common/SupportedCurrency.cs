using System.Text.RegularExpressions;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions;
using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace FinanceExample.Domain.Common
{
    public class SupportedCurrency : AuditEntity<SupportedCurrencyId>
    {
        public string CurrencyCode { get; private set; } = null!;
        public string CurrencyName { get; private set; } = null!;
        public string Country { get; private set; } = null!;
        public bool IsActive { get; private set; }

        // For EF Core
        private SupportedCurrency() { }

        private SupportedCurrency(
            string currencyCode,
            string currencyName,
            string country,
            bool isActive = true) : base(SupportedCurrencyId.From(currencyCode))
        {
            CurrencyCode = currencyCode;
            CurrencyName = currencyName;
            Country = country;
            IsActive = isActive;
        }

        public static Result<SupportedCurrency> Create(
            string currencyCode,
            string currencyName,
            string country,
            bool isActive = true)
        {
            var errors = ValidateSupportedCurrency(currencyCode, currencyName, country);
            if (errors.Any())
            {
                return Result<SupportedCurrency>.Failure(errors);
            }

            return new SupportedCurrency(
                currencyCode.ToUpperInvariant(),
                currencyName.Trim(),
                country.Trim(),
                isActive);
        }

        private static List<Error> ValidateSupportedCurrency(string currencyCode, string currencyName, string country)
        {
            var errors = new List<Error>();

            // Currency code validation - ISO 4217 currency codes are 3 alphabetic characters
            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                errors.Add(Error.Validation("Currency code cannot be empty"));
            }
            else if (!IsValidCurrencyCode(currencyCode))
            {
                errors.Add(Error.Validation("Currency code must be a valid ISO 4217 code (3 alphabetic characters)"));
            }

            // Currency name validation
            if (string.IsNullOrWhiteSpace(currencyName))
            {
                errors.Add(Error.Validation("Currency name cannot be empty"));
            }
            else if (currencyName.Trim().Length < 2)
            {
                errors.Add(Error.Validation("Currency name must be at least 2 characters long"));
            }
            else if (currencyName.Trim().Length > 100)
            {
                errors.Add(Error.Validation("Currency name cannot exceed 100 characters"));
            }

            // Country validation
            if (string.IsNullOrWhiteSpace(country))
            {
                errors.Add(Error.Validation("Country cannot be empty"));
            }
            else if (country.Trim().Length < 2)
            {
                errors.Add(Error.Validation("Country must be at least 2 characters long"));
            }
            else if (country.Trim().Length > 100)
            {
                errors.Add(Error.Validation("Country cannot exceed 100 characters"));
            }

            return errors;
        }

        private static bool IsValidCurrencyCode(string currencyCode)
        {
            if (currencyCode.Length != 3)
            {
                return false;
            }

            // Check if all characters are letters
            return Regex.IsMatch(currencyCode, "^[A-Za-z]{3}$");
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void UpdateDetails(string currencyName, string country)
        {
            var errors = new List<Error>();

            // Validate currency name
            if (string.IsNullOrWhiteSpace(currencyName))
            {
                errors.Add(Error.Validation("Currency name cannot be empty"));
            }
            else if (currencyName.Trim().Length < 2)
            {
                errors.Add(Error.Validation("Currency name must be at least 2 characters long"));
            }
            else if (currencyName.Trim().Length > 100)
            {
                errors.Add(Error.Validation("Currency name cannot exceed 100 characters"));
            }

            // Validate country
            if (string.IsNullOrWhiteSpace(country))
            {
                errors.Add(Error.Validation("Country cannot be empty"));
            }
            else if (country.Trim().Length < 2)
            {
                errors.Add(Error.Validation("Country must be at least 2 characters long"));
            }
            else if (country.Trim().Length > 100)
            {
                errors.Add(Error.Validation("Country cannot exceed 100 characters"));
            }

            if (errors.Any())
            {
                throw new ArgumentException($"Validation failed: {string.Join(", ", errors.Select(e => e.Description))}");
            }

            CurrencyName = currencyName.Trim();
            Country = country.Trim();
        }

        public override string ToString()
        {
            return $"{CurrencyCode}-{CurrencyName}-{Country}";
        }
    }
}