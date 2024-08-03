using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.Emit;

namespace RoslynPad.Roslyn;

internal sealed class Compiler(ImmutableList<SyntaxTree> syntaxTrees, VisualBasicParseOptions parseOptions,
    OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
    Platform platform = Platform.AnyCpu, IEnumerable<MetadataReference>? references = null,
    IEnumerable<GlobalImport>? imports = null, string? workingDirectory = null,
    SourceReferenceResolver? sourceResolver = null,
    OptimizationLevel optimizationLevel = OptimizationLevel.Debug,
    bool checkOverflow = false, bool allowUnsafe = true)
{
    public ImmutableList<SyntaxTree> SyntaxTrees { get; } = syntaxTrees;
    public OutputKind OutputKind { get; } = outputKind;
    public Platform Platform { get; } = platform;
    public ImmutableArray<MetadataReference> References { get; } = references?.AsImmutable() ?? [];
    public SourceReferenceResolver SourceResolver { get; } = sourceResolver ??
                         (workingDirectory != null
                             ? new SourceFileResolver([], workingDirectory)
                             : SourceFileResolver.Default);
    public ImmutableArray<GlobalImport> Imports { get; } = imports?.AsImmutable() ?? [];
    public VisualBasicParseOptions ParseOptions { get; } = parseOptions;
    public OptimizationLevel OptimizationLevel { get; } = optimizationLevel;
    public bool CheckOverflow { get; } = checkOverflow;
    public bool AllowUnsafe { get; } = allowUnsafe;

    public ImmutableArray<Diagnostic> CompileAndSaveAssembly(string assemblyPath, CancellationToken cancellationToken = default)
    {
        var compilation = GetCompilationFromCode(Path.GetFileNameWithoutExtension(assemblyPath));

        var diagnostics = compilation.GetParseDiagnostics(cancellationToken);
        if (!diagnostics.IsEmpty)
        {
            return diagnostics;
        }

        var diagnosticsBag = new DiagnosticBag();
        SaveAssembly(assemblyPath, compilation, diagnosticsBag, cancellationToken);
        return GetDiagnostics(diagnosticsBag, includeWarnings: true);
    }

    private static void SaveAssembly(string assemblyPath, Compilation compilation, DiagnosticBag diagnostics, CancellationToken cancellationToken)
    {
        using var peStream = File.OpenWrite(assemblyPath);
        using var pdbStream = File.OpenWrite(Path.ChangeExtension(assemblyPath, "pdb"));
        var emitResult = compilation.Emit(
            peStream: peStream,
            pdbStream: pdbStream,
            options: new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb),
            cancellationToken: cancellationToken);

        diagnostics.AddRange(emitResult.Diagnostics);
    }

    private VisualBasicCompilation GetCompilationFromCode(string assemblyName)
    {
        var compilationOptions = new VisualBasicCompilationOptions(
            OutputKind,
            mainTypeName: null,
            scriptClassName: ParseOptions.Kind == SourceCodeKind.Script ? "Program" : null,
            globalImports: Imports,
            optimizationLevel: OptimizationLevel,
            checkOverflow: CheckOverflow,
            platform: Platform,
            deterministic: true,
            xmlReferenceResolver: null,
            sourceReferenceResolver: SourceResolver,
            assemblyIdentityComparer: AssemblyIdentityComparer.Default
        );

        return VisualBasicCompilation.Create(
             assemblyName,
             SyntaxTrees,
             References,
             compilationOptions);
    }

    private static ImmutableArray<Diagnostic> GetDiagnostics(DiagnosticBag diagnostics, bool includeWarnings)
    {
        if (diagnostics.IsEmptyWithoutResolution)
        {
            return [];
        }

        return diagnostics.AsEnumerable().Where(d =>
            d.Severity == DiagnosticSeverity.Error || (includeWarnings && d.Severity == DiagnosticSeverity.Warning)).AsImmutable();
    }

    private class DiagnosticBag
    {
        private ConcurrentQueue<Diagnostic>? _lazyBag;

        public bool IsEmptyWithoutResolution => _lazyBag?.IsEmpty != false;

        private ConcurrentQueue<Diagnostic> Bag
        {
            get
            {
                var bag = _lazyBag;
                if (bag != null)
                {
                    return bag;
                }

                var newBag = new ConcurrentQueue<Diagnostic>();
                return Interlocked.CompareExchange(ref _lazyBag, newBag, null) ?? newBag;
            }
        }

        public void AddRange<T>(ImmutableArray<T> diagnostics) where T : Diagnostic
        {
            if (!diagnostics.IsDefaultOrEmpty)
            {
                var bag = Bag;
                foreach (var t in diagnostics)
                {
                    bag.Enqueue(t);
                }
            }
        }

        public IEnumerable<Diagnostic> AsEnumerable()
        {
            var bag = Bag;

            var foundVoid = bag.Any(diagnostic => diagnostic.Severity == DiagnosticSeverityVoid);

            return foundVoid
                ? AsEnumerableFiltered()
                : bag;
        }

        internal void Clear()
        {
            var bag = _lazyBag;
            if (bag != null)
            {
                _lazyBag = null;
            }
        }

        private static DiagnosticSeverity DiagnosticSeverityVoid => ~DiagnosticSeverity.Info;

        private IEnumerable<Diagnostic> AsEnumerableFiltered() =>
            Bag.Where(diagnostic => diagnostic.Severity != DiagnosticSeverityVoid);
    }
}
