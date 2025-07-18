using Microsoft.Extensions.Caching.Distributed;
using System.Net;
using static rinha_de_backend_2025_processor_availability.Services;

namespace rinha_de_backend_2025_processor_availability
{
	public class Worker : BackgroundService
	{
		private readonly ILogger<Worker> _logger;
		private readonly IDistributedCache _cache;
		private readonly PaymentProcessorDefaultService _defaultProcessor;
		private readonly PaymentProcessorFallbackService _fallbackProcessor;

		public Worker(ILogger<Worker> logger, IDistributedCache cache, PaymentProcessorDefaultService defaultProcessor, PaymentProcessorFallbackService fallbackProcessor)
		{
			_logger = logger;
			_cache = cache;
			_defaultProcessor = defaultProcessor;
			_fallbackProcessor = fallbackProcessor;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				var defaultResponse = await _defaultProcessor.HealthCheck();
				var fallbackResponse = await _fallbackProcessor.HealthCheck();

				string selectedProcessor;

				bool IsUnhealthy(BaseResponse<HealthCheckResponse> response) =>
					response.StatusCode != HttpStatusCode.OK &&
					response.StatusCode != HttpStatusCode.TooManyRequests;

				bool IsFailing(BaseResponse<HealthCheckResponse> response) =>
					response.Data.HasValue && response.Data.Value.Failing;

				bool IsMuchSlower(BaseResponse<HealthCheckResponse> primary, BaseResponse<HealthCheckResponse> secondary) =>
					primary.Data.HasValue && secondary.Data.HasValue &&
					primary.Data.Value.MinResponseTime > 100 &&
					primary.Data.Value.MinResponseTime > (secondary.Data.Value.MinResponseTime * 2);

				if (IsUnhealthy(defaultResponse))
					selectedProcessor = EProcessorService.Fallback.GetDescription();
				else if (IsUnhealthy(fallbackResponse))
					selectedProcessor = EProcessorService.Default.GetDescription();
				else if (IsFailing(defaultResponse))
					selectedProcessor = EProcessorService.Fallback.GetDescription();
				else if (IsFailing(fallbackResponse))
					selectedProcessor = EProcessorService.Default.GetDescription();
				else if (IsMuchSlower(defaultResponse, fallbackResponse))
					selectedProcessor = EProcessorService.Fallback.GetDescription();
				else
					selectedProcessor = EProcessorService.Default.GetDescription();

				await _cache.SetStringAsync("processor", selectedProcessor, stoppingToken);
				await Task.Delay(5000, stoppingToken);
			}
		}
	}
}
