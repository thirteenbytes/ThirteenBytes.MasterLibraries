using FCT.DDD.Primitives.Abstractions.Data;
using FinanceExample.Application.Contracts.AccountHolders;
using FinanceExample.Application.Features.AccountHolders;
using FinanceExample.Domain.Accounts;
using Moq;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data;
using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace FinanceExample.UnitTests.ApplicationTests
{
    public class AccountHolderHandlerTests
    {
        private readonly Mock<IRepository<AccountHolder, AccountHolderId, Guid>> _mockRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;

        public AccountHolderHandlerTests()
        {
            _mockRepository = new Mock<IRepository<AccountHolder, AccountHolderId, Guid>>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
        }

        #region CreateAccountHolder Tests

        [Fact]
        public async Task CreateAccountHolder_WithValidInputs_ReturnsSuccessResponse()
        {
            // Arrange
            var handler = new CreateAccountHolder.Handler(_mockRepository.Object, _mockUnitOfWork.Object);
            var command = new CreateAccountHolder.Command("John", "Doe", "john.doe@example.com");

            _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.IsType<CreateAccountHolderResponse>(result.Value);
            Assert.NotEqual(Guid.Empty, result.Value.AccountHolderId);

            // Verify repository interactions
            _mockRepository.Verify(x => x.AddAsync(
                It.IsAny<AccountHolder>(), 
                It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAccountHolder_WithInvalidFirstName_ReturnsValidationError()
        {
            // Arrange
            var handler = new CreateAccountHolder.Handler(_mockRepository.Object, _mockUnitOfWork.Object);
            var command = new CreateAccountHolder.Command("J", "Doe", "john.doe@example.com"); // Too short

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Description.Contains("First name"));

            // Verify no repository interactions occurred
            _mockRepository.Verify(x => x.AddAsync(
                It.IsAny<AccountHolder>(), 
                It.IsAny<CancellationToken>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateAccountHolder_WithInvalidEmail_ReturnsValidationError()
        {
            // Arrange
            var handler = new CreateAccountHolder.Handler(_mockRepository.Object, _mockUnitOfWork.Object);
            var command = new CreateAccountHolder.Command("John", "Doe", "invalid-email");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Description.Contains("email"));

            // Verify no repository interactions occurred
            _mockRepository.Verify(x => x.AddAsync(
                It.IsAny<AccountHolder>(), 
                It.IsAny<CancellationToken>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateAccountHolder_WithRepositoryFailure_PropagatesError()
        {
            // Arrange
            var handler = new CreateAccountHolder.Handler(_mockRepository.Object, _mockUnitOfWork.Object);
            var command = new CreateAccountHolder.Command("John", "Doe", "john.doe@example.com");

            _mockRepository.Setup(x => x.AddAsync(It.IsAny<AccountHolder>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.Handle(command, CancellationToken.None));
        }

        #endregion

        #region GetAccountHolderById Tests

        [Fact]
        public async Task GetAccountHolderById_WithExistingId_ReturnsAccountHolder()
        {
            // Arrange
            var handler = new GetAccountHolderById.Handler(_mockRepository.Object);
            var accountHolderId = Guid.NewGuid();
            var query = new GetAccountHolderById.Query(accountHolderId);

            var existingAccountHolder = CreateTestAccountHolder();
            _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<AccountHolderId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingAccountHolder);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(existingAccountHolder.Id.Value, result.Value.Id);
            Assert.Equal(existingAccountHolder.Name.FirstName, result.Value.FirstName);
            Assert.Equal(existingAccountHolder.Name.LastName, result.Value.LastName);
            Assert.Equal(existingAccountHolder.EmailAddress.Value, result.Value.EmailAddress);

            // Verify repository interaction
            _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<AccountHolderId>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAccountHolderById_WithNonExistentId_ReturnsNotFoundError()
        {
            // Arrange
            var handler = new GetAccountHolderById.Handler(_mockRepository.Object);
            var accountHolderId = Guid.NewGuid();
            var query = new GetAccountHolderById.Query(accountHolderId);

            _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<AccountHolderId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AccountHolder?)null);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Description.Contains("AccountHolder not found"));

            // Verify repository interaction
            _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<AccountHolderId>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region UpdateAccountHolder Tests

        [Fact]
        public async Task UpdateAccountHolder_WithValidInputs_ReturnsUpdatedAccountHolder()
        {
            // Arrange
            var handler = new UpdateAccountHolder.Handler(_mockRepository.Object, _mockUnitOfWork.Object);
            var accountHolderId = Guid.NewGuid();
            var command = new UpdateAccountHolder.Command(
                accountHolderId, 
                "Jane", 
                "Smith", 
                "jane.smith@example.com");

            var existingAccountHolder = CreateTestAccountHolder();
            _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<AccountHolderId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingAccountHolder);

            _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("Jane", result.Value.FirstName);
            Assert.Equal("Smith", result.Value.LastName);
            Assert.Equal("jane.smith@example.com", result.Value.EmailAddress);

            // Verify repository interactions
            _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<AccountHolderId>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.UpdateAsync(
                It.IsAny<AccountHolder>(), 
                It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAccountHolder_WithNonExistentId_ReturnsNotFoundError()
        {
            // Arrange
            var handler = new UpdateAccountHolder.Handler(_mockRepository.Object, _mockUnitOfWork.Object);
            var accountHolderId = Guid.NewGuid();
            var command = new UpdateAccountHolder.Command(
                accountHolderId, 
                "Jane", 
                "Smith", 
                "jane.smith@example.com");

            _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<AccountHolderId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AccountHolder?)null);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Description.Contains("AccountHolder not found"));

            // Verify interactions
            _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<AccountHolderId>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.UpdateAsync(
                It.IsAny<AccountHolder>(), 
                It.IsAny<CancellationToken>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAccountHolder_WithInvalidEmail_ReturnsValidationError()
        {
            // Arrange
            var handler = new UpdateAccountHolder.Handler(_mockRepository.Object, _mockUnitOfWork.Object);
            var accountHolderId = Guid.NewGuid();
            var command = new UpdateAccountHolder.Command(
                accountHolderId, 
                "Jane", 
                "Smith", 
                "invalid-email");

            var existingAccountHolder = CreateTestAccountHolder();
            _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<AccountHolderId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingAccountHolder);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Description.Contains("email"));

            // Verify no update occurred
            _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<AccountHolderId>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.UpdateAsync(
                It.IsAny<AccountHolder>(), 
                It.IsAny<CancellationToken>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region Helper Methods

        private static AccountHolder CreateTestAccountHolder()
        {
            var result = AccountHolder.Create(
                "John", 
                "Doe", 
                "john.doe@example.com", 
                HolderType.Primary);
            
            return result.Value!;
        }

        #endregion
    }
}