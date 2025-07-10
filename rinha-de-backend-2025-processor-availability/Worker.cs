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
			BaseResponse<HealthCheckResponse> defaultResponse = await _defaultProcessor.HealthCheck();
			BaseResponse<HealthCheckResponse> fallbackResponse = await _fallbackProcessor.HealthCheck();

			var teste = await _cache.GetStringAsync("processor");
			if (defaultResponse.StatusCode != HttpStatusCode.OK && defaultResponse.StatusCode != HttpStatusCode.TooManyRequests)
				await _cache.SetStringAsync("processor", "2", stoppingToken);
			else if (fallbackResponse.StatusCode != HttpStatusCode.OK && fallbackResponse.StatusCode != HttpStatusCode.TooManyRequests)
				await _cache.SetStringAsync("processor", "1", stoppingToken);
			else if (defaultResponse.Data.HasValue && defaultResponse.Data.Value.Failing)
				await _cache.SetStringAsync("processor", "2", stoppingToken);
			else if (fallbackResponse.Data.HasValue && fallbackResponse.Data.Value.Failing)
				await _cache.SetStringAsync("processor", "1", stoppingToken);
			else if (defaultResponse.Data.HasValue && fallbackResponse.Data.HasValue && defaultResponse.Data.Value.MinResponseTime > (fallbackResponse.Data.Value.MinResponseTime * 2))
				await _cache.SetStringAsync("processor", "2", stoppingToken);
			else
				await _cache.SetStringAsync("processor", "1", stoppingToken);

			await Task.Delay(5000, stoppingToken);
		}
	}
}
