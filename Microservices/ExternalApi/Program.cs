using ApplicationUtils;
using GrpcService;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ExternalApi.Models;

namespace ExternalApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        // Configure Kestrel server to use a specific port
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(5002);
        });

        builder.Services.AddGrpcClient<AccountsService.AccountsServiceClient>(o =>
        {
            var grpcServiceAddress = builder.Configuration["GrpcServiceWithDbGrpcClientSettings:Address"];
            ArgumentNullException.ThrowIfNull(grpcServiceAddress, "GrpcServiceWithDbGrpcClientSettings:Address");
            o.Address = new Uri(grpcServiceAddress);
        });
        
        builder.Services.AddLogging();

        // Configure OpenTelemetry
        builder.Services.ConfigureOpenTelemetryTracerProvider(tracerProviderBuilder =>
        {
            tracerProviderBuilder
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MinimalApiService"))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddConsoleExporter()
                .AddZipkinExporter(o =>
                {
                    o.Endpoint = new Uri("http://localhost:9411/api/v2/spans"); // Zipkin endpoint
                })
                .AddJaegerExporter(o =>
                {
                    o.AgentHost = "localhost"; // Jaeger's agent host
                    o.AgentPort = 6831; // Jaeger's agent port
                });
        });

        builder.Services.ConfigureOpenTelemetryMeterProvider(meterProviderBuilder =>
        {
            meterProviderBuilder
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MinimalApiService"))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddConsoleExporter();
        });

        // Configure logging
        builder.Logging.ClearProviders();
        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeScopes = true;
            options.ParseStateValues = true;
            options.AddConsoleExporter();
        });
        
        var app = builder.Build();
        app.UseMiddleware<LoggingErrorHandler>();
        
        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapGet("/accounts", (AccountsService.AccountsServiceClient client) =>
                client.GetAll(new Empty()).Entities.Select(account => account.ToApi()))
            .WithName("GetTodoItems")
            .Produces<List<Models.Account>>(StatusCodes.Status200OK);

        app.MapGet("/accounts/{id}", (string id, AccountsService.AccountsServiceClient client) =>
            {
                var accountOrDefault = client.Get(new AccountId { Id = id })?.Account?.ToApi();
                
                return Task.FromResult(accountOrDefault != null
                    ? Results.Ok(accountOrDefault)
                    : Results.NotFound());
            })
            .WithName("GetTodoItem")
            .Produces<Models.Account>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/accounts", async (Models.CreateAccount account, AccountsService.AccountsServiceClient client) =>
            {
                var createdAccount = await client.CreateAsync(new GrpcService.CreateAccount
                {
                    Name = account.Name
                });

                return Results.Created($"/accounts/{createdAccount.Id}", createdAccount.ToApi());
            })
            .WithName("CreateTodoItem")
            .Produces<Models.Account>(StatusCodes.Status201Created);

        app.MapPut("/accounts/{id}", async (string id, Models.Account account, AccountsService.AccountsServiceClient client) =>
            {
                var updated = await client.UpdateAsync(account.ToGrpc());

                if (updated is null) throw new Exception("Unexpected error");
                
                return Results.NoContent();
            })
            .WithName("UpdateTodoItem")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        app.MapDelete("/accounts/{id}", async (string id, AccountsService.AccountsServiceClient client) =>
            {
                var account = await client.DeleteAsync(new AccountId { Id = id });
                return account == null ? Results.NotFound() : Results.Ok(account.ToApi());
            })
            .WithName("DeleteTodoItem")
            .Produces<Models.Account>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.Run();
    }
}