using DurableTask.Core;
using FluentDurableTask.Core;

namespace FluentDurableTask;

public sealed class GenericTaskActivity<TInput, TResult, TDurableActivity> : AsyncTaskActivity<TInput, TResult>
    where TDurableActivity : IDurableActivity<TResult, TInput>
{
    private readonly TDurableActivity _durableActivity;

    public GenericTaskActivity(TDurableActivity durableActivity)
    {
        _durableActivity = durableActivity;
    }

    protected override Task<TResult> ExecuteAsync(TaskContext context, TInput input)
    {
        var contextV2 = new TaskActivityContext();
        return _durableActivity.Execute(contextV2, input);
    }
}

public sealed class GenericTaskOrchestration<TResult, TInput, TDurableOrchestration> : TaskOrchestration<TResult, TInput>
    where TDurableOrchestration : IDurableOrchestration<TResult, TInput>
{
    private readonly TDurableOrchestration _orchestrationService;

    public GenericTaskOrchestration(TDurableOrchestration orchestrationService)
    {
        _orchestrationService = orchestrationService;
    }

    public override Task<TResult> RunTask(OrchestrationContext context, TInput input)
    {
        var contextV2 = new IDurableOrchestration<TResult, TInput>.Context();
        return _orchestrationService.Execute(contextV2, input);
    }
}
