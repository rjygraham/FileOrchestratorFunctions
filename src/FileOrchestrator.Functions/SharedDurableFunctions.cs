using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace FileOrchestrator.Functions
{
	public class SharedDurableFunctions
	{
		private readonly HashSet<string> requiredEvents;

		public SharedDurableFunctions()
		{
			requiredEvents = new HashSet<string>(Environment.GetEnvironmentVariable("RequiredEvents").Split(','));
		}

		// Main orchestrator for file processing.
		[FunctionName(nameof(FileOrchestrationAsync))]
		public async Task FileOrchestrationAsync(
			[OrchestrationTrigger] IDurableOrchestrationContext context,
			ILogger log)
		{
			// Do not proceed with processing until all 3 files have been received.
			var tasks = new HashSet<Task>();

			foreach (var eventName in requiredEvents)
			{
				tasks.Add(context.WaitForExternalEvent(eventName));
			}

			await Task.WhenAll(tasks);

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
			log.LogInformation($"Beginning job for {input.InstanceId}!");
		}
	}
}
