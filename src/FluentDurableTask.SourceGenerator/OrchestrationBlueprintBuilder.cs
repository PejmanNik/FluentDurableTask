using FluentDurableTask.Core;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace FluentDurableTask.SourceGenerator;

public static class OrchestrationBlueprintBuilder
{
    public static IEnumerable<MemberDeclarationSyntax> Build(
        IEnumerable<OrchestrationDefinition> orchestrationDefinitions)
    {
        foreach (var orchestrationDefinition in orchestrationDefinitions)
        {
            var blueprint = BuildOrchestrationBlueprintInterface(
                orchestrationDefinition.OrchestrationsInfo);

            if (blueprint is not null)
            {
                yield return blueprint;
            }
        }
    }

    private static InterfaceDeclarationSyntax? BuildOrchestrationBlueprintInterface(
        IEnumerable<OrchestrationInfo> orchestrationsInfo)
    {
        var orchestration = orchestrationsInfo
            .FirstOrDefault(x => x.MethodName == nameof(ITaskOrchestrationBuilder.Orchestration));

        if (orchestration is null)
        {
            return null;
        }

        var activities = orchestrationsInfo
           .Where(x => x.MethodName == nameof(ITaskOrchestrationBuilder.ITaskActivityBuilder.Activity));

        var activityProperties = BuildActivityProperties(activities);
        var activityInterfaces = BuildActivityInterfaces(activities);
        var orchestrationInterface = BuildOrchestrationInterface(orchestration);

        var members = activityProperties
            .AsEnumerable<MemberDeclarationSyntax>()
            .Concat(activityInterfaces)
            .Append(orchestrationInterface)
            .ToArray();

        return SyntaxFactory
            .InterfaceDeclaration($"I{orchestration.Name}")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithBaseList(SyntaxFactory.BaseList(
                SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                SyntaxFactory.SimpleBaseType(
                SyntaxFactory.ParseTypeName(
                    $"{nameof(IOrchestrationBlueprint)}"))))
            )
            .AddMembers(members);
    }

    private const string _orchestrationInterfaceName = "IOrchestration";
    private static InterfaceDeclarationSyntax BuildOrchestrationInterface(OrchestrationInfo orchestration)
    {
        var type = $"{nameof(ITaskOrchestration<int, int, IOrchestrationBlueprint>)}" +
            $"<{orchestration.ReturnType}, {orchestration.InputType}, I{orchestration.Name}>";

        return SyntaxFactory
            .InterfaceDeclaration(_orchestrationInterfaceName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithBaseList(SyntaxFactory.BaseList(
                SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                SyntaxFactory.SimpleBaseType(
                SyntaxFactory.ParseTypeName(type))))
            );
    }

    private static IEnumerable<PropertyDeclarationSyntax> BuildActivityProperties(
        IEnumerable<OrchestrationInfo> activities)
    {
        foreach (var activity in activities)
        {
            var type = $"{nameof(ITaskActivity<int, int>)}<{activity.ReturnType}, {activity.InputType}>";

            yield return SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.ParseTypeName(type), activity.Name)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.SingletonList(
                     SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                     .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))))
                 );
        }
    }

    private static IEnumerable<InterfaceDeclarationSyntax> BuildActivityInterfaces(
        IEnumerable<OrchestrationInfo> activities)
    {
        foreach (var activity in activities)
        {
            var type = $"{nameof(ITaskActivity<int, int>)}<{activity.ReturnType}, {activity.InputType}>";

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
