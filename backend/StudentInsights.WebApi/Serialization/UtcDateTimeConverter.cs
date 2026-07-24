using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentInsights.WebApi.Serialization;

/// <summary>
/// Normalizes every DateTime that crosses the API boundary to
/// DateTimeKind.Utc, in both directions. Registered once in Program.cs;
/// System.Text.Json automatically applies a registered DateTime converter
/// to DateTime? as well, so this single converter covers every current
/// *Utc property (CreatedAtUtc, UpdatedAtUtc, DueDateUtc, CompletedAtUtc,
/// ...) and every one a future module adds, with no per-handler or
/// per-DTO changes required.
///
/// Reading: the default DateTime converter preserves whatever Kind the
/// input string implies — Utc for a 'Z'/+00:00 suffix, Local (converted
/// to the server's local timezone!) for any other explicit offset, and
/// Unspecified when the client omits an offset entirely. Left alone, an
/// explicit non-'Z' offset would be silently mis-stored as the server's
/// local time instead of the intended UTC instant. This converter
/// enforces the fix: Utc passes through, Local is properly converted via
/// ToUniversalTime(), and Unspecified is stamped as Utc via SpecifyKind —
/// the correct interpretation given this project's own *Utc naming
/// convention already declares every such property to be UTC.
///
/// Writing: entities read back from EF Core commonly come back with
/// Kind=Unspecified (most providers don't persist Kind on plain
/// datetime2/timestamp columns), which makes System.Text.Json omit the
/// 'Z' suffix on output even though the value is a UTC instant. This
/// converter forces Kind=Utc before writing so every *Utc field in every
/// response is unambiguous, ISO-8601, 'Z'-suffixed UTC — matching what
/// the property name already promises.
/// </summary>
public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetDateTime();

        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utcValue = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        writer.WriteStringValue(utcValue);
    }
}