// See https://aka.ms/new-console-template for more information
using FluentDurableTask;

Console.WriteLine("Hello, World!");

public class OrchestrationProfile1 : IOrchestrationDefinition
{
    public void Configure(ITaskOrchestrationBuilder builder)
    {
        builder.Orchestration<string, int>("GreetingsOrchestration")
            .Activity<int, string>("DoX")
            .Activity<int, string>("DoY");

        builder.Orchestration<string, int>("HelloOrchestration")
        .Activity<int, string>("DoX2")
        .Activity<int, string>("DoY2");
    }
}

