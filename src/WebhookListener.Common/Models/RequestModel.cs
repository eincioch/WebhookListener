using System.Collections.Generic;

namespace WebhookListener.Common.Models
{
	public class RequestModel
	{
		public string Id { get; set; }
		public Dictionary<string, string> Headers { get; set; }
		public Dictionary<string, string> QueryParams { get; set; }
		public object Body { get; set; }
	}
}