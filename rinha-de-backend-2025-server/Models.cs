using System.ComponentModel;
using System.Net;
using System.Reflection;

namespace rinha_de_backend_2025_server
{
	public class BaseResponse<TResponse> where TResponse : struct
	{
		public TResponse? Data { get; set; }

		public HttpStatusCode StatusCode { get; set; }

		public bool IsSuccess { get; set; }
	}

	public struct PaymentRequest
	{
		public Guid CorrelationId { get; set; }

		public decimal Amount { get; set; }
	}

	public struct PaymentResponse
	{
		public Guid CorrelationId { get; set; }

		public decimal Amount { get; set; }

		public DateTime RequestAt { get; set; }
	}

	public struct PaymentSummaryResponse
	{
		public IndividualPaymentSummary Default { get; set; }

		public IndividualPaymentSummary Fallback { get; set; }
	}

	public struct IndividualPaymentSummary
	{
		public int TotalRequests { get; set; }

		public decimal TotalAmount { get; set; }
	}

	public enum EProcessorService
	{
		[Description("1")]
		Default = 1,

		[Description("2")]
		Fallback = 2
	}

	public static class EnumExtensions
	{
		public static string GetDescription(this Enum value)
		{
			var type = value.GetType();
			var name = Enum.GetName(type, value);

			if (name == null)
				return value.ToString();

			var field = type.GetField(name);
			var attr = field?.GetCustomAttribute<DescriptionAttribute>();

			return attr?.Description ?? value.ToString();
		}
	}
}
