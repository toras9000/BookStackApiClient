using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BookStackApiClient.Utility;

/// <summary>戻り値を伴うAPI呼び出し処理デリゲート</summary>
/// <typeparam name="TResult">戻り値の型</typeparam>
/// <param name="client">クライアントインスタンス</param>
/// <param name="cancelToken">キャンセルトークン</param>
/// <returns>戻り値を得るタスク</returns>
public delegate Task<TResult> ApiInvoker<TResult>(BookStackClient client, CancellationToken cancelToken);

/// <summary>戻り値のないAPI呼び出し処理デリゲート</summary>
/// <param name="client">クライアントインスタンス</param>
/// <param name="cancelToken">キャンセルトークン</param>
/// <returns>呼び出しタスク</returns>
public delegate Task ApiInvoker(BookStackClient client, CancellationToken cancelToken);

/// <summary>API制限発生時イベント引数</summary>
/// <param name="ex">API制限を表す例外オブジェクト</param>
public class ApiLimitHandlerArgs(ApiLimitResponseException ex)
{
    /// <summary>API制限を表す例外オブジェクト</summary>
    public ApiLimitResponseException Exception { get; } = ex;
}

/// <summary>API制限発生時の処理ハンドラ型</summary>
/// <param name="args">API制限情報</param>
/// <returns>制限処理タスク</returns>
public delegate ValueTask ApiLimitAsyncHandler(ApiLimitHandlerArgs args);

/// <summary>BookStackClientクラスを補助するヘルパクラス</summary>
/// <remarks>
/// このクラスでは主にAPI呼び出し制限への対応するための補助処理を含む。
/// シンプルな利用方法としては、以下のような
/// <code>
/// using var helper = new 
/// </code>
/// </remarks>
public class BookStackClientHelper : IDisposable
{
    // 構築
    #region コンストラクタ
    /// <summary>クライアントの生成パラメータを指定するコンストラクタ</summary>
    /// <param name="baseUri">APIベースURI。<see cref="BookStackClient" /> </param>
    /// <param name="token">APIトークンID</param>
    /// <param name="secret">APIトークンシークレット。</param>
    /// <param name="cancelToken">デフォルトのキャンセルトークン</param>
    public BookStackClientHelper(Uri baseUri, string token, string secret, CancellationToken cancelToken = default)
    {
        this.Client = new BookStackClient(baseUri, token, secret);
        this.ownClient = true;
        this.breaker = cancelToken;
    }

    /// <summary>クライアントインスタンスを指定するコンストラクタ</summary>
    /// <param name="client">クライアントインスタンス</param>
    /// <param name="cancelToken">デフォルトのキャンセルトークン</param>
    /// <param name="own">クライアントの所有権を持たせるか(インスタンス破棄するか)否か</param>
    public BookStackClientHelper(BookStackClient client, CancellationToken cancelToken = default, bool own = false)
    {
        this.Client = client;
        this.ownClient = own;
        this.breaker = cancelToken;
    }
    #endregion

    // 公開プロパティ
    #region クライアント
    /// <summary>クライアントインスタンス</summary>
    public BookStackClient Client { get; }
    #endregion

    #region API制限サポート
    /// <summary>Tryメソッドの最大試行回数</summary>
    public int MaxTryCount { get; set; } = int.MaxValue;
    #endregion

    #region ユーティリティ
    /// <summary>リスティングAPIで一度に取得する件数</summary>
    public int BatchCount { get; set; } = 500;
    #endregion

    // 公開イベント
    #region API制限サポート
    /// <summary>API制限</summary>
    public event ApiLimitAsyncHandler? LimitHandler;
    #endregion

    // 公開メソッド
    #region API制限サポート
    /// <summary>API呼び出し制限に対処するヘルパメソッド</summary>
    /// <param name="breaker">キャンセルトークン</param>
    /// <param name="invoker">API呼び出し処理</param>
    /// <typeparam name="TResult">APIの戻り値型</typeparam>
    /// <returns>APIの戻り値</returns>
    /// <remarks>
    /// このメソッドはAPI呼び出し処理での <see cref="ApiLimitResponseException"/> 発生をハンドルする。
    /// ApiLimitResponseException の発生時に <see cref="LimitHandler"/> イベントを発行し、必要に応じて待機を加えてAPI呼び出しをリトライする。
    /// その他の例外に対しては一切ケアしない。
    /// </remarks>
    public async ValueTask<TResult> Try<TResult>(CancellationToken breaker, ApiInvoker<TResult> invoker)
    {
        var count = 0;
        while (true)
        {
            try
            {
                return await invoker(this.Client, breaker);
            }
            catch (ApiLimitResponseException ex) when (count < this.MaxTryCount)
            {
                var watch = Stopwatch.StartNew();

                // 制限ハンドラを呼び出し
                var handler = this.LimitHandler;
                if (handler != null)
                {
                    var args = new ApiLimitHandlerArgs(ex);
                    await handler(args);
                }

                // 強制続行が指定されていなくて、必要な待機時間に足りていない場合は待機する
                var timeElapsed = watch.Elapsed;
                var needWait = TimeSpan.FromSeconds(ex.RetryAfter);
                if (timeElapsed < needWait)
                {
                    var restTime = needWait - timeElapsed;
                    var waitTime = restTime + TimeSpan.FromMilliseconds(100);
                    await Task.Delay(waitTime, breaker);
                }
            }
        }
    }

    /// <summary>API呼び出し制限に対処するヘルパメソッド</summary>
    /// <param name="invoker">API呼び出し処理</param>
    /// <typeparam name="TResult">APIの戻り値型</typeparam>
    /// <returns>APIの戻り値</returns>
    public ValueTask<TResult> Try<TResult>(ApiInvoker<TResult> invoker)
        => this.Try(this.breaker, invoker);

    /// <summary>API呼び出し制限に対処するヘルパメソッド(戻り値なし)</summary>
    /// <param name="breaker">キャンセルトークン</param>
    /// <param name="invoker">API呼び出し処理</param>
    public async ValueTask Try(CancellationToken breaker, ApiInvoker invoker)
    {
        await Try(breaker, async (c, b) => { await invoker(c, b); return 0; });
    }

    /// <summary>API呼び出し制限に対処するヘルパメソッド(戻り値なし)</summary>
    /// <param name="invoker">API呼び出し処理</param>
    public ValueTask Try(ApiInvoker invoker)
        => this.Try(this.breaker, invoker);
    #endregion

    #region ユーティリティ：棚
    /// <summary>全ての棚を列挙する</summary>
    /// <param name="batchCount">一度に取得する件数</param>
    /// <param name="filters">フィルタ条件</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得した棚情報を列挙する非同期シーケンス</returns>
    public async IAsyncEnumerable<ShelfSummary> EnumerateAllShelvesAsync(int batchCount, IReadOnlyList<Filter>? filters = default, [EnumeratorCancellation] CancellationToken cancelToken = default)
    {
        var ct = cancelToken == default ? this.breaker : cancelToken;
        var offset = 0;
        while (true)
        {
            var shelves = await this.Try(ct, (c, t) => c.ListShelvesAsync(new(offset, count: batchCount, filters: filters), t));
            foreach (var shelf in shelves.data)
            {
                yield return shelf;
            }

            offset += shelves.data.Length;
            var finished = (shelves.data.Length <= 0) || (shelves.total <= offset);
            if (finished) break;
        }
    }

    /// <summary>全ての棚を列挙する</summary>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得した棚情報を列挙する非同期シーケンス</returns>
    public IAsyncEnumerable<ShelfSummary> EnumerateAllShelvesAsync(CancellationToken cancelToken = default)
        => this.EnumerateAllShelvesAsync(this.BatchCount, default, cancelToken);
    #endregion

    #region ユーティリティ：本
    /// <summary>全ての本を列挙する</summary>
    /// <param name="batchCount">一度に取得する件数</param>
    /// <param name="filters">フィルタ条件</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得した本情報を列挙する非同期シーケンス</returns>
    public async IAsyncEnumerable<BookSummary> EnumerateAllBooksAsync(int batchCount, IReadOnlyList<Filter>? filters = default, [EnumeratorCancellation] CancellationToken cancelToken = default)
    {
        var ct = cancelToken == default ? this.breaker : cancelToken;
        var offset = 0;
        while (true)
        {
            var books = await this.Try(ct, (c, t) => c.ListBooksAsync(new(offset, count: batchCount, filters: filters), t));
            foreach (var book in books.data)
            {
                yield return book;
            }

            offset += books.data.Length;
            var finished = (books.data.Length <= 0) || (books.total <= offset);
            if (finished) break;
        }
    }

    /// <summary>全ての本を列挙する</summary>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得した本情報を列挙する非同期シーケンス</returns>
    public IAsyncEnumerable<BookSummary> EnumerateAllBooksAsync(CancellationToken cancelToken = default)
        => this.EnumerateAllBooksAsync(this.BatchCount, default, cancelToken);
    #endregion

    #region ユーティリティ：チャプタ
    /// <summary>全てのチャプタを列挙する</summary>
    /// <param name="batchCount">一度に取得する件数</param>
    /// <param name="filters">フィルタ条件</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得したチャプタ情報を列挙する非同期シーケンス</returns>
    public async IAsyncEnumerable<ChapterSummary> EnumerateAllChaptersAsync(int batchCount, IReadOnlyList<Filter>? filters = default, [EnumeratorCancellation] CancellationToken cancelToken = default)
    {
        var ct = cancelToken == default ? this.breaker : cancelToken;
        var offset = 0;
        while (true)
        {
            var pages = await this.Try(ct, (c, t) => c.ListChaptersAsync(new(offset, count: batchCount, filters: filters), t));
            foreach (var page in pages.data)
            {
                yield return page;
            }

            offset += pages.data.Length;
            var finished = (pages.data.Length <= 0) || (pages.total <= offset);
            if (finished) break;
        }
    }

    /// <summary>全てのチャプタを列挙する</summary>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得したチャプタ情報を列挙する非同期シーケンス</returns>
    public IAsyncEnumerable<ChapterSummary> EnumerateAllChaptersAsync(CancellationToken cancelToken = default)
        => this.EnumerateAllChaptersAsync(this.BatchCount, default, cancelToken);
    #endregion

    #region ユーティリティ：ページ
    /// <summary>全てのページを列挙する</summary>
    /// <param name="batchCount">一度に取得する件数</param>
    /// <param name="filters">フィルタ条件</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得したページ情報を列挙する非同期シーケンス</returns>
    public async IAsyncEnumerable<PageSummary> EnumerateAllPagesAsync(int batchCount, IReadOnlyList<Filter>? filters = default, [EnumeratorCancellation] CancellationToken cancelToken = default)
    {
        var ct = cancelToken == default ? this.breaker : cancelToken;
        var offset = 0;
        while (true)
        {
            var pages = await this.Try(ct, (c, t) => c.ListPagesAsync(new(offset, count: batchCount, filters: filters), t));
            foreach (var page in pages.data)
            {
                yield return page;
            }

            offset += pages.data.Length;
            var finished = (pages.data.Length <= 0) || (pages.total <= offset);
            if (finished) break;
        }
    }

    /// <summary>全てのページを列挙する</summary>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得したページ情報を列挙する非同期シーケンス</returns>
    public IAsyncEnumerable<PageSummary> EnumerateAllPagesAsync(CancellationToken cancelToken = default)
        => this.EnumerateAllPagesAsync(this.BatchCount, default, cancelToken);
    #endregion

    #region ユーティリティ：ユーザ
    /// <summary>全てのユーザを列挙する</summary>
    /// <param name="batchCount">一度に取得する件数</param>
    /// <param name="filters">フィルタ条件</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得したユーザ情報を列挙する非同期シーケンス</returns>
    public async IAsyncEnumerable<UserSummary> EnumerateAllUsersAsync(int batchCount, IReadOnlyList<Filter>? filters = default, [EnumeratorCancellation] CancellationToken cancelToken = default)
    {
        var ct = cancelToken == default ? this.breaker : cancelToken;
        var offset = 0;
        var allUsers = new List<UserSummary>();
        while (true)
        {
            var users = await this.Try(ct, (c, t) => c.ListUsersAsync(new(offset, count: batchCount, filters: filters), t));
            foreach (var user in users.data)
            {
                yield return user;
            }

            offset += users.data.Length;
            var finished = (users.data.Length <= 0) || (users.total <= offset);
            if (finished) break;
        }
    }

    /// <summary>全てのユーザを列挙する</summary>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得したユーザ情報を列挙する非同期シーケンス</returns>
    public IAsyncEnumerable<UserSummary> EnumerateAllUsersAsync(CancellationToken cancelToken = default)
        => this.EnumerateAllUsersAsync(this.BatchCount, default, cancelToken);
    #endregion

    #region ユーティリティ：ロール
    /// <summary>全てのロールを列挙する</summary>
    /// <param name="batchCount">一度に取得する件数</param>
    /// <param name="filters">フィルタ条件</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得したロール情報を列挙する非同期シーケンス</returns>
    public async IAsyncEnumerable<RoleSummary> EnumerateAllRolesAsync(int batchCount, IReadOnlyList<Filter>? filters = default, [EnumeratorCancellation] CancellationToken cancelToken = default)
    {
        var ct = cancelToken == default ? this.breaker : cancelToken;
        var offset = 0;
        var allRoles = new List<RoleSummary>();
        while (true)
        {
            var roles = await this.Try(ct, (c, t) => c.ListRolesAsync(new(offset, count: batchCount, filters: filters), t));
            foreach (var role in roles.data)
            {
                yield return role;
            }

            offset += roles.data.Length;
            var finished = (roles.data.Length <= 0) || (roles.total <= offset);
            if (finished) break;
        }
    }

    /// <summary>全てのロールを列挙する</summary>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得したロール情報を列挙する非同期シーケンス</returns>
    public IAsyncEnumerable<RoleSummary> EnumerateAllRolesAsync(CancellationToken cancelToken = default)
        => this.EnumerateAllRolesAsync(this.BatchCount, default, cancelToken);
    #endregion

    #region ユーティリティ：添付アイテム
    /// <summary>全ての添付アイテムを列挙する</summary>
    /// <param name="batchCount">一度に取得する件数</param>
    /// <param name="filters">フィルタ条件</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得した添付アイテム情報を列挙する非同期シーケンス</returns>
    public async IAsyncEnumerable<AttachmentItem> EnumerateAllAttachmentsAsync(int batchCount, IReadOnlyList<Filter>? filters = default, [EnumeratorCancellation] CancellationToken cancelToken = default)
    {
        var ct = cancelToken == default ? this.breaker : cancelToken;
        var offset = 0;
        while (true)
        {
            // Obtain attachment information.
            var attachments = await this.Try(ct, (c, t) => c.ListAttachmentsAsync(new(offset, count: batchCount, filters: filters), t));
            foreach (var attach in attachments.data)
            {
                yield return attach;
            }

            // Update search information and determine end of search.
            offset += attachments.data.Length;
            var finished = (attachments.data.Length <= 0) || (attachments.total <= offset);
            if (finished) break;
        }
    }

    /// <summary>全ての添付アイテムを列挙する</summary>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得した添付アイテム情報を列挙する非同期シーケンス</returns>
    public IAsyncEnumerable<AttachmentItem> EnumerateAllAttachmentsAsync(CancellationToken cancelToken = default)
        => this.EnumerateAllAttachmentsAsync(this.BatchCount, default, cancelToken);

    /// <summary>ページの添付アイテムを列挙する</summary>
    /// <param name="pageId">対象とするページID</param>
    /// <param name="batchCount">一度に取得する件数</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得した添付アイテムを列挙する非同期シーケンス</returns>
    public IAsyncEnumerable<AttachmentItem> EnumeratePageAttachmentsAsync(long pageId, int batchCount, CancellationToken cancelToken = default)
    {
        // Filter criteria to identify the target page.
        var pageFilter = new Filter[]
        {
            new ($"uploaded_to", $"{pageId}"),
        };

        return this.EnumerateAllAttachmentsAsync(batchCount, pageFilter, cancelToken);
    }

    /// <summary>ページの添付アイテムを列挙する</summary>
    /// <param name="pageId">対象とするページID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得した添付アイテムを列挙する非同期シーケンス</returns>
    public IAsyncEnumerable<AttachmentItem> EnumeratePageAttachmentsAsync(long pageId, CancellationToken cancelToken = default)
        => this.EnumeratePageAttachmentsAsync(pageId, this.BatchCount, cancelToken);
    #endregion

    #region ユーティリティ：画像
    /// <summary>全ての画像を列挙する</summary>
    /// <param name="batchCount">一度に取得する件数</param>
    /// <param name="filters">フィルタ条件</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得した画像情報を列挙する非同期シーケンス</returns>
    public async IAsyncEnumerable<ImageSummary> EnumerateAllImagesAsync(int batchCount, IReadOnlyList<Filter>? filters = default, [EnumeratorCancellation] CancellationToken cancelToken = default)
    {
        var ct = cancelToken == default ? this.breaker : cancelToken;
        var offset = 0;
        while (true)
        {
            // Obtain image information.
            var images = await this.Try(ct, (c, t) => c.ListImagesAsync(new(offset, count: batchCount, filters: filters), t));
            foreach (var image in images.data)
            {
                yield return image;
            }

            // Update search information and determine end of search.
            offset += images.data.Length;
            var finished = (images.data.Length <= 0) || (images.total <= offset);
            if (finished) break;
        }
    }

    /// <summary>全ての画像を列挙する</summary>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得した画像情報を列挙する非同期シーケンス</returns>
    public IAsyncEnumerable<ImageSummary> EnumerateAllImagesAsync(CancellationToken cancelToken = default)
        => this.EnumerateAllImagesAsync(this.BatchCount, default, cancelToken);

    /// <summary>ページの画像を列挙する</summary>
    /// <param name="pageId">対象とするページID</param>
    /// <param name="batchCount">一度に取得する件数</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得した画像情報を列挙する非同期シーケンス</returns>
    public IAsyncEnumerable<ImageSummary> EnumeratePageImagesAsync(long pageId, int batchCount, CancellationToken cancelToken = default)
    {
        // Filter criteria to identify the target page.
        var pageFilter = new Filter[]
        {
            new ($"uploaded_to", $"{pageId}"),
        };

        return this.EnumerateAllImagesAsync(batchCount, pageFilter, cancelToken);
    }

    /// <summary>ページの画像を列挙する</summary>
    /// <param name="pageId">対象とするページID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得した画像情報を列挙する非同期シーケンス</returns>
    public IAsyncEnumerable<ImageSummary> EnumeratePageImagesAsync(long pageId, CancellationToken cancelToken = default)
        => this.EnumeratePageImagesAsync(pageId, this.BatchCount, cancelToken);
    #endregion

    #region ユーティリティ：ゴミ箱
    /// <summary>全てのゴミ箱アイテムを列挙する</summary>
    /// <param name="batchCount">一度に取得する件数</param>
    /// <param name="filters">フィルタ条件</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得したゴミ箱アイテムを列挙する非同期シーケンス</returns>
    public async IAsyncEnumerable<RecycleItem> EnumerateAllRecycleItemsAsync(int batchCount, IReadOnlyList<Filter>? filters = default, [EnumeratorCancellation] CancellationToken cancelToken = default)
    {
        var ct = cancelToken == default ? this.breaker : cancelToken;
        var offset = 0;
        while (true)
        {
            var items = await this.Try(ct, (c, t) => c.ListRecycleBinAsync(new(offset, count: batchCount, filters: filters), t));
            foreach (var item in items.data)
            {
                yield return item;
            }

            offset += items.data.Length;
            var finished = (items.data.Length <= 0) || (items.total <= offset);
            if (finished) break;
        }
    }

    /// <summary>全てのゴミ箱アイテムを列挙する</summary>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得したゴミ箱アイテムを列挙する非同期シーケンス</returns>
    public IAsyncEnumerable<RecycleItem> EnumerateAllRecycleItemsAsync(CancellationToken cancelToken = default)
        => this.EnumerateAllRecycleItemsAsync(this.BatchCount, default, cancelToken);
    #endregion

    #region ユーティリティ：監査ログ
    /// <summary>全ての監査ログを列挙する</summary>
    /// <param name="batchCount">一度に取得する件数</param>
    /// <param name="filters">フィルタ条件</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得した監査ログを列挙する非同期シーケンス</returns>
    public async IAsyncEnumerable<AuditLogItem> EnumerateAllAuditLogsAsync(int batchCount, IReadOnlyList<Filter>? filters = default, [EnumeratorCancellation] CancellationToken cancelToken = default)
    {
        var ct = cancelToken == default ? this.breaker : cancelToken;
        var offset = 0;
        while (true)
        {
            var logs = await this.Try(ct, (c, t) => c.ListAuditLogAsync(new(offset, count: batchCount, filters: filters), t));
            foreach (var log in logs.data)
            {
                yield return log;
            }

            offset += logs.data.Length;
            var finished = (logs.data.Length <= 0) || (logs.total <= offset);
            if (finished) break;
        }
    }

    /// <summary>全ての監査ログを列挙する</summary>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得した監査ログを列挙する非同期シーケンス</returns>
    public IAsyncEnumerable<AuditLogItem> EnumerateAllAuditLogsAsync(CancellationToken cancelToken = default)
        => this.EnumerateAllAuditLogsAsync(this.BatchCount, default, cancelToken);
    #endregion

    #region 破棄
    /// <summary>Dispose resources</summary>
    public void Dispose()
    {
        if (this.ownClient)
        {
            this.Client.Dispose();
        }
    }
    #endregion

    // 非公開フィールド
    #region リソース管理
    /// <summary>クライアントを所有しているか否か</summary>
    private readonly bool ownClient;

    /// <summary>デフォルトのキャンセルトークン</summary>
    private readonly CancellationToken breaker;
    #endregion
}

