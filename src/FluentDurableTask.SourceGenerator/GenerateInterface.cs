using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace FluentDurableTask.SourceGenerator;

public static class GenerateInterface
{
    public static NamespaceDeclarationSyntax Generate(
        SemanticModel semanticModel,
        ClassDeclarationSyntax classSyntax)
    {
        // extract Configure methods from the class
        var configureMethod = classSyntax.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(x => x.Identifier.ToString() == nameof(IOrchestrationDefinition.Configure))
            ?? throw new InvalidOperationException($"Class {classSyntax.Identifier} does not implement {nameof(IOrchestrationDefinition)}");

        var orchestrationDefinitions = BuildOrchestrationDefinition
            .BuildFromConfigureMethod(configureMethod, semanticModel);

        var orchestrationBlueprints = OrchestrationBlueprintBuilder.Build(orchestrationDefinitions);

        var orchestrationProperties = DurableOrchestrationsPropertyBuilder.Build(orchestrationDefinitions);
        var durableOrchestrations = CreateDurableOrchestrationsInterface(
            orchestrationProperties.ToArray()
        );

        return SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName(nameof(FluentDurableTask)))
            .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{nameof(FluentDurableTask)}.{nameof(FluentDurableTask.Core)}")))
            .AddMembers(orchestrationBlueprints.ToArray())
            .AddMembers(durableOrchestrations);
    }

    private const string _durableOrchestrationsInterfaceName = "IDurableOrchestrations";

    private static InterfaceDeclarationSyntax CreateDurableOrchestrationsInterface(
        MemberDeclarationSyntax[] members)
    {
        var baseInterface = SyntaxFactory.InterfaceDeclaration(_durableOrchestrationsInterfaceName)
            .WithModifiers(SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.PartialKeyword)
             ))
            .AddMembers(members);

        return baseInterface;
    }
}
