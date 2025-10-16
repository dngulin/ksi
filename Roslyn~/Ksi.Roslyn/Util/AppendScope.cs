using System;
using System.Text;

namespace Ksi.Roslyn.Util;

public readonly struct AppendScope : IDisposable
{
    public const string Indent = "    ";

    private readonly StringBuilder _sb;
    private readonly bool _closeScope;

    public readonly int Depth;

    public static AppendScope Root(StringBuilder sb)
    {
        sb.Clear();
        return new AppendScope(sb, 0, false);
    }

    private AppendScope(StringBuilder sb, int depth, bool closeScope)
    {
        _sb = sb;
        Depth = depth;
        _closeScope = closeScope;
    }

    public AppendScope Sub(string? header)
    {
        if (header == null)
            return new AppendScope(_sb, Depth, false);

        _sb.AppendLineIndented(Depth, header);
        _sb.AppendLineIndented(Depth, "{");

        return new AppendScope(_sb, Depth + 1, true);
    }

    public void AppendLine(string value) => _sb.AppendLineIndented(Depth, value);

    public void Dispose()
    {
        if (_closeScope)
            _sb.AppendLineIndented(Depth - 1, "}");
    }
}

public static class AppendScopeExtensions
{
    public static AppendScope OptNamespace(this AppendScope self, string ns)
    {
        return self.Sub(ns == "" ? null : $"namespace {ns}");
    }

    public static AppendScope PubStat(this AppendScope self, string expr)
    {
        return self.Sub("public static " + expr);
    }
}

public static class StringBuilderExtensions
{
    private static void AppendIndent(this StringBuilder self, int depth)
    {
        for (var i = 0; i < depth; i++)
            self.Append(AppendScope.Indent);
    }

    public static void AppendLineIndented(this StringBuilder self, int depth, string value)
    {
        if (!value.Contains("\n"))
        {
            self.AppendIndent(depth);
            self.AppendLine(value);
            return;
        }

        foreach (var line in value.Split('\n'))
        {
            self.AppendIndent(depth);
            self.AppendLine(line);
        }
    }
}