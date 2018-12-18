using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace V2FunctionsLoadTest
{
    public static class StartWorkflow
    {
        private const string TestUrl = "https://xtension01.azurewebsites.net/api/Increment01?input=1";
        private const int numParallelExecutions = 100;
        [FunctionName("StartWorkflow")]
        public static async Task<Dictionary<string, object>> Run([OrchestrationTrigger] DurableOrchestrationContext context, ILogger logger)
        {
            var result = new Dictionary<string, object>();

            var eventTelemetry = new EventTelemetry("Workflow Execution");
            eventTelemetry.Context.Operation.Id = context.InstanceId;

            var properties = eventTelemetry.Properties;

            properties.Add("InstanceId", context.InstanceId);
            properties.Add("IsReplay", context.IsReplaying.ToString());
            properties.Add("Status", "Pending");

            ApplicationInsights.CurrentClient.TrackEvent(eventTelemetry);

            try
            {
                var input = new Dictionary<string, object>
                {
                    {"Url", TestUrl},
                    {"RequestType", "GET"}
                };

                var parallelExecutions = new List<Task<Dictionary<string, object>>>();
                for (var i = 0; i < numParallelExecutions; i++)
                {
                    parallelExecutions.Add(context.CallActivityAsync<Dictionary<string, object>>("RestActivity", input));
                }

                await Task.WhenAll(parallelExecutions);

                for (var i = 0; i < numParallelExecutions; i++)
                {
                    var results = parallelExecutions[i].Result;
                    result.Add(i.ToString(), results["ResponseStatusCode"]);
                }

                context.SetCustomStatus("Success");

                properties["Status"] = "Completed";
                properties.Add("Output", JsonConvert.SerializeObject(result));
            }
            catch (Exception ex)
            {
                logger.LogError($"{ex.Message}. {ex.StackTrace}");
                context.SetCustomStatus("Fail");

                properties["Error"] = $"Error: {ex.Message}. {ex.StackTrace}";
                properties["Status"] = "Fail";
            }

            ApplicationInsights.CurrentClient.TrackEvent(eventTelemetry);

            return result;
        }
    }
}
