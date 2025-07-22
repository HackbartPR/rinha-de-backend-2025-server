using rinha_de_backend_2025_processor_availability;
using static rinha_de_backend_2025_processor_availability.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddStackExchangeRedisCache(options =>
{
	options.Configuration = builder.Configuration["Redis:Connection"] ?? string.Empty;
	options.InstanceName = builder.Configuration["Redis:Suffix"] ?? string.Empty;
});

builder.Services.AddHttpClient<PaymentProcessorDefaultService>("default", client =>
{
	client.BaseAddress = new Uri(builder.Configuration["Payments:Default"] ?? string.Empty);
});

builder.Services.AddHttpClient<PaymentProcessorFallbackService>("fallback", client =>
{
	client.BaseAddress = new Uri(builder.Configuration["Payments:Fallback"] ?? string.Empty);
});

var host = builder.Build();
host.Run();
