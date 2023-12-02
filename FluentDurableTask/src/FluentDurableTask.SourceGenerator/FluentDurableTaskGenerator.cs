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
            var compilationUnitSyntax = GenerateInterface.Generate(context, classSyntax);
           
            var newCode = compilationUnitSyntax.NormalizeWhitespace().ToFullString();
            context.AddSource($"{classSyntax.Identifier}.g.cs", SourceText.From(newCode, Encoding.UTF8));
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG_GENERATOR
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif

        context.RegisterForSyntaxNotifications(() => new OrchestrationDefinitionSyntaxReceiver());
    }
}
