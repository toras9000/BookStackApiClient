using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookStackApiClient.Converters;

/// <summary>APIレスポンスの日時フィールド変換用</summary>
/// <remarks>
/// BookStack v23.05 時点で、一部のAPIがUTC形式ではない日時書式で応答する。
/// 標準の JsonSerializer がそれを変換できないようなので作成したコンバータとなる。
/// </remarks>
public class AmbiguousTimeJsonConverter : JsonConverter<DateTime>
{
    /// <inheritdoc />
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var text = reader.GetString() ?? throw new JsonException();
        var styles = DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowInnerWhite;

        // 一旦通常の形式でパースを試みる
        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, styles, out var normal))
        {
            return normal;
        }

        // 通常パースできなかったら指定書式でのパースを試みる
        return DateTime.ParseExact(text, DateTimeFormat, CultureInfo.InvariantCulture, styles);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(DateTimeFormat));
    }

    /// <summary>応答解釈する日時書式</summary>
    private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
}
