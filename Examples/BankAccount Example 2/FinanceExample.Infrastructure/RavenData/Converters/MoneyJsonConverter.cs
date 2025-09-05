using FinanceExample.Domain.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FinanceExample.Infrastructure.RavenData.Converters
{
    public class MoneyJsonConverter : JsonConverter<Money>
    {
        public override Money ReadJson(JsonReader reader, Type objectType, Money? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var amount = jsonObject["Amount"]?.Value<decimal>() ?? 0;
            var currency = jsonObject["Currency"]?.Value<string>() ?? "USD";

            // Use reflection to create Money instance bypassing validation for deserialization
            var constructor = typeof(Money).GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                new[] { typeof(decimal), typeof(string) },
                null);

            return (Money)constructor!.Invoke(new object[] { amount, currency });
        }

        public override void WriteJson(JsonWriter writer, Money? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();
            writer.WritePropertyName("Amount");
            writer.WriteValue(value.Amount);
            writer.WritePropertyName("Currency");
            writer.WriteValue(value.Currency);
            writer.WriteEndObject();
        }
    }
}