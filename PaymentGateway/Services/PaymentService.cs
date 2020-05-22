using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

using PaymentGateway.Data;
using PaymentGateway.Data.Entities;

namespace PaymentGateway.Services
{
	public class PaymentService : PaymentGatewayService.PaymentGatewayServiceBase
	{
		private readonly ILogger<PaymentService> _logger;
		private readonly IPaymentSystemRepository _repository;
		public PaymentService(ILogger<PaymentService> logger, IPaymentSystemRepository repository)
		{
			_logger = logger;
			_repository = repository;
		}

		public override async Task<PaymentResponse> CreatePayment(PaymentRequest request, ServerCallContext context)
		{
			var response = new PaymentResponse()
			{
				Message = "Ok",
				ResponseStatus = ResponseStatus.Successful,
				ReceivingTime = Timestamp.FromDateTime(DateTime.UtcNow)
			};

			try
			{
				ValidateRequest(request);

				var transaction = new Transaction()
				{
					TransactionType = (PublicEnums.TransactionType) request.TransactionType,
					Amount = request.Amount,
					CurrencyType = (PublicEnums.CurrencyType) request.CurrencyType,
					Source = request.Source
				};

				await _repository.AddTransactionAsync(transaction);

				await _repository.SaveAllAsync();

				_logger.LogInformation($"Transaction saved to db, transactionId: {transaction.TransactionId}");

				response.TransactionId = transaction.TransactionId;
			}
			catch (RpcException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Exception thrown during creation of payment transaction: {ex}");
				response.Message = "Exception thrown during process";
				response.ResponseStatus = ResponseStatus.Failed;
			}

			return response;
		}

		public override async Task<StreamResponse> GetPaymentStream(IAsyncStreamReader<PaymentRequest> requestStream, ServerCallContext context)
		{
			var response = new StreamResponse();

			try 
			{
				await foreach (var paymentRequest in requestStream.ReadAllAsync())
				{
					response.Count++;
					try 
					{
						ValidateRequest(paymentRequest);

						var transaction = new Transaction()
						{
							TransactionType = (PublicEnums.TransactionType) paymentRequest.TransactionType,
							Amount = paymentRequest.Amount,
							CurrencyType = (PublicEnums.CurrencyType) paymentRequest.CurrencyType,
							Source = paymentRequest.Source
						};

						await _repository.AddTransactionAsync(transaction);

						await _repository.SaveAllAsync();

						response.SuccessfulCount++;
					} 
					catch (RpcException)
					{
						response.FailedCount++;
					}
				}

				_logger.LogInformation($"{response.Count} transactions saved to db");
			}
			catch (Exception ex)
			{
				_logger.LogError($"Exception thrown during processing transaction1: {ex}");
				throw new RpcException(Status.DefaultCancelled, "Exception thrown during process");
			}

			return response;
		}

		public override async Task<TransactionResponse> GetTransaction(TransactionRequest request, ServerCallContext context)
		{
			if (request.TransactionId < 0)
			{
				throw new RpcException(new Status(StatusCode.InvalidArgument, "Requested transactionId is invalid"));
			}

			var response = new TransactionResponse()
			{
				TransactionId = request.TransactionId
			};

			try
			{
				var transaction = await _repository.GetTransactionAsync(request.TransactionId);

				if (transaction != null)
				{
					response.TransactionAmount = transaction.Amount;
					response.TransactionStatus = (TransactionResponse.Types.TransactionStatus) (transaction.TransactionStatus + 1);
				}
				else
				{
					response.TransactionStatus = TransactionResponse.Types.TransactionStatus.NotFound;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Exception thrown during retrieving the transaction: {ex}");
				throw new RpcException(Status.DefaultCancelled, "Exception thrown during process");
			}

			return response;
		}

		#region Private Stuff
		private void ValidateRequest(PaymentRequest request)
		{
			if (request.Merchant == null ||
				string.IsNullOrWhiteSpace(request.Merchant.MerchantName) ||
				string.IsNullOrWhiteSpace(request.Merchant.TransactionKey)) // here could be a check Merchant for validity
			{
				throw new RpcException(new Status(StatusCode.InvalidArgument, "Merchant data is invalid"));
			}

			if (request.TransactionType == TransactionType.Invalid)
			{
				var trailer = new Metadata() // key-value pairs of additional exception info
				{
					{ "BadValueIn", nameof(request.TransactionType) },
					{ "Message", "Transaction is invalid" }
				};
				throw new RpcException(Status.DefaultCancelled, trailer, "Transaction is invalid");
			}
		}
		#endregion
	}
}
