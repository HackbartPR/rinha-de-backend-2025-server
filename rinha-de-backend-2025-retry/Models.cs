using System.Net;

namespace rinha_de_backend_2025_retry
{
	public class BaseResponse<TResponse> where TResponse : struct
	{
		public TResponse? Data { get; set; }

		public HttpStatusCode StatusCode { get; set; }

		public bool IsSuccess { get; set; }
	}

	public struct PaymentRequest
	{
		public Guid CorrelationId { get; set; }

		public decimal Amount { get; set; }
	}

	public struct PaymentResponse
	{
		public Guid CorrelationId { get; set; }

		public decimal Amount { get; set; }

		public DateTime RequestAt { get; set; }
	}
}
