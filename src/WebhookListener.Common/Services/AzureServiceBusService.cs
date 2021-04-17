using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace WebhookListener.Common.Services
{
	public class AzureServiceBusService
	{
		public string ConnectionString { get; set; }

		public AzureServiceBusService(string connectionString)
		{
			this.ConnectionString = connectionString;
		}

		public async Task SendMessageAsync(string queueName, string message)
		{
			var queueClient = new QueueClient(this.ConnectionString, queueName);
			var messageBody = new Message(Encoding.UTF8.GetBytes(message));
			await queueClient.SendAsync(messageBody);
			await queueClient.CloseAsync();
		}

		public void RegisterMessageHandler(string queueName, Func<byte[], CancellationToken, Task> messageHandler)
		{
			var queueClient = new QueueClient(this.ConnectionString, queueName);
			var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
			{
				MaxConcurrentCalls = 1,
				AutoComplete = false
			};

			Func<Message, CancellationToken, Task> processMessage = async (message, cancellationToken) =>
			{
				await queueClient.CompleteAsync(message.SystemProperties.LockToken);
				messageHandler.DynamicInvoke(message.Body, cancellationToken);
			};

			queueClient.RegisterMessageHandler(processMessage, messageHandlerOptions);
		}

		private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
		{
			var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
			return Task.CompletedTask;
		}
	}
}