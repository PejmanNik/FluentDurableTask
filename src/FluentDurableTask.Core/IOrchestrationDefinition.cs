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
