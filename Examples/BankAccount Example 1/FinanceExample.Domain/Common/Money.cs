using System;
using System.Collections.Generic;
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

        public static Result<Money> Create(decimal amount, string currency)
        {
            return WithValidation(
                () => ValidateMoney(amount, currency),
                () => new Money(amount, currency));
        }

        private static List<Error> ValidateMoney(decimal amount, string currency)
        {
            var errors = new List<Error>();
            
            // Amount validation
            if (amount < 0)
            {
                errors.Add(Error.Validation("Amount cannot be negative"));
            }
            
            // Currency validation - ISO 4217 currency codes are 3 alphabetic characters
            if (string.IsNullOrWhiteSpace(currency))
            {
                errors.Add(Error.Validation("Currency code cannot be empty"));
            }
            else if (!IsValidCurrencyCode(currency))
            {
                errors.Add(Error.Validation("Currency must be a valid ISO 4217 code (3 alphabetic characters)"));
            }
            
            return errors;
        }

        private static bool IsValidCurrencyCode(string currency)
        {
            // Basic ISO 4217 validation: exactly 3 alphabetic characters
            if (currency.Length != 3)
            {
                return false;
            }

            // Check if all characters are letters
            return Regex.IsMatch(currency, "^[A-Z]{3}$");
        }

        // Common currency operations
        public Money Add(Money other)
        {
            EnsureSameCurrency(other);
            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            EnsureSameCurrency(other);
            return new Money(Amount - other.Amount, Currency);
        }

        public Money Multiply(decimal factor)
        {
            return new Money(Amount * factor, Currency);
        }

        private void EnsureSameCurrency(Money other)
        {
            if (Currency != other.Currency)
            {
                throw new InvalidOperationException(
                    $"Cannot perform operations on money with different currencies: {Currency} and {other.Currency}");
            }
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