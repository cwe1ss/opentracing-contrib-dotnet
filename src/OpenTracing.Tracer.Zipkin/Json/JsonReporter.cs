using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenTracing.Tracer.BatchReporter;
using OpenTracing.Tracer.Zipkin.Configuration;

namespace OpenTracing.Tracer.Zipkin.Json
{
    public class JsonReporter : BatchReporterBase, IReporter
    {
        // This header prevents HTTP requests from this reporter to generate new spans.
        private const string PropertyIgnore = "ot-ignore";

        private const string ContentType = "application/json";

        private readonly HttpClient _httpClient;
        private readonly Uri _zipkinUri;

        public JsonReporter(ZipkinTracerOptions zipkinOptions, JsonReporterOptions options)
            : base(options)
        {
            if (string.IsNullOrWhiteSpace(zipkinOptions.ZipkinUri))
                throw new ArgumentNullException($"{nameof(zipkinOptions)}.{nameof(ZipkinTracerOptions.ZipkinUri)}");

            _httpClient = new HttpClient();
            _zipkinUri = new Uri(zipkinOptions.ZipkinUri.TrimEnd('/') + "/api/v1/spans");
        }

        protected override async Task ReportBatchAsync(IReadOnlyCollection<ISpan> spans)
        {
            List<JsonSpan> jsonSpanList = new List<JsonSpan>();
            foreach (var untypedSpan in spans)
            {
                jsonSpanList.Add(new JsonSpan((ZipkinSpan)untypedSpan));
            }

            string jsonString = JsonConvert.SerializeObject(jsonSpanList);

            var request = new HttpRequestMessage(HttpMethod.Post, _zipkinUri);
            request.Content = new StringContent(jsonString, Encoding.UTF8, ContentType);
            request.Properties[PropertyIgnore] = true;

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _httpClient?.Dispose();
            }
        }
    }
}