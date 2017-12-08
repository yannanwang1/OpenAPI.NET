// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.Interfaces
{
    /// <summary>
    /// Something that can dereference an OpenApiReference
    /// </summary>
    public interface IOpenApiReferenceTarget
    {
        
        /// <summary>
        /// Dereference an OpenApiReference to return some implementation of IOpenApiElement
        /// </summary>
        IOpenApiElement ResolveReference(OpenApiReference reference);

    }
}