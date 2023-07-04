namespace BookStackApiClient;

/// <summary>
/// BookStackClient で利用する例外の基本クラス
/// </summary>
public class BookStackClientException : Exception
{
    // 構築
    #region コンストラクタ
    /// <summary>デフォルトコンストラクタ</summary>
    public BookStackClientException() : base() { }

    /// <summary>メッセージを指定するコンストラクタ</summary>
    /// <param name="message">例外メッセージ</param>
    public BookStackClientException(string? message) : base(message) { }

    /// <summary>例外メッセージと内部例外を指定するコンストラクタ</summary>
    /// <param name="message">例外メッセージ</param>
    /// <param name="innerException">内部例外</param>
    public BookStackClientException(string? message, Exception? innerException) : base(message, innerException) { }
    #endregion
}

/// <summary>
/// API要求に対する予期しない応答を表す例外クラス
/// </summary>
public class UnexpectedResponseException : BookStackClientException
{
    // 構築
    #region コンストラクタ
    /// <summary>要求識別子と例外メッセージを指定するコンストラクタ</summary>
    /// <param name="message">例外メッセージ</param>
    public UnexpectedResponseException(string message) : base(message) { }
    #endregion
}

/// <summary>
/// API要求数の制限に達したことを示す例外クラス
/// </summary>
public class ApiLimitResponseException : BookStackClientException
{
    // 構築
    #region コンストラクタ
    /// <summary>要求識別子と例外メッセージを指定するコンストラクタ</summary>
    /// <param name="limit">APIリクエスト制限値 [毎分]</param>
    /// <param name="retryAfter">リトライまでの必要時間 [秒]</param>
    /// <param name="message">例外メッセージ</param>
    public ApiLimitResponseException(long limit, long retryAfter, string message) : base(message)
    {
        this.RequestsPerMin = limit;
        this.RetryAfter = retryAfter;
    }
    #endregion

    // 公開プロパティ
    #region 情報
    /// <summary>API要求数制限値 [毎分]</summary>
    public long RequestsPerMin { get; }

    /// <summary>リトライまでの必要時間 [秒]</summary>
    public long RetryAfter { get; }
    #endregion
}

/// <summary>
/// 応答内容の解釈エラーを表す例外クラス
/// </summary>
public class ResponseInterpretException : BookStackClientException
{
    // 構築
    #region コンストラクタ
    /// <summary>要求識別子と例外メッセージを指定するコンストラクタ</summary>
    /// <param name="message">例外メッセージ</param>
    /// <param name="innerException">内部例外</param>
    public ResponseInterpretException(string message, Exception? innerException = null) : base(message, innerException) { }
    #endregion
}

/// <summary>
/// API要求に対するエラー応答を表す例外クラス
/// </summary>
public class ErrorResponseException : BookStackClientException
{
    // 構築
    #region コンストラクタ
    /// <summary>エラー応答情報を指定するコンストラクタ</summary>
    /// <param name="code">応答に含まれたエラーコード</param>
    /// <param name="message">応答に含まれたエラーメッセージ</param>
    public ErrorResponseException(int code, string message) : base(message)
    {
        this.Code = code;
    }
    #endregion

    // 公開プロパティ
    #region コンテキスト情報
    /// <summary>応答に含まれたエラーコード</summary>
    public int Code { get; }
    #endregion
}