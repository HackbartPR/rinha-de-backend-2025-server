using Microsoft.Extensions.Configuration;
using rinha_de_backend_2025_retry;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddHttpClient<PaymentProcessorDefaultService>("default", client =>
{
	client.BaseAddress = new Uri(builder.Configuration["Payments:Default"] ?? string.Empty);
});

builder.Services.AddHttpClient<PaymentProcessorFallbackService>("fallback", client =>
{
	client.BaseAddress = new Uri(builder.Configuration["Payments:Fallback"] ?? string.Empty);
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(new ConfigurationOptions
{
	EndPoints = { builder.Configuration["Redis:Connection"] ?? string.Empty },
	AbortOnConnectFail = false,
}));

builder.Services.AddStackExchangeRedisCache(options =>
{
	options.Configuration = builder.Configuration["Redis:Connection"] ?? string.Empty;
	options.InstanceName = builder.Configuration["Redis:Suffix"] ?? string.Empty;
});

builder.Services.AddScoped<PaymentProcessorServiceFactory>();
builder.Services.AddScoped<ServerRepository>();

var host = builder.Build();
host.Run();
