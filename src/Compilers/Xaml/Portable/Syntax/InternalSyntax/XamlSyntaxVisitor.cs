// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Xaml.Syntax.InternalSyntax
{
    internal abstract partial class XamlSyntaxVisitor<TResult>
    {
        public virtual TResult Visit(XamlSyntaxNode node)
        {
            if (node == null)
            {
                return default(TResult);
            }

            return node.Accept(this);
        }

        public virtual TResult VisitToken(SyntaxToken token)
        {
            return this.DefaultVisit(token);
        }

        public virtual TResult VisitTrivia(SyntaxTrivia trivia)
        {
            return this.DefaultVisit(trivia);
        }

        protected virtual TResult DefaultVisit(XamlSyntaxNode node)
        {
            return default(TResult);
        }
    }

    internal abstract partial class XamlSyntaxVisitor
    {
        public virtual void Visit(XamlSyntaxNode node)
        {
            if (node == null)
            {
                return;
            }

            node.Accept(this);
        }

        public virtual void VisitToken(SyntaxToken token)
        {
            this.DefaultVisit(token);
        }

        public virtual void VisitTrivia(SyntaxTrivia trivia)
        {
            this.DefaultVisit(trivia);
        }

        public virtual void DefaultVisit(XamlSyntaxNode node)
        {
        }
    }
}
