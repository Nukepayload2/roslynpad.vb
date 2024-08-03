// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Extensions;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace RoslynPad.Roslyn.BraceMatching;

[ExportBraceMatcher(LanguageNames.VisualBasic)]
internal class StringLiteralBraceMatcher : IBraceMatcher
{
    public async Task<BraceMatchingResult?> FindBracesAsync(Document document, int position, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var token = root!.FindToken(position);

        if (!token.ContainsDiagnostics)
        {
            if (token.IsKind(SyntaxKind.StringLiteralToken))
            {
                return new BraceMatchingResult(
                    new TextSpan(token.SpanStart, 1),
                    new TextSpan(token.Span.End - 1, 1));
            }

            if (token.IsKind(SyntaxKind.EndOfInterpolatedStringToken))
            {
                if (token.Parent is InterpolatedStringExpressionSyntax interpolatedString)
                {
                    return new BraceMatchingResult(new TextSpan(interpolatedString.SpanStart, 1), token.Span);
                }
            }
        }

        return null;
    }
}
