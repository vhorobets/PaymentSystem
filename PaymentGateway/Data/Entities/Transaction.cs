namespace PaymentGateway.Data.Entities
{
	public class Transaction
	{
		public int TransactionId { get; set; }
		public PublicEnums.TransactionType TransactionType { get; set; }
		public PublicEnums.CurrencyType CurrencyType { get; set; }
		public double Amount { get; set; }
		public PublicEnums.TransactionStatus TransactionStatus { get; set; }
		public string Source { get; set; }
	}
}
