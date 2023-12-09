using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FluentDurableTask.Core;

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
