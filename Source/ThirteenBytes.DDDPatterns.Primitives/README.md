# ThirteenBytes.DDDPatterns.Primitives

A comprehensive library of Domain-Driven Design (DDD) primitives and patterns for .NET applications. This package provides essential building blocks for implementing DDD principles, including aggregate roots, entities, value objects, domain events, and repository abstractions.

## Features

- **Strongly-Typed Identifiers**: Type-safe entity IDs supporting various underlying types (Guid, string, Ulid, etc.)
- **Entity Base Classes**: Abstract base classes for entities with identity-based equality
- **Audit Support**: Built-in audit tracking with creation and modification timestamps
- **Value Objects**: Base classes for immutable value objects with structural equality
- **Aggregate Roots**: Event-sourcing capable aggregate roots with domain event management
- **Domain Events**: Interfaces and base classes for domain event implementation
- **Repository Pattern**: Generic repository interface for data access abstraction
- **Event Store Abstraction**: Interface for event sourcing persistence
- **Result Pattern**: Functional error handling without exceptions
- **Pagination Support**: Built-in pagination for event streams and query results

## Quick Start

### 1. Define a Strongly-Typed ID
```
public record UserId : IEntityId<UserId, Guid> 
{ 
    public Guid Value { get; }
    private UserId(Guid value) => Value = value;

    public static UserId New() => new(Guid.NewGuid());
    public static UserId From(Guid value) => new(value);
}
```

### 2. Create a Value Object
```
public class Email : ValueObject 
{ 
    public string Value { get; }
    private Email(string value) => Value = value;

    public static Result<Email> Create(string email)
    {
        return WithValidation(
            () => Validate(email),
            () => new Email(email));
    }

    private static List<Error> Validate(string email)
    {
        var errors = new List<Error>();
    
        if (string.IsNullOrWhiteSpace(email))
            errors.Add(Error.Validation("Email cannot be empty"));
        
        if (!email.Contains("@"))
            errors.Add(Error.Validation("Email must contain @ symbol"));
        
        return errors;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

### 3. Create an Entity

```
public class User : Entity<UserId>
{
    public UserId Id { get; private set; }
    public Email Email { get; private set; } // Using the previously defined Value Object

    // Private constructor for ORM
    private User() { }

    private User(UserId id, Email email)
    {
        Id = id;
        Email = email;
    }

    public static Result<User> Create(UserId id, string email)
    {
        return WithValidation(
            () => Email.Create(email), // Validate and create Email value object
            emailObj => new User(id, emailObj.Value)); // Create User with validated Email
    }
}
```

### 4. Create an Aggregate Root with Domain Events
```
// Domain Events 

public record UserRegistered(UserId UserId, string Name, string Email) : DomainEvent; 
public record EmailChanged(UserId UserId, string NewEmail) : DomainEvent;

// Aggregate Root 
public class UserAccount : AggregateRoot<UserId, Guid> 
{ 
    public string Name { get; private set; } 
    public Email Email { get; private set; }

    // EF Core constructor - register event handlers here
    private UserAccount() 
    {
        On<UserRegistered>(When);
        On<EmailChanged>(When);
    }

    public UserAccount(UserId id, string name, Email email) : this()
    {
        var @event = new UserRegistered(id, name, email.Value);
        Apply(@event);
    }

    public Result ChangeEmail(Email newEmail)
    {
        var @event = new EmailChanged(Id, newEmail.Value);
        return Apply(@event);
    }

    // Event handlers - pure state mutation, no validation
    private void When(UserRegistered @event)
    {
        Name = @event.Name;
        Email = Email.Create(@event.Email).ValueOrThrow(); // Safe since validation happened in command
    }

    private void When(EmailChanged @event)
    {
        Email = Email.Create(@event.NewEmail).ValueOrThrow(); // Safe since validation happened in command
    }
}

```

### 5. Use the Repository Pattern
```
public class UserService 
{ 
    private readonly IRepository<User, UserId, Guid> _repository; 
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IRepository<User, UserId, Guid> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<User>> CreateUserAsync(string name, string email)
    {
        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
            return emailResult.Errors;
    
        var user = new User(UserId.New(), name, emailResult.Value);
        await _repository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
    
        return user;
    }

    public async Task<Result<User>> UpdateUserEmailAsync(Guid userId, string newEmail)
    {
        var id = UserId.From(userId);
        var user = await _repository.GetByIdAsync(id);
    
        if (user == null)
            return Error.NotFound("User not found");
        
        var emailResult = Email.Create(newEmail);
        if (emailResult.IsFailure)
            return emailResult.Errors;
        
        var updateResult = user.UpdateEmail(emailResult.Value);
        if (updateResult.IsFailure)
            return updateResult.Errors;
        
        await _repository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    
        return user;
    }
}
```

### 6. Event Sourcing with Event Store

```
public class UserAccountService 
{ 
    private readonly IEventStore _eventStore;
    public UserAccountService(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<Result<UserAccount>> CreateAccountAsync(string name, string email)
    {
        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
            return emailResult.Errors;
        
        var account = new UserAccount(UserId.New(), name, emailResult.Value);
    
        // Save events for new aggregate (version 0)
        await account.SaveNewAggregateEventsAsync(_eventStore);
    
        return account;
    }

    public async Task<Result<UserAccount>> ChangeEmailAsync(Guid userId, string newEmail)
    {
        var id = UserId.From(userId);
    
        // Load aggregate from event stream
        var events = await _eventStore.GetEventsAsync<UserId, Guid>(id);
        if (!events.Any())
            return Error.NotFound("User account not found");
        
        var account = new UserAccount(); // Create empty aggregate
        var replayResult = account.Replay(events); // Rebuild state from events
        if (replayResult.IsFailure)
            return replayResult.Errors;
        
        // Perform business operation
        var emailResult = Email.Create(newEmail);
        if (emailResult.IsFailure)
            return emailResult.Errors;
        
        var changeResult = account.ChangeEmail(emailResult.Value);
        if (changeResult.IsFailure)
            return changeResult.Errors;
        
        // Save new events
        var currentVersion = await _eventStore.GetAggregateVersionAsync<UserId, Guid>(id);
        await account.SaveEventsAsync(_eventStore, currentVersion);
    
        return account;
    }

    public async Task<PagedResult<IDomainEvent>> GetAccountHistoryAsync(Guid userId, int pageNumber = 1, int pageSize = 10)
    {
        var id = UserId.From(userId);
        return await _eventStore.GetEventsPagedAsync<UserId, Guid>(id, pageNumber, pageSize);
    }
}
``



### 7. Result Pattern for Error Handling

```
public async Task<Result<UserAccount>> ProcessUserRegistration(string name, string email) 
{ 
    // Validate email 
    var emailResult = Email.Create(email); 
    if (emailResult.IsFailure) return emailResult.Errors; 
    
    // Implicit conversion from errors to Result<T>
    // Check if user already exists
    var existingUser = await _repository.GetAsync(u => u.Email.Value == email);
    if (existingUser != null)
        return Error.Exists("User with this email already exists");

    // Create and save user
    var user = new UserAccount(UserId.New(), name, emailResult.Value);
    await user.SaveNewAggregateEventsAsync(_eventStore);

    return user; // Implicit conversion from value to Result<T>
    }
    // Usage with pattern matching var result = await ProcessUserRegistration("John Doe", "john@example.com");
    return result.Match( 
        onSuccess: user => 
            Ok(new { UserId = user.Id.Value, user.Name }), 
        onFailure: errors => 
            BadRequest(errors.Select(e => e.Description)));
}
```

## Best Practices

1. **Always validate in value object factories** - Use the `Create` pattern with validation
2. **Keep aggregates small** - Focus on transaction boundaries
3. **Use Result pattern consistently** - Avoid exceptions for business logic failures
4. **Register event handlers in parameterless constructor** - For event sourcing reconstruction
5. **Implement proper equality** - Override GetHashCode and Equals for value objects
6. **Use strongly-typed IDs** - Prevent ID mixups and improve type safety
7. **Separate commands from queries** - Follow CQRS principles
8. **Make value objects immutable** - Ensure thread safety and predictable behavior

## Real-World Examples

This library includes comprehensive examples demonstrating real-world usage:

- **Example 1**: In-memory implementation with simple CRUD operations and event sourcing
- **Example 2**: Advanced implementation with SQLite persistence and RavenDB event store

Key features demonstrated in the examples:
- Bank account management with money deposits/withdrawals
- Account holder management with validation
- Event sourcing with event replay
- CQRS pattern implementation
- Clean Architecture structure
- Unit testing patterns

Check the `/Examples` folder in the source repository for complete working applications.

## API Reference

### Core Interfaces

- `IEntityId<TSelf, TValue>` - Contract for strongly-typed identifiers
- `IEntity<TId, TValue>` - Base entity contract
- `IAggregateRoot` - Aggregate root with event management
- `IRepository<T, TId, TValue>` - Repository pattern interface
- `IEventStore` - Event sourcing persistence
- `IUnitOfWork` - Transaction management
- `IDomainEventDispatcher` - Event publishing

### Base Classes

- `Entity<TId, TValue>` - Entity with identity-based equality
- `AuditEntity<TId, TValue>` - Entity with audit timestamps
- `AggregateRoot<TId, TValue>` - Event-sourced aggregate root
- `ValueObject` - Immutable value object with structural equality
- `DomainEvent` - Base record for domain events

### Utility Classes

- `Result` / `Result<T>` - Functional error handling
- `Error` - Standardized error representation
- `PagedResult<T>` - Pagination support

## Requirements

- .NET 8.0 or higher
- C# 11+ (for static abstract interface members)

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests to the GitHub repository.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For questions, issues, or feature requests, please visit the GitHub repository or contact the maintainers.



