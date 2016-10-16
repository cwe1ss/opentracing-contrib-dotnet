using System;
using System.Net.Http;

namespace OpenTracing.Contrib.Http
{
    /// <summary>
    /// <para>The default for outgoing HTTP client operation names.</para>
    /// <para>Will use "sales/invoices" for "http://www.example.com/sales/invoices?id=1234".</para>
    /// </summary>
    public class DefaultOutgoingHttpOperationName : IOutgoingHttpOperationName
    {
        public string GetOperationName(HttpRequestMessage request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return request.RequestUri.AbsolutePath.TrimStart('/');
        }
    }
}