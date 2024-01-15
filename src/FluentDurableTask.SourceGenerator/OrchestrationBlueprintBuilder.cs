using FluentDurableTask.Core;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace FluentDurableTask.SourceGenerator;

public static class OrchestrationBlueprintBuilder
{
    public static NamespaceDeclarationSyntax Build(
        IEnumerable<OrchestrationInfo> orchestrationDefinitions)
    {
        var orchestrationsInterfaces = BuildOrchestrationsInterface(orchestrationDefinitions);

        return SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName(nameof(FluentDurableTask)))
            .AddUsings(SyntaxFactory.UsingDirective(
                SyntaxFactory.ParseName($"{nameof(FluentDurableTask)}.{nameof(FluentDurableTask.Core)}"))
            )
            .AddMembers(orchestrationsInterfaces.ToArray());
    }

    private static IEnumerable<InterfaceDeclarationSyntax> BuildOrchestrationsInterface(
        IEnumerable<OrchestrationInfo> orchestrationDefinitions)
    {
        foreach (var orchestrationDefinition in orchestrationDefinitions)
        {
            var blueprint = BuildOrchestrationBlueprintInterface(
                orchestrationDefinition.TasksInfo);

            if (blueprint is not null)
            {
                yield return blueprint;
            }
        }
    }

    private static InterfaceDeclarationSyntax? BuildOrchestrationBlueprintInterface(
        IEnumerable<TaskInfo> orchestrationsInfo)
    {
        var orchestration = orchestrationsInfo
            .FirstOrDefault(x => x.MethodName == nameof(ITaskOrchestrationBuilder.Orchestration));

        if (orchestration is null)
        {
            return null;
        }

        var activities = orchestrationsInfo
           .Where(x => x.MethodName == nameof(ITaskOrchestrationBuilder.ITaskActivityBuilder.Activity));

        var activityInterfaces = BuildActivityInterfaces(activities);
        return BuildOrchestrationInterface(orchestration)
            .AddMembers(activityInterfaces.ToArray());
    }

    private static InterfaceDeclarationSyntax BuildOrchestrationInterface(TaskInfo orchestration)
    {
        var type = $"{nameof(IDurableOrchestration<int, int>)}" +
            $"<{orchestration.ReturnType}, {orchestration.InputType}>";

        return SyntaxFactory
            .InterfaceDeclaration($"I{orchestration.Name}")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithBaseList(SyntaxFactory.BaseList(
                SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                SyntaxFactory.SimpleBaseType(
                SyntaxFactory.ParseTypeName(type))))
            );
    }

    private static IEnumerable<InterfaceDeclarationSyntax> BuildActivityInterfaces(
        IEnumerable<TaskInfo> activities)
    {
        foreach (var activity in activities)
        {
            var type = $"{nameof(IDurableActivity<int, int>)}<{activity.ReturnType}, {activity.InputType}>";

            yield return SyntaxFactory
                 .InterfaceDeclaration($"I{activity.Name}")
                 .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                 .WithBaseList(SyntaxFactory.BaseList(
                     SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                     SyntaxFactory.SimpleBaseType(
                     SyntaxFactory.ParseTypeName(type)
                     )))
                 );
        }
    }
}
