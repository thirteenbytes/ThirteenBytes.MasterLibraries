using FinanceExample.Infrastructure;
using FinanceExample.Infrastructure.SqliteData;
using FinanceExample.WebApi.Endpoints;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting Finance Example Web API");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog to the logging pipeline
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Register infrastructure services with configuration
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    // Seed the database
    await DatabaseSeeder.SeedAsync(app.Services);

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // Configure endpoints
    app.MapAccountHoldersEndpoints();
    app.MapBankAccountsEndpoints();
    app.MapSupportedCurrenciesEndpoints();

    Log.Information("Finance Example Web API started successfully");
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
