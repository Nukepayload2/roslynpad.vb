using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Extensions;

namespace RoslynPad.Roslyn.Completion.Providers;

internal static class DirectiveCompletionProviderUtilities
{
    internal static bool TryGetStringLiteralToken(this SyntaxTree tree, int position, SyntaxKind directiveKind, out SyntaxToken stringLiteral, CancellationToken cancellationToken)
    {
        if (tree.IsEntirelyWithinStringLiteral(position, cancellationToken))
        {
            var token = tree.GetRoot(cancellationToken).FindToken(position, findInsideTrivia: true);
            if (token.IsKind(SyntaxKind.EndOfInterpolatedStringToken) || token.IsKind(SyntaxKind.EndOfXmlToken) || token.IsKind(SyntaxKind.EndOfFileToken))
            {
                token = token.GetPreviousToken(includeSkipped: true, includeDirectives: true);
            }

            if (token.IsKind(SyntaxKind.StringLiteralToken) && token.Parent?.IsKind(directiveKind) is true)
            {
                stringLiteral = token;
                return true;
            }
        }

        stringLiteral = default;
        return false;
    }
}
