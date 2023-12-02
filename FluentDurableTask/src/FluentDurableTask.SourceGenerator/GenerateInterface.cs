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
        GeneratorExecutionContext context,
        ClassDeclarationSyntax classSyntax)
    {
        var semanticModel = context.Compilation.GetSemanticModel(classSyntax.SyntaxTree);
        var interfaces = new List<MemberDeclarationSyntax>();

        // extract Configure methods from the class
        var configureMethod = classSyntax.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(x => x.Identifier.ToString() == nameof(IOrchestrationDefinition.Configure))
            ?? throw new InvalidOperationException($"Class {classSyntax.Identifier} does not implement {nameof(IOrchestrationDefinition)}");

        var orchestrations = configureMethod.DescendantNodes().OfType<ExpressionStatementSyntax>();
        foreach (var orchestration in orchestrations)
        {
            var invocations = orchestration.DescendantNodes().OfType<InvocationExpressionSyntax>();
            var invocationsX = BuildInvocationInfos(semanticModel, invocations);
            var resultInterface = BuildOrchestrationInterface(invocationsX);
            interfaces.Add(resultInterface);
        }


        var namespaceDeclaration = classSyntax.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();
       // var namespaceDeclaration2 = classSyntax.Ancestors().OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault();
        var mem = interfaces.ToArray();
        if (namespaceDeclaration != null)
        {
            mem = new[] {
            SyntaxFactory.NamespaceDeclaration(namespaceDeclaration.Name)
            .AddMembers(interfaces.ToArray())
            };
        }

        var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(nameof(FluentDurableTask)));

        return SyntaxFactory.CompilationUnit()
            .AddUsings(usingDirective)
            .AddMembers(mem);
    }


    class InvocationInfo(string methodName, string name, string returnType, string inputType)
    {
        public string MethodName => methodName;
        public string Name => name;
        public string ReturnType => returnType;
        public string InputType => inputType;
    }

    private static IEnumerable<InvocationInfo> BuildInvocationInfos(
        SemanticModel semanticModel,
        IEnumerable<InvocationExpressionSyntax> invocations)
    {
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

                yield return new InvocationInfo(methodName, name, returnType, inputType);
            }
        }
    }

    private static InterfaceDeclarationSyntax? BuildOrchestrationInterface(IEnumerable<InvocationInfo> invocations)
    {
        var activityInterfaces = new List<InterfaceDeclarationSyntax>();
        var activityInvocations = invocations
            .Where(x => x.MethodName == nameof(ITaskOrchestrationBuilder.ITaskActivityBuilder.Activity));

        foreach (var activityInvocation in activityInvocations)
        {
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

        var orchInvocations = invocations
            .FirstOrDefault(x => x.MethodName == nameof(ITaskOrchestrationBuilder.Orchestration));
        if (orchInvocations is null) return null;

        return SyntaxFactory
                .InterfaceDeclaration($"I{orchInvocations.Name}")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBaseList(SyntaxFactory.BaseList(
                    SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                    SyntaxFactory.SimpleBaseType(
                    SyntaxFactory.ParseTypeName(
                        $"{nameof(ITaskOrchestration<int, int>)}<{orchInvocations.ReturnType}, {orchInvocations.InputType}>"))))
                )
                .AddMembers(activityInterfaces.ToArray());
    }
}
