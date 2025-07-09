using System.Net;
using System.Text.Json.Serialization;

namespace rinha_de_backend_2025_server
{
	public class BaseResponse<TResponse> where TResponse : struct
	{
		public TResponse? Data { get; set; }

		public HttpStatusCode StatusCode { get; set; }
	}

	public struct PaymentPostRequest
	{
		//[JsonPropertyName("correlationId")]
		public Guid CorrelationId { get; set; }

		//[JsonPropertyName("amount")]
		public decimal Amount { get; set; }
	}

	public struct PaymentPostResponse
	{
		//[JsonPropertyName("correlationId")]
		public Guid CorrelationId { get; set; }

		//[JsonPropertyName("amount")]
		public decimal Amount { get; set; }

		public DateTime RequestAt { get; set; }
	}

	public struct HealthCheckResponse
	{
		public bool Failing { get; set; }

		public int MinResponseTime { get; set; }
	}
}
