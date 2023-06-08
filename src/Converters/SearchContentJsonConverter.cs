using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookStackApiClient.Converters;

/// <summary>
/// 検索結果コンテンツを内容に応じた型にデシリアライズするJSONコンバータ
/// </summary>
public class SearchContentJsonConverter : JsonConverter<SearchContent>
{
    /// <inheritdoc />
    public override SearchContent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // デシリアライズ対象がどの種別のコンテンツかを判別
        // Utf8JsonReader は構造体であり、インスタンスのコピーを行うとポイントする位置はそれぞれ個別になる。
        // そのため、通常の(refではない)コピーを渡した先で判定のために読み取り位置を進めても、このメソッド内には影響がない。
        var contentType = JsonConverterHelper.FindPropertyString(reader, "type")?.ToLowerInvariant() ?? "page";

        // 種別に応じて具体型にデシリアライズ
        return contentType switch
        {
            "book" => JsonSerializer.Deserialize<SearchContentBook>(ref reader) ?? throw new JsonException(),
            "chapter" => JsonSerializer.Deserialize<SearchContentChapter>(ref reader) ?? throw new JsonException(),
            "page" => JsonSerializer.Deserialize<SearchContentPage>(ref reader) ?? throw new JsonException(),
            "bookshelf" => JsonSerializer.Deserialize<SearchContentShelf>(ref reader) ?? throw new JsonException(),
            _ => throw new JsonException(),
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, SearchContent value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            // シリアライズ対象インスタンスの実体型に応じたシリアライズを行う。
            JsonSerializer.Serialize(writer, value, value.GetType());
        }
    }
}
