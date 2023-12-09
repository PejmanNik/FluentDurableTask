using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;
using System.Text;

namespace FluentDurableTask.SourceGenerator;

[Generator]
public class FluentDurableTaskGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not OrchestrationDefinitionSyntaxReceiver receiver)
            return;

        foreach (var classSyntax in receiver.Classes)
        {
            var semanticModel = context.Compilation.GetSemanticModel(classSyntax.SyntaxTree);

            var durableOrchestrationsSyntax = GenerateInterface.Generate(semanticModel, classSyntax);
            var code = durableOrchestrationsSyntax.NormalizeWhitespace().ToFullString();
            context.AddSource($"{classSyntax.Identifier}.g.cs", SourceText.From(code, Encoding.UTF8));
        }

        var durableTaskClientSyntax = DurableTaskClientBuilder.Build();
        var durableTaskClientCode = durableTaskClientSyntax.NormalizeWhitespace().ToFullString();
        context.AddSource($"DurableTaskClient.g.cs", SourceText.From(durableTaskClientCode, Encoding.UTF8));
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new OrchestrationDefinitionSyntaxReceiver());

#if DEBUG_GENERATOR
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif
    }
}
