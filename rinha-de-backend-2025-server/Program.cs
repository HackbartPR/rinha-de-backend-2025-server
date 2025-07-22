using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Npgsql;
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

builder.Services.AddSingleton<NpgsqlDataSource>(sp =>
{
	var npgsqlbuilder = new NpgsqlDataSourceBuilder(builder.Configuration["Banco:ConnectionString"] ?? string.Empty);
	return npgsqlbuilder.Build();
});

builder.Services.AddScoped<PaymentProcessorServiceFactory>();
builder.Services.AddScoped<ServerRepository>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/payments", async (PaymentRequest request, IDistributedCache cache, PaymentProcessorServiceFactory serviceFactory, ServerRepository repository, CancellationToken cancellationToken = default) =>
{
	try
	{
		string processor = await cache.GetStringAsync("processor") ?? "1";
        IPaymentProcessorsService service = serviceFactory.GetService(processor);

        BaseResponse<PaymentResponse> response = await service.Payments(request, cancellationToken);

        if (!response.IsSuccess)
        {
            processor = processor == "1" ? "2" : "1";
            IPaymentProcessorsService anotherService = serviceFactory.GetService(processor);
            response = await anotherService.Payments(request, cancellationToken);
        }

        if (response.IsSuccess)
            await repository.Add(int.Parse(processor), request.Amount, cancellationToken);

        return Results.StatusCode((int)response.StatusCode);
	}
	catch (Exception ex) { return Results.Problem(); }
});

app.MapGet("/payments-summary", async ([FromQuery] DateTime? from, [FromQuery] DateTime? to, ServerRepository repository, CancellationToken cancellationToken = default) =>
{
	try
	{
		IEnumerable<PaymentDTO> payments = await repository.Summary(from, to, cancellationToken);
		IEnumerable<PaymentDTO> paymentsDefault = payments.Where(p => p.Processor == 1);
        IEnumerable<PaymentDTO> paymentsFallback = payments.Where(p => p.Processor == 2);

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
