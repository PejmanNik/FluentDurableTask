using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace FluentDurableTask.SourceGenerator;

public class TaskInfo(
    string methodName,
    string name,
    string returnType,
    string inputType)
{
    public string MethodName => methodName;
    public string Name => name;
    public string ReturnType => returnType;
    public string InputType => inputType;
}

public class OrchestrationInfo(
    IEnumerable<TaskInfo> tasksInfo)
{
    public IEnumerable<TaskInfo> TasksInfo => tasksInfo;
}

public static class OrchestrationInfoBuilder
{
    public static IEnumerable<OrchestrationInfo> BuildFromConfigureMethod(
        MethodDeclarationSyntax configureMethod,
        SemanticModel semanticModel)
    {
        var orchestrations = configureMethod.DescendantNodes().OfType<ExpressionStatementSyntax>();
        foreach (var orchestration in orchestrations)
        {
            var info = Build(semanticModel, orchestration);
            yield return new OrchestrationInfo(info);
        }
    }

    private static IEnumerable<TaskInfo> Build(
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

                yield return new TaskInfo(methodName, name, returnType, inputType);
            }
        }
    }
}
