using rinha_de_backend_2025_server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<PaymentProcessorDefaultService>("default", client =>
{
	client.BaseAddress = new Uri("http://localhost:8001/");
});

builder.Services.AddHttpClient<PaymentProcessorFallbackService>("fallback", client =>
{
	client.BaseAddress = new Uri("http://localhost:8002/");
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/health", async (PaymentProcessorDefaultService defaultProcessor, PaymentProcessorFallbackService fallbackProcessor) =>
{
	BaseResponse<HealthCheckResponse> defaultResponse = await defaultProcessor.HealthCheck();
	BaseResponse<HealthCheckResponse> fallbackResponse = await fallbackProcessor.HealthCheck();
	return Results.StatusCode((int)defaultResponse.StatusCode);
});

app.MapPost("/payment", async (PaymentPostRequest request, PaymentProcessorDefaultService defaultProcessor, PaymentProcessorFallbackService fallbackProcessor) =>
{
	try
	{

	}
	catch
	{

	}
});

app.Run();
