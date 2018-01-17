# Tern
POC project to identify how to apply code fixes from a C# program

Currently fixing multiple diagnostic at once will generate a NullRef in Roslyn due to IDocumentTextDifferencingService not being found.
See https://github.com/dotnet/roslyn/issues/22710
