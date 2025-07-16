using rinha_de_backend_2025_processor_availability;
using static rinha_de_backend_2025_processor_availability.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddStackExchangeRedisCache(options =>
{
	options.Configuration = "redis-server:6379, password=senha123";
	options.InstanceName = "Rinha_";
});

builder.Services.AddHttpClient<PaymentProcessorDefaultService>("default", client =>
{
	client.BaseAddress = new Uri("http://payment-processor-default:8080/");
});

builder.Services.AddHttpClient<PaymentProcessorFallbackService>("fallback", client =>
{
	client.BaseAddress = new Uri("http://payment-processor-fallback:8080/");
});

var host = builder.Build();
host.Run();
