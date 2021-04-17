using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RestSharp;
using WebhookListener.Common.Models;
using WebhookListener.Common.Services;

namespace WebhookListener.Console.Services
{
	public class WebhookService
	{
		private readonly IConfiguration _configuration;
		private readonly AzureServiceBusService _azureServiceBusService;
		private readonly string _queueName;
		private readonly string _webhookEnpoint;
		private readonly RestClient _restClient;

		public WebhookService(IConfiguration configuration, AzureServiceBusService azureServiceBusService)
		{
			_configuration = configuration;
			_azureServiceBusService = azureServiceBusService;
			_queueName = _configuration["ServiceBusQueueName"];
			_webhookEnpoint = _configuration["WebhookEndpoint"];
			_restClient = new RestClient(_webhookEnpoint);
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
		}

		internal void Run()
		{
			_azureServiceBusService.RegisterMessageHandler(_queueName, MessageHandler);
			System.Console.WriteLine($"[INFO] Service Bus Queue initialized. Listening '{_queueName}'");
		}

		private async Task MessageHandler(byte[] message, CancellationToken cancellationToken)
		{
			System.Console.WriteLine();
			System.Console.WriteLine($"[INFO] New message arrived");
			var messageStr = Encoding.UTF8.GetString(message);
			//System.Console.WriteLine(messageStr);
			var request = JsonSerializer.Deserialize<RequestModel>(messageStr);
			await this.SendRequestAsync(request);
		}

		private async Task SendRequestAsync(RequestModel request)
		{
			request.Headers.Remove("Host");
			var resource = string.IsNullOrEmpty(request.Id) ? string.Empty : $"/{request.Id}";
			var restRequest = new RestRequest(resource, Method.POST);
			restRequest.RequestFormat = DataFormat.Json;
			restRequest.AddHeaders(request.Headers);
			restRequest.AddParameter("application/json", request.Body, ParameterType.RequestBody);
			foreach (var parameter in request.QueryParams)
			{
				restRequest.AddQueryParameter(parameter.Key, parameter.Value);
			}

			var response = await _restClient.ExecuteAsync(restRequest);
			System.Console.WriteLine(response.ResponseStatus);
		}
	}
}