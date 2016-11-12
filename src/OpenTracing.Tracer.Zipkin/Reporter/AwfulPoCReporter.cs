using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenTracing.Tracer.BatchReporter;
using OpenTracing.Tracer.Zipkin.Json;

namespace OpenTracing.Tracer.Zipkin.Reporter
{
    /// <summary>
    /// A very awful reporter that sends spans immediately to the default Zipkin port on the local machine using JSON.
    /// You better not use it in production. :)
    /// </summary>
    public class AwfulPoCReporter : BatchReporterBase
    {
        private readonly HttpClient _httpClient;

        public AwfulPoCReporter(BatchReporterOptions options)
            : base(options)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:9411/");
        }

        protected override async Task ReportBatchAsync(IReadOnlyCollection<ISpan> spans)
        {
            List<JsonSpan> jsonSpanList = new List<JsonSpan>();
            foreach (var untypedSpan in spans)
            {
                jsonSpanList.Add(new JsonSpan((ZipkinSpan)untypedSpan));
            }

            string jsonString = JsonConvert.SerializeObject(jsonSpanList);

            var response = await _httpClient.PostAsync("api/v1/spans", new StringContent(jsonString));
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