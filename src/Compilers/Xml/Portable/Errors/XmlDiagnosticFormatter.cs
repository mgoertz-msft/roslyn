// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.Xml
{
    public class XmlDiagnosticFormatter : DiagnosticFormatter
    {
        internal XmlDiagnosticFormatter()
        {
        }

        public new static XmlDiagnosticFormatter Instance { get; } = new XmlDiagnosticFormatter();
    }
}
