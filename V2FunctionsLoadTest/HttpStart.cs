using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace V2FunctionsLoadTest
{
    public static class HttpStart
    {
        [FunctionName("HttpStart")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, methods: "post", Route = "Start/{functionName=StartWorkflow}/{instances=600}")] HttpRequest req,
            [OrchestrationClient] DurableOrchestrationClientBase starter,
            string functionName, int instances,
            ILogger log)
        {
            List<Task<string>> tasks = new List<Task<string>>();
            for (var i = 0; i < instances; i++)
            {
                tasks.Add(starter.StartNewAsync(functionName, null));
            }

            return new OkObjectResult(await Task.WhenAll(tasks));
        }
    }
}
