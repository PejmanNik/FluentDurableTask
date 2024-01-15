using FluentDurableTask.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
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
            ParseOrchestrationDefinitionSyntax(context, classSyntax);
            //var durableOrchestrationsSyntax = GenerateInterface.Generate(semanticModel, classSyntax);
            //var code = durableOrchestrationsSyntax.NormalizeWhitespace().ToFullString();
            //context.AddSource($"{classSyntax.Identifier}.g.cs", SourceText.From(code, Encoding.UTF8));
        }

        var durableTaskClientSyntax = DurableTaskClientBuilder.Build();
        AddSource(context, durableTaskClientSyntax, $"{DurableTaskClientBuilder.DurableTaskClient}.g.cs");
    }

    private void ParseOrchestrationDefinitionSyntax(
        GeneratorExecutionContext context,
        ClassDeclarationSyntax classSyntax)
    {
        // extract Configure methods from the class
        var configureMethod = classSyntax.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(x => x.Identifier.ToString() == nameof(IOrchestrationDefinition.Configure));

        if (configureMethod is null)
        {
            //TODO: log warning
            return;
        }

        var semanticModel = context.Compilation.GetSemanticModel(classSyntax.SyntaxTree);

        var orchestrationsInfo = OrchestrationInfoBuilder
            .BuildFromConfigureMethod(configureMethod, semanticModel);

        var orchestrationBlueprints = OrchestrationBlueprintBuilder.Build(orchestrationsInfo);
        AddSource(context, orchestrationBlueprints, $"{classSyntax.Identifier}.Blueprint.g.cs");

        var durableOrchestrations = DurableOrchestrationsBuilder.Build(orchestrationsInfo);
        AddSource(context, durableOrchestrations, $"{classSyntax.Identifier}.Orchestrations.g.cs");
    }

    private void AddSource(GeneratorExecutionContext context, CSharpSyntaxNode syntax, string name)
    {
        var code = syntax.NormalizeWhitespace().ToFullString();
        context.AddSource(name, SourceText.From(code, Encoding.UTF8));
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
