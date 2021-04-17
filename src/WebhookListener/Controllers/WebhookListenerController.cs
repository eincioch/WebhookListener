using Microsoft.AspNetCore.Mvc;
using WebhookListener.Services;

namespace WebhookListener.Controllers
{
	[ApiController]
	[Route("webhook")]
	public class WebhookListenerController : ControllerBase
	{
		private readonly WebhookService _webhookService;

		public WebhookListenerController(WebhookService webhookService)
		{
			_webhookService = webhookService;
		}

		[HttpPost]
		[Route("{id}")]
		public IActionResult SendWebhookId([FromRoute] string id)
		{
			_webhookService.SendWebhook(HttpContext.Request, id);
			return Ok();
		}

		[HttpPost]
		[Route("")]
		public IActionResult SendWebhook()
		{
			_webhookService.SendWebhook(HttpContext.Request);
			return Ok();
		}
	}
}