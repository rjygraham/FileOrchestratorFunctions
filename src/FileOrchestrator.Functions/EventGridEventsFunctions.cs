// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileOrchestrator.Functions
{
    public class EventGridEventsFunctions
    {
		private readonly HashSet<string> ignoredFiles;

		public EventGridEventsFunctions()
		{
			ignoredFiles = new HashSet<string>(Environment.GetEnvironmentVariable("BlobIgnoreFiles").Split(','));
		}

        [FunctionName(nameof(HandleEventGridEventsFunctionsAsync))]
        public async Task HandleEventGridEventsFunctionsAsync([EventGridTrigger]EventGridEvent eventGridEvent,
		[DurableClient] IDurableClient client,
			ILogger log)
        {
			var startIndex = eventGridEvent.Subject.LastIndexOf('/') + 1;
			var length = eventGridEvent.Subject.LastIndexOf('.') - startIndex;
			var name = eventGridEvent.Subject.Substring(startIndex, length);
			var nameParts = name.Split('-');
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
