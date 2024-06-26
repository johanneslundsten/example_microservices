using ApplicationUtils;
using GrpcServiceWithDb.Persistence;
using GrpcServiceWithDb.Services;
using Microsoft.EntityFrameworkCore;

namespace GrpcServiceWithDb;

public class Startup(IConfiguration configuration)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddGrpc(options =>
        {
            options.Interceptors.Add<TraceLoggingInterceptor>();
            options.Interceptors.Add<GrpcExceptionHandlerInterceptor>();
        });

        services.AddSingleton<TraceLoggingInterceptor>();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AccountsDbContext>(options =>
            options.UseNpgsql(connectionString));
    }

    public void Configure(IApplicationBuilder app, IHostEnvironment env, AccountsDbContext dbContext)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Apply migrations on startup
        try
        {
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            // Log errors or handle them as needed
            Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
            throw;
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGrpcService<AccountService>();
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client.");
            });
        });
    }
}