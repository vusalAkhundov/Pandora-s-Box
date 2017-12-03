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
    public class InterfaceAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "InterfaceAnalyzer";       
        private const string Category = "Naming";
        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, "Type name doesn't match ICamelCase", "Type name '{0}' doesn't match ICamelCase", Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "Type names should match ICamelCase.");
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {           
            context.RegisterSymbolAction(AnalyzeSymbolInterface, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbolInterface(SymbolAnalysisContext context)
        {
            Regex IUpperCamelCase = new Regex("I[A-Z][a-zA-Z0-9]*");
            if (context.Symbol is INamedTypeSymbol iface)
            {
                if(iface.TypeKind == TypeKind.Interface)
                {
                    if (!IUpperCamelCase.IsMatch(iface.Name))
                    {
                        var diagnostic = Diagnostic.Create(Rule, context.Symbol.Locations[0], context.Symbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }

                }
            }
        }
    }
}
