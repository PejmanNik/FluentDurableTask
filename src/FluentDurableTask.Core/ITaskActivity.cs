using System.Threading.Tasks;

namespace FluentDurableTask.Core;

public class TaskActivityContext
{

}

public interface ITaskActivity<TResult, TInput>
{
    Task<TResult> Execute(TaskActivityContext context, TInput input);
}
