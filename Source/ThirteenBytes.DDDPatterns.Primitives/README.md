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

````````markdown

### 2. Create an Entity
