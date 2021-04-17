using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using WebhookListener.Common.Models;
using WebhookListener.Common.Services;

namespace WebhookListener.Services
{
	public class WebhookService
	{
		private readonly AzureServiceBusService _azureServiceBusService;
		private readonly IConfiguration _configuration;

		public WebhookService(AzureServiceBusService azureServiceBusService, IConfiguration configuration)
		{
			_azureServiceBusService = azureServiceBusService;
			_configuration = configuration;
		}

		internal async void SendWebhook(HttpRequest request, string id = null)
		{
			using StreamReader stream = new StreamReader(request.Body);
			var jsonBody = await stream.ReadToEndAsync();
			object body = string.IsNullOrEmpty(jsonBody) ? null : JsonSerializer.Deserialize<object>(jsonBody);
			Dictionary<string, string> headers = request.Headers.ToDictionary(a => a.Key, a => string.Join(";", a.Value));
			Dictionary<string, string> queryParams = request.Query.ToDictionary(a => a.Key, a => string.Join(";", a.Value));

			var requestModel = new RequestModel
			{
				Id = id,
				Headers = headers,
				QueryParams = queryParams,
				Body = body
			};

			var json = JsonSerializer.Serialize(requestModel);
			await _azureServiceBusService.SendMessageAsync(_configuration.GetValue<string>("ServiceBusQueueName"), json);
		}
	}
}