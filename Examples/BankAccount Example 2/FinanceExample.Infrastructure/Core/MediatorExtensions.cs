using FinanceExample.Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace FinanceExample.Infrastructure.Core
{
    internal static class MediatorExtensions
    {
        /// <summary>
        /// Registers IMediator, IRequestHandler<,>s, and IPipelineBehavior<,>s found in the provided assemblies.
        /// </summary>
        public static IServiceCollection AddMediator(
            this IServiceCollection services,
            params Assembly[] assemblies)
        {
            // Avoid duplicate IMediator registrations if called multiple times.
            services.TryAddScoped<IMediator, Mediator>();

            if (assemblies is null || assemblies.Length == 0)
                return services;

            foreach (var assembly in assemblies.Distinct())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
                        continue;

                    foreach (var @interface in type.GetInterfaces())
                    {
                        if (!@interface.IsGenericType) continue;

                        var genericTypeDefinition = @interface.GetGenericTypeDefinition();

                        // Handlers
                        if (genericTypeDefinition == typeof(IRequestHandler<,>))
                        {
                            services.AddScoped(@interface, type);
                        }

                        // Pipeline behaviors
                        if (genericTypeDefinition == typeof(IPipelineBehavior<,>))
                        {
                            // Behaviors are resolved as IEnumerable<>, registration order matters for your pipeline.
                            services.AddScoped(@interface, type);
                        }
                    }
                }
            }

            return services;
        }

        /// <summary>
        /// Convenience overload: pass any marker types; their assemblies will be scanned.
        /// </summary>
        public static IServiceCollection AddMediatorFromMarkers(
            this IServiceCollection services,
            params Type[] markerTypes)
        {
            var assemblies = markerTypes
                .Where(t => t?.Assembly != null)
                .Select(t => t!.Assembly)
                .Distinct()
                .ToArray();

            return services.AddMediator(assemblies);
        }
    }
}
