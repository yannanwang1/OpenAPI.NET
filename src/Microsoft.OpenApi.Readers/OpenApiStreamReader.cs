﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. 

using System;
using System.IO;
using System.Linq;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers.Interface;
using Microsoft.OpenApi.Readers.Services;
using Microsoft.OpenApi.Services;
using SharpYaml;
using SharpYaml.Serialization;

namespace Microsoft.OpenApi.Readers
{
    /// <summary>
    /// Service class for converting streams into OpenApiDocument instances
    /// </summary>
    public class OpenApiStreamReader : IOpenApiReader<Stream, OpenApiDiagnostic>
    {
        private OpenApiReaderSettings _settings;

        /// <summary>
        /// Create stream reader with custom settings if desired.
        /// </summary>
        /// <param name="settings"></param>
        public OpenApiStreamReader(OpenApiReaderSettings settings = null)
        {
            _settings = settings ?? new OpenApiReaderSettings();

        }
        /// <summary>
        /// Reads the stream input and parses it into an Open API document.
        /// </summary>
        /// <param name="input">Stream containing OpenAPI description to parse.</param>
        /// <param name="diagnostic">Returns diagnostic object containing errors detected during parsing</param>
        /// <returns>Instance of newly created OpenApiDocument</returns>
        public OpenApiDocument Read(Stream input, out OpenApiDiagnostic diagnostic)
        {
            ParsingContext context;
            YamlDocument yamlDocument;
            diagnostic = new OpenApiDiagnostic();

            // Parse the YAML/JSON
            try
            {
                yamlDocument = LoadYamlDocument(input);
            }
            catch (SyntaxErrorException ex)
            {
                diagnostic.Errors.Add(new OpenApiError(string.Empty, ex.Message));
                return new OpenApiDocument();
            }

            context = new ParsingContext
            {
                ExtensionParsers = _settings.ExtensionParsers
            };

            // Parse the OpenAPI Document
            var document = context.Parse(yamlDocument, diagnostic);

            // Resolve References if requested
            switch (_settings.ReferenceResolution) 
            {
                case ReferenceResolutionSetting.ResolveAllReferences:
                    throw new ArgumentException(Properties.SRResource.CannotResolveRemoteReferencesSynchronously);
                case ReferenceResolutionSetting.ResolveLocalReferences:
                    var resolver = new OpenApiReferenceResolver(document);
                    var walker = new OpenApiWalker(resolver);
                    walker.Walk(document);
                    break;
                case ReferenceResolutionSetting.DoNotResolveReferences:
                    break;
            }

            // Validate the document
            var errors = document.Validate(_settings.RuleSet);
            foreach (var item in errors)
            {
                diagnostic.Errors.Add(new OpenApiError(item.ErrorPath, item.ErrorMessage));
            } 

            return document;
        }

        /// <summary>
        /// Helper method to turn streams into YamlDocument
        /// </summary>
        /// <param name="input">Stream containing YAML formatted text</param>
        /// <returns>Instance of a YamlDocument</returns>
        internal static YamlDocument LoadYamlDocument(Stream input)
        {
            YamlDocument yamlDocument;
            using (var streamReader = new StreamReader(input))
            {
                var yamlStream = new YamlStream();
                yamlStream.Load(streamReader);
                yamlDocument = yamlStream.Documents.First();
            }
            return yamlDocument;
        }
    }
}