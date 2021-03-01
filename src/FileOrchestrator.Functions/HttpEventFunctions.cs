using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace FileOrchestrator.Functions
{
	public class HttpEventFunctions
    {
        [FunctionName(nameof(HandleHttpEventAsync))]
        public async Task<IActionResult> HandleHttpEventAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "files/{instanceId}/{eventName}")] HttpRequest req,
			string instanceId,
            string eventName, 
            [DurableClient] IDurableClient client,
            ILogger log)
        {
			log.LogInformation("Received event {eventName} for {instanceId}.", eventName, instanceId);

			var status = await client.GetStatusAsync(instanceId);
            
            if (status == null)
            {
				log.LogInformation("Event {eventName} is the first event received for {instanceId}. Starting new orchestration.", eventName, instanceId);
				await client.StartNewAsync(nameof(SharedDurableFunctions.FileOrchestrationAsync), instanceId);
				status = await client.GetStatusAsync(instanceId);
			}

			if (status.RuntimeStatus == OrchestrationRuntimeStatus.Pending || status.RuntimeStatus == OrchestrationRuntimeStatus.Running)
			{
				log.LogInformation("Raising {eventName} for {instanceId}.", eventName, instanceId);
				await client.RaiseEventAsync(instanceId, eventName);
			}

            return new OkObjectResult(instanceId);
		}
    }
}
