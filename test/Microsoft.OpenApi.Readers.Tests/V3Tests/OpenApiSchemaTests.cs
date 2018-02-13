﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. 

using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers.ParseNodes;
using Microsoft.OpenApi.Readers.V3;
using SharpYaml.Serialization;
using Xunit;

namespace Microsoft.OpenApi.Readers.Tests.V3Tests
{
    [Collection("DefaultSettings")]
    public class OpenApiSchemaTests
    {
        private const string SampleFolderPath = "V3Tests/Samples/OpenApiSchema/";

        [Fact]
        public void ParsePrimitiveSchemaShouldSucceed()
        {
            using (var stream = Resources.GetStream(Path.Combine(SampleFolderPath, "primitiveSchema.yaml")))
            {
                var yamlStream = new YamlStream();
                yamlStream.Load(new StreamReader(stream));
                var yamlNode = yamlStream.Documents.First().RootNode;

                var context = new ParsingContext();
                var diagnostic = new OpenApiDiagnostic();

                var node = new MapNode(context, diagnostic, (YamlMappingNode)yamlNode);

                // Act
                var schema = OpenApiV3Deserializer.LoadSchema(node);

                // Assert
                diagnostic.ShouldBeEquivalentTo(new OpenApiDiagnostic());

                schema.ShouldBeEquivalentTo(
                    new OpenApiSchema
                    {
                        Type = "string",
                        Format = "email"
                    });
            }
        }

        [Fact]
        public void ParseSimpleSchemaShouldSucceed()
        {
            using (var stream = Resources.GetStream(Path.Combine(SampleFolderPath, "simpleSchema.yaml")))
            {
                var yamlStream = new YamlStream();
                yamlStream.Load(new StreamReader(stream));
                var yamlNode = yamlStream.Documents.First().RootNode;

                var context = new ParsingContext();
                var diagnostic = new OpenApiDiagnostic();

                var node = new MapNode(context, diagnostic, (YamlMappingNode)yamlNode);

                // Act
                var schema = OpenApiV3Deserializer.LoadSchema(node);

                // Assert
                diagnostic.ShouldBeEquivalentTo(new OpenApiDiagnostic());

                schema.ShouldBeEquivalentTo(
                    new OpenApiSchema
                    {
                        Type = "object",
                        Required =
                        {
                            "name"
                        },
                        Properties =
                        {
                            ["name"] = new OpenApiSchema
                            {
                                Type = "string"
                            },
                            ["address"] = new OpenApiSchema
                            {
                                Type = "string"
                            },
                            ["age"] = new OpenApiSchema
                            {
                                Type = "integer",
                                Format = "int32",
                                Minimum = 0
                            }
                        }
                    });
            }
        }

        [Fact]
        public void ParseDictionarySchemaShouldSucceed()
        {
            using (var stream = Resources.GetStream(Path.Combine(SampleFolderPath, "dictionarySchema.yaml")))
            {
                var yamlStream = new YamlStream();
                yamlStream.Load(new StreamReader(stream));
                var yamlNode = yamlStream.Documents.First().RootNode;

                var context = new ParsingContext();
                var diagnostic = new OpenApiDiagnostic();

                var node = new MapNode(context, diagnostic, (YamlMappingNode)yamlNode);

                // Act
                var schema = OpenApiV3Deserializer.LoadSchema(node);

                // Assert
                diagnostic.ShouldBeEquivalentTo(new OpenApiDiagnostic());

                schema.ShouldBeEquivalentTo(
                    new OpenApiSchema
                    {
                        Type = "object",
                        AdditionalProperties = new OpenApiSchema
                        {
                            Type = "string"
                        }
                    });
            }
        }

        [Fact]
        public void ParseBasicSchemaWithExampleShouldSucceed()
        {
            using (var stream = Resources.GetStream(Path.Combine(SampleFolderPath, "basicSchemaWithExample.yaml")))
            {
                var yamlStream = new YamlStream();
                yamlStream.Load(new StreamReader(stream));
                var yamlNode = yamlStream.Documents.First().RootNode;

                var context = new ParsingContext();
                var diagnostic = new OpenApiDiagnostic();

                var node = new MapNode(context, diagnostic, (YamlMappingNode)yamlNode);

                // Act
                var schema = OpenApiV3Deserializer.LoadSchema(node);

                // Assert
                diagnostic.ShouldBeEquivalentTo(new OpenApiDiagnostic());

                schema.ShouldBeEquivalentTo(
                    new OpenApiSchema
                    {
                        Type = "object",
                        Properties =
                        {
                            ["id"] = new OpenApiSchema
                            {
                                Type = "integer",
                                Format = "int64"
                            },
                            ["name"] = new OpenApiSchema
                            {
                                Type = "string"
                            }
                        },
                        Required =
                        {
                            "name"
                        },
                        Example = new OpenApiObject
                        {
                            ["name"] = new OpenApiString("Puma"),
                            ["id"] = new OpenApiInteger(1)
                        }
                    });
            }
        }

        [Fact]
        public void ParseBasicSchemaWithReferenceShouldSucceed()
        {
            using (var stream = Resources.GetStream(Path.Combine(SampleFolderPath, "basicSchemaWithReference.yaml")))
            {
                // Act
                var openApiDoc = new OpenApiStreamReader().Read(stream, out var diagnostic);

                // Assert
                var components = openApiDoc.Components;

                diagnostic.ShouldBeEquivalentTo(
                    new OpenApiDiagnostic() { SpecificationVersion = OpenApiSpecVersion.OpenApi3_0 });

                components.ShouldBeEquivalentTo(
                    new OpenApiComponents
                    {
                        Schemas =
                        {
                            ["ErrorModel"] = new OpenApiSchema
                            {
                                Type = "object",
                                Properties =
                                {
                                    ["code"] = new OpenApiSchema
                                    {
                                        Type = "integer",
                                        Minimum = 100,
                                        Maximum = 600
                                    },
                                    ["message"] = new OpenApiSchema
                                    {
                                        Type = "string"
                                    }
                                },
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.Schema,
                                    Id = "ErrorModel"
                                },
                                Required =
                                {
                                    "message",
                                    "code"
                                }
                            },
                            ["ExtendedErrorModel"] = new OpenApiSchema
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.Schema,
                                    Id = "ExtendedErrorModel"
                                },
                                AllOf =
                                {
                                    new OpenApiSchema
                                    {
                                        Reference = new OpenApiReference
                                        {
                                            Type = ReferenceType.Schema,
                                            Id = "ErrorModel"
                                        },
                                        // Schema should be dereferenced in our model, so all the properties
                                        // from the ErrorModel above should be propagated here.
                                        Type = "object",
                                        Properties =
                                        {
                                            ["code"] = new OpenApiSchema
                                            {
                                                Type = "integer",
                                                Minimum = 100,
                                                Maximum = 600
                                            },
                                            ["message"] = new OpenApiSchema
                                            {
                                                Type = "string"
                                            }
                                        },
                                        Required =
                                        {
                                            "message",
                                            "code"
                                        }
                                    },
                                    new OpenApiSchema
                                    {
                                        Type = "object",
                                        Required = {"rootCause"},
                                        Properties =
                                        {
                                            ["rootCause"] = new OpenApiSchema
                                            {
                                                Type = "string"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });
            }
        }

        [Fact]
        public void ParseAdvancedSchemaWithReferenceShouldSucceed()
        {
            using (var stream = Resources.GetStream(Path.Combine(SampleFolderPath, "advancedSchemaWithReference.yaml")))
            {
                // Act
                var openApiDoc = new OpenApiStreamReader().Read(stream, out var diagnostic);

                // Assert
                var components = openApiDoc.Components;

                diagnostic.ShouldBeEquivalentTo(
                    new OpenApiDiagnostic() { SpecificationVersion = OpenApiSpecVersion.OpenApi3_0 });

                components.ShouldBeEquivalentTo(
                    new OpenApiComponents
                    {
                        Schemas =
                        {
                            ["Pet"] = new OpenApiSchema
                            {
                                Type = "object",
                                Discriminator = new OpenApiDiscriminator
                                {
                                    PropertyName = "petType"
                                },
                                Properties =
                                {
                                    ["name"] = new OpenApiSchema
                                    {
                                        Type = "string"
                                    },
                                    ["petType"] = new OpenApiSchema
                                    {
                                        Type = "string"
                                    }
                                },
                                Required =
                                {
                                    "name",
                                    "petType"
                                }, 
                                Reference = new OpenApiReference()
                                {
                                    Id= "Pet",
                                    Type = ReferenceType.Schema
                                }
                            },
                            ["Cat"] = new OpenApiSchema
                            {
                                Description = "A representation of a cat",
                                AllOf =
                                {
                                    new OpenApiSchema
                                    {
                                        Reference = new OpenApiReference
                                        {
                                            Type = ReferenceType.Schema,
                                            Id = "Pet"
                                        },
                                        // Schema should be dereferenced in our model, so all the properties
                                        // from the Pet above should be propagated here.
                                        Type = "object",
                                        Discriminator = new OpenApiDiscriminator
                                        {
                                            PropertyName = "petType"
                                        },
                                        Properties =
                                        {
                                            ["name"] = new OpenApiSchema
                                            {
                                                Type = "string"
                                            },
                                            ["petType"] = new OpenApiSchema
                                            {
                                                Type = "string"
                                            }
                                        },
                                        Required =
                                        {
                                            "name",
                                            "petType"
                                        }
                                    },
                                    new OpenApiSchema
                                    {
                                        Type = "object",
                                        Required = {"huntingSkill"},
                                        Properties =
                                        {
                                            ["huntingSkill"] = new OpenApiSchema
                                            {
                                                Type = "string",
                                                Description = "The measured skill for hunting",
                                                Enum =
                                                {
                                                    new OpenApiString("clueless"),
                                                    new OpenApiString("lazy"),
                                                    new OpenApiString("adventurous"),
                                                    new OpenApiString("aggressive")
                                                }
                                            }
                                        }
                                    }
                                },
                                Reference = new OpenApiReference()
                                {
                                    Id= "Cat",
                                    Type = ReferenceType.Schema
                                }
                            },
                            ["Dog"] = new OpenApiSchema
                            {
                                Description = "A representation of a dog",
                                AllOf =
                                {
                                    new OpenApiSchema
                                    {
                                        Reference = new OpenApiReference
                                        {
                                            Type = ReferenceType.Schema,
                                            Id = "Pet"
                                        },
                                        // Schema should be dereferenced in our model, so all the properties
                                        // from the Pet above should be propagated here.
                                        Type = "object",
                                        Discriminator = new OpenApiDiscriminator
                                        {
                                            PropertyName = "petType"
                                        },
                                        Properties =
                                        {
                                            ["name"] = new OpenApiSchema
                                            {
                                                Type = "string"
                                            },
                                            ["petType"] = new OpenApiSchema
                                            {
                                                Type = "string"
                                            }
                                        },
                                        Required =
                                        {
                                            "name",
                                            "petType"
                                        }
                                    },
                                    new OpenApiSchema
                                    {
                                        Type = "object",
                                        Required = {"packSize"},
                                        Properties =
                                        {
                                            ["packSize"] = new OpenApiSchema
                                            {
                                                Type = "integer",
                                                Format = "int32",
                                                Description = "the size of the pack the dog is from",
                                                // TODO: Issue #26. This should be parsed as number.
                                                Default = new OpenApiString("0"),
                                                Minimum = 0
                                            }
                                        }
                                    }
                                },
                                Reference = new OpenApiReference()
                                {
                                    Id= "Dog",
                                    Type = ReferenceType.Schema
                                }
                            }
                        }
                    });
            }
        }


        [Fact]
        public void ParseSelfReferencingSchemaShouldNotStackOverflow()
        {
            using (var stream = Resources.GetStream(Path.Combine(SampleFolderPath, "selfReferencingSchema.yaml")))
            {
                // Act
                var openApiDoc = new OpenApiStreamReader().Read(stream, out var diagnostic);

                // Assert
                var components = openApiDoc.Components;

                diagnostic.ShouldBeEquivalentTo(
                    new OpenApiDiagnostic() { SpecificationVersion = OpenApiSpecVersion.OpenApi3_0 });

                var schemaExtension = new OpenApiSchema()
                {
                    AllOf = { new OpenApiSchema()
                    {
                        Title = "schemaExtension",
                        Type = "object",
                        Properties = {
                                        ["description"] = new OpenApiSchema() { Type = "string", Nullable = true},
                                        ["targetTypes"] = new OpenApiSchema() {
                                            Type = "array",
                                            Items = new OpenApiSchema() {
                                                Type = "string"
                                            }
                                        },
                                        ["status"] = new OpenApiSchema() { Type = "string"},
                                        ["owner"] = new OpenApiSchema() { Type = "string"},
                                        ["child"] = null
                                    }
                        }
                    },
                    Reference = new OpenApiReference()
                    {
                        Type = ReferenceType.Schema,
                        Id = "microsoft.graph.schemaExtension"
                    }
                };

                schemaExtension.AllOf[0].Properties["child"] = schemaExtension;

                components.Schemas["microsoft.graph.schemaExtension"].ShouldBeEquivalentTo(components.Schemas["microsoft.graph.schemaExtension"].AllOf[0].Properties["child"]);
            }
        }
    }
}