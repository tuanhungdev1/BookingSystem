using System.Text.Json;
using System.Text.Json.Serialization;
namespace BookingSystem.Extensions
{
	public class UtcDateTimeConverter : JsonConverter<DateTime>
	{
		public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			// Khi đọc từ DB hoặc request → coi là UTC
			var dateStr = reader.GetString();
			if (string.IsNullOrEmpty(dateStr)) return DateTime.MinValue;

			return DateTime.Parse(dateStr).Kind == DateTimeKind.Utc
				? DateTime.Parse(dateStr)
				: DateTime.SpecifyKind(DateTime.Parse(dateStr), DateTimeKind.Utc);
		}

		public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
		{
			// Luôn trả về UTC + có chữ Z
			var utcValue = value.Kind switch
			{
				DateTimeKind.Utc => value,
				DateTimeKind.Local => value.ToUniversalTime(),
				_ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
			};

			writer.WriteStringValue(utcValue.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFZ"));
		}
	}
}
