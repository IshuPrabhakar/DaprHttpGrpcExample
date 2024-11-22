using Grpc.Core;
using GrpcGreeter;
using Serilog;

namespace DaprServer.Services;

public class GreeterService(ILogger<GreeterService> logger) : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger = logger;

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        Log.Information($"Request received dapr GRPC request");
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }
}
