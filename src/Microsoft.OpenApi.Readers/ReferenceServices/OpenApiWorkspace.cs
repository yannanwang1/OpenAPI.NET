// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Exceptions;

namespace Microsoft.OpenApi {

    /// <summary>
    /// Holds references to OpenApiDocuments and document fragments that have references between them.
    /// </summary>
    public class OpenApiWorkspace {
        private Dictionary<string, IOpenApiReferenceTarget> _documents = new Dictionary<string,IOpenApiReferenceTarget>();
        private HttpClient httpClient;

        /// <summary>
        /// Default constructor 
        /// </summary>        
        public OpenApiWorkspace()  // Need to accept base Url/ base Path for relative Urls
        {
            httpClient = new HttpClient();
        }

//    wkspace.LoadFragment(Stream stream);
//wkspace.LoadDocument(Stream stream);
//Wkspace.Load(Stream stream);   // Figure out if it is a Fragment or Document

        /// <summary>
        /// Used to return a model object referenced by OpenApiReference 
        /// </summary> 
        public IOpenApiElement ResolveReference(OpenApiReference reference) {
            IOpenApiReferenceTarget document = _documents[reference.ExternalResource];
            return document.ResolveReference(reference);
        } 

        /// <summary>
        /// Main entry point for adding a document pointed to by an OpenApiReference 
        /// </summary> 
        public async Task LoadAsync(string externalReference) {
            MemoryStream memoryStream;

            using (var stream = await LoadStreamAsync(externalReference)) {
            
                if (stream is MemoryStream) {
                    memoryStream = (MemoryStream)stream;
                } else {
                    memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                }
                }
            // Attempt to load stream into regular OpenApiDocument
            // If there are errors, then load it into a Fragment
            var reader = new OpenApiStreamReader();

            var diagnostic = new OpenApiDiagnostic();
            var openApiDocument = reader.Read(memoryStream, out diagnostic);

        }

        private async Task<Stream> LoadStreamAsync(string externalReference) {
            // Figure out if it is a FilePath or uriReference

            Uri uri = new Uri(externalReference, UriKind.RelativeOrAbsolute);

            // Try and load it as a local file first
            if (!uri.IsAbsoluteUri && File.Exists(uri.OriginalString))
            {
                return new FileStream(uri.OriginalString, FileMode.Open);
            }

            // Then try and resolve it as a HTTP request
            var response = await httpClient.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStreamAsync();
            } else
            {
                throw new OpenApiException(string.Format("Cannot load external resource {0} via HTTP. Status code was {1} ", uri.OriginalString, response.StatusCode));
            }
        }

    }
}
