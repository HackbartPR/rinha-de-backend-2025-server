using Npgsql;
using StackExchange.Redis;
using System.Net.Http.Json;

namespace rinha_de_backend_2025_retry
{
	public interface IPaymentProcessorsService
	{
		public Task<BaseResponse<PaymentResponse>> Payments(PaymentRequest request, CancellationToken cancellationToken);
	}

	public class PaymentProcessorServiceFactory(PaymentProcessorDefaultService defaultProcessor, PaymentProcessorFallbackService fallbackProcessor)
	{
		private readonly PaymentProcessorDefaultService _defaultProcessor = defaultProcessor ?? throw new ArgumentNullException(nameof(defaultProcessor));
		private readonly PaymentProcessorFallbackService _fallbackProcessor = fallbackProcessor ?? throw new ArgumentNullException(nameof(fallbackProcessor));

        public IPaymentProcessorsService GetService(string service)
            => service == "1" ? _defaultProcessor : _fallbackProcessor;
    }

	public class PaymentProcessorDefaultService(HttpClient httpClient) : IPaymentProcessorsService
	{
		private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

		public async Task<BaseResponse<PaymentResponse>> Payments(PaymentRequest request, CancellationToken cancellationToken)
		{
			HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/payments", request, cancellationToken);
			return new BaseResponse<PaymentResponse>
			{
				Data = null,
				StatusCode = response.StatusCode,
				IsSuccess = response.IsSuccessStatusCode
			};
		}
	}

	public class PaymentProcessorFallbackService(HttpClient httpClient) : IPaymentProcessorsService
	{
		private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

		public async Task<BaseResponse<PaymentResponse>> Payments(PaymentRequest request, CancellationToken cancellationToken)
		{
			HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/payments", request, cancellationToken);
			return new BaseResponse<PaymentResponse>
			{
				Data = null,
				StatusCode = response.StatusCode,
				IsSuccess = response.IsSuccessStatusCode
			};
		}
	}

	public class ServerRepository (NpgsqlDataSource dataSource)
	{
		private readonly NpgsqlDataSource _dataSource = dataSource;

		public async Task Add(int processor, decimal amount, CancellationToken cancellationToken)
		{
			await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
			var cmd = new NpgsqlCommand("INSERT INTO payments (processor, amount, requested_at) VALUES (@p0, @p1, @p2)", conn);
			
			cmd.Parameters.AddWithValue("p0", processor);
			cmd.Parameters.AddWithValue("p1", amount);
			cmd.Parameters.AddWithValue("p2", DateTime.UtcNow);
			cmd.Prepare();
			
			await cmd.ExecuteNonQueryAsync(cancellationToken);
		}
	}
}
