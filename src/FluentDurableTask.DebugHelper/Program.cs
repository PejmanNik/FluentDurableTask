// See https://aka.ms/new-console-template for more information
using DurableTask.AzureStorage;
using DurableTask.Core;
using FluentDurableTask;
using FluentDurableTask.Core;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var settings = new AzureStorageOrchestrationServiceSettings
{
    StorageAccountDetails = new StorageAccountDetails { ConnectionString = "UseDevelopmentStorage=true" },
    TaskHubName = "hubdurtest",
};

var orchestrationServiceAndClient = new AzureStorageOrchestrationService(settings);

builder.Services.AddFluentDurableTask(
    orchestrationServiceAndClient,
    typeof(OrchestrationProfile1).Assembly);

var app = builder.Build();

app.MapGet("/", async (TaskHubClient client) =>
{
    var i = await client.CreateOrchestrationInstanceAsync("GreetingsOrchestration", string.Empty, 194);
    return $"Hello World! {i}";
});

app.Run();

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

public class GreetingsOrchestration : IGreetingsOrchestration
{
    public Task<string> Execute(IDurableOrchestration<string, int>.Context context, int input)
    {
        throw new NotImplementedException();
    }
}