using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Roslyn.Utilities;

namespace RoslynPad.Roslyn;

#pragma warning disable IL3000 // Assembly.Location is fine here

public class RoslynHostReferences
{
    public static RoslynHostReferences Empty { get; } = new(
        [],
        ImmutableDictionary<string, string>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase),
        []);

    /// <summary>
    /// Returns namespace-only (no assemblies) defaults that fit all frameworks.
    /// </summary>
    public static RoslynHostReferences NamespaceDefault { get; } = Empty.With(imports: new[]{
        "Microsoft.VisualBasic",
        "System",
        "System.Collections",
        "System.Collections.Generic",
        "System.Diagnostics",
        "System.Linq",
        "System.Threading.Tasks",
        "System.Xml.Linq",
    });

    public RoslynHostReferences With(IEnumerable<MetadataReference>? references = null, IEnumerable<string>? imports = null,
        IEnumerable<Assembly>? assemblyReferences = null, IEnumerable<string>? assemblyPathReferences = null, IEnumerable<Type>? typeNamespaceImports = null)
    {
        var referenceLocations = _referenceLocations;
        var importsParsed = imports?.WhereNotNull().Select(it => GlobalImport.Parse(it));
        ImmutableArray<GlobalImport> importsArray = Imports;
        if (importsParsed != null)
        {
            importsArray = importsArray.AddRange(importsParsed);
        } // End If

        var locations =
            assemblyReferences!.WhereNotNull().Select(c => c.Location).Concat(
            assemblyPathReferences!.WhereNotNull());

        foreach (var location in locations)
        {
            referenceLocations = referenceLocations.SetItem(location, string.Empty);
        }

        foreach (var type in typeNamespaceImports!.WhereNotNull())
        {
            importsArray = importsArray.Add(GlobalImport.Parse(type!.Namespace!));
            var location = type.Assembly.Location;
            referenceLocations = referenceLocations.SetItem(location, string.Empty);
        }

        return new RoslynHostReferences(
            _references.AddRange(references!.WhereNotNull()),
            referenceLocations,
            importsArray);
    }

    private RoslynHostReferences(
        ImmutableArray<MetadataReference> references,
        ImmutableDictionary<string, string> referenceLocations,
        ImmutableArray<GlobalImport> imports)
    {
        _references = references;
        _referenceLocations = referenceLocations;
        Imports = imports;
    }

    private readonly ImmutableArray<MetadataReference> _references;

    private readonly ImmutableDictionary<string, string> _referenceLocations;

    public ImmutableArray<GlobalImport> Imports { get; }

    public ImmutableArray<MetadataReference> GetReferences(Func<string, DocumentationProvider>? documentationProviderFactory = null) =>
        Enumerable.Concat(_references, Enumerable.Select(_referenceLocations, c => MetadataReference.CreateFromFile(c.Key, documentation: documentationProviderFactory?.Invoke(c.Key))))
            .ToImmutableArray();
}
