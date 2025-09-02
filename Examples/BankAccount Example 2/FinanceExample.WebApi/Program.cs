using FinanceExample.Infrastructure;
using FinanceExample.WebApi.Endpoints;
using Serilog;

namespace FinanceExample.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create initial bootstrap logger
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting Finance API");

                var builder = WebApplication.CreateBuilder(args);

                // Configure Serilog from configuration
                builder.Host.UseSerilog((context, services, configuration) =>
                    configuration
                        .ReadFrom.Configuration(context.Configuration)
                        .ReadFrom.Services(services)
                        .Enrich.FromLogContext());

                // Add infrastructure services
                builder.Services.AddInfrastructure();

                // Add services to the container.
                builder.Services.AddAuthorization();

                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseAuthorization();

                // Add Serilog request logging
                app.UseSerilogRequestLogging(options =>
                {
                    options.MessageTemplate = "Handled {RequestPath} in {Elapsed:0.0000} ms";
                    options.GetLevel = (httpContext, elapsed, ex) =>
                    {
                        return ex != null ? Serilog.Events.LogEventLevel.Error :
                               elapsed > 1000 ? Serilog.Events.LogEventLevel.Warning :
                               Serilog.Events.LogEventLevel.Information;
                    };
                });

                // Map endpoints
                app.MapAccountHoldersEndpoints();
                app.MapBankAccountsEndpoints();
                app.MapSupportedCurrenciesEndpoints();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Finance API terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
