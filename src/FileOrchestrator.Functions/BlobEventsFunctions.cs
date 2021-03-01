using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FileOrchestrator.Functions
{
	public class BlobEventsFunctions
    {
		private readonly HashSet<string> ignoredFiles;

		public BlobEventsFunctions()
		{
			ignoredFiles = new HashSet<string>(Environment.GetEnvironmentVariable("BlobIgnoreFiles").Split(','));
		}

        [FunctionName(nameof(BlobEventsFunctionsAsync))]
        public async Task BlobEventsFunctionsAsync(
			[BlobTrigger("input/{name}", Connection = "AzureWebJobsStorage")] CloudBlockBlob myBlob,
			string name,
			[DurableClient] IDurableClient client,
			ILogger log
		)
        {
			var nameParts = name.Substring(0, name.LastIndexOf('.')).Split('-');
			var instanceId = nameParts[0];
			var eventName = nameParts[1];

			log.LogInformation("Received event {eventName} for {instanceId}.", eventName, instanceId);

			if (!ignoredFiles.Contains(eventName))
			{
				log.LogInformation("Processing {eventName} for {instanceId}.", eventName, instanceId);
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
			}
			else
			{
				log.LogInformation("Ignoring {eventName} for {instanceId}.", eventName, instanceId);
			}
        }
	}
}
