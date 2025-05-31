using System.Diagnostics.CodeAnalysis;

namespace BookStackApiClient;

/// <summary>BookStackのバージョン</summary>
/// <param name="major">メジャーバージョン</param>
/// <param name="minor">マイナーバージョン</param>
/// <param name="revision">リビジョン</param>
/// <param name="ext">追加情報</param>
public class BookStackVersion(int major, int minor, int revision, string ext = "") : IEquatable<BookStackVersion>, IComparable<BookStackVersion>
{
    // 公開プロパティ
    #region バージョン情報
    /// <summary>メジャーバージョン</summary>
    public int Major { get; } = major;

    /// <summary>マイナーバージョン</summary>
    public int Minor { get; } = minor;

    /// <summary>リビジョン</summary>
    public int Revision { get; } = revision;

    /// <summary>追加情報</summary>
    public string Ext { get; } = ext;
    #endregion

    // 公開メソッド
    #region パース
    /// <summary>バージョン文字列をパースする</summary>
    /// <param name="text">パース対象文字列</param>
    /// <returns>パース結果</returns>
    public static BookStackVersion Parse(ReadOnlySpan<char> text)
        => TryParse(text, out var version) ? version : throw new BookStackClientException("Unexpected version format");

    /// <summary>バージョン文字列をパースする</summary>
    /// <param name="text">パース対象文字列</param>
    /// <param name="version">パース結果</param>
    /// <returns>パース成否</returns>
    public static bool TryParse(ReadOnlySpan<char> text, [NotNullWhen(true)] out BookStackVersion? version)
    {
        // 出力値の初期化
        version = null;

        // 先頭のvを許容する
        var scan = text.Trim();
        if (scan is ['v', ..]) scan = scan[1..];

        // 文字列先頭のトークンを取得するローカル関数
        static ReadOnlySpan<char> takeToken(ReadOnlySpan<char> span, ReadOnlySpan<char> delimiters, out ReadOnlySpan<char> next, out char term)
        {
            var delim = span.IndexOfAny(delimiters);
            if (delim < 0)
            {
                next = span[span.Length..];
                term = '\0';
                return span;
            }

            var token = span[..delim];
            next = span[(delim + 1)..];
            term = span[delim];
            return token;
        }

        // メジャー番号パート
        var token1 = takeToken(scan, ['.'], out scan, out _);
        if (!int.TryParse(token1, out var major)) return false;

        // マイナー番号パート
        var token2 = takeToken(scan, ['.', '-', '+'], out scan, out var delim2);
        if (!int.TryParse(token2, out var minor)) return false;

        // リビジョン番号パート
        var revision = 0;
        if (!scan.IsEmpty && delim2 == '.')
        {
            var token3 = takeToken(scan, ['.', '-', '+'], out var next, out _);
            // 3つ目のパートは数値解釈出来たらその後ろを、出来なかったら3つ目のパート自体を残り情報にする
            if (int.TryParse(token3, out revision)) scan = next;
        }

        // 残った情報
        var ext = scan.IsEmpty ? "" : scan.ToString();

        // パース結果をインスタンス化
        version = new(major, minor, revision, ext);
        return true;
    }
    #endregion

    #region 等価比較
    /// <inheritdoc />
    public bool Equals(BookStackVersion? other)
    {
        if (other == null) return false;
        return this.Major == other.Major
            && this.Minor == other.Minor
            && this.Revision == other.Revision
            && this.Ext == other.Ext;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => Equals(obj as BookStackVersion);
    #endregion

    #region 大小比較
    /// <inheritdoc />
    public int CompareTo(BookStackVersion? other)
    {
        if (other == null) return int.MaxValue;
        var major = this.Major - other.Major;
        if (major != 0) return major;
        var minor = this.Minor - other.Minor;
        if (minor != 0) return minor;
        var revision = this.Revision - other.Revision;
        if (revision != 0) return revision;
        var ext = Comparer<string>.Default.Compare(this.Ext, other.Ext);
        if (ext != 0) return ext;
        return 0;
    }
    #endregion

    #region 演算子
    /// <inheritdoc />
    public static bool operator <(BookStackVersion x, BookStackVersion y) => x.CompareTo(y) < 0;

    /// <inheritdoc />
    public static bool operator >(BookStackVersion x, BookStackVersion y) => x.CompareTo(y) > 0;

    /// <inheritdoc />
    public static bool operator <=(BookStackVersion x, BookStackVersion y) => x.CompareTo(y) <= 0;

    /// <inheritdoc />
    public static bool operator >=(BookStackVersion x, BookStackVersion y) => x.CompareTo(y) >= 0;
    #endregion

    #region インフラ
    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(this.Major, this.Minor, this.Revision, this.Ext);
    #endregion
}
