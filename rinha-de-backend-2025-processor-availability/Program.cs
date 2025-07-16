using rinha_de_backend_2025_processor_availability;
using static rinha_de_backend_2025_processor_availability.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddStackExchangeRedisCache(options =>
{
	options.Configuration = Environment.GetEnvironmentVariable("Redis__Connection") ?? string.Empty;
	options.InstanceName = Environment.GetEnvironmentVariable("Redis__Suffix") ?? string.Empty;
});

builder.Services.AddHttpClient<PaymentProcessorDefaultService>("default", client =>
{
	client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("Payments__Default") ?? string.Empty);
});

builder.Services.AddHttpClient<PaymentProcessorFallbackService>("fallback", client =>
{
	client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("Payments__Fallback") ?? string.Empty);
});

var host = builder.Build();
host.Run();
