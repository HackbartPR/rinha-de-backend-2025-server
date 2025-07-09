namespace rinha_de_backend_2025_server
{
	public interface IPaymentProcessorsServices
	{
		public Task<BaseResponse<HealthCheckResponse>> HealthCheck();

		public Task<BaseResponse<PaymentPostResponse>> MakePayment(PaymentPostRequest request);
	}

	public class PaymentProcessorDefaultService(HttpClient httpClient) : IPaymentProcessorsServices
	{
		private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

		public async Task<BaseResponse<HealthCheckResponse>> HealthCheck()
		{
			HttpResponseMessage response = await _httpClient.GetAsync("/payments/service-health");
			//HealthCheckResponse content = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();

			return new BaseResponse<HealthCheckResponse>
			{
				Data = null,
				StatusCode = response.StatusCode,
			};
		}

		public async Task<BaseResponse<PaymentPostResponse>> MakePayment(PaymentPostRequest request)
		{
			HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/payments", request);
			return new BaseResponse<PaymentPostResponse>
			{
				Data = null,
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
			//HealthCheckResponse content = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();

			return new BaseResponse<HealthCheckResponse>
			{
				Data = null,
				StatusCode = response.StatusCode,
			};
		}

		public async Task<BaseResponse<PaymentPostResponse>> MakePayment(PaymentPostRequest request)
		{
			HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/payments", request);
			return new BaseResponse<PaymentPostResponse>
			{
				Data = null,
				StatusCode = response.StatusCode,
			};
		}
	}
}
