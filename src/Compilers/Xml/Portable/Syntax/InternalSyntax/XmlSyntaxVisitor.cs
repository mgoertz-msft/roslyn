// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Xml.Syntax.InternalSyntax
{
    internal abstract partial class XmlSyntaxVisitor<TResult>
    {
        public virtual TResult Visit(XmlSyntaxNode node)
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

        protected virtual TResult DefaultVisit(XmlSyntaxNode node)
        {
            return default(TResult);
        }
    }

    internal abstract partial class XmlSyntaxVisitor
    {
        public virtual void Visit(XmlSyntaxNode node)
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

        public virtual void DefaultVisit(XmlSyntaxNode node)
        {
        }
    }
}
