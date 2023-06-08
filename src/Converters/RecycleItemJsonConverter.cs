using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookStackApiClient.Converters;

/// <summary>
/// ゴミ箱アイテムを内容に応じた型にデシリアライズするJSONコンバータ
/// </summary>
public class RecycleItemJsonConverter : JsonConverter<RecycleItem>
{
    /// <inheritdoc />
    public override RecycleItem? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // 削除対象コンテンツの読み取り用に構造体コピーを取っておく
        var contentReader = reader;

        // コンテンツプロパティ以外をデシリアライズ。
        // 元のリーダはこのオブジェクトの次まで読み進めておく。
        var item = JsonSerializer.Deserialize<RecycleItemFrame>(ref reader) ?? throw new JsonException();

        // デシリアライズ対象がどの種別のコンテンツかを判別
        var found = JsonConverterHelper.ForwardToProperty(ref contentReader, "deletable");
        if (!found) throw new JsonException();

        // 
        DeletableContent content = item.deletable_type switch
        {
            "book" => JsonSerializer.Deserialize<DeletableContentBook>(ref contentReader) ?? throw new JsonException(),
            "chapter" => JsonSerializer.Deserialize<DeletableContentChapter>(ref contentReader) ?? throw new JsonException(),
            "page" => JsonSerializer.Deserialize<DeletableContentPage>(ref contentReader) ?? throw new JsonException(),
            "bookshelf" => JsonSerializer.Deserialize<DeletableContentShelf>(ref contentReader) ?? throw new JsonException(),
            _ => throw new JsonException(),
        };

        return new(item.id, item.deletable_type, item.deletable_id, item.deleted_by, content, item.created_at, item.updated_at);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, RecycleItem value, JsonSerializerOptions options)
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

    private record RecycleItemFrame(
        long id, string deletable_type, long deletable_id, long deleted_by,
        DateTime created_at, DateTime updated_at
    );
}
