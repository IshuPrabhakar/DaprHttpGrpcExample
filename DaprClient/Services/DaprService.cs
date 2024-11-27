using System;
using Grpc.Core;
using GrpcGreeter;
using Serilog;
using Dappr = Dapr.Client;

namespace DaprClient.Services;

public class DaprService(
    Greeter.GreeterClient grpcService
    // ,Dappr.DaprClient daprClient
    ) : IDaprService
{
    private readonly HttpClient _httpClient = Dappr.DaprClient.CreateInvokeHttpClient("daprserver");
    private readonly Greeter.GreeterClient _grpcService = grpcService;
    // private readonly Dappr.DaprClient _daprClient = daprClient;

    public async Task<string> InvokeGrpc()
    {
        Log.Information($"Invoking dapr Grpc request..");
        var metadata = new Metadata
        {
            { "dapr-app-id", "daprserver" }
        };

        // Call the gRPC service via Dapr Service Invocation
        var res = await _grpcService.SayHelloAsync(new HelloRequest() { Name = "Ishu" }, metadata);

        Log.Information($"Invoked dapr Grpc request..");

        return res.Message;
    }

    public async Task InvokeHttp()
    {
        Log.Information($"Invoking dapr http request..");

        var request = new HttpRequestMessage(HttpMethod.Get, "invoked-http");
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        Log.Information($"Invoked dapr http request..");
    }
}
