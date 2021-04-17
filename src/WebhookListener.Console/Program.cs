using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebhookListener.Common.Services;
using WebhookListener.Console.Services;

namespace WebhookListener.Console
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var serviceProvider = RegisterServices();
			using (var scope = serviceProvider.CreateScope())
			{
				var webhookService = scope.ServiceProvider.GetRequiredService<WebhookService>();
				webhookService.Run();
				System.Console.ReadLine();
			}

			System.Console.WriteLine("FINISH");
		}

		private static ServiceProvider RegisterServices()
		{
			var basePath = Directory.GetCurrentDirectory();
			IConfiguration config = new ConfigurationBuilder().SetBasePath(basePath).AddJsonFile("appsettings.json", false, true).Build();

			var services = new ServiceCollection();
			services.AddSingleton(config);
			services.AddSingleton(x => new AzureServiceBusService(config.GetConnectionString("AzureServiceBus")));
			services.AddScoped<WebhookService>();

			return services.BuildServiceProvider(true);
		}
	}
}