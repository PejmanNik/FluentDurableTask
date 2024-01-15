using DurableTask.Core;
using FluentDurableTask.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FluentDurableTask;

public class TaskType(bool isOrchestration, Type service, Type implementation)
{
    public bool IsOrchestration => isOrchestration;
    public bool IsActivity => !isOrchestration;

    public Type Service => service;
    public Type Implementation => implementation;
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFluentDurableTask<TService>(
        this IServiceCollection services,
        TService orchestrationService,
        Assembly assembly)
        where TService : IOrchestrationServiceClient, IOrchestrationService
    {
        services.AddSingleton<IOrchestrationServiceClient>(orchestrationService);
        services.AddSingleton<IOrchestrationService>(orchestrationService);
        services.AddSingleton<TaskHubWorker>();
        services.AddScoped<TaskHubClient>();

        var tasks = FindAllTasks(assembly);
        foreach (var task in tasks)
        {
            services.AddScoped(task.Service, task.Implementation);
        }

        services.AddTransient(typeof(GenericTaskOrchestration<,,>));
        services.AddTransient(typeof(GenericTaskActivity<,,>));

        services.AddHostedService(sp => new DurableTaskHostedService(sp, tasks));

        return services;
    }

    private static IEnumerable<TaskType> FindAllTasks(Assembly assembly)
    {
        var types = assembly.GetTypes();

        foreach (var type in types)
        {
            if (!type.IsClass || type.IsAbstract)
                continue;

            var interfaces = type.GetInterfaces();
            foreach (var i in interfaces)
            {
                if (!i.IsGenericType)
                    continue;

                var taskDefinition = i.GetGenericTypeDefinition();
                var isOrchestration = taskDefinition == typeof(IDurableOrchestration<,>);
                var isActivity = taskDefinition == typeof(IDurableActivity<,>);
                if (isOrchestration || isActivity)
                {
                    yield return new TaskType(isOrchestration, i, type);
                }
            }
        }
    }
}
