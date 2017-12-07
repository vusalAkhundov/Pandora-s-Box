using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Text.RegularExpressions;

namespace Analyzer1
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ClassDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ClassAnalyzer";
        private const string Category = "Naming";
        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor (
            DiagnosticId, 
            "Type name doesn't match UpperCamelCase", 
            "Type name '{0}' doesn't match UpperCamelCase", 
            Category, DiagnosticSeverity.Warning, 
            isEnabledByDefault: true, 
            description: "Type names should match UpperCamelCase.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeSymbolClass, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbolClass(SymbolAnalysisContext context)
        {
            Regex UpperCamelCase = new Regex("^[A-Z]{1,1}[a-zA-Z0-9]*");
            if (context.Symbol is INamedTypeSymbol isClass)
            {
                if (isClass.TypeKind == TypeKind.Class)
                {
                    if (!UpperCamelCase.IsMatch(isClass.Name))
                    {
                        var diagnostic = Diagnostic.Create(Rule, context.Symbol.Locations[0], context.Symbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }

                }
            }
        }
    }
}
