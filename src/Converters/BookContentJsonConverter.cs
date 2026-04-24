using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookStackApiClient.Converters;

/// <summary>
/// ブックコンテンツを内容に応じた型にデシリアライズするJSONコンバータ
/// </summary>
public class BookContentJsonConverter : JsonConverter<BookContent>
{
    /// <inheritdoc />
    public override BookContent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // デシリアライズ対象がどの種別のコンテンツかを判別
        // Utf8JsonReader は構造体であり、インスタンスのコピーを行うとポイントする位置はそれぞれ個別になる。
        // そのため、通常の(refではない)コピーを渡した先で判定のために読み取り位置を進めても、このメソッド内には影響がない。
        var contentType = JsonConverterHelper.FindPropertyString(reader, "type")?.ToLowerInvariant() ?? "page";

        // 判別情報を見つけられずに終えたら、デフォルトではページとみなす。
        // (API例でページコンテンツの時にtypeプロパティがなく、過去の仕様ではそうだったのかと思われたため。)
        return contentType switch
        {
            "chapter" => JsonSerializer.Deserialize(ref reader, BookStackTypeInfo.Default.BookContentChapter) ?? throw new JsonException(),
            "page" => JsonSerializer.Deserialize(ref reader, BookStackTypeInfo.Default.BookContentPage) ?? throw new JsonException(),
            _ => throw new JsonException(),
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, BookContent value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            // シリアライズ対象インスタンスの実体型に応じたシリアライズを行う。
            switch (value)
            {
                case BookContentChapter: JsonSerializer.Serialize(writer, value, BookStackTypeInfo.Default.BookContentChapter); break;
                case BookContentPage: JsonSerializer.Serialize(writer, value, BookStackTypeInfo.Default.BookContentPage); break;
                default: throw new JsonException();
            }
        }
    }
}
