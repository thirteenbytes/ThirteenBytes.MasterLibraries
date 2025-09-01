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

                    foreach (var i in type.GetInterfaces())
                    {
                        if (!i.IsGenericType) continue;

                        var gtd = i.GetGenericTypeDefinition();

                        // Handlers
                        if (gtd == typeof(IRequestHandler<,>))
                        {
                            services.AddScoped(i, type);
                        }

                        // Pipeline behaviors
                        if (gtd == typeof(IPipelineBehavior<,>))
                        {
                            // Behaviors are resolved as IEnumerable<>, registration order matters for your pipeline.
                            services.AddScoped(i, type);
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
