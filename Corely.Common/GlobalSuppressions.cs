using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Private event field names should be prefixed with '_' to conform with private field naming conventions", Scope = "module")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Primary constructors don't support making params readonly (as of 12-30-23)", Scope = "module")]
