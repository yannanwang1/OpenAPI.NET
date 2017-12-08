using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.OpenApi.Readers.Tests.ReferenceService
{
    public class ReferenceTests
    {

        [Fact]
        public void ReferencedComponentShouldCreateASingleInstance()
        {
            var input = @"
openapi: 3.0.0
info: 
  title: test a reference
  version: 1.0.0
paths:
  '/':
    get:
      responses:
        200:
          description: Here is a response
          content:
            text/plain:
              schema:
                $ref: '#/components/schemas/thing'
components:
  schemas:
    thing:
      type: string
";
            var reader = new OpenApiStringReader();
            var doc = reader.Read(input, out var diag);

            diag.Errors.Should().BeEmpty();

            var schema = doc.Paths["/"].Operations[Models.OperationType.Get].Responses["200"].Content["text/plain"].Schema;
            schema.Should().BeSameAs(doc.Components.Schemas["thing"]);

        }
    }
}
