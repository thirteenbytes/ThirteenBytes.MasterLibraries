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
public class Order : AggregateRoot<OrderId>
{
    public OrderId Id { get; private set; }
    public UserId CustomerId { get; private set; }
    public List<OrderItem> Items { get; private set; } = new List<OrderItem>();

    // Private constructor for ORM
    private Order() { }

    private Order(OrderId id, UserId customerId)
    {
        Id = id;
        CustomerId = customerId;
    }

    public static Result<Order> Create(OrderId id, UserId customerId)
    {
        return WithValidation(
            () => new Order(id, customerId));
    }

    public void AddItem(ProductId productId, int quantity)
    {
        // Business logic to add an item to the order
        var orderItem = new OrderItem(productId, quantity);
        Items.Add(orderItem);

        // Raise domain event
        AddDomainEvent(new OrderItemAddedEvent(this, orderItem));
    }
}

public class OrderItem
{
    public ProductId ProductId { get; private set; }
    public int Quantity { get; private set; }

    public OrderItem(ProductId productId, int quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }
}

public class OrderItemAddedEvent : IDomainEvent
{
    public Order Order { get; }
    public OrderItem OrderItem { get; }

    public OrderItemAddedEvent(Order order, OrderItem orderItem)
    {
        Order = order;
        OrderItem = orderItem;
    }
}
```

