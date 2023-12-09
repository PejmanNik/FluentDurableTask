using FluentDurableTask.Core;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection.Metadata;

namespace FluentDurableTask.SourceGenerator;

public static class DurableTaskClientBuilder
{
    public static NamespaceDeclarationSyntax Build()
    {
        var parameterList = SyntaxFactory.ParameterList(
        SyntaxFactory.SeparatedList(
                [SyntaxFactory.Parameter(SyntaxFactory.Identifier("taskHubClient"))
                        .WithType(SyntaxFactory.ParseTypeName("TaskHubClient"))]
                )

        );

        // Create constructor declaration
        var constructorDeclaration = SyntaxFactory.ConstructorDeclaration("DurableTaskClient")
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
            .WithParameterList(parameterList)
            .WithInitializer(
                SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                .AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("taskHubClient")))
             )
            .WithBody(SyntaxFactory.Block());

        var c = SyntaxFactory.ClassDeclaration("DurableTaskClient")
            .WithModifiers(SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.PartialKeyword)
            ))
            .WithBaseList(SyntaxFactory.BaseList(
                SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                SyntaxFactory.SimpleBaseType(
                SyntaxFactory.ParseTypeName(
                    $"{nameof(BaseDurableTaskClient<int>)}<IDurableOrchestrations>"))))
            )
            .AddMembers(constructorDeclaration);

        UsingDirectiveSyntax[] usingSyntax = [
            SyntaxFactory.UsingDirective(
                SyntaxFactory.ParseName($"{nameof(FluentDurableTask)}.{nameof(FluentDurableTask.Core)}")),
            SyntaxFactory.UsingDirective(
                SyntaxFactory.ParseName("DurableTask.Core"))
            ];

        return SyntaxFactory
              .NamespaceDeclaration(SyntaxFactory.ParseName(nameof(FluentDurableTask)))
              .AddUsings(usingSyntax)
              .AddMembers(c);
    }
}
