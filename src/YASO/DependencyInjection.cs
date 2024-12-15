using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using YASO.Abstractions;
using YASO.Domain;

namespace YASO;

public static class DependencyInjection
{
    public static IServiceCollection AddYASO(this IServiceCollection services)
    {
        // Registers a singleton Saga instance. If you need a new Saga per request or scope, adjust the lifetime.
        services.AddSingleton<Saga>();
        return services;
    }

    public static IServiceCollection AddStepsFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var stepTypes = assembly.GetTypes()
            .Where(t => typeof(ISagaStep).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

        foreach (var stepType in stepTypes)
        {
            services.AddTransient(stepType);
        }

        return services;
    }

    public static IServiceCollection AddDataStorage(this IServiceCollection services, ISagaRepository repository)
    {
        services.AddSingleton(repository);
        return services;
    }
}
