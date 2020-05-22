namespace PaymentGateway.Data.Entities
{
	public class PublicEnums
	{
		public enum TransactionType
		{
			INVALID = 0,
			PAYMENT = 1,
			REFUND = 2
		}

		public enum CurrencyType
		{
			USD = 0,
			CAD = 1,
			GBP = 2,
			EUR = 3
		}

		public enum TransactionStatus
		{
			NOTPROCESSED = 0,
			APPROVED = 1,
			DECLINED = 2
		}
	}
}
