using DurableTask.Core;
using FluentDurableTask.Core;
using System.Linq.Expressions;

namespace FluentDurableTask;


public class OrchestrationScheduler<TReturn, TInput>
{
    private readonly TaskHubClient _client;
    private readonly string _name;

    internal OrchestrationScheduler(TaskHubClient client, string name)
    {
        _client = client;
        _name = name;
    }

    public OrchestrationSchedulerWithInput<TReturn, TInput> WithInput(TInput input)
    {
        return OrchestrationSchedulerWithInput<TReturn, TInput>.Create(_client, _name, input);
    }
}

public class OrchestrationSchedulerWithInput<TReturn, TInput>
{
    private readonly TaskHubClient _client;
    private readonly string _name;
    private readonly TInput _input;
    private readonly string? _version;
    private readonly DateTime? _startAt;

    private OrchestrationSchedulerWithInput(
        TaskHubClient client,
        string name,
        TInput input,
        string? version,
        DateTime? startAt)
    {
        _client = client;
        _name = name;
        _input = input;
        _version = version;
        _startAt = startAt;
    }

    public static OrchestrationSchedulerWithInput<TReturn, TInput> Create(
        TaskHubClient client,
        string name,
        TInput input,
        string? version = null,
        DateTime? _startAt = null)
    {
        return new OrchestrationSchedulerWithInput<TReturn, TInput>(client, name, input, version, _startAt);
    }

    public OrchestrationSchedulerWithInput<TReturn, TInput> WithVersion(string version)
    {
        return Create(_client, _name, _input, version, _startAt);
    }

    public Task<OrchestrationInstance> ExecuteAsync()
    {
        return _client.CreateOrchestrationInstanceAsync(_name, _version, _input);
    }
}

public class DurableTaskClient
{
    private readonly TaskHubClient _taskHubClient;

    public DurableTaskClient(TaskHubClient taskHubClient)
    {
        this._taskHubClient = taskHubClient;
    }


    public OrchestrationScheduler<TReturn, TInput> ScheduleOrchestration<TReturn, TInput>(
        Expression<Func<IOrchestrations, ITaskOrchestration<TReturn, TInput>>> selector)
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

