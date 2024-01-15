using DurableTask.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FluentDurableTask;

public sealed class DurableTaskHostedService : IHostedService
{
    private readonly TaskHubWorker _taskHubWorker;

    public DurableTaskHostedService(
        IServiceProvider serviceProvider,
        IEnumerable<TaskType> taskTypes)
    {
        _taskHubWorker = serviceProvider.GetRequiredService<TaskHubWorker>();
        _taskHubWorker.AddTaskOrchestrations(
            taskTypes.Where(x => x.IsOrchestration)
                .Select(x => new ScopedOrchestrationCreator(serviceProvider, x.Implementation.Name, x.Service))
                .ToArray()
            );

        _taskHubWorker.AddTaskActivities(
            taskTypes.Where(x => x.IsActivity)
                .Select(x => new ScopedActivityCreator(serviceProvider, x.Implementation.Name, x.Service))
                .ToArray()
            );
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _taskHubWorker.StartAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _taskHubWorker.StopAsync();
    }
}