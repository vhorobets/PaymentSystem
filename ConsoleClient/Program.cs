using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaymentClientsCommon;
using PaymentGateway.Services;

namespace ConsoleClient
{
	class ConsoleClient
	{
		static async Task Main(string[] args)
		{
			using (var serviceProvider = ServiceCollection.BuildServiceProvider())
			{
				var logger = serviceProvider.GetService<ILogger<ConsoleClient>>();
				var paymentRequestFactory = serviceProvider.GetService<PaymentRequestFactory>();

				try
				{
					var paymentRequest = paymentRequestFactory.Get();
					paymentRequest.Merchant = null;

					var paymentResponse = await GRPCClient.CreatePaymentAsync(paymentRequest);

					Console.WriteLine(paymentResponse);

					//-------------------------------------------------------------------------

					var transactionRequest = new TransactionRequest() { TransactionId = paymentResponse.TransactionId };

					var transactionResponse = await GRPCClient.GetTransactionAsync(transactionRequest);

					Console.WriteLine(transactionResponse);
				} 
				catch (RpcException ex)
				{
					logger.LogError($"An error occured during GRPC call. Exception: {ex}");
				}
			}
		}

		private static PaymentGatewayService.PaymentGatewayServiceClient _grpcClient = null;
		private static PaymentGatewayService.PaymentGatewayServiceClient GRPCClient
		{
			get 
			{ 
				if (_grpcClient == null)
				{
					var serviceProvider = ServiceCollection.BuildServiceProvider();
					
					var config = serviceProvider.GetService<IConfiguration>();

					var grpcChannelOptions = new GrpcChannelOptions()
					{
						LoggerFactory = serviceProvider.GetService<ILoggerFactory>()
					};

					var channel = GrpcChannel.ForAddress(config["Service:ServiceUrl"], grpcChannelOptions);

					_grpcClient = new PaymentGatewayService.PaymentGatewayServiceClient(channel);
				}

				return _grpcClient;
			}
		}

		private static IServiceCollection _serviceCollection = null;
		private static IServiceCollection ServiceCollection
		{
			get 
			{
				if (_serviceCollection == null)
				{
					_serviceCollection = new ServiceCollection();

					var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

					_serviceCollection.AddLogging(configure => configure.AddConsole());
					_serviceCollection.AddSingleton<IConfiguration>(config);
					_serviceCollection.AddSingleton<PaymentRequestFactory>();
				}

				return _serviceCollection;
			}
		}
	}
}
