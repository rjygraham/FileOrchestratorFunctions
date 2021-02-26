using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace FileOrchestrator.Functions
{
	public class HttpTestFunctions
    {
        // This example function handles HTTP requests, but could just as easily handle
        // events from blobs created in a Storage Account container.
        [FunctionName(nameof(HandledHttpEventAsync))]
        public async Task<IActionResult> HandledHttpEventAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "files/{name}")] HttpRequest req,
            string name, 
            [DurableClient] IDurableClient client,
            ILogger log)
        {
            // For this demo, file names are in the format of:
            // YYYYMMDD##-a, YYYYMMDD##-b, YYYYMMDD##-c
            // Split the file name and treat the numeric portion as the Durable Functions Instance ID.
            var fileNameParts = name.Split('-');

            var status = await client.GetStatusAsync(fileNameParts[0]);
            var instanceId = status?.InstanceId ?? fileNameParts[0];
            
            if (status == null)
            {
                await client.StartNewAsync(nameof(FileOrchestrationAsync), instanceId);
				status = await client.GetStatusAsync(fileNameParts[0]);
			}

			if (status.RuntimeStatus == OrchestrationRuntimeStatus.Pending || status.RuntimeStatus == OrchestrationRuntimeStatus.Running)
			{
				await client.RaiseEventAsync(instanceId, fileNameParts[1]);
			}

            return new OkObjectResult(instanceId);
		}

        // Main orchestrator for file processing.
        [FunctionName(nameof(FileOrchestrationAsync))]
        public async Task FileOrchestrationAsync(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            // Do not proceed with processing until all 3 files have been received.
            var aFile = context.WaitForExternalEvent("a");
            var bFile = context.WaitForExternalEvent("b");
            var cFile = context.WaitForExternalEvent("c");
            await Task.WhenAll(aFile, bFile, cFile);

            // Start processing once all files have been received.
            await context.CallActivityAsync(nameof(ProcessFileSetAsync), null);
        }

        // Function is called upon all files having been received.
        [FunctionName(nameof(ProcessFileSetAsync))]
        public async Task ProcessFileSetAsync(
            [ActivityTrigger] IDurableActivityContext input,
            ILogger log
        )
        {
            log.LogInformation($"Beginning to process job for {input.InstanceId}!");
		}
    }
}
