using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace FluentDurableTask.SourceGenerator;

public class OrchestrationDefinitionSyntaxReceiver : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> Classes { get; } = [];

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax classSyntax)
        {
            var isOrchestrationDefinition = classSyntax.BaseList?.Types.Any(t =>
                    t.Type is SimpleNameSyntax syntax &&
                    syntax.Identifier.Text == nameof(IOrchestrationDefinition));

            if (isOrchestrationDefinition == true)
            {
                Classes.Add(classSyntax);
            }
        }
    }
}