using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FluentDurableTask.Core;

public class TaskOrchestrationContext<TDurableOrchestrations, TOrchestrationBlueprint>
    where TOrchestrationBlueprint : IOrchestrationBlueprint
    where TDurableOrchestrations : IDurableOrchestrations
{
    public void ScheduleActivity<TReturn, TInput>(
    Expression<Func<TOrchestrationBlueprint, IDurableActivity<TReturn, TInput>>> selector)
    {
        if (selector.Body is not MemberExpression member)
        {
            throw new ArgumentException(string.Format(
                "Expression '{0}' is not valid.", selector.ToString()));
        }

        Console.WriteLine(member.Member.Name);
    }

    //public OrchestrationScheduler<TReturn, TInput> ScheduleExternalActivity<TReturn, TInput, TBlueprint, TReturn2, TInput2>(
    //    Expression<Func<TDurableOrchestrations, ITaskOrchestration<TReturn, TInput, TBlueprint, TDurableOrchestrations>>> selector,
    //    Expression<Func<TBlueprint, ITaskActivity<TReturn2, TInput2>>> s2)
    //    where TBlueprint : IOrchestrationBlueprint
    //{
    //    if (selector.Body is not MemberExpression member)
    //    {
    //        throw new ArgumentException(string.Format(
    //            "Expression '{0}' is not valid.", selector.ToString()));
    //    }

    //    return new OrchestrationScheduler<TReturn, TInput>(
    //        null,
    //        member.Member.Name);
    //}
}

public interface IDurableOrchestration<TResult, TInput>
{
    public class Context
    {

    }

    Task<TResult> Execute(Context context, TInput input);
}
