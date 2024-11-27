using Dapr.Client;
using Dapr.Extensions.Configuration;
using DaprServer.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

//builder.Configuration.AddDaprSecretStore("evz-secretstore-dev",
//    new DaprClientBuilder().Build(),
//    TimeSpan.FromSeconds(30)
//);

builder.Services.AddGrpc();

builder.Host.UseSerilog((context, config) =>
{
    config.WriteTo.Console().MinimumLevel.Information();
    //config.ReadFrom.Configuration(context.Configuration);
});

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

app.MapGet("/invoked-http", () =>
{
    Log.Information($"Request received dapr http request");
});

app.MapGet("/invoked-grpc", () =>
{

});

app.UseCors(opt =>
    opt.AllowAnyHeader()
    .AllowAnyMethod()
    .AllowAnyOrigin()
);


app.MapGrpcService<GreeterService>();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
