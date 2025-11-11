using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Ksi.Roslyn.Extensions;

public static class AnalysisContextExtensions
{
    public static void Report(
        this SyntaxNodeAnalysisContext self, Location l, DiagnosticDescriptor d, params object?[] args
    )
    {
        self.ReportDiagnostic(Diagnostic.Create(d, l, args));
    }

    public static void Report(
        this OperationAnalysisContext self, Location l, DiagnosticDescriptor d, params object?[] args
    )
    {
        self.ReportDiagnostic(Diagnostic.Create(d, l, args));
    }

    public static void Report(
        this SymbolAnalysisContext self, Location l, DiagnosticDescriptor d, params object?[] args
    )
    {
        self.ReportDiagnostic(Diagnostic.Create(d, l, args));
    }
}