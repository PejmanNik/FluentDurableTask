using SomePackage.Core;

namespace FluentDurableTask.DebugHelper;

public class OrchestrationProfile2 : IOrchestrationDefinition
{
    public void Configure(ITaskOrchestrationBuilder builder)
    {
        builder.Orchestration<string, int>("GreetingsOrchestration2")
            .Activity<DataParam, string>("DoX3")
            .Activity<int, string>("DoY3");

        builder.Orchestration<string, int>("HelloOrchestration2")
        .Activity<int, string>("DoX4")
        .Activity<int, string>("DoY4");
    }
}