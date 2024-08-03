using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;

namespace RoslynPad.Build;

internal class ExecutionHostParameters(
    string buildPath,
    string nuGetConfigPath,
    ImmutableArray<GlobalImport> imports,
    ImmutableArray<string> disabledDiagnostics,
    string workingDirectory,
    SourceCodeKind sourceCodeKind,
    bool checkOverflow = false)
{
    public string BuildPath { get; } = buildPath;
    public string NuGetConfigPath { get; } = nuGetConfigPath;
    public ImmutableArray<GlobalImport> Imports { get; set; } = imports;
    public ImmutableArray<string> DisabledDiagnostics { get; } = disabledDiagnostics;
    public string WorkingDirectory { get; set; } = workingDirectory;
    public SourceCodeKind SourceCodeKind { get; set; } = sourceCodeKind;
    public bool CheckOverflow { get; } = checkOverflow;
}
