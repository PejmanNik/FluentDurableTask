using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FluentDurableTask;

public interface ITaskOrchestrationBuilder
{
    ITaskActivityBuilder Orchestration<TResult, TInput>(string name);

    interface ITaskActivityBuilder
    {
        ITaskActivityBuilder Activity<TResult, TInput>(string name);
    }
}

public interface IOrchestrationDefinition
{
    void Configure(ITaskOrchestrationBuilder builder);
}

public interface ITaskActivity<TResult, TInput>
{
    Task<TResult> Execute(TInput input);
}

public interface ITaskOrchestration<TResult, TInput>
{
    Task<TResult> Execute(TInput input);
}
