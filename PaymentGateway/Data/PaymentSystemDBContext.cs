using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data.Entities;

namespace PaymentGateway.Data
{
	public class PaymentSystemDBContext : DbContext
	{
		public PaymentSystemDBContext(DbContextOptions options) : base(options)
		{
		}

		public DbSet<Transaction> Transactions { get; set; }
	}
}
