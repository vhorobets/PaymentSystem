using System.Threading.Tasks;
using PaymentGateway.Data.Entities;

namespace PaymentGateway.Data
{
	public interface IPaymentSystemRepository
	{
		Task AddTransactionAsync(Transaction transaction);
		Task<Transaction> GetTransactionAsync(int transactionId);

		Task<bool> SaveAllAsync();
	}
}
