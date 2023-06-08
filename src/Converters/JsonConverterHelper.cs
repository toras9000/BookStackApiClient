using System.Text.Json;

namespace BookStackApiClient.Converters;

/// <summary>
/// JSONコンバータ向けのヘルパ処理
/// </summary>
internal static class JsonConverterHelper
{
    /// <summary>JSONリーダを指定のプロパティの値まで進める</summary>
    /// <param name="reader">JSONリーダ</param>
    /// <param name="name">プロパティ名称</param>
    /// <returns>プロパティを見つけたか否か</returns>
    public static bool ForwardToProperty(ref Utf8JsonReader reader, string name)
    {
        // オブジェクトでなければ想定外
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        // 判別情報まで読み進める
        while (reader.Read())
        {
            // 判別情報が見つからずにオブジェクト終端になったら探索終了。
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            // プロパティ以外を検出したら想定外
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            // プロパティ名が判別情報であるかを判定。違えばプロパティ値をスキップ。
            var propName = reader.GetString();
            if (propName != name)
            {
                reader.Skip();
                continue;
            }

            // プロパティ値に進め、プロパティを検出したことを返却
            reader.Read();

            return true;
        }

        // プロパティが見つからなければその結果を返却
        return false;
    }

    /// <summary>JSONオブジェクトの文字列プロパティ値をがどのブックコンテンツであるかを判別する</summary>
    /// <param name="reader">JSONリーダ</param>
    /// <param name="name">検索するプロパティ名</param>
    /// <returns>プロパティ値。プロパティが見つからない場合や文字列でない場合は nulll を返却。</returns>
    public static string? FindPropertyString(Utf8JsonReader reader, string name)
    {
        // リーダを指定のプロパティまで進める
        var found = ForwardToProperty(ref reader, name);

        // プロパティが見つからなかった場合は null を返却
        if (!found) return null;

        // プロパティが文字列でなければ null を返却
        if (reader.TokenType != JsonTokenType.String) return null;

        // プロパティ値文字列を返却
        return reader.GetString();
    }
}
