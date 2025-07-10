using rinha_de_backend_2025_processor_availability;
using static rinha_de_backend_2025_processor_availability.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddStackExchangeRedisCache(options =>
{
	options.Configuration = "localhost:6380, password=senha123";
	options.InstanceName = "Rinha_";
});

builder.Services.AddHttpClient<PaymentProcessorDefaultService>("default", client =>
{
	client.BaseAddress = new Uri("http://localhost:8001/");
});

builder.Services.AddHttpClient<PaymentProcessorFallbackService>("fallback", client =>
{
	client.BaseAddress = new Uri("http://localhost:8002/");
});

var host = builder.Build();
host.Run();
