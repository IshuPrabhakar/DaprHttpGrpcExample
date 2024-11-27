using Dapr.Client;
using Dapr.Extensions.Configuration;
using DaprClient.Services;
using Grpc.Core;
using GrpcGreeter;
using Microsoft.AspNetCore.Mvc;
using Serilog;

var daprHttpUrl = "http://localhost:5005";
var daprGrpcUrl = "http://localhost:5005";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseSerilog((context, config) =>
{
    config.WriteTo.Console().MinimumLevel.Information();
    //config.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddDaprClient();

// Example: Add a typed gRPC client
builder.Services.AddGrpcClient<Greeter.GreeterClient>((provider, client) =>
{
    // Use Dapr's service invocation address
    client.Address = new Uri(daprGrpcUrl);
});


builder.Configuration.AddDaprSecretStore(
    "evz-secretstore-dev",
   new DaprClientBuilder()
    .UseHttpEndpoint(daprHttpUrl)
    .UseGrpcEndpoint(daprGrpcUrl)
    .Build(),
    TimeSpan.FromSeconds(30)
);

builder.Services.AddTransient<IDaprService, DaprService>();
// Add Cors
builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    Log.Information($"{builder.Configuration.GetSection("ConnectionStrings:DefaultConnection").Value}");
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/invoke-http", async ([FromServices] IDaprService service) =>
{
    await service.InvokeHttp();
    Log.Information($"Successfully  called http method using dapr sidecar");
});

app.MapGet("/invoke-grpc", async ([FromServices] IDaprService service) =>
{
    var res = await service.InvokeGrpc();
    Log.Information($"{res}");
    Log.Information($"Successfully  called Grpc method using dapr sidecar");
});

app.UseCors(opt =>
    opt.AllowAnyHeader()
    .AllowAnyMethod()
    .AllowAnyOrigin()
);

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
