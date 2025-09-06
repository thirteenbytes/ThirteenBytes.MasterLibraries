using FinanceExample.Application.Abstractions.Messaging;
using FinanceExample.Infrastructure.Behaviors;
using FinanceExample.Infrastructure.Clock;
using FinanceExample.Infrastructure.Core;
using FinanceExample.Infrastructure.InMemory;
using Microsoft.Extensions.DependencyInjection;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Clock;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data;

namespace FinanceExample.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Mediator services
            services.AddScoped<IMediator, Mediator>();

            // Register pipeline behaviors (order matters - they execute in registration order)
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

            // Register handlers from Application assembly
            var applicationAssembly = typeof(IMediator).Assembly;
            services.AddMediator(applicationAssembly);

            // Data persistence services
            services.AddScoped<InMemoryDatabase>();
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<InMemoryDatabase>());
            services.AddScoped(typeof(IRepository<,,>), typeof(InMemoryRepository<,,>));

            // Event Store services
            services.AddScoped<IEventStore, InMemoryEventStore>();
            
            // Clock services
            services.AddSingleton<IDateTimeProvider, SystemClockDateTimeProvider>();

            return services;
        }
    }
}