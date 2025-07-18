using Dapper;
using Npgsql;

namespace rinha_de_backend_2025_server
{
	public interface IPaymentProcessorsService
	{
		public Task<BaseResponse<PaymentResponse>> Payments(PaymentRequest request, CancellationToken cancellationToken);
	}

	public class PaymentProcessorServiceFactory(PaymentProcessorDefaultService defaultProcessor, PaymentProcessorFallbackService fallbackProcessor)
	{
		private readonly PaymentProcessorDefaultService _defaultProcessor = defaultProcessor ?? throw new ArgumentNullException(nameof(defaultProcessor));
		private readonly PaymentProcessorFallbackService _fallbackProcessor = fallbackProcessor ?? throw new ArgumentNullException(nameof(fallbackProcessor));

		public IPaymentProcessorsService GetService(EProcessorService service)
			=> service == EProcessorService.Default ? _defaultProcessor : _fallbackProcessor;
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

	public class ServerRepository
	{
		private readonly string _connectionString = "Server=server-db;Port=5432;User Id=postgres;Password=postgres;Database=rinha;Pooling=true;Minimum Pool Size=10;Maximum Pool Size=200;";

		public async Task Add(EProcessorService processor, decimal amount, CancellationToken cancellationToken)
		{
			using var conn = new NpgsqlConnection(_connectionString);
			try
			{
				await conn.OpenAsync(cancellationToken);

				string query = @$"INSERT INTO payments (processor, amount, requested_at) VALUES (@p0, @p1, @p2)";

				await conn.ExecuteAsync(query, new { p0 = (int)processor, p1 = amount, p2 = DateTime.UtcNow });
			}
			catch (Exception) { throw; }
			finally { conn.Close(); }
		}

		public async Task<IEnumerable<PaymentDTO>> Summary(DateTime? From, DateTime? To, CancellationToken cancellationToken)
		{
			using var conn = new NpgsqlConnection(_connectionString);
			try
			{
				await conn.OpenAsync(cancellationToken);

				string query = @$"SELECT processor, amount FROM payments WHERE 
					(@p0::timestamp IS NULL OR requested_at >= @p0::timestamp) AND 
					(@p1::timestamp IS NULL OR requested_at <= @p1::timestamp)";

				return await conn.QueryAsync<PaymentDTO>(query, new { p0 = From, p1 = To });
			}
			catch (Exception) { throw; }
			finally { conn.Close(); }
		}
	}
}
