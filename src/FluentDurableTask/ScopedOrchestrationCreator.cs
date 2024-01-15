using DurableTask.Core;
using Microsoft.Extensions.DependencyInjection;

namespace FluentDurableTask;

public sealed class ScopedActivityCreator : ObjectCreator<TaskActivity>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Type _serviceType;

    public ScopedActivityCreator(IServiceProvider serviceProvider, string name, Type serviceType)
    {
        Name = name;
        Version = string.Empty;
        _serviceProvider = serviceProvider;
        _serviceType = serviceType;
    }

    public override TaskActivity Create()
    {
        using var x = _serviceProvider.CreateScope();

        var type = typeof(GenericTaskActivity<,,>)
                   .MakeGenericType([.. _serviceType.GetGenericArguments(), _serviceType]);

        return (TaskActivity)x.ServiceProvider.GetRequiredService(type);
    }
}
public sealed class ScopedOrchestrationCreator : ObjectCreator<TaskOrchestration>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Type _serviceType;

    public ScopedOrchestrationCreator(IServiceProvider serviceProvider, string name, Type serviceType)
    {
        Name = name;
        Version = string.Empty;
        _serviceProvider = serviceProvider;
        _serviceType = serviceType;
    }

    public override TaskOrchestration Create()
    {
        using var x = _serviceProvider.CreateScope();

        var type = typeof(GenericTaskOrchestration<,,>)
            .MakeGenericType([.. _serviceType.GetGenericArguments(), _serviceType]);

        return (TaskOrchestration)x.ServiceProvider.GetRequiredService(type);
    }
}