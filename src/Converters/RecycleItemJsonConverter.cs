using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookStackApiClient.Converters;

/// <summary>
/// ゴミ箱アイテムを内容に応じた型にデシリアライズするJSONコンバータ
/// </summary>
public partial class RecycleItemJsonConverter : JsonConverter<RecycleItem>
{
    /// <inheritdoc />
    public override RecycleItem? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // 削除対象コンテンツの読み取り用に構造体コピーを取っておく
        var contentReader = reader;

        // コンテンツプロパティ以外をデシリアライズ。
        // 元のリーダはこのオブジェクトの次まで読み進めておく。
        var item = JsonSerializer.Deserialize(ref reader, ConverterTypeInfo.Default.RecycleItemFrame) ?? throw new JsonException();

        // デシリアライズ対象がどの種別のコンテンツかを判別
        var found = JsonConverterHelper.ForwardToProperty(ref contentReader, "deletable");
        if (!found) throw new JsonException();

        // コンテンツ種別に応じたデシリアライズ
        DeletableContent content = item.deletable_type switch
        {
            "book" => JsonSerializer.Deserialize(ref contentReader, BookStackTypeInfo.Default.DeletableContentBook) ?? throw new JsonException(),
            "chapter" => JsonSerializer.Deserialize(ref contentReader, BookStackTypeInfo.Default.DeletableContentChapter) ?? throw new JsonException(),
            "page" => JsonSerializer.Deserialize(ref contentReader, BookStackTypeInfo.Default.DeletableContentPage) ?? throw new JsonException(),
            "bookshelf" => JsonSerializer.Deserialize(ref contentReader, BookStackTypeInfo.Default.DeletableContentShelf) ?? throw new JsonException(),
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
            JsonSerializer.Serialize(writer, value, BookStackTypeInfo.Default.RecycleItem);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:命名スタイル", Justification = "API JSONデータインタフェース用のため言語の命名標準には従わない。")]
    private record RecycleItemFrame(
        long id, string deletable_type, long deletable_id, long deleted_by,
        DateTime created_at, DateTime updated_at
    );

    [JsonSerializable(typeof(RecycleItemFrame))]
    private partial class ConverterTypeInfo : JsonSerializerContext;
}
