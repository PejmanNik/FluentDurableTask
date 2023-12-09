using DurableTask.Core;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace FluentDurableTask.Core;

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

    private static readonly MethodInfo? _createOrchestrationMethod = typeof(TaskHubClient).
        GetMethod("InternalCreateOrchestrationInstanceWithRaisedEventAsync", BindingFlags.NonPublic);

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

    public OrchestrationSchedulerWithInput<TReturn, TInput> WithDelay(DateTime startAt)
    {
        return Create(_client, _name, _input, _version, startAt);
    }

    private Task<OrchestrationInstance> InvokeCreateOrchestrationMethod()
    {
        try
        {
            var result = _createOrchestrationMethod?
                .Invoke(_client, new object?[] {
                    _name,           //string orchestrationName
                    _version ?? "",  //string orchestrationVersion
                    null,            //string orchestrationInstanceId
                    _input,          //object orchestrationInput
                    null,            //IDictionary< string, string> orchestrationTags
                    null,            //OrchestrationStatus[] dedupeStatuses
                    null,            //string eventName
                    null,            //object eventData
                    _startAt         //DateTime? startAt
                });

            if (result is Task<OrchestrationInstance> task)
                return task;
        }
        catch
        {

        }

        //Todo: add logging for ignoring the start time
        return _client.CreateOrchestrationInstanceAsync(_name, _version, _input);
    }

    public Task<OrchestrationInstance> ScheduleAsync()
    {
        return InvokeCreateOrchestrationMethod();
    }
}

