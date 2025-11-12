# ѯ-Framework Roslyn

You can open this solution with any C# IDE to browse code and run unity tests.
It doesn't depend on Unity.

Projects in the solution:
- `Ksi` — project _importing_ unity package runtime code.
  Also contains `UnityApiStub.cs` that provides some `Unity.Collections` API. 
- `Ksi.Tests` — project _importing_ unity package `NUnit` tests.
  They are not dependent on Unity and can be executed directly in IDE
- `Ksi.Roslyn` — Roslyn analyzers and code generators
- `Ksi.Roslyn.Tests` — `xUnit` tests for Roslyn analyzers and code generators
- `Ksi.Roslyn.DocGen` — tool to generate Markdown documentation from XML documentation comments.
  Supports only used subset of XML tags