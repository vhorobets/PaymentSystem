# PaymentSystem

This is demo project for demonstrating using of grpc in .Net Core applications.

The solution consists of `PaymentGateway` grpc service and two grpc client `ConsoleClient` and `WorkerClient`. All of them simulates payment system, when clients generate and send payment transactions to the service and the service simple save received transactions to db.

### Build the Database

If you don't have the Entity Framework Core tools installed for .NET Core, you'll need to run next command:

    C:/>dotnet tool install --global dotnet-ef
  
Then you need to navigate to `PaymentGateway` project root and run next command in the console:

    dotnet ef database update
  




