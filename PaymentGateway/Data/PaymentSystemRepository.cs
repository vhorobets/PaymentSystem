using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PaymentGateway.Data.Entities;

namespace PaymentGateway.Data
{
	public class PaymentSystemRepository : IPaymentSystemRepository
	{
		private readonly PaymentSystemDBContext _dbContext;
		private readonly ILogger<PaymentSystemRepository> _logger;

		public PaymentSystemRepository(PaymentSystemDBContext dbContext, ILogger<PaymentSystemRepository> logger)
		{
			_dbContext = dbContext;
			_logger = logger;
		}
		public async Task AddTransactionAsync(Transaction transaction)
		{
			var entityEntry = await _dbContext.AddAsync(transaction);
			_logger.LogInformation($"Transaction added");
		}

		public async Task<Transaction> GetTransactionAsync(int transactionId)
		{
			return await _dbContext.Transactions.FindAsync(transactionId);
		}

		public async Task<bool> SaveAllAsync()
		{
			return await _dbContext.SaveChangesAsync() > 0;
		}
	}
}
