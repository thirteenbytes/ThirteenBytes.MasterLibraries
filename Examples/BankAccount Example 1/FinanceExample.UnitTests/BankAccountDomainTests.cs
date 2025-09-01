using FinanceExample.Domain.Accounts;
using FinanceExample.Domain.Accounts.Events;

namespace FinanceExample.UnitTests
{
    public class BankAccountDomainTests
    {
        [Fact]
        public void Open_WithValidInputs_ReturnsSuccessAndCreatesAccount()
        {
            // Arrange
            var accountHolderId = AccountHolderId.New();
            decimal initialAmount = 100.00m;
            string currency = "USD";

            // Act
            var accountResult = BankAccount.Open(accountHolderId, initialAmount, currency);

            // Assert
            Assert.True(accountResult.IsSuccess);

            var bankAccount = accountResult.Value!;
            Assert.NotNull(bankAccount);
            Assert.NotNull(bankAccount.Id);
            Assert.Equal(accountHolderId, bankAccount.AccountHolderId);
            Assert.Equal(initialAmount, bankAccount.Balance.Amount);
            Assert.Equal(currency, bankAccount.Balance.Currency);
            Assert.Equal(AccountStatus.Opened, bankAccount.Status);

            // Check that domain events were recorded
            var events = bankAccount.GetUncommittedEvents().ToList();
            Assert.Single(events);
            Assert.IsType<BankAccountOpenedEvent>(events[0]);

            var openedEvent = (BankAccountOpenedEvent)events[0];
            Assert.Equal(bankAccount.Id, openedEvent.BankAccountId);
            Assert.Equal(accountHolderId, openedEvent.AccountHolderId);
            Assert.Equal(initialAmount, openedEvent.InitialDeposit.Amount);
            Assert.Equal(currency, openedEvent.InitialDeposit.Currency);
        }

        [Fact]
        public void Open_WithNegativeInitialDeposit_ReturnsValidationError()
        {
            // Arrange
            var accountHolderId = AccountHolderId.New();
            decimal negativeAmount = -50.00m;
            string currency = "USD";

            // Act
            var accountResult = BankAccount.Open(accountHolderId, negativeAmount, currency);

            // Assert
            Assert.False(accountResult.IsSuccess);
            Assert.Contains(accountResult.Errors, e => e.Description.Contains("negative"));
        }

        [Fact]
        public void Open_WithInvalidCurrency_ReturnsValidationError()
        {
            // Arrange
            var accountHolderId = AccountHolderId.New();
            decimal amount = 100.00m;
            string invalidCurrency = "US"; // Too short, should be 3 chars

            // Act
            var accountResult = BankAccount.Open(accountHolderId, amount, invalidCurrency);

            // Assert
            Assert.False(accountResult.IsSuccess);
            Assert.Contains(accountResult.Errors, e => e.Description.Contains("Currency"));
        }

        [Fact]
        public void Open_WithNullAccountHolderId_ReturnsValidationError()
        {
            // Arrange
            AccountHolderId? nullAccountHolderId = null;
            decimal amount = 100.00m;
            string currency = "USD";

            // Act
            var accountResult = BankAccount.Open(nullAccountHolderId!, amount, currency);

            // Assert
            Assert.False(accountResult.IsSuccess);
            Assert.Contains(accountResult.Errors, e => e.Description.Contains("Account holder ID"));
        }

        #region Deposit Tests

        [Fact]
        public void Deposit_WithValidAmount_ReturnsSuccessAndUpdatesBalance()
        {
            // Arrange
            var accountHolderId = AccountHolderId.New();
            var account = BankAccount.Open(accountHolderId, 100.00m, "USD").Value!;
            account.ClearDomainEvents(); // Clear initial events

            decimal depositAmount = 50.00m;

            // Act
            var result = account.Deposit(depositAmount, "USD");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(150.00m, account.Balance.Amount);
            Assert.Equal("USD", account.Balance.Currency);

            // Check domain events
            var events = account.GetUncommittedEvents().ToList();
            Assert.Single(events);
            Assert.IsType<MoneyDepositedEvent>(events[0]);

            var depositEvent = (MoneyDepositedEvent)events[0];
            Assert.Equal(account.Id, depositEvent.BankAccountId);
            Assert.Equal(depositAmount, depositEvent.Amount.Amount);
            Assert.Equal("USD", depositEvent.Amount.Currency);
        }

        [Fact]
        public void Deposit_ToClosedAccount_ReturnsValidationError()
        {
            // Arrange
            var accountHolderId = AccountHolderId.New();
            var account = BankAccount.Open(accountHolderId, 0.00m, "USD").Value!; // Start with zero balance
            account.Close(); // Can close with zero balance

            // Act
            var result = account.Deposit(50.00m, "USD");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Description.Contains("closed account"));
        }

        [Fact]
        public void Deposit_WithDifferentCurrency_ReturnsValidationError()
        {
            // Arrange
            var accountHolderId = AccountHolderId.New();
            var account = BankAccount.Open(accountHolderId, 100.00m, "USD").Value!;

            // Act
            var result = account.Deposit(50.00m, "EUR");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Description.Contains("Currency mismatch"));
        }

        [Fact]
        public void Deposit_WithNegativeAmount_ReturnsValidationError()
        {
            // Arrange
            var accountHolderId = AccountHolderId.New();
            var account = BankAccount.Open(accountHolderId, 100.00m, "USD").Value!;

            // Act
            var result = account.Deposit(-50.00m, "USD");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Description.Contains("negative"));
        }

        #endregion

        #region Withdraw Tests

        [Fact]
        public void Withdraw_WithValidAmount_ReturnsSuccessAndUpdatesBalance()
        {
            // Arrange
            var accountHolderId = AccountHolderId.New();
            var account = BankAccount.Open(accountHolderId, 100.00m, "USD").Value!;
            account.ClearDomainEvents(); // Clear initial events

            decimal withdrawAmount = 30.00m;

            // Act
            var result = account.Withdraw(withdrawAmount, "USD");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(70.00m, account.Balance.Amount);
            Assert.Equal("USD", account.Balance.Currency);

            // Check domain events
            var events = account.GetUncommittedEvents().ToList();
            Assert.Single(events);
            Assert.IsType<MoneyWithdrawnEvent>(events[0]);

            var withdrawEvent = (MoneyWithdrawnEvent)events[0];
            Assert.Equal(account.Id, withdrawEvent.BankAccountId);
            Assert.Equal(withdrawAmount, withdrawEvent.Amount.Amount);
            Assert.Equal("USD", withdrawEvent.Amount.Currency);
        }

        [Fact]
        public void Withdraw_FromClosedAccount_ReturnsValidationError()
        {
            // Arrange
            var accountHolderId = AccountHolderId.New();
            var account = BankAccount.Open(accountHolderId, 0.00m, "USD").Value!; // Start with zero balance
            account.Close(); // Can close with zero balance

            // Act
            var result = account.Withdraw(50.00m, "USD");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Description.Contains("closed account"));
        }

        [Fact]
        public void Withdraw_WithInsufficientFunds_ReturnsValidationError()
        {
            // Arrange
            var accountHolderId = AccountHolderId.New();
            var account = BankAccount.Open(accountHolderId, 100.00m, "USD").Value!;

            // Act
            var result = account.Withdraw(150.00m, "USD");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Description.Contains("Insufficient funds"));
        }

        [Fact]
        public void Withdraw_WithDifferentCurrency_ReturnsValidationError()
        {
            // Arrange
            var accountHolderId = AccountHolderId.New();
            var account = BankAccount.Open(accountHolderId, 100.00m, "USD").Value!;

            // Act
            var result = account.Withdraw(50.00m, "EUR");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Description.Contains("Currency mismatch"));
        }

        [Fact]
        public void Withdraw_WithNegativeAmount_ReturnsValidationError()
        {
            // Arrange
            var accountHolderId = AccountHolderId.New();
            var account = BankAccount.Open(accountHolderId, 100.00m, "USD").Value!;

            // Act
            var result = account.Withdraw(-50.00m, "USD");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Description.Contains("negative"));
        }

        #endregion

        #region Close Tests

        [Fact]
        public void Close_WithZeroBalance_ReturnsSuccessAndUpdatesStatus()
        {
            // Arrange
            var accountHolderId = AccountHolderId.New();
            var account = BankAccount.Open(accountHolderId, 0.00m, "USD").Value!; // Start with zero balance
            account.ClearDomainEvents(); // Clear initial events

            // Act
            var result = account.Close();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AccountStatus.Closed, account.Status);

            // Check domain events
            var events = account.GetUncommittedEvents().ToList();
            Assert.Single(events);
            Assert.IsType<BankAccountClosedEvent>(events[0]);

            var closedEvent = (BankAccountClosedEvent)events[0];
            Assert.Equal(account.Id, closedEvent.BankAccountId);
        }

        [Fact]
        public void Close_WithNonZeroBalance_ReturnsValidationError()
        {
            // Arrange
            var accountHolderId = AccountHolderId.New();
            var account = BankAccount.Open(accountHolderId, 100.00m, "USD").Value!; // Start with non-zero balance

            // Act
            var result = account.Close();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Description.Contains("balance must be zero"));
        }

        [Fact]
        public void Close_AfterWithdrawingAllFunds_ReturnsSuccess()
        {
            // Arrange
            var accountHolderId = AccountHolderId.New();
            var account = BankAccount.Open(accountHolderId, 100.00m, "USD").Value!;

            // Withdraw all funds to make balance zero
            account.Withdraw(100.00m, "USD");
            account.ClearDomainEvents(); // Clear events to focus on close event

            // Act
            var result = account.Close();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AccountStatus.Closed, account.Status);
            Assert.Equal(0.00m, account.Balance.Amount);

            // Check domain events
            var events = account.GetUncommittedEvents().ToList();
            Assert.Single(events);
            Assert.IsType<BankAccountClosedEvent>(events[0]);
        }

        [Fact]
        public void Close_WithAlreadyClosedAccount_ReturnsValidationError()
        {
            // Arrange
            var accountHolderId = AccountHolderId.New();
            var account = BankAccount.Open(accountHolderId, 0.00m, "USD").Value!; // Start with zero balance
            account.Close(); // Close the account first

            // Act
            var result = account.Close(); // Try to close again

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Description.Contains("already closed"));
        }

        #endregion
    }
}