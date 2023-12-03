using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentDurableTask.SourceGenerator;

public static class GenerateInterface
{
    public static CompilationUnitSyntax Generate(
        SemanticModel semanticModel,
        ClassDeclarationSyntax classSyntax)
    {


        // extract Configure methods from the class
        var configureMethod = classSyntax.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(x => x.Identifier.ToString() == nameof(IOrchestrationDefinition.Configure))
            ?? throw new InvalidOperationException($"Class {classSyntax.Identifier} does not implement {nameof(IOrchestrationDefinition)}");

        var interfaces = PP(configureMethod, semanticModel);

        return BuildR(AddNameSpace(classSyntax, interfaces.ToArray()));
    }

    private static MemberDeclarationSyntax[] AddNameSpace(ClassDeclarationSyntax classSyntax, MemberDeclarationSyntax[] members)
    {
        var namespaceDeclaration = classSyntax.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();
        if (namespaceDeclaration != null)
        {
            return [
                SyntaxFactory
                    .NamespaceDeclaration(namespaceDeclaration.Name)
                    .AddMembers(members)
             ];
        }

        return members;
    }

    private static CompilationUnitSyntax BuildR(MemberDeclarationSyntax[] members)
    {
        var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(nameof(FluentDurableTask)));

        return SyntaxFactory.CompilationUnit()
            .AddUsings(usingDirective)
            .AddMembers(members);
    }

    private static IEnumerable<InterfaceDeclarationSyntax> PP(MethodDeclarationSyntax configureMethod, SemanticModel semanticModel)
    {
        var orchestrations = configureMethod.DescendantNodes().OfType<ExpressionStatementSyntax>();
        foreach (var orchestration in orchestrations)
        {
            var orchestrationsInfo = BuildOrchestrationsInfo(semanticModel, orchestration);
            var resultInterface = BuildOrchestrationInterface(orchestrationsInfo);

            if (resultInterface is not null)
                yield return resultInterface;
        }
    }

    class OrchestrationInfo(string methodName, string name, string returnType, string inputType)
    {
        public string MethodName => methodName;
        public string Name => name;
        public string ReturnType => returnType;
        public string InputType => inputType;
    }

    private static IEnumerable<OrchestrationInfo> BuildOrchestrationsInfo(
        SemanticModel semanticModel,
        ExpressionStatementSyntax orchestration)
    {
        var invocations = orchestration.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var methodName = memberAccess.Name.Identifier.Text;

                var argument = invocation.ArgumentList.Arguments.FirstOrDefault();
                if (argument?.Expression is not LiteralExpressionSyntax argumentExpression)
                    continue;

                var name = argumentExpression.Token.ValueText;

                var mapToSymbol = semanticModel.GetSymbolInfo(memberAccess).Symbol;
                if (mapToSymbol is not IMethodSymbol methodSymbol)
                    continue;

                var typeArguments = methodSymbol.TypeArguments;
                if (typeArguments.Length != 2)
                    continue;

                var returnType = typeArguments[0].ToString();
                var inputType = typeArguments[1].ToString();

                yield return new OrchestrationInfo(methodName, name, returnType, inputType);
            }
        }
    }

    private static InterfaceDeclarationSyntax? BuildOrchestrationInterface(
        IEnumerable<OrchestrationInfo> orchestrationsInfo)
    {
        var activityInterfaces = new List<MemberDeclarationSyntax>();
        var activityInvocations = orchestrationsInfo
            .Where(x => x.MethodName == nameof(ITaskOrchestrationBuilder.ITaskActivityBuilder.Activity));

        foreach (var activityInvocation in activityInvocations)
        {
            var readOnlyProperty = SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.ParseTypeName(
                      $"{nameof(ITaskActivity<int, int>)}<{activityInvocation.ReturnType}, {activityInvocation.InputType}>"), activityInvocation.Name)
                 .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                 .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.SingletonList(
                     SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                     .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))))
                 );
            activityInterfaces.Add(readOnlyProperty);

            activityInterfaces.Add(SyntaxFactory
                .InterfaceDeclaration($"I{activityInvocation.Name}")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBaseList(SyntaxFactory.BaseList(
                    SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                    SyntaxFactory.SimpleBaseType(
                    SyntaxFactory.ParseTypeName(
                      $"{nameof(ITaskActivity<int, int>)}<{activityInvocation.ReturnType}, {activityInvocation.InputType}>"))))
                ));
        }

        var orchInvocations = orchestrationsInfo
            .FirstOrDefault(x => x.MethodName == nameof(ITaskOrchestrationBuilder.Orchestration));
        if (orchInvocations is null) return null;

        activityInterfaces.Add(SyntaxFactory
            .InterfaceDeclaration($"IOrchestration")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithBaseList(SyntaxFactory.BaseList(
                SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                SyntaxFactory.SimpleBaseType(
                SyntaxFactory.ParseTypeName(
                  $"{nameof(ITaskOrchestration<int, int, IOrchestrationBlueprint>)}<{orchInvocations.ReturnType}, {orchInvocations.InputType}, I{orchInvocations.Name}>"))))
            ));

        return SyntaxFactory
                .InterfaceDeclaration($"I{orchInvocations.Name}")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBaseList(SyntaxFactory.BaseList(
                    SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                    SyntaxFactory.SimpleBaseType(
                    SyntaxFactory.ParseTypeName(
                        $"{nameof(IOrchestrationBlueprint)}"))))
                )
                .AddMembers(activityInterfaces.ToArray());
    }
}
