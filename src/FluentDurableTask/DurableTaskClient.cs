using DurableTask.Core;
using FluentDurableTask.Core;
using System.Linq.Expressions;

namespace FluentDurableTask;

public class DurableTaskClient
{
    private readonly TaskHubClient _taskHubClient;

    public DurableTaskClient(TaskHubClient taskHubClient)
    {
        this._taskHubClient = taskHubClient;
    }


    public OrchestrationScheduler<TReturn, TInput> ScheduleOrchestration<TReturn, TInput>(
        Expression<Func<IOrchestrations, ITaskOrchestration<TReturn, TInput, IOrchestrationBlueprint>>> selector)
    {
        if (selector.Body is not MemberExpression member)
        {
            throw new ArgumentException(string.Format(
                "Expression '{0}' is not valid.", selector.ToString()));
        }

        return new OrchestrationScheduler<TReturn, TInput>(
            _taskHubClient,
            member.Member.Name);
    }
}

