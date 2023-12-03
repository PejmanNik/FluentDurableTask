using FluentDurableTask.Core;
using System.Linq.Expressions;
using System;
using System.Threading.Tasks;

namespace FluentDurableTask;

public interface ITaskOrchestrationBuilder
{
    ITaskActivityBuilder Orchestration<TResult, TInput>(string name, ushort version = 1);

    interface ITaskActivityBuilder
    {
        ITaskActivityBuilder Activity<TResult, TInput>(string name);
    }
}

public interface IOrchestrationDefinition
{
    void Configure(ITaskOrchestrationBuilder builder);
}

public class TaskActivityContext
{

}

public interface ITaskActivity<TResult, TInput>
{
    Task<TResult> Execute(TaskActivityContext context, TInput input);
}

public class TaskOrchestrationContext<TOrchestration> where TOrchestration : IOrchestrationBlueprint
{
    public void ScheduleActivity<TReturn, TInput>(
    Expression<Func<TOrchestration, ITaskActivity<TReturn, TInput>>> selector)
    {
        if (selector.Body is not MemberExpression member)
        {
            throw new ArgumentException(string.Format(
                "Expression '{0}' is not valid.", selector.ToString()));
        }

      Console.WriteLine(member.Member.Name);
    }
}

public interface ITaskOrchestration<TResult, TInput, TOrchestration>
    where TOrchestration : IOrchestrationBlueprint
{
    Task<TResult> Execute(TaskOrchestrationContext<TOrchestration> context, TInput input);
}
