using System.Net;
using System.Net.Http.Json;

namespace rinha_de_backend_2025_processor_availability
{
	public class Services
	{
		public interface IPaymentProcessorsServices
		{
			public Task<BaseResponse<HealthCheckResponse>> HealthCheck();
		}

		public class PaymentProcessorDefaultService(HttpClient httpClient) : IPaymentProcessorsServices
		{
			private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

			public async Task<BaseResponse<HealthCheckResponse>> HealthCheck()
			{
				HttpResponseMessage response = await _httpClient.GetAsync("/payments/service-health");

				HealthCheckResponse? data = null;
				if (response.IsSuccessStatusCode)
					data = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();

				return new BaseResponse<HealthCheckResponse>
				{
					Data = data,
					StatusCode = response.StatusCode,
				};
			}
		}

		public class PaymentProcessorFallbackService(HttpClient httpClient) : IPaymentProcessorsServices
		{
			private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

			public async Task<BaseResponse<HealthCheckResponse>> HealthCheck()
			{
				HttpResponseMessage response = await _httpClient.GetAsync("/payments/service-health");

				HealthCheckResponse? data = null;
				if (response.IsSuccessStatusCode)
					data = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();

				return new BaseResponse<HealthCheckResponse>
				{
					Data = data,
					StatusCode = response.StatusCode,
				};
			}
		}
	}
}
