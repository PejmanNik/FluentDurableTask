using FluentDurableTask.Core;
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
     

      
        return SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName(nameof(FluentDurableTask)))
            .AddUsings(SyntaxFactory.UsingDirective(
                SyntaxFactory.ParseName($"{nameof(FluentDurableTask)}.{nameof(FluentDurableTask.Core)}"))
            );
    }

    private const string _durableOrchestrationsInterfaceName = "IDurableOrchestrations";

    private static InterfaceDeclarationSyntax CreateDurableOrchestrationsInterface(
        MemberDeclarationSyntax[] members)
    {
        var baseInterfaceType = $"{nameof(FluentDurableTask)}.{nameof(FluentDurableTask.Core)}.{nameof(IDurableOrchestrations)}";

        var baseInterface = SyntaxFactory.InterfaceDeclaration(_durableOrchestrationsInterfaceName)
            .WithBaseList(SyntaxFactory.BaseList(
                SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                SyntaxFactory.SimpleBaseType(
                SyntaxFactory.ParseTypeName(
                    baseInterfaceType))))
            )
            .WithModifiers(SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.PartialKeyword)
             ))
            .AddMembers(members);

        return baseInterface;
    }
}
