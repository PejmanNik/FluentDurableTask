using FluentDurableTask.Core;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace FluentDurableTask.SourceGenerator;

public static class DurableOrchestrationsPropertyBuilder
{
    public static IEnumerable<MemberDeclarationSyntax> Build(
        IEnumerable<OrchestrationDefinition> orchestrationDefinitions)
    {
        foreach (var orchestrationDefinition in orchestrationDefinitions)
        {
            var orchestration = orchestrationDefinition
                .OrchestrationsInfo
                .FirstOrDefault(x => x.MethodName == nameof(ITaskOrchestrationBuilder.Orchestration));

            if (orchestration is null)
            {
                //TODO: log $"Can't find the orchestration declaration");
                continue;
            }

            //var type = $"{nameof(ITaskOrchestration<int, int, IOrchestrationBlueprint>)}" +
            //    $"<{orchestration.ReturnType}, {orchestration.InputType}, I{orchestration.Name}>";

            yield return SyntaxFactory.PropertyDeclaration(
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
    }
}
