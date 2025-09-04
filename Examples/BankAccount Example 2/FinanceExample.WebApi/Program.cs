using FinanceExample.Infrastructure;
using FinanceExample.Infrastructure.SqliteData;
using FinanceExample.WebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

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

app.Run();
