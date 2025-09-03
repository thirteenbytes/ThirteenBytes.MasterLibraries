using System.Text.RegularExpressions;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions;
using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace FinanceExample.Domain.Common
{
    public class Money : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        private Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        // Factory method for basic format validation (existing behavior)
        public static Result<Money> Create(decimal amount, string currency)
        {
            return WithValidation(
                () => ValidateMoneyFormat(amount, currency),
                () => new Money(amount, currency));
        }

        // New factory method that accepts pre-validated currency
        public static Result<Money> CreateWithValidatedCurrency(decimal amount, string currency)
        {
            var errors = new List<Error>();

            // Amount validation
            if (amount < 0)
            {
                errors.Add(Error.Validation("Amount cannot be negative"));
            }

            // Assume currency is already validated
            if (string.IsNullOrWhiteSpace(currency))
            {
                errors.Add(Error.Validation("Currency code cannot be empty"));
            }

            if (errors.Any())
            {
                return Result<Money>.Failure(errors);
            }

            return new Money(amount, currency.ToUpperInvariant());
        }

        private static List<Error> ValidateMoneyFormat(decimal amount, string currency)
        {
            var errors = new List<Error>();

            // Amount validation
            if (amount < 0)
            {
                errors.Add(Error.Validation("Amount cannot be negative"));
            }

            // Currency format validation - ISO 4217 currency codes are 3 alphabetic characters
            if (string.IsNullOrWhiteSpace(currency))
            {
                errors.Add(Error.Validation("Currency code cannot be empty"));
            }
            else if (!IsValidCurrencyCodeFormat(currency))
            {
                errors.Add(Error.Validation("Currency must be a valid ISO 4217 code (3 alphabetic characters)"));
            }

            return errors;
        }

        private static bool IsValidCurrencyCodeFormat(string currency)
        {
            // Basic ISO 4217 validation: exactly 3 alphabetic characters
            if (currency.Length != 3)
            {
                return false;
            }

            // Check if all characters are letters
            return Regex.IsMatch(currency, "^[A-Z]{3}$", RegexOptions.IgnoreCase);
        }

        // Currency operations now return Result<Money> instead of throwing exceptions
        public Result<Money> Add(Money other)
        {
            var currencyValidation = ValidateSameCurrency(other);
            if (currencyValidation.IsFailure)
            {
                return Result<Money>.Failure(currencyValidation.Errors);
            }

            return new Money(Amount + other.Amount, Currency);
        }

        public Result<Money> Subtract(Money other)
        {
            var currencyValidation = ValidateSameCurrency(other);
            if (currencyValidation.IsFailure)
            {
                return Result<Money>.Failure(currencyValidation.Errors);
            }

            return new Money(Amount - other.Amount, Currency);
        }

        public Money Multiply(decimal factor)
        {
            return new Money(Amount * factor, Currency);
        }

        private Result ValidateSameCurrency(Money other)
        {
            if (Currency != other.Currency)
            {
                return Error.InvalidInput(
                    $"Cannot perform operations on money with different currencies: {Currency} and {other.Currency}");
            }

            return Result.Success();
        }

        public override string ToString()
        {
            return $"{Amount} {Currency}";
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }
}