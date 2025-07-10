using System.Net;

namespace rinha_de_backend_2025_processor_availability
{
	public class BaseResponse<TResponse> where TResponse : struct
	{
		public TResponse? Data { get; set; }

		public HttpStatusCode StatusCode { get; set; }
	}

	public struct HealthCheckResponse
	{
		public bool Failing { get; set; }

		public int MinResponseTime { get; set; }
	}
}
