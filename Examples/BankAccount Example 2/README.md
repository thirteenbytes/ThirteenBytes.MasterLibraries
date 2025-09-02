# Bank Account Example 2 - Clean Architecture with DDD

This example demonstrates Domain-Driven Design (DDD) patterns implemented using Clean Architecture principles with an in-memory database and event store.

## Architecture Overview

This solution follows Clean Architecture with these layers:

### Domain Layer (`FinanceExample.Domain`)
- **Entities**: `BankAccount`, `AccountHolder`
- **Value Objects**: `Money`, `FullName`, `EmailAddress`
- **Domain Events**: `BankAccountOpenedEvent`, `MoneyDepositedEvent`, etc.
- **Enums**: `AccountStatus`, `HolderType`

### Application Layer (`FinanceExample.Application`)
- **Use Cases**: CQRS handlers for commands and queries
- **Contracts**: Request/Response DTOs
- **Abstractions**: Interfaces for infrastructure concerns

### Infrastructure Layer (`FinanceExample.Infrastructure`)
- **In-Memory Database**: Simple concurrent dictionary-based storage
- **In-Memory Event Store**: Event sourcing implementation
- **Repository Pattern**: Generic repository with Unit of Work
- **Pipeline Behaviors**: Logging, exception handling

### Presentation Layer (`FinanceExample.WebApi`)
- **Minimal APIs**: Endpoint mappings for RESTful operations
- **Swagger/OpenAPI**: Auto-generated API documentation

## Features Demonstrated

### Domain-Driven Design Patterns
- ✅ **Aggregate Roots**: `BankAccount` with business logic encapsulation
- ✅ **Value Objects**: Immutable objects like `Money` with validation
- ✅ **Domain Events**: Event sourcing for audit trails
- ✅ **Repository Pattern**: Data access abstraction
- ✅ **Specification Pattern**: Query building (implicit in repository)

### Clean Architecture Principles
- ✅ **Dependency Inversion**: Application layer defines interfaces
- ✅ **Single Responsibility**: Each layer has distinct responsibilities
- ✅ **Open/Closed Principle**: Extensible through interfaces

### CQRS (Command Query Responsibility Segregation)
- ✅ **Commands**: `OpenBankAccount`, `DepositToBankAccount`, etc.
- ✅ **Queries**: `GetBankAccountById`, `GetBankAccountEvents`
- ✅ **Separation**: Different models for reads and writes

### Event Sourcing
- ✅ **Event Store**: Persistent event history
- ✅ **Event Replay**: Aggregate reconstruction from events
- ✅ **Event Querying**: Paginated event retrieval

## API Endpoints

### Account Holders
- `POST /account-holders` - Create account holder
- `GET /account-holders/{id}` - Get account holder details
- `PUT /account-holders/{id}` - Update account holder

### Bank Accounts
- `POST /bank-accounts` - Open new bank account
- `GET /bank-accounts/{id}` - Get bank account details
- `GET /bank-accounts/{id}/events` - Get account event history
- `POST /bank-accounts/{id}/deposit` - Deposit money
- `POST /bank-accounts/{id}/withdraw` - Withdraw money
- `POST /bank-accounts/{id}/close` - Close account

## Running the Example

1. **Clone the repository**