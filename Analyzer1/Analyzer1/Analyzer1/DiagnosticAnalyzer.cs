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
    public class Analyzer1Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "Analyzer1";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSymbolAction(AnalyzeSymbolNamedType, SymbolKind.NamedType);
            context.RegisterSymbolAction(AnalyzeSymbolInterface, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbolNamedType(SymbolAnalysisContext context)
        {
            Regex UpperCamelCase = new Regex("[A-Z][a-zA-Z0-9]*");
            Regex IUpperCamelCase = new Regex("I[A-Z][a-zA-Z0-9]*");
            Regex TUpperCamelCase = new Regex("T[A-Z][a-zA-Z0-9]*");
            Regex lowerCamelCase = new Regex("[a-z][a-zA-Z0-9]*");
            Regex _lowerCamelCase = new Regex("_[a-z][a-zA-Z0-9]*");
            Regex defaultPattern = UpperCamelCase;
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            if (context.Symbol as ITypeParameterSymbol!=null)
            {
                defaultPattern = TUpperCamelCase;
            }
            else if (context.Symbol as ILocalSymbol != null)
            {
                defaultPattern = lowerCamelCase;
            }
            if (!defaultPattern.IsMatch(context.Symbol.Name))
           {
               // For all such symbols, produce a diagnostic.
               //iydioqwe
               var diagnostic = Diagnostic.Create(Rule, context.Symbol.Locations[0], context.Symbol.Name);
           
               context.ReportDiagnostic(diagnostic);
           }

            
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
