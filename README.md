# ThirteenBytes DDD Patterns & Examples

A comprehensive showcase of Domain-Driven Design (DDD) patterns implementation in .NET 8, featuring a reusable primitives library and real-world banking examples demonstrating Clean Architecture, CQRS, and Event Sourcing.

Domain-Driven Design is an approach to software development that:
1. Centers the domain model: Software should reflect the business’s real concepts, rules, and language.
2. Uses Ubiquitous Language: A shared vocabulary across developers and business experts that’s embedded in code.
3. Defines boundaries (Bounded Contexts): Each model has a clear boundary where its definitions are consistent and cohesive.
4. Provides tactical building blocks: Entities (with identity), Value Objects (immutable), Aggregates (consistency boundaries), Repositories, Services, and Factories.
5. Strategic design: Distinguishes between Core Domains (where innovation happens) and Supporting/Generic subdomains (where reuse or simpler solutions may suffice).
6. Pragmatism: DDD is most useful in complex, evolving domains, where deep collaboration between domain experts and developers is necessary to avoid chaos.

### Basic Definitions
- **Bounded Context**: Boundary where a model has a single, consistent meaning.
- **Aggregate Root**:	The entry point to an Aggregate, enforces invariants, consistency rules.
- **Entity**:	Object defined by identity that persists through state changes.
- **Value Object**: Object defined only by attributes, immutable, no identity.

### Detailed Definitions:
#### Bounded Context
- **Eric Evans (DDD)**: A Bounded Context is an explicit boundary within which a particular model applies. The meaning of terms, rules, and entities are consistent only inside that boundary. Outside, the same words may mean different things, so you must define translations or integrations.
- **Vaughn Vernon (IDDD)**: A Bounded Context is the guardrail that keeps a model coherent. It’s both a linguistic and technical boundary, ensuring that terms have unambiguous meaning and implementations don’t drift into corruption. Teams can evolve independently within their bounded contexts.
- **Martin Fowler**: A Bounded Context ensures that the model is not spread too thin. It provides clarity by stating “this is where this model applies, and outside we don’t make assumptions.” It’s crucial for large systems to avoid “semantic diffusion” (where words lose meaning).

#### Aggregate Root (and Aggregates)
- **Eric Evans**: An Aggregate is a cluster of associated objects treated as a unit for data changes. It enforces invariants across its members. One Entity is designated the Aggregate Root, and only the root is accessible from outside—other members are reached through it.
- **Vaughn Vernon**: Aggregates are the transactional consistency boundaries. The Aggregate Root protects the integrity of the aggregate and enforces invariants. Clients only hold references to the root, never to its children directly.
- **Martin Fowler**: Aggregates simplify transactional rules. Rather than every entity enforcing consistency globally, you encapsulate rules inside an Aggregate and enforce them there, making it clear what must be consistent together.

#### Entities
- **Eric Evans**: An Entity is an object distinguished by its identity, not just by its attributes. Two Entities with the same attributes but different identities are not the same. Identity runs through its entire lifecycle.
- **Vaughn Vernon**: Entities model concepts that endure over time. Their identity is explicit (often via a unique ID), and their attributes and associations can change, but they remain the same entity.
- **Martin Fowler**: In enterprise applications, Entities represent things like “Customer” or “Order” where who it is matters more than its current data. Identity is fundamental, not just the data snapshot.

#### Value Objects
- **Eric Evans**: A Value Object is an object that is defined only by its attributes, not by a distinct identity. They are immutable and replaceable. For example, two Money objects with the same amount and currency are indistinguishable.
- **Vaughn Vernon**: Value Objects should be immutable and side-effect free. They are often used to model concepts like measurements, addresses, or date ranges. They can encapsulate logic (e.g., currency conversions) but should never have identity.
- **Martin Fowler**: A Value Object represents descriptive aspects of the domain. They make code more expressive and reduce complexity by avoiding unnecessary identity. They’re safer because immutability eliminates unintended side effects.

## 🏗️ Repository Structure
├── Source/ │   
└── ThirteenBytes.DDDPatterns.Primitives/     
# 📦 NuGet Package - Core DDD primitives 
├── Examples/ 
│   
├── BankAccount Example 1/# 🏦 In-Memory Implementation 
│   
│   
├── FinanceExample.Domain/# Domain layer 
│   
│   
├── FinanceExample.Application/# Application layer (CQRS) 
│   
│   
├── FinanceExample.Infrastructure/# Infrastructure (In-Memory) 
│   
│   
├── FinanceExample.WebApi/# Presentation layer (Minimal APIs) 
│   
│   
└── FinanceExample.UnitTests/# Unit tests 
│   
└── BankAccount Example 2/# 🏦 Advanced Implementation 
│       
├── FinanceExample.Domain/# Domain layer 
│       
├── FinanceExample.Application/# Application layer (CQRS) 
│       
├── FinanceExample.Infrastructure/# Infrastructure (SQLite + RavenDB) 
│       
├── FinanceExample.WebApi/# Presentation layer (Minimal APIs) 
│       
└── FinanceExample.UnitTests/# Unit tests 
└── README.md                                    
# This file

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022 or VS Code
- Git

### Clone & Run
Clone the repository
git clone https://github.com/thirteenbytes/ddd-patterns-primitives.git cd ddd-patterns-primitives
Run Example 1 (In-Memory)
cd "Examples/BankAccount Example 1/FinanceExample.WebApi" dotnet run
Or run Example 2 (SQLite + RavenDB)
cd "Examples/BankAccount Example 2/FinanceExample.WebApi" dotnet run


### API Endpoints
Both examples expose the same REST API:

- **Account Holders**
  - `POST /account-holders` - Create account holder
  - `GET /account-holders/{id}` - Get account holder details
  - `PUT /account-holders/{id}` - Update account holder

- **Bank Accounts**
  - `POST /bank-accounts` - Open new bank account
  - `GET /bank-accounts/{id}` - Get bank account details
  - `GET /bank-accounts/{id}/events` - Get account event history
  - `POST /bank-accounts/{id}/deposit` - Deposit money
  - `POST /bank-accounts/{id}/withdraw` - Withdraw money
  - `POST /bank-accounts/{id}/close` - Close account

## 📦 ThirteenBytes.DDDPatterns.Primitives Library

The core library provides essential DDD building blocks:

### Core Features
- **🔑 Strongly-Typed IDs** - Type-safe entity identifiers
- **🏛️ Entity Base Classes** - Identity-based equality semantics
- **📝 Audit Support** - Automatic timestamp tracking
- **💎 Value Objects** - Immutable objects with structural equality
- **🎯 Aggregate Roots** - Event sourcing with domain event management
- **📬 Domain Events** - Event-driven architecture support
- **🗄️ Repository Pattern** - Data access abstraction
- **📊 Event Store** - Event sourcing persistence interface
- **✅ Result Pattern** - Functional error handling without exceptions
- **📄 Pagination** - Built-in pagination support

### Installation
dotnet add package ThirteenBytes.DDDPatterns.Primitives


See the [library README](Source/ThirteenBytes.DDDPatterns.Primitives/README.md) for detailed usage examples.

## 🏦 Banking Examples

### Example 1: In-Memory Implementation
**Perfect for**: Learning, prototyping, unit testing

**Architecture**:
- **Clean Architecture** with clear layer separation
- **In-Memory Database** using `ConcurrentDictionary`
- **In-Memory Event Store** for event sourcing
- **CQRS** with MediatR handlers
- **Domain Events** with automatic event management

**Key Features**:
- Complete banking operations (open, deposit, withdraw, close)
- Account holder management
- Event sourcing with event replay
- Optimistic concurrency control
- Comprehensive unit tests

### Example 2: Advanced Implementation
**Perfect for**: Production-ready scenarios, advanced patterns

**Architecture**:
- **Clean Architecture** with enhanced infrastructure
- **SQLite Database** with Entity Framework Core
- **RavenDB Event Store** for production event sourcing
- **Currency Validation Service** with external API integration
- **Advanced Error Handling** and logging

**Key Features**:
- All features from Example 1, plus:
- Real database persistence
- Production-grade event store
- Currency validation against external service
- Enhanced domain model with more business rules
- Integration tests

## 🔧 Technology Stack

### Core Technologies
- **.NET 8** - Target framework
- **C# 12** - Programming language
- **MediatR** - CQRS implementation
- **ASP.NET Core** - Web API framework

### Example 1 Stack
- **In-Memory Storage** - `ConcurrentDictionary`
- **Custom Event Store** - In-memory implementation
- **xUnit** - Unit testing framework

### Example 2 Stack
- **Entity Framework Core** - ORM for SQLite
- **RavenDB** - Document database for event store
- **SQLite** - Lightweight database
- **Serilog** - Structured logging

## 📚 Learning Path

1. **Start with the Primitives Library** - Understand the core patterns
2. **Explore Example 1** - Learn the fundamentals with simple in-memory implementation
3. **Study the Tests** - Understand how to test DDD applications
4. **Move to Example 2** - See production-ready implementations
5. **Experiment** - Modify examples and add new features

## 🎓 Educational Value

This repository demonstrates:

### Domain-Driven Design
- **Ubiquitous Language** - Clear domain terminology
- **Bounded Contexts** - Well-defined boundaries
- **Aggregate Design** - Transaction consistency boundaries
- **Domain Events** - Decoupled communication

### Clean Architecture
- **Dependency Inversion** - Framework-independent domain
- **Layer Separation** - Clear architectural boundaries
- **Testability** - Easy unit and integration testing

### CQRS & Event Sourcing
- **Command Query Separation** - Different models for reads/writes
- **Event Streams** - Complete audit trail
- **Event Replay** - State reconstruction from events
- **Optimistic Concurrency** - Version-based conflict resolution

### Modern .NET Patterns
- **Result Pattern** - Functional error handling
- **Generic Abstractions** - Reusable repository patterns
- **Minimal APIs** - Modern ASP.NET Core endpoints
- **Dependency Injection** - Service composition

## 🤝 Contributing

Contributions are welcome! Please:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** your changes (`git commit -m 'Add amazing feature'`)
4. **Push** to the branch (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

### Areas for Contribution
- Additional example implementations
- More comprehensive tests
- Documentation improvements
- Performance optimizations
- New DDD patterns

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Eric Evans** - Domain-Driven Design concepts
- **Vernon Vaughn** - Implementing Domain-Driven Design
- **Martin Fowler** - Enterprise Application Architecture patterns
- **.NET Community** - Inspiration and best practices

## 📞 Support & Community

- **Issues**: Report bugs or request features via [GitHub Issues](https://github.com/thirteenbytes/ddd-patterns-primitives/issues)
- **Discussions**: Join conversations in [GitHub Discussions](https://github.com/thirteenbytes/ddd-patterns-primitives/discussions)
- **Wiki**: Additional documentation in the [project wiki](https://github.com/thirteenbytes/ddd-patterns-primitives/wiki)

---

⭐ **Star this repository** if you find it helpful for learning DDD patterns!
