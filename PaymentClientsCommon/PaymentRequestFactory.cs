using System;
using Microsoft.Extensions.Configuration;
using PaymentGateway.Services;

namespace PaymentClientsCommon
{
	/// <summary>
	/// PaymentRequestFactory, generates random transaction for testing purposes
	/// </summary>
	public class PaymentRequestFactory
	{
		private IConfiguration _config;

		public PaymentRequestFactory(IConfiguration config)
		{
			_config = config;
		}

		public PaymentRequest Get()
		{
			var random = new Random();

			return new PaymentRequest()
			{
				Merchant = new Merchant()
				{
					MerchantName = _config["Service:MerchantName"],
					TransactionKey = _config["Service:TransactionKey"]
				},
				Amount = random.Next(100),
				TransactionType = random.Next(100) % 3 == 0 ? TransactionType.Refund : TransactionType.Payment,
				CurrencyType = (CurrencyType) (random.Next(100) % 4),
				Source = _config["ApplicationName"]
			};
		}
	}
}
