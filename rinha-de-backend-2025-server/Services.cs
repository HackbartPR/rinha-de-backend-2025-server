using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

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
		private readonly string _connectionString = "";

		public async Task Add(EProcessorService processor, decimal amount, CancellationToken cancellationToken)
		{
			using var conn = new SqlConnection(_connectionString);
			try
			{
				await conn.OpenAsync(cancellationToken);

				string query = @$"INSERT INTO payments (processor, amount, requested_at) 
					VALUES ({(int)processor}, {amount}, {DateTime.UtcNow})";

				await conn.ExecuteAsync(query, cancellationToken);
			}
			catch (Exception) { throw; }
			finally { conn.Close(); }
		}

		public async Task Summary(DateTime? From, DateTime? To, CancellationToken cancellationToken)
		{
			using var conn = new SqlConnection(_connectionString);
			try
			{
				await conn.OpenAsync(cancellationToken);

				string query = @$"SELECT processor, amount FROM payments
					WHERE ({From} IS NULL OR requested_at >= '{From}') AND ({To} IS NULL OR requested_at <= '{To}')";

				await conn.ExecuteAsync(query, cancellationToken);
			}
			catch (Exception) { throw; }
			finally { conn.Close(); }
		}
	}
}
