# ѯ-Framework Roslyn

You can open this solution with any C# IDE to browse the code and run Unity tests.
It doesn't depend on Unity.

Projects in the solution:
- `Ksi`: A project _importing_ Unity package runtime code.
  It also contains `UnityApiStub.cs`, which provides some `Unity.Collections` API.
- `Ksi.Tests`: A project _importing_ Unity package `NUnit` tests.
  These tests do not depend on Unity and can be executed directly in the IDE.
- `Ksi.Roslyn`: Roslyn analyzers and code generators.
- `Ksi.Roslyn.Tests`: `xUnit` tests for Roslyn analyzers and code generators.
- `Ksi.Roslyn.DocGen`: A tool to generate Markdown documentation from XML documentation comments.
  It supports only a subset of XML tags.