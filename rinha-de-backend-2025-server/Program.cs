using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using rinha_de_backend_2025_server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<PaymentProcessorDefaultService>("default", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Payments:Default"] ?? string.Empty);
});

builder.Services.AddHttpClient<PaymentProcessorFallbackService>("fallback", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Payments:Fallback"] ?? string.Empty);
});

builder.Services.AddStackExchangeRedisCache(options =>
{
	options.Configuration = builder.Configuration["Redis:Connection"] ?? string.Empty;
	options.InstanceName = builder.Configuration["Redis:Suffix"] ?? string.Empty;
});

builder.Services.AddScoped<PaymentProcessorServiceFactory>();
builder.Services.AddScoped<ServerRepository>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/payments", async (PaymentRequest request, IDistributedCache cache, PaymentProcessorServiceFactory serviceFactory,  ServerRepository repository, CancellationToken cancellationToken = default) =>
{
	try
	{
		EProcessorService processor = Enum.Parse<EProcessorService>(await cache.GetStringAsync("processor") ?? EProcessorService.Default.GetDescription());
		IPaymentProcessorsService service = serviceFactory.GetService(processor);

		BaseResponse<PaymentResponse> response = await service.Payments(request, cancellationToken);
		
		if (response.IsSuccess)
			await repository.Add(processor, request.Amount, cancellationToken);
		else
		{
			EProcessorService anotherProcessor = processor == EProcessorService.Default ? EProcessorService.Fallback : EProcessorService.Default;
			IPaymentProcessorsService anotherService = serviceFactory.GetService(anotherProcessor);
			BaseResponse<PaymentResponse> anotherResponse = await anotherService.Payments(request, cancellationToken);

			if (response.IsSuccess)
				await repository.Add(anotherProcessor, request.Amount, cancellationToken);
		}

		return Results.StatusCode((int)response.StatusCode);
	}
	catch (Exception ex) { return Results.Problem(); }
});

app.MapGet("/payments-summary", async ([FromQuery] DateTime? from, [FromQuery] DateTime? to, ServerRepository repository, CancellationToken cancellationToken = default) =>
{
	try
	{
		IEnumerable<PaymentDTO> payments = await repository.Summary(from, to, cancellationToken);
		IEnumerable<PaymentDTO> paymentsDefault = payments.Where(p => p.Processor == EProcessorService.Default);
        IEnumerable<PaymentDTO> paymentsFallback = payments.Where(p => p.Processor == EProcessorService.Fallback);

        PaymentSummaryResponse response = new()
		{
			Default = new IndividualPaymentSummary()
			{
				TotalAmount = paymentsDefault.Sum(p => p.Amount),
				TotalRequests = paymentsDefault.Count(),
			},

			Fallback = new IndividualPaymentSummary()
			{
				TotalAmount = paymentsFallback.Sum(p => p.Amount),
				TotalRequests = paymentsFallback.Count(),
			}
		};

		return Results.Ok(response);
	}
	catch (Exception ex) { return Results.Problem(); }
});

app.Run();
