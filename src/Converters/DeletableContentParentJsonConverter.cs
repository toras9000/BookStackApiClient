using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookStackApiClient.Converters;

/// <summary>
/// ゴミ箱アイテムの親を内容に応じた型にデシリアライズするJSONコンバータ
/// </summary>
public class DeletableContentParentJsonConverter : JsonConverter<DeletableContentParent>
{
    /// <inheritdoc />
    public override DeletableContentParent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // デシリアライズ対象がどの種別のコンテンツかを判別
        // Utf8JsonReader は構造体であり、インスタンスのコピーを行うとポイントする位置はそれぞれ個別になる。
        // そのため、通常の(refではない)コピーを渡した先で判定のために読み取り位置を進めても、このメソッド内には影響がない。
        var contentType = JsonConverterHelper.FindPropertyString(reader, "type")?.ToLowerInvariant() ?? "page";

        // 種別に応じて具体型にデシリアライズ
        return contentType switch
        {
            "book" => JsonSerializer.Deserialize<DeletableContentParentBook>(ref reader) ?? throw new JsonException(),
            "chapter" => JsonSerializer.Deserialize<DeletableContentParentChapter>(ref reader) ?? throw new JsonException(),
            _ => throw new JsonException(),
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DeletableContentParent value, JsonSerializerOptions options)
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
