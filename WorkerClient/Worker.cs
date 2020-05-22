using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentClientsCommon;
using PaymentGateway.Services;

namespace WorkerClient
{
	public class Worker : BackgroundService
	{
		private readonly ILogger<Worker> _logger;
		private readonly IConfiguration _config;
		private readonly PaymentRequestFactory _paymentRequestFactory;
		private readonly ILoggerFactory _loggerFactory;
		private PaymentGatewayService.PaymentGatewayServiceClient _grpcClient;

		public Worker(ILogger<Worker> logger, IConfiguration config, PaymentRequestFactory paymentRequestFactory, ILoggerFactory loggerFactory)
		{
			_logger = logger;
			_config = config;
			_paymentRequestFactory = paymentRequestFactory;
			_loggerFactory = loggerFactory;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

				try 
				{
					var transactionCount = new Random().Next(2, 5);

					var stream = GRPCClient.GetPaymentStream();

					for (int i = 0; i < transactionCount; i++)
					{
						await stream.RequestStream.WriteAsync(_paymentRequestFactory.Get());
					}

					await stream.RequestStream.CompleteAsync(); // complete sending requests, close stream

					var streamResponse = await stream.ResponseAsync; // get overall response

					_logger.LogInformation($"Completed sending transactions. Result: {streamResponse}");
				}
				catch (RpcException ex)
				{
					_logger.LogError($"An error occured during GRPC call. Exception: {ex}");
				}
				catch (Exception ex)
				{
					_logger.LogError($"An error occured: {ex}");
				}


				await Task.Delay(5000, stoppingToken);
			}
		}

		private PaymentGatewayService.PaymentGatewayServiceClient GRPCClient
		{
			get
			{
				if (_grpcClient == null)
				{
					var grpcChannelOptions = new GrpcChannelOptions()
					{
						LoggerFactory = _loggerFactory
					};

					var channel = GrpcChannel.ForAddress(_config["Service:ServiceUrl"], grpcChannelOptions);

					_grpcClient = new PaymentGatewayService.PaymentGatewayServiceClient(channel);
				}

				return _grpcClient;
			}
		}
	}
}
