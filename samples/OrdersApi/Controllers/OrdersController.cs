using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Constants;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenTracing.Contrib;

namespace Samples.OrdersApi.Controllers
{
    [Route("orders")]
    public class OrdersController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ISpanAccessor _spanAccessor;

        public OrdersController(HttpClient httpClient, ISpanAccessor spanAccessor)
        {
            if (httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));

            if (spanAccessor == null)
                throw new ArgumentNullException(nameof(spanAccessor));

            _httpClient = httpClient;
            _spanAccessor = spanAccessor;
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] PlaceOrderCommand cmd)
        {
            var customer = await GetCustomer(cmd.CustomerId.Value);

            var span = _spanAccessor.CurrentSpan;

            span?.Log(new Dictionary<string, object> {
                { "event", "OrderPlaced" },
                { "customer", cmd.CustomerId },
                { "customer_name", customer.Name },
                { "item_number", cmd.ItemNumber },
                { "quantity", cmd.Quantity }
            });

            return Ok();
        }

        private async Task<Customer> GetCustomer(int customerId)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Constants.CustomersUrl + "customers/" + customerId)
            };

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Customer>(body);
        }
    }
}