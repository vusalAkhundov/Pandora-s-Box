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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ClassCodeFixProvider)), Shared]
    public class ClassCodeFixProvider : CodeFixProvider
    {
        private const string title = "Make UpperCamelCase";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(ClassDiagnosticAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic classDiag = context.Diagnostics.First();
            context.RegisterCodeFix(CodeAction.Create(title, async token =>
            {
                // файл, в кот произошла диагностика
                Document doc = context.Document;

                // корневой элемент синтаксического дерева
                var root = await doc.GetSyntaxRootAsync(token);

                //узел, кот описывает объявление интерфейса
                ClassDeclarationSyntax node = root.FindNode(classDiag.Location.SourceSpan) as ClassDeclarationSyntax;
                // получаем имя интерфейса
                string className = node.Identifier.Text;
                char first = className[0];
                className = className.Remove(0, 1).Insert(0, char.ToUpper(first).ToString());

                // синтаксич дерево неизменяемо
                // создаем новый узел через замену, т.к. нет конструктора
                ClassDeclarationSyntax newNode = node.ReplaceToken(node.Identifier, SyntaxFactory.Identifier(className) );
                //SyntaxFactory - создает новый идентификатор

                var newDoc = doc.WithSyntaxRoot(root.ReplaceNode(node, newNode));
                return newDoc;
            }), classDiag);


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