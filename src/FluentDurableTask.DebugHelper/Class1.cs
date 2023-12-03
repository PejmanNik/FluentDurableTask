using SomePackage.Core;

namespace FluentDurableTask.DebugHelper;

public class OrchestrationProfile2 : IOrchestrationDefinition
{
    public void Configure(ITaskOrchestrationBuilder builder)
    {
        builder.Orchestration<string, int>("GreetingsOrchestration3")
            .Activity<DataParam, string>("DoX3")
            .Activity<int, string>("DoY3");

        builder.Orchestration<string, int>("HelloOrchestration2")
        .Activity<int, string>("DoX4")
        .Activity<int, string>("DoY4");
    }
}

public class XX : IGreetingsOrchestration3.IOrchestration
{
    public Task<string> Execute(TaskOrchestrationContext<IGreetingsOrchestration3> context, int input)
    {
        context.ScheduleActivity(x => x.DoX3);

        return Task.FromResult("Hello");
    }
}