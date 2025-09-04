using FinanceExample.Domain.Common;
using FinanceExample.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Clock;

namespace FinanceExample.Infrastructure.SqliteData
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<FinanceDbContext>>();

            try
            {
                await context.Database.EnsureCreatedAsync();
                await SeedSupportedCurrencies(context, dateTimeProvider, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        private static async Task SeedSupportedCurrencies(
            FinanceDbContext context,
            IDateTimeProvider dateTimeProvider,
            ILogger logger)
        {
            if (!await context.Set<SupportedCurrency>().AnyAsync())
            {
                logger.LogInformation("Seeding supported currencies...");

                var currencies = SupportedCurrencySeeder.GetSeedData();
                var now = dateTimeProvider.UtcNow;

                foreach (var currency in currencies)
                {
                    currency.CreatedDateUtc = now;
                    currency.LastModifiedDateUtc = now;
                }

                await context.Set<SupportedCurrency>().AddRangeAsync(currencies);
                await context.SaveChangesAsync();

                logger.LogInformation("Successfully seeded {Count} supported currencies", currencies.Count);
            }
        }
    }
}