﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;

namespace RoslynPad.Roslyn.BraceMatching;

[ExportBraceMatcher(LanguageNames.VisualBasic)]
internal class OpenCloseBraceBraceMatcher : AbstractVisualBasicBraceMatcher
{
    public OpenCloseBraceBraceMatcher()
        : base(SyntaxKind.OpenBraceToken, SyntaxKind.CloseBraceToken)
    {
    }
}
