using FinanceExample.Application.Abstractions.Messaging;
using FinanceExample.Application.Abstractions.Services;
using FinanceExample.Infrastructure.Behaviors;
using FinanceExample.Infrastructure.Clock;
using FinanceExample.Infrastructure.Core;
using FinanceExample.Infrastructure.RavenData;
using FinanceExample.Infrastructure.Services;
using FinanceExample.Infrastructure.SqliteData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Clock;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data;

namespace FinanceExample.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Mediator services
            services.AddScoped<IMediator, Mediator>();

            // Register pipeline behaviors (order matters - they execute in registration order)
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

            // Register handlers from Application assembly
            var applicationAssembly = typeof(IMediator).Assembly;
            services.AddMediator(applicationAssembly);

            // SQLite Database services for entities
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=Data\\SqliteDb\\finance.db";

            services.AddDbContext<FinanceDbContext>(options =>
                options.UseSqlite(connectionString));

            // Register FinanceDbContext as IUnitOfWork
            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<FinanceDbContext>());
            services.AddScoped(typeof(IRepository<,>), typeof(SqliteRepository<,>));

            // RavenDB Event store services (embedded)
            services.AddSingleton<IDocumentStore>(provider =>
            {
                var logger = provider.GetService<ILogger<IDocumentStore>>();
                return RavenDbConfiguration.CreateEmbeddedStore(configuration, logger);
            });
            services.AddScoped<IEventStore, RavenEventStore>();

            // Domain services
            services.AddScoped<ICurrencyValidationService, CurrencyValidationService>();

            // Clock services
            services.AddSingleton<IDateTimeProvider, SystemClockDateTimeProvider>();

            return services;
        }
    }
}