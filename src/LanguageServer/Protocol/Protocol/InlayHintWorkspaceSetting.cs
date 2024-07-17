﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Serialization;

namespace Roslyn.LanguageServer.Protocol
{
    /// <summary>
    /// Class representing the workspace inlay hint client capabilities.
    ///
    /// See the <see href="https://microsoft.github.io/language-server-protocol/specifications/lsp/3.17/specification/#inlayHintWorkspaceClientCapabilities">Language Server Protocol specification</see> for additional information.
    /// </summary>
    internal class InlayHintWorkspaceSetting
    {
        /// <summary>
        /// Gets or sets a value indicating whether the client supports a refresh request sent from the server to the client.
        /// </summary>
        [JsonPropertyName("refreshSupport")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool RefreshSupport { get; set; }
    }
}
