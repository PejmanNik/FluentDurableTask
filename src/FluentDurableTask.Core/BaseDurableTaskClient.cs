using DurableTask.Core;
using System;
using System.Linq.Expressions;

namespace FluentDurableTask.Core;


public interface IDurableTaskClient<TOrchestrations>
{

}

public class BaseDurableTaskClient<TOrchestrations> : IDurableTaskClient<TOrchestrations>
    //where TOrchestrations : IDurableOrchestrations
{
    private readonly TaskHubClient _taskHubClient;

    public BaseDurableTaskClient(TaskHubClient taskHubClient)
    {
        this._taskHubClient = taskHubClient;
    }

    public OrchestrationScheduler<TReturn, TInput> ScheduleOrchestration<TReturn, TInput>(
        Expression<Func<TOrchestrations, IDurableOrchestration<TReturn, TInput>>> selector)
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

