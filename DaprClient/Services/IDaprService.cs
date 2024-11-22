using System;

namespace DaprClient.Services;

public interface IDaprService
{
    Task InvokeHttp();

    Task<string> InvokeGrpc();
}
