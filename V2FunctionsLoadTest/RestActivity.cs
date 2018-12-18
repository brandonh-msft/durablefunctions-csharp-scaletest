using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace V2FunctionsLoadTest
{
    public class RestActivity
    {
        private static readonly HttpClient Client = new HttpClient();

        [FunctionName("RestActivity")]
        public static async Task<Dictionary<string, object>> Execute([ActivityTrigger] Dictionary<string, object> request, ILogger logger)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(new HttpMethod((string)request["RequestType"]), (string)request["Url"]);

            if (request.ContainsKey("RequestHeaders") && request["RequestHeaders"].ToString() != "")
            {
                foreach (var header in (Dictionary<string, string>)request["RequestHeaders"])
                {
                    requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            var response = await Client.SendAsync(requestMessage);

            var responseBody = await response.Content.ReadAsByteArrayAsync();
            var responseBodyString = Encoding.UTF8.GetString(responseBody);

            Dictionary<string, object> output = new Dictionary<string, object>
            {
                {"ResponseContent", responseBodyString},
                {
                    "ResponseHeaders",
                    response.Headers.ToDictionary(header => header.Key, header => string.Join(",", header.Value))
                },
                {"ResponseStatusCode", (int) response.StatusCode}
            };

            return output;
        }
    }
}
