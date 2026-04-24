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
            "book" => JsonSerializer.Deserialize(ref reader, BookStackTypeInfo.Default.SearchContentBook) ?? throw new JsonException(),
            "chapter" => JsonSerializer.Deserialize(ref reader, BookStackTypeInfo.Default.SearchContentChapter) ?? throw new JsonException(),
            "page" => JsonSerializer.Deserialize(ref reader, BookStackTypeInfo.Default.SearchContentPage) ?? throw new JsonException(),
            "bookshelf" => JsonSerializer.Deserialize(ref reader, BookStackTypeInfo.Default.SearchContentShelf) ?? throw new JsonException(),
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
            switch (value)
            {
                case SearchContentBook: JsonSerializer.Serialize(writer, value, BookStackTypeInfo.Default.SearchContentBook); break;
                case SearchContentChapter: JsonSerializer.Serialize(writer, value, BookStackTypeInfo.Default.SearchContentChapter); break;
                case SearchContentPage: JsonSerializer.Serialize(writer, value, BookStackTypeInfo.Default.SearchContentPage); break;
                case SearchContentShelf: JsonSerializer.Serialize(writer, value, BookStackTypeInfo.Default.SearchContentShelf); break;
                default: throw new JsonException();
            }
        }
    }
}
