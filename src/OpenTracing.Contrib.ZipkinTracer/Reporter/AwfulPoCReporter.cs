using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using OpenTracing.Contrib.ZipkinTracer.Json;

namespace OpenTracing.Contrib.ZipkinTracer.Reporter
{
    /// <summary>
    /// A very awful reporter that sends spans immediately to the default Zipkin port on the local machine using JSON.
    /// You better not use it in production. :)
    /// </summary>
    public class AwfulPoCReporter : ISpanReporter
    {
        private readonly HttpClient _httpClient;

        public AwfulPoCReporter()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:9411/");
        }

        public void ReportSpan(ZipkinSpan span)
        {
            if (span == null)
                throw new ArgumentNullException(nameof(span));

            List<JsonSpan> jsonSpanList = new List<JsonSpan>();
            jsonSpanList.Add(new JsonSpan(span));

            string jsonString = JsonConvert.SerializeObject(jsonSpanList);

            var response = _httpClient.PostAsync("api/v1/spans", new StringContent(jsonString)).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }
    }
}