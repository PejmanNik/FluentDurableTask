using FluentDurableTask.Core;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace FluentDurableTask.SourceGenerator;

public static class DurableOrchestrationsBuilder
{
    public static NamespaceDeclarationSyntax Build(
           IEnumerable<OrchestrationInfo> orchestrationDefinitions)
    {

        var members = BuildOrchestrationsMembers(orchestrationDefinitions);
 
        var orchestrationInterface = SyntaxFactory.InterfaceDeclaration("IDurableOrchestrations")
            .WithModifiers(SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.PartialKeyword)
            ))
            .AddMembers(members.ToArray());

        return SyntaxFactory
                 .NamespaceDeclaration(SyntaxFactory.ParseName(nameof(FluentDurableTask)))
                 .AddUsings(SyntaxFactory.UsingDirective(
                     SyntaxFactory.ParseName($"{nameof(FluentDurableTask)}.{nameof(FluentDurableTask.Core)}"))
                 )
                 .AddMembers(orchestrationInterface);
    }

    private static IEnumerable<MemberDeclarationSyntax> BuildOrchestrationsMembers(
        IEnumerable<OrchestrationInfo> orchestrationDefinitions)
    {
        foreach (var orchestrationDefinition in orchestrationDefinitions)
        {
            var orchestration = orchestrationDefinition.TasksInfo
                .FirstOrDefault(x => x.MethodName == nameof(ITaskOrchestrationBuilder.Orchestration));

            if (orchestration is null)
            {
                //TODO: log warning
                continue;
            }

            yield return BuildOrchestrationProperty(orchestration);
            yield return BuildOrchestrationInterface(orchestration, orchestrationDefinition);
        }
    }
    private static PropertyDeclarationSyntax BuildOrchestrationProperty(TaskInfo orchestration)
    {
        return SyntaxFactory.PropertyDeclaration(
                 SyntaxFactory.ParseTypeName($"I{orchestration.Name}"),
                 orchestration.Name
              )
             .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
             .WithAccessorList(
                 SyntaxFactory.AccessorList(
                 SyntaxFactory.SingletonList(
                 SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                 .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))))
             );
    }

    private static InterfaceDeclarationSyntax BuildOrchestrationInterface(TaskInfo orchestration, OrchestrationInfo orchestrationDefinition)
    {
        var activities = orchestrationDefinition.TasksInfo
            .Where(x => x.MethodName == nameof(ITaskOrchestrationBuilder.ITaskActivityBuilder.Activity));

        var activityProperties = BuildActivityProperties(activities);
        var type = $"{nameof(IDurableOrchestration<int, int>)}<{orchestration.ReturnType}, {orchestration.InputType}>";

        return SyntaxFactory
            .InterfaceDeclaration($"I{orchestration.Name}")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithBaseList(SyntaxFactory.BaseList(
                SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                SyntaxFactory.SimpleBaseType(
                SyntaxFactory.ParseTypeName(type))))
            )
            .AddMembers(activityProperties.ToArray());
    }

    private static IEnumerable<PropertyDeclarationSyntax> BuildActivityProperties(
        IEnumerable<TaskInfo> activities)
    {
        foreach (var activity in activities)
        {
            var type = $"{nameof(IDurableActivity<int, int>)}<{activity.ReturnType}, {activity.InputType}>";

            yield return SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.ParseTypeName(type), activity.Name)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.SingletonList(
                     SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                     .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))))
                 );
        }
    }
}
