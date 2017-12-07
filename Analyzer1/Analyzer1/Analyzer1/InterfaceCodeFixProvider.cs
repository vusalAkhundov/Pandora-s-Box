using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace Analyzer1
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InterfaceCodeFixProvider)), Shared]
    public class InterfaceCodeFixProvider : CodeFixProvider
    {
        private const string title = "Make ICamelCase";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(InterfaceDiagnosticAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic interfaceDiag = context.Diagnostics.First();
            context.RegisterCodeFix(CodeAction.Create(title, async token =>
            {
                // файл, в кот произошла диагностика
                Document doc = context.Document;

                // корневой элемент синтаксического дерева
                var root = await doc.GetSyntaxRootAsync(token);

                //уел, кот описывает объявление интерфейса
                InterfaceDeclarationSyntax node = root.FindNode(interfaceDiag.Location.SourceSpan) as InterfaceDeclarationSyntax;
                // получаем имя интерфейса
                string interfaceName = node.Identifier.Text;
                if (interfaceName[0] == 'i')
                {
                    interfaceName = interfaceName.Remove(0, 1).Insert(0, "I");
                    if (char.IsLower(interfaceName[1]))
                    {
                        char second = interfaceName[1];
                        interfaceName = interfaceName.Remove(1, 1).Insert(1, char.ToUpper(second).ToString());
                    }
                }
                else if (interfaceName[0] == 'I')
                {
                    char second = interfaceName[1];
                    interfaceName = interfaceName.Remove(1, 1).Insert(1, char.ToUpper(second).ToString());
                }
                else
                {
                    char first = interfaceName[0];
                    interfaceName = interfaceName.Remove(0, 1).Insert(0, "I" + char.ToUpper(first));
                }

                // синтаксич дерево неизменяемо
                // создаем новый узел через замену, т.к. нет конструктора
                InterfaceDeclarationSyntax newNode = node.ReplaceToken(node.Identifier, SyntaxFactory.Identifier(interfaceName)); //SyntaxFactory - создает новый идентификатор

                var newDoc = doc.WithSyntaxRoot(root.ReplaceNode(node, newNode));
                return newDoc;
            }), interfaceDiag);


        }

        private async Task<Solution> MakeUppercaseAsync(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            // Compute new uppercase name.
            var identifierToken = typeDecl.Identifier;
            var newName = identifierToken.Text.ToUpperInvariant();

            // Get the symbol representing the type to be renamed.
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);

            // Produce a new solution that has all references to that type renamed, including the declaration.
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            // Return the new solution with the now-uppercase type name.
            return newSolution;
        }
    }
}