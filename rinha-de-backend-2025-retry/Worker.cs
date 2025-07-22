using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace rinha_de_backend_2025_retry
{
	public class Worker : BackgroundService
	{

		private readonly PaymentProcessorServiceFactory _serviceFactory;
		private readonly ServerRepository _repository;
		private readonly IDistributedCache _cache;
		private readonly IConnectionMultiplexer _connection;
		private readonly string _channelName = "payments";

		public Worker(IConnectionMultiplexer connection, IDistributedCache cache, PaymentProcessorServiceFactory serviceFactory, ServerRepository repository)
		{
			_serviceFactory = serviceFactory;
			_repository = repository;
			_cache = cache;
			_connection = connection;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var subscriber = _connection.GetSubscriber();

			await subscriber.SubscribeAsync(_channelName, async (channel, message) =>
			{
				PaymentRequest? request = JsonSerializer.Deserialize<PaymentRequest?>(message);

				if (request.HasValue)
					return;

				string processor = await _cache.GetStringAsync("processor") ?? "1";
				IPaymentProcessorsService service = _serviceFactory.GetService(processor);

				BaseResponse<PaymentResponse> response = await service.Payments(request.Value, stoppingToken);

				if (!response.IsSuccess)
				{
					await subscriber.PublishAsync(_channelName, message);
					return;
				}

				if (response.IsSuccess)
					await _repository.Add(int.Parse(processor), request.Value.Amount, stoppingToken);
			});
		}
	}
}
