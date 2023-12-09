using DurableTask.Core;
using FluentDurableTask.Core;
using System;
using System.Linq.Expressions;

namespace FluentDurableTask.Core;

public class BaseDurableTaskClient<TOrchestrations>
{
    private readonly TaskHubClient _taskHubClient;

    public BaseDurableTaskClient(TaskHubClient taskHubClient)
    {
        this._taskHubClient = taskHubClient;
    }

    public OrchestrationScheduler<TReturn, TInput> ScheduleOrchestration<TReturn, TInput, TBlueprint>(
        Expression<Func<TOrchestrations, ITaskOrchestration<TReturn, TInput, TBlueprint>>> selector)
        where TBlueprint : IOrchestrationBlueprint
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

    public OrchestrationScheduler<TReturn, TInput> ScheduleSubOrchestration<TReturn, TInput, TBlueprint, TReturn2, TInput2>(
    Expression<Func<TOrchestrations, ITaskOrchestration<TReturn, TInput, TBlueprint>>> selector,
    Expression<Func<TBlueprint, ITaskActivity<TReturn2, TInput2>>> s2)
    where TBlueprint : IOrchestrationBlueprint
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

