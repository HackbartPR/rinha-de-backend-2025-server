using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using rinha_de_backend_2025_server;
using System.Diagnostics;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<PaymentProcessorDefaultService>("default", client =>
{
	client.BaseAddress = new Uri("http://localhost:8001/");
});

builder.Services.AddHttpClient<PaymentProcessorFallbackService>("fallback", client =>
{
	client.BaseAddress = new Uri("http://localhost:8002/");
});

builder.Services.AddStackExchangeRedisCache(options =>
{
	options.Configuration = "localhost:6380, password=senha123";
	options.InstanceName = "Rinha_";
});

builder.Services.AddScoped<PaymentProcessorServiceFactory>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/payments", async (PaymentRequest request, IDistributedCache cache, PaymentProcessorServiceFactory serviceFactory, CancellationToken cancellationToken = default) =>
{
	try
	{
		EProcessorService processor = Enum.Parse<EProcessorService>(await cache.GetStringAsync("processor") ?? EProcessorService.Default.GetDescription());
		IPaymentProcessorsService service = serviceFactory.GetService(processor);

		BaseResponse<PaymentResponse> response = await service.Payments(request, cancellationToken);
		
		if (response.IsSuccess)
		{
			//TODO: Trocar para o banco Postgress
			string summaryString = await cache.GetStringAsync($"summary_processor_{processor}", cancellationToken) ?? string.Empty;
			IndividualPaymentSummary paymentSumary = string.IsNullOrEmpty(summaryString) ? new IndividualPaymentSummary() : JsonSerializer.Deserialize<IndividualPaymentSummary>(summaryString);

			paymentSumary.TotalRequests += 1;
			paymentSumary.TotalAmount += request.Amount;

			summaryString = JsonSerializer.Serialize(paymentSumary);
			await cache.SetStringAsync($"summary_processor_{processor}", summaryString, cancellationToken);
		}

		return Results.StatusCode((int)response.StatusCode);
	}
	catch (Exception ex) { return Results.Problem(ex.Message); }
});

app.MapGet("/payments-summary", async ([FromQuery] DateTime? from, [FromQuery] DateTime? to, IDistributedCache cache, PaymentProcessorServiceFactory serviceFactory, CancellationToken cancellationToken = default) =>
{
	try
	{
		// Utilizar banco Postgress
		string summaryDefaultString = await cache.GetStringAsync($"summary_processor_{EProcessorService.Default}", cancellationToken) ?? string.Empty;
		string summaryFallbackString = await cache.GetStringAsync($"summary_processor_{EProcessorService.Fallback}", cancellationToken) ?? string.Empty;

		IndividualPaymentSummary paymentSumaryDefault = string.IsNullOrEmpty(summaryDefaultString) ? new IndividualPaymentSummary() : JsonSerializer.Deserialize<IndividualPaymentSummary>(summaryDefaultString);
		IndividualPaymentSummary paymentSumaryFallback = string.IsNullOrEmpty(summaryFallbackString) ? new IndividualPaymentSummary() : JsonSerializer.Deserialize<IndividualPaymentSummary>(summaryFallbackString);

		PaymentSummaryResponse response = new()
		{
			Default = paymentSumaryDefault,
			Fallback = paymentSumaryFallback,
		};

		return Results.Ok(response);
	}
	catch (Exception ex) { return Results.Problem(ex.Message); }
});

app.Run();
