using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.Readers.ReferenceServices
{
    internal class ExternalDocumentLoader : IStreamLoader
    {
        private HttpClient httpClient = new HttpClient();

        async Task<ExternalDocument> IStreamLoader.Load(string pointer)
        {
            var uri = new Uri(pointer, UriKind.RelativeOrAbsolute);
            if (!uri.IsAbsoluteUri && File.Exists(uri.OriginalString))
            {
                return new ExternalDocument(
                    new FileStream(uri.OriginalString, FileMode.Open)
                );
            }
            var response = await httpClient.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                return new ExternalDocument(await response.Content.ReadAsStreamAsync());
            } else
            {
                return null;  // Any failure should return null
            }
        }
    }

    public class ExternalDocument
    {
        public ExternalDocument(Stream stream)
        {
            this.Stream = stream;
            this.IsFragment = true;
        }

        public Stream Stream { get; private set; }
        bool IsFragment { get; }
        public OpenApiDocument OpenApiDocument { get;  }
    }
}
