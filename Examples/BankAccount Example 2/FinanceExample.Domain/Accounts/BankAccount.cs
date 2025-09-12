using FinanceExample.Domain.Accounts.Events;
using FinanceExample.Domain.Common;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions;
using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace FinanceExample.Domain.Accounts
{
    public class BankAccount : AggregateRoot<BankAccountId>
    {
        public AccountHolderId AccountHolderId { get; private set; } = null!;
        public Money Balance { get; private set; } = null!;
        public AccountStatus Status { get; private set; }

        // For EF Core
        private BankAccount()
        {
            RegisterEventHandlers();
        }

        private BankAccount(BankAccountId id) : base(id)
        {
            RegisterEventHandlers();
        }

        private void RegisterEventHandlers()
        {
            On<BankAccountOpenedEvent>(OnBankAccountOpened);
            On<BankAccountClosedEvent>(OnBankAccountClosed);
            On<MoneyDepositedEvent>(OnMoneyDeposited);
            On<MoneyWithdrawnEvent>(OnMoneyWithdrawn);
        }

        // Factory method
        public static Result<BankAccount> Open(
            AccountHolderId accountHolderId,
            decimal initialAmount,
            string currency)
        {
            // Validate the money amount
            var moneyResult = Money.Create(initialAmount, currency);
            if (moneyResult.IsFailure)
            {
                return Result<BankAccount>.Failure(moneyResult.Errors);
            }

            if (accountHolderId == null)
            {
                return Result<BankAccount>.Failure(
                    Error.Validation("Account holder ID cannot be null"));
            }
            
            // Create a new BankAccount instance
            var account = new BankAccount(BankAccountId.New());
            
            // Apply the opened event directly
            var result = account.Apply(new BankAccountOpenedEvent(
                account.Id,
                accountHolderId,
                moneyResult.Value!));
                
            if (result.IsFailure)
            {
                return Result<BankAccount>.Failure(result.Errors);
            }
            
            return account;
        }

        public Result Close()
        {
            if (Status == AccountStatus.Closed)
            {
                return Error.InvalidInput("Account is already closed");
            }

            if(Balance.Amount != 0)
            {
                return Error.Validation("Account balance must be zero to close the account");
            }

            return Apply(new BankAccountClosedEvent(Id));
        }

        public Result Deposit(decimal amount, string currency)
        {
            if (Status == AccountStatus.Closed)
            {
                return Error.InvalidInput("Cannot deposit to a closed account");
            }

            var moneyResult = Money.Create(amount, currency);
            if (moneyResult.IsFailure)
            {
                return Result.Failure(moneyResult.Errors);
            }

            // Validate currency match
            if (Balance != null && Balance.Amount != 0 && Balance.Currency != currency)
            {
                return Error.InvalidInput($"Currency mismatch: account is in {Balance.Currency}, deposit is in {currency}");
            }

            return Apply(new MoneyDepositedEvent(Id, moneyResult.Value!));
        }

        public Result Withdraw(decimal amount, string currency)
        {
            if (Status == AccountStatus.Closed)
            {
                return Error.InvalidInput("Cannot withdraw from a closed account");
            }

            var moneyResult = Money.Create(amount, currency);
            if (moneyResult.IsFailure)
            {
                return Result.Failure(moneyResult.Errors);
            }

            // Validate currency match
            if (Balance.Currency != currency)
            {
                return Error.InvalidInput($"Currency mismatch: account is in {Balance.Currency}, withdrawal is in {currency}");
            }

            // Check if sufficient funds
            var withdrawalAmount = moneyResult.Value!;
            var canWithdrawResult = CanWithdraw(withdrawalAmount);
            if (canWithdrawResult.IsFailure)
            {
                return canWithdrawResult;
            }

            return Apply(new MoneyWithdrawnEvent(Id, withdrawalAmount));
        }

        private Result CanWithdraw(Money amount)
        {
            if (Balance is null)
            {
                return Error.InvalidInput("Insufficient funds");
            }

            var remainingResult = Balance.Subtract(amount);
            if (remainingResult.IsFailure)
            {
                return Result.Failure(remainingResult.Errors);
            }

            if (remainingResult.Value!.Amount < 0)
            {
                return Error.InvalidInput("Insufficient funds");
            }

            return Result.Success();
        }

        // Event handlers - no validation, just state reconstruction
        private void OnBankAccountOpened(BankAccountOpenedEvent @event)
        {
            AccountHolderId = @event.AccountHolderId;
            Balance = @event.InitialDeposit;
            Status = AccountStatus.Opened;
        }

        private void OnBankAccountClosed(BankAccountClosedEvent @event)
        {
            Status = AccountStatus.Closed;
        }

        private void OnMoneyDeposited(MoneyDepositedEvent @event)
        {         
            if (Balance.Amount == 0)
            {
                // First deposit or zero balance sets the currency
                Balance = @event.Amount;
            }
            else
            {
                // Event handlers should not fail - this is state reconstruction
                // We assume the events were already validated when originally applied
                var addResult = Balance.Add(@event.Amount);
                if (addResult.IsSuccess)
                {
                    Balance = addResult.Value!;
                }
                // If this fails during event replay, it indicates data corruption
                // Consider logging this scenario or handling it based on your business rules
            }
        }

        private void OnMoneyWithdrawn(MoneyWithdrawnEvent @event)
        {         
            // Event handlers should not fail - this is state reconstruction
            // We assume the events were already validated when originally applied
            var subtractResult = Balance.Subtract(@event.Amount);
            if (subtractResult.IsSuccess)
            {
                Balance = subtractResult.Value!;
            }
            // If this fails during event replay, it indicates data corruption
            // Consider logging this scenario or handling it based on your business rules
        }
    }
}