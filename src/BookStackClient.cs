using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookStackApiClient;

/// <summary>
/// BookStack API クライアント
/// </summary>
public class BookStackClient : IDisposable
{
    // 構築
    #region コンストラクタ
    /// <summary>コンストラクタ</summary>
    /// <param name="baseUri">
    /// APIベースURI。"http://localhost:8080/api/" のようなURIを指定する。
    /// API要求のエンドポイントはこのベースURIからの相対で決定する。
    /// その際にはUriクラスの相対パス連結の仕様に沿うため、末尾の区切り記号が重要な意味を持つ点に注意。
    /// たとえばベースが "http://localhost:8080/api/" である場合、docs APIのエンドポイントは "http://localhost:8080/api/docs" となる。
    /// しかしベースが "http://localhost:8080/api" の場合は "http://localhost:8080/docs" となる。
    /// </param>
    /// <param name="token">APIトークンID</param>
    /// <param name="secret">APIトークンシークレット。</param>
    /// <param name="clientFactory">HttpClient生成デリゲート。IHttpClientFactory による生成を仲介することを推奨。指定しない場合は新しくインスタンスを生成する。</param>
    public BookStackClient(Uri baseUri, string token, string secret, Func<HttpClient>? clientFactory = null)
    {
        this.BaseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
        this.http = clientFactory?.Invoke() ?? new HttpClient();
        this.http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", $"{token}:{secret}");
    }
    #endregion

    // 公開プロパティ
    #region APIアクセス情報
    /// <summary>APIベースURI</summary>
    public Uri BaseUri { get; }
    #endregion

    // 公開メソッド
    #region docs
    /// <summary>APIドキュメントを取得する。</summary>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得結果のAPIドキュメント</returns>
    public Task<ApiDocResult> DocsAsync(CancellationToken cancelToken = default)
        => contextGetRequest(apiEp("docs.json"), cancelToken).JsonResponseAsync<ApiDocResult>();
    #endregion

    #region attachments
    /// <summary>添付ファイル/リンクの一覧を取得する。</summary>
    /// <param name="listing">リスト要求オプション</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得結果の添付ファイル一覧</returns>
    public Task<ListAttachmentsResult> ListAttachmentsAsync(ListingOptions? listing = null, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp("attachments", listing), cancelToken).JsonResponseAsync<ListAttachmentsResult>();

    /// <summary>ファイルパスを指定してファイルを添付する。</summary>
    /// <param name="args">ファイル添付共通パラメータ</param>
    /// <param name="path">添付するファイルのパス</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>添付されたファイル情報</returns>
    public Task<AttachmentItem> CreateFileAttachmentAsync(CreateAttachmentArgs args, string path, CancellationToken cancelToken = default)
        => createFileAttachmentAsync(apiEp("attachments"), args, pathFileContentGenerator(path) ?? throw new ArgumentNullException(nameof(path)), cancelToken).JsonResponseAsync<AttachmentItem>();

    /// <summary>ファイル内容を指定してファイルを添付する。</summary>
    /// <param name="args">ファイル添付共通パラメータ</param>
    /// <param name="content">添付するファイル内容</param>
    /// <param name="fileName">添付するファイル名称</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>添付されたファイルの情報</returns>
    public Task<AttachmentItem> CreateFileAttachmentAsync(CreateAttachmentArgs args, byte[] content, string fileName, CancellationToken cancelToken = default)
        => createFileAttachmentAsync(apiEp("attachments"), args, binaryFileContentGenerator(content, fileName) ?? throw new ArgumentNullException(nameof(content)), cancelToken).JsonResponseAsync<AttachmentItem>();

    /// <summary>外部リンクを添付する。</summary>
    /// <param name="args">外部リンクの添付パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>添付されたリンクの情報</returns>
    public Task<AttachmentItem> CreateLinkAttachmentAsync(CreateLinkAttachmentArgs args, CancellationToken cancelToken = default)
        => contextPostRequest(apiEp("attachments"), notNullArgs(args), cancelToken).JsonResponseAsync<AttachmentItem>();

    /// <summary>添付ファイル/リンクの詳細と内容を取得する。</summary>
    /// <param name="id">添付ファイル/リンクID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>添付ファイル詳細情報</returns>
    public Task<ReadAttachmentResult> ReadAttachmentAsync(long id, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp($"attachments/{id}"), cancelToken).JsonResponseAsync<ReadAttachmentResult>();

    /// <summary>ファイルパスを指定して添付ファイルを更新する。</summary>
    /// <param name="id">添付ファイル/リンクID</param>
    /// <param name="args">添付ファイル更新パラメータ</param>
    /// <param name="path">添付するファイルのパス</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>添付ファイル情報</returns>
    public Task<AttachmentItem> UpdateFileAttachmentAsync(long id, UpdateAttachmentArgs args, string? path = null, CancellationToken cancelToken = default)
        => updateFileAttachmentAsync(apiEp($"attachments/{id}"), args, pathFileContentGenerator(path), cancelToken).JsonResponseAsync<AttachmentItem>();

    /// <summary>ファイル内容を指定して添付ファイルを更新する。</summary>
    /// <param name="id">添付ファイル/リンクID</param>
    /// <param name="args">添付ファイル更新パラメータ</param>
    /// <param name="content">添付するファイル内容</param>
    /// <param name="fileName">添付するファイル名称</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>添付ファイル情報</returns>
    public Task<AttachmentItem> UpdateFileAttachmentAsync(long id, UpdateAttachmentArgs args, byte[]? content, string fileName, CancellationToken cancelToken = default)
        => updateFileAttachmentAsync(apiEp($"attachments/{id}"), args, binaryFileContentGenerator(content, fileName), cancelToken).JsonResponseAsync<AttachmentItem>();

    /// <summary>添付リンクを更新する。</summary>
    /// <param name="id">添付ファイル/リンクID</param>
    /// <param name="args">添付フリン更新パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>添付ファイル情報</returns>
    public Task<AttachmentItem> UpdateLinkAttachmentAsync(long id, UpdateLinkAttachmentArgs args, CancellationToken cancelToken = default)
        => contextPutRequest(apiEp($"attachments/{id}"), notNullArgs(args), cancelToken, JsonIgnoreNulls).JsonResponseAsync<AttachmentItem>();

    /// <summary>添付ファイル/リンクを削除する。</summary>
    /// <param name="id">添付ファイル/リンクID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    public Task DeleteAttachmentAsync(long id, CancellationToken cancelToken = default)
        => contextDeleteRequest(apiEp($"attachments/{id}"), cancelToken).JsonResponseAsync<EmptyResult>();
    #endregion

    #region books
    /// <summary>ブックの一覧を取得する。</summary>
    /// <param name="listing">リスト要求オプション</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得結果のブック一覧</returns>
    public Task<ListBooksResult> ListBooksAsync(ListingOptions? listing = null, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp("books", listing), cancelToken).JsonResponseAsync<ListBooksResult>();

    /// <summary>ブックを作成する。</summary>
    /// <param name="args">ブック作成パラメータ</param>
    /// <param name="imgPath">ブックカバーにする画像ファイルパス</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>添付されたファイル情報</returns>
    public Task<BookItem> CreateBookAsync(CreateBookArgs args, string? imgPath = null, CancellationToken cancelToken = default)
        => contextCreateBook(apiEp("books"), args, pathFileContentGenerator(imgPath), cancelToken).JsonResponseAsync<BookItem>();

    /// <summary>ブックを作成する。</summary>
    /// <param name="args">ブック作成パラメータ</param>
    /// <param name="imgContent">ブックカバーにする画像バイナリ</param>
    /// <param name="imgName">ブックカバー画像名称</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>添付されたファイル情報</returns>
    public Task<BookItem> CreateBookAsync(CreateBookArgs args, byte[] imgContent, string imgName, CancellationToken cancelToken = default)
        => contextCreateBook(apiEp("books"), args, binaryFileContentGenerator(imgContent, imgName), cancelToken).JsonResponseAsync<BookItem>();

    /// <summary>ブックの詳細と内容を取得する。</summary>
    /// <param name="id">添付ファイル/リンクID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ブック詳細情報</returns>
    public Task<ReadBookResult> ReadBookAsync(long id, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp($"books/{id}"), cancelToken).JsonResponseAsync<ReadBookResult>();

    /// <summary>ブックを更新する。</summary>
    /// <param name="id">ブックID</param>
    /// <param name="args">ブック更新パラメータ</param>
    /// <param name="imgPath">ブックカバーにする画像ファイルパス</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ブック情報</returns>
    public Task<BookItem> UpdateBookAsync(long id, UpdateBookArgs args, string? imgPath = null, CancellationToken cancelToken = default)
        => contextUpdateBook(apiEp($"books/{id}"), args, pathFileContentGenerator(imgPath), cancelToken).JsonResponseAsync<BookItem>();

    /// <summary>ブックを更新する。</summary>
    /// <param name="id">ブックID</param>
    /// <param name="args">ブック更新パラメータ</param>
    /// <param name="imgContent">ブックカバーにする画像バイナリ</param>
    /// <param name="imgName">ブックカバー画像名称</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ブック情報</returns>
    public Task<BookItem> UpdateBookAsync(long id, UpdateBookArgs args, byte[] imgContent, string imgName, CancellationToken cancelToken = default)
        => contextUpdateBook(apiEp($"books/{id}"), args, binaryFileContentGenerator(imgContent, imgName), cancelToken).JsonResponseAsync<BookItem>();

    /// <summary>ブックを削除する。</summary>
    /// <param name="id">ブックID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    public Task DeleteBookAsync(long id, CancellationToken cancelToken = default)
        => contextDeleteRequest(apiEp($"books/{id}"), cancelToken).JsonResponseAsync<EmptyResult>();

    /// <summary>ブックをHTMLでエクスポートする</summary>
    /// <param name="id">ブックID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ブック内容HTML</returns>
    public Task<string> ExportBookHtmlAsync(long id, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp($"books/{id}/export/html"), cancelToken).TextResponseAsync();

    /// <summary>ブックをプレーンテキストでエクスポートする</summary>
    /// <param name="id">ブックID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ブック内容プレーンテキスト</returns>
    public Task<string> ExportBookPlainAsync(long id, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp($"books/{id}/export/plaintext"), cancelToken).TextResponseAsync();

    /// <summary>ブックをMarkdownでエクスポートする</summary>
    /// <param name="id">ブックID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ブック内容Markdown</returns>
    public Task<string> ExportBookMarkdownAsync(long id, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp($"books/{id}/export/markdown"), cancelToken).TextResponseAsync();

    /// <summary>ブックをPDFでエクスポートする</summary>
    /// <param name="id">ブックID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ブック内容PDF</returns>
    public Task<byte[]> ExportBookPdfAsync(long id, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp($"books/{id}/export/pdf"), cancelToken).BinaryResponseAsync();
    #endregion

    #region chapters
    /// <summary>チャプタの一覧を取得する。</summary>
    /// <param name="listing">リスト要求オプション</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得結果のチャプタ一覧</returns>
    public Task<ListChaptersResult> ListChaptersAsync(ListingOptions? listing = null, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp("chapters", listing), cancelToken).JsonResponseAsync<ListChaptersResult>();

    /// <summary>チャプタを作成する。</summary>
    /// <param name="args">チャプタ作成パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>チャプタ情報</returns>
    public Task<ChapterItem> CreateChapterAsync(CreateChapterArgs args, CancellationToken cancelToken = default)
        => contextPostRequest(apiEp("chapters"), args, cancelToken, JsonIgnoreNulls).JsonResponseAsync<ChapterItem>();

    /// <summary>チャプタの詳細と内容を取得する。</summary>
    /// <param name="id">チャプタID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>チャプタ詳細情報</returns>
    public Task<ReadChapterResult> ReadChapterAsync(long id, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp($"chapters/{id}"), cancelToken).JsonResponseAsync<ReadChapterResult>();

    /// <summary>チャプタを更新する。</summary>
    /// <param name="id">チャプタID</param>
    /// <param name="args">チャプタ更新パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>チャプタ情報</returns>
    public Task<ChapterItem> UpdateChapterAsync(long id, UpdateChapterArgs args, CancellationToken cancelToken = default)
        => contextPutRequest(apiEp($"chapters/{id}"), args, cancelToken, JsonIgnoreNulls).JsonResponseAsync<ChapterItem>();

    /// <summary>チャプタを削除する。</summary>
    /// <param name="id">チャプタID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    public Task DeleteChapterAsync(long id, CancellationToken cancelToken = default)
        => contextDeleteRequest(apiEp($"chapters/{id}"), cancelToken).JsonResponseAsync<EmptyResult>();

    /// <summary>チャプタをHTMLでエクスポートする</summary>
    /// <param name="id">チャプタID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>チャプタ内容HTML</returns>
    public Task<string> ExportChapterHtmlAsync(long id, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp($"chapters/{id}/export/html"), cancelToken).TextResponseAsync();

    /// <summary>チャプタをプレーンテキストでエクスポートする</summary>
    /// <param name="id">チャプタID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>チャプタ内容プレーンテキスト</returns>
    public Task<string> ExportChapterPlainAsync(long id, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp($"chapters/{id}/export/plaintext"), cancelToken).TextResponseAsync();

    /// <summary>チャプタをMarkdownでエクスポートする</summary>
    /// <param name="id">チャプタID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>チャプタ内容Markdown</returns>
    public Task<string> ExportChapterMarkdownAsync(long id, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp($"chapters/{id}/export/markdown"), cancelToken).TextResponseAsync();

    /// <summary>チャプタをPDFでエクスポートする</summary>
    /// <param name="id">チャプタID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>チャプタ内容PDF</returns>
    public Task<byte[]> ExportChapterPdfAsync(long id, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp($"chapters/{id}/export/pdf"), cancelToken).BinaryResponseAsync();
    #endregion

    #region pages
    /// <summary>ページの一覧を取得する。</summary>
    /// <param name="listing">リスト要求オプション</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得結果のページ一覧</returns>
    public Task<ListPagesResult> ListPagesAsync(ListingOptions? listing = null, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp("pages", listing), cancelToken).JsonResponseAsync<ListPagesResult>();

    /// <summary>ページを作成する。</summary>
    /// <param name="args">ページ作成パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ページ情報</returns>
    public Task<PageItem> CreatePageAsync(CreatePageArgs args, CancellationToken cancelToken = default)
        => contextPostRequest(apiEp("pages"), args, cancelToken, JsonIgnoreNulls).JsonResponseAsync<PageItem>();

    /// <summary>ブック内にMarkdownでページを作成する。</summary>
    /// <param name="args">ページ作成パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ページ情報</returns>
    public Task<PageItem> CreateMarkdownPageInBookAsync(CreateMarkdownPageInBookArgs args, CancellationToken cancelToken = default)
        => contextPostRequest(apiEp("pages"), args, cancelToken, JsonIgnoreNulls).JsonResponseAsync<PageItem>();

    /// <summary>チャプタ内にMarkdownでページを作成する。</summary>
    /// <param name="args">ページ作成パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ページ情報</returns>
    public Task<PageItem> CreateMarkdownPageInChapterAsync(CreateMarkdownPageInChapterArgs args, CancellationToken cancelToken = default)
        => contextPostRequest(apiEp("pages"), args, cancelToken, JsonIgnoreNulls).JsonResponseAsync<PageItem>();

    /// <summary>ブック内にHTMLでページを作成する。</summary>
    /// <param name="args">ページ作成パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ページ情報</returns>
    public Task<PageItem> CreateHtmlPageInBookAsync(CreateHtmlPageInBookArgs args, CancellationToken cancelToken = default)
        => contextPostRequest(apiEp("pages"), args, cancelToken, JsonIgnoreNulls).JsonResponseAsync<PageItem>();

    /// <summary>チャプタ内にHTMLでページを作成する。</summary>
    /// <param name="args">ページ作成パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ページ情報</returns>
    public Task<PageItem> CreateHtmlPageInChapterAsync(CreateHtmlPageInChapterArgs args, CancellationToken cancelToken = default)
        => contextPostRequest(apiEp("pages"), args, cancelToken, JsonIgnoreNulls).JsonResponseAsync<PageItem>();

    /// <summary>ページの詳細と内容を取得する。</summary>
    /// <param name="id">ページID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ページ詳細情報</returns>
    public Task<ReadPageResult> ReadPageAsync(long id, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp($"pages/{id}"), cancelToken).JsonResponseAsync<ReadPageResult>();

    /// <summary>ページを更新する。</summary>
    /// <param name="id">ページID</param>
    /// <param name="args">ページ更新パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ページ情報</returns>
    public Task<PageItem> UpdatePageAsync(long id, UpdatePageArgs args, CancellationToken cancelToken = default)
        => contextPutRequest(apiEp($"pages/{id}"), args, cancelToken, JsonIgnoreNulls).JsonResponseAsync<PageItem>();

    /// <summary>ページを削除する。</summary>
    /// <param name="id">ページID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    public Task DeletePageAsync(long id, CancellationToken cancelToken = default)
        => contextDeleteRequest(apiEp($"pages/{id}"), cancelToken).JsonResponseAsync<EmptyResult>();

    /// <summary>ページをHTMLでエクスポートする</summary>
    /// <param name="id">ページID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ページ内容HTML</returns>
    public Task<string> ExportPageHtmlAsync(long id, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp($"pages/{id}/export/html"), cancelToken).TextResponseAsync();

    /// <summary>ページをプレーンテキストでエクスポートする</summary>
    /// <param name="id">ページID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ページ内容プレーンテキスト</returns>
    public Task<string> ExportPagePlainAsync(long id, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp($"pages/{id}/export/plaintext"), cancelToken).TextResponseAsync();

    /// <summary>ページをMarkdownでエクスポートする</summary>
    /// <param name="id">ページID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ページ内容Markdown</returns>
    public Task<string> ExportPageMarkdownAsync(long id, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp($"pages/{id}/export/markdown"), cancelToken).TextResponseAsync();

    /// <summary>ページをPDFでエクスポートする</summary>
    /// <param name="id">ページID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ページ内容PDF</returns>
    public Task<byte[]> ExportPagePdfAsync(long id, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp($"pages/{id}/export/pdf"), cancelToken).BinaryResponseAsync();
    #endregion

    #region shelves
    /// <summary>棚の一覧を取得する。</summary>
    /// <param name="listing">リスト要求オプション</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得結果の棚一覧</returns>
    public Task<ListShelvesResult> ListShelvesAsync(ListingOptions? listing = null, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp("shelves", listing), cancelToken).JsonResponseAsync<ListShelvesResult>();

    /// <summary>棚を作成する。</summary>
    /// <param name="args">棚作成パラメータ</param>
    /// <param name="imgPath">棚カバーにする画像ファイルパス</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>添付されたファイル情報</returns>
    public Task<ShelfItem> CreateShelfAsync(CreateShelfArgs args, string? imgPath = null, CancellationToken cancelToken = default)
        => contextCreateShelve(apiEp("shelves"), args, pathFileContentGenerator(imgPath), cancelToken).JsonResponseAsync<ShelfItem>();

    /// <summary>棚を作成する。</summary>
    /// <param name="args">棚作成パラメータ</param>
    /// <param name="imgContent">棚カバーにする画像バイナリ</param>
    /// <param name="imgName">棚カバー画像名称</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>添付されたファイル情報</returns>
    public Task<ShelfItem> CreateShelfAsync(CreateShelfArgs args, byte[] imgContent, string imgName, CancellationToken cancelToken = default)
        => contextCreateShelve(apiEp("shelves"), args, binaryFileContentGenerator(imgContent, imgName), cancelToken).JsonResponseAsync<ShelfItem>();

    /// <summary>棚の詳細と内容を取得する。</summary>
    /// <param name="id">添付ファイル/リンクID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>棚詳細情報</returns>
    public Task<ReadShelfResult> ReadShelfAsync(long id, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp($"shelves/{id}"), cancelToken).JsonResponseAsync<ReadShelfResult>();

    /// <summary>棚を更新する。</summary>
    /// <param name="id">棚ID</param>
    /// <param name="args">棚更新パラメータ</param>
    /// <param name="imgPath">棚カバーにする画像ファイルパス</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>棚情報</returns>
    public Task<ShelfItem> UpdateShelfAsync(long id, UpdateShelfArgs args, string? imgPath = null, CancellationToken cancelToken = default)
        => contextUpdateShelve(apiEp($"shelves/{id}"), args, pathFileContentGenerator(imgPath), cancelToken).JsonResponseAsync<ShelfItem>();

    /// <summary>棚を更新する。</summary>
    /// <param name="id">棚ID</param>
    /// <param name="args">棚更新パラメータ</param>
    /// <param name="imgContent">棚カバーにする画像バイナリ</param>
    /// <param name="imgName">棚カバー画像名称</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>棚情報</returns>
    public Task<ShelfItem> UpdateShelfAsync(long id, UpdateShelfArgs args, byte[] imgContent, string imgName, CancellationToken cancelToken = default)
        => contextUpdateShelve(apiEp($"shelves/{id}"), args, binaryFileContentGenerator(imgContent, imgName), cancelToken).JsonResponseAsync<ShelfItem>();

    /// <summary>棚を削除する。</summary>
    /// <param name="id">棚ID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    public Task DeleteShelfAsync(long id, CancellationToken cancelToken = default)
        => contextDeleteRequest(apiEp($"shelves/{id}"), cancelToken).JsonResponseAsync<EmptyResult>();
    #endregion

    #region image-gallery
    /// <summary>ギャラリ画像の一覧を取得する。</summary>
    /// <param name="listing">リスト要求オプション</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得結果のギャラリ画像一覧</returns>
    public Task<ListImagesResult> ListImagesAsync(ListingOptions? listing = null, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp("image-gallery", listing), cancelToken).JsonResponseAsync<ListImagesResult>();

    /// <summary>ギャラリ画像を作成する。</summary>
    /// <param name="args">ギャラリ画像作成パラメータ</param>
    /// <param name="path">アップロードするファイルのパス</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ギャラリ画像情報</returns>
    public Task<ImageItem> CreateImageAsync(CreateImageArgs args, string path, CancellationToken cancelToken = default)
        => createImageAsync(apiEp("image-gallery"), args, pathFileContentGenerator(path) ?? throw new ArgumentNullException(nameof(path)), cancelToken).JsonResponseAsync<ImageItem>();

    /// <summary>ギャラリ画像を作成する。</summary>
    /// <param name="args">ギャラリ画像作成パラメータ</param>
    /// <param name="content">アップロードするファイル内容</param>
    /// <param name="fileName">アップロードするファイル名称</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ギャラリ画像情報</returns>
    public Task<ImageItem> CreateImageAsync(CreateImageArgs args, byte[] content, string fileName, CancellationToken cancelToken = default)
        => createImageAsync(apiEp("image-gallery"), args, binaryFileContentGenerator(content, fileName) ?? throw new ArgumentNullException(nameof(content)), cancelToken).JsonResponseAsync<ImageItem>();

    /// <summary>ギャラリ画像の詳細と内容を取得する。</summary>
    /// <param name="id">ギャラリ画像ID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ギャラリ画像情報</returns>
    public Task<ImageItem> ReadImageAsync(long id, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp($"image-gallery/{id}"), cancelToken).JsonResponseAsync<ImageItem>();

    /// <summary>ギャラリ画像を更新する。</summary>
    /// <param name="id">ギャラリ画像ID</param>
    /// <param name="args">ギャラリ画像更新パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ギャラリ画像情報</returns>
    public Task<ImageItem> UpdateImageAsync(long id, UpdateImageArgs args, CancellationToken cancelToken = default)
        => contextPutRequest(apiEp($"image-gallery/{id}"), args, cancelToken, JsonIgnoreNulls).JsonResponseAsync<ImageItem>();

    /// <summary>ギャラリ画像を削除する。</summary>
    /// <param name="id">ギャラリ画像ID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    public Task DeleteImageAsync(long id, CancellationToken cancelToken = default)
        => contextDeleteRequest(apiEp($"image-gallery/{id}"), cancelToken).JsonResponseAsync<EmptyResult>();
    #endregion

    #region search
    /// <summary>コンテンツ内容を検索する。</summary>
    /// <param name="args">検索パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>検索結果</returns>
    public Task<SearchResult> SearchAsync(SearchArgs args, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp("search", args), cancelToken).JsonResponseAsync<SearchResult>();
    #endregion

    #region users
    /// <summary>ユーザの一覧を取得する。</summary>
    /// <param name="listing">リスト要求オプション</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得結果のユーザ一覧</returns>
    public Task<ListUsersResult> ListUsersAsync(ListingOptions? listing = null, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp("users", listing), cancelToken).JsonResponseAsync<ListUsersResult>();

    /// <summary>ユーザを作成する。</summary>
    /// <param name="args">ユーザ作成パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ユーザ情報</returns>
    public Task<UserItem> CreateUserAsync(CreateUserArgs args, CancellationToken cancelToken = default)
        => contextPostRequest(apiEp("users"), args, cancelToken, JsonIgnoreNulls).JsonResponseAsync<UserItem>();

    /// <summary>ユーザの詳細と内容を取得する。</summary>
    /// <param name="id">ユーザID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ユーザ詳細情報</returns>
    public Task<UserItem> ReadUserAsync(long id, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp($"users/{id}"), cancelToken).JsonResponseAsync<UserItem>();

    /// <summary>ユーザを更新する。</summary>
    /// <param name="id">ユーザID</param>
    /// <param name="args">ユーザ更新パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ユーザ情報</returns>
    public Task<UserItem> UpdateUserAsync(long id, UpdateUserArgs args, CancellationToken cancelToken = default)
        => contextPutRequest(apiEp($"users/{id}"), args, cancelToken, JsonIgnoreNulls).JsonResponseAsync<UserItem>();

    /// <summary>ユーザを削除する。</summary>
    /// <param name="id">ユーザID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    public Task DeleteUserAsync(long id, CancellationToken cancelToken = default)
        => contextDeleteRequest(apiEp($"users/{id}"), cancelToken).JsonResponseAsync<EmptyResult>();
    #endregion

    #region roles
    /// <summary>ロールの一覧を取得する。</summary>
    /// <param name="listing">リスト要求オプション</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>取得結果のロール一覧</returns>
    public Task<ListRolesResult> ListRolesAsync(ListingOptions? listing = null, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp("roles", listing), cancelToken).JsonResponseAsync<ListRolesResult>();

    /// <summary>ロールを作成する。</summary>
    /// <param name="args">ロール作成パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ロール情報</returns>
    public Task<RoleItem> CreateRoleAsync(CreateRoleArgs args, CancellationToken cancelToken = default)
        => contextPostRequest(apiEp("roles"), args, cancelToken, JsonIgnoreNulls).JsonResponseAsync<RoleItem>();

    /// <summary>ロールの詳細と内容を取得する。</summary>
    /// <param name="id">ロールID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ロール詳細情報</returns>
    public Task<RoleItem> ReadRoleAsync(long id, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp($"roles/{id}"), cancelToken).JsonResponseAsync<RoleItem>();

    /// <summary>ロールを更新する。</summary>
    /// <param name="id">ロールID</param>
    /// <param name="args">ロール更新パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ロール情報</returns>
    public Task<RoleItem> UpdateRoleAsync(long id, UpdateRoleArgs args, CancellationToken cancelToken = default)
        => contextPutRequest(apiEp($"roles/{id}"), args, cancelToken, JsonIgnoreNulls).JsonResponseAsync<RoleItem>();

    /// <summary>ロールを削除する。</summary>
    /// <param name="id">ロールID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    public Task DeleteRoleAsync(long id, CancellationToken cancelToken = default)
        => contextDeleteRequest(apiEp($"roles/{id}"), cancelToken).JsonResponseAsync<EmptyResult>();
    #endregion

    #region content-permissions
    /// <summary>コンテンツパーミッションを取得する。</summary>
    /// <param name="type">コンテンツ種別</param>
    /// <param name="id">コンテンツID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>コンテンツパーミッション情報</returns>
    public Task<ContentPermissionsItem> ReadContentPermissionsAsync(string type, long id, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp($"content-permissions/{type}/{id}"), cancelToken).JsonResponseAsync<ContentPermissionsItem>();

    /// <summary>棚パーミッションを取得する。</summary>
    /// <param name="id">コンテンツID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>棚パーミッション情報</returns>
    public Task<ContentPermissionsItem> ReadShelfPermissionsAsync(long id, CancellationToken cancelToken = default)
        => ReadContentPermissionsAsync("bookshelf", id, cancelToken);

    /// <summary>ブックパーミッションを取得する。</summary>
    /// <param name="id">コンテンツID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ブックパーミッション情報</returns>
    public Task<ContentPermissionsItem> ReadBookPermissionsAsync(long id, CancellationToken cancelToken = default)
        => ReadContentPermissionsAsync("book", id, cancelToken);

    /// <summary>チャプタパーミッションを取得する。</summary>
    /// <param name="id">コンテンツID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>チャプタパーミッション情報</returns>
    public Task<ContentPermissionsItem> ReadChapterPermissionsAsync(long id, CancellationToken cancelToken = default)
        => ReadContentPermissionsAsync("chapter", id, cancelToken);

    /// <summary>ページパーミッションを取得する。</summary>
    /// <param name="id">コンテンツID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ページパーミッション情報</returns>
    public Task<ContentPermissionsItem> ReadPagePermissionsAsync(long id, CancellationToken cancelToken = default)
        => ReadContentPermissionsAsync("page", id, cancelToken);

    /// <summary>コンテンツパーミッションを更新する。</summary>
    /// <param name="type">コンテンツ種別</param>
    /// <param name="id">コンテンツID</param>
    /// <param name="args">更新パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>コンテンツパーミッション情報</returns>
    public Task<ContentPermissionsItem> UpdateContentPermissionsAsync(string type, long id, UpdateContentPermissionsArgs args, CancellationToken cancelToken = default)
        => contextPutRequest(apiEp($"content-permissions/{type}/{id}"), args, cancelToken, JsonIgnoreNulls).JsonResponseAsync<ContentPermissionsItem>();

    /// <summary>棚パーミッションを更新する。</summary>
    /// <param name="id">コンテンツID</param>
    /// <param name="args">更新パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>コンテンツパーミッション情報</returns>
    public Task<ContentPermissionsItem> UpdateShelfPermissionsAsync(long id, UpdateContentPermissionsArgs args, CancellationToken cancelToken = default)
        => UpdateContentPermissionsAsync("bookshelf", id, args, cancelToken);

    /// <summary>ブックパーミッションを更新する。</summary>
    /// <param name="id">コンテンツID</param>
    /// <param name="args">更新パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ブックパーミッション情報</returns>
    public Task<ContentPermissionsItem> UpdateBookPermissionsAsync(long id, UpdateContentPermissionsArgs args, CancellationToken cancelToken = default)
        => UpdateContentPermissionsAsync("book", id, args, cancelToken);

    /// <summary>チャプタパーミッションを更新する。</summary>
    /// <param name="id">コンテンツID</param>
    /// <param name="args">更新パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>チャプタパーミッション情報</returns>
    public Task<ContentPermissionsItem> UpdateChapterPermissionsAsync(long id, UpdateContentPermissionsArgs args, CancellationToken cancelToken = default)
        => UpdateContentPermissionsAsync("chapter", id, args, cancelToken);

    /// <summary>ページパーミッションを更新する。</summary>
    /// <param name="id">コンテンツID</param>
    /// <param name="args">更新パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ページパーミッション情報</returns>
    public Task<ContentPermissionsItem> UpdatePagePermissionsAsync(long id, UpdateContentPermissionsArgs args, CancellationToken cancelToken = default)
        => UpdateContentPermissionsAsync("page", id, args, cancelToken);
    #endregion

    #region recycle-bin
    /// <summary>ゴミ箱内容の一覧を取得する。</summary>
    /// <param name="listing">リスト要求オプション</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>ゴミ箱内容の一覧</returns>
    public Task<ListRecycleBinResult> ListRecycleBinAsync(ListingOptions? listing = null, CancellationToken cancelToken = default)
        => contextGetRequest(apiEp("recycle-bin", listing), cancelToken).JsonResponseAsync<ListRecycleBinResult>();

    /// <summary>ゴミ箱アイテムを復元する。</summary>
    /// <param name="id">ゴミ箱アイテムID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>復元情報</returns>
    public Task<RestoreRecycleItemResult> RestoreRecycleItemAsync(long id, CancellationToken cancelToken = default)
        => contextPutRequest(apiEp($"recycle-bin/{id}"), new object(), cancelToken).JsonResponseAsync<RestoreRecycleItemResult>();

    /// <summary>ゴミ箱アイテムを削除する。</summary>
    /// <param name="id">ゴミ箱アイテムID</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    public Task DestroyRecycleItemAsync(long id, CancellationToken cancelToken = default)
        => contextDeleteRequest(apiEp($"recycle-bin/{id}"), cancelToken).JsonResponseAsync<EmptyResult>();
    #endregion

    #region 破棄
    /// <summary>リソース破棄</summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion

    // 保護メソッド
    #region 破棄
    /// <summary>リソース破棄</summary>
    /// <param name="disposing">マネージ破棄過程であるか否か</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.isDisposed)
        {
            // マネージ破棄過程であればマネージオブジェクトを破棄する
            if (disposing)
            {
                this.http?.Dispose();
            }

            // 破棄済みマーク
            this.isDisposed = true;
        }
    }
    #endregion

    // 非公開型
    #region APIアクセス用の型
    /// <summary>APIエンドポイント型</summary>
    /// <param name="Path">APIパス</param>
    /// <param name="Uri">API URI</param>
    private record struct ApiEndpoint(string Path, Uri Uri);

    /// <summary>空の応答(応答ボディ無し)を期待する際に利用する型</summary>
    private record EmptyResult;

    /// <summary>要求コンテキスト型</summary>
    /// <remarks>
    /// この型はAPI要求の結果を任意の型に解釈するための中継的な役割のクラスとなる。
    /// 各APIによって
    /// </remarks>
    private class RequestContext
    {
        /// <summary>API要求応答を指定するコンストラクタ</summary>
        /// <param name="ep">要求APIのエンドポイント</param>
        /// <param name="cancelToken">キャンセルトークン</param>
        /// <param name="requester">API要求デリゲート</param>
        public RequestContext(ApiEndpoint ep, CancellationToken cancelToken, Func<RequestContext, Task<HttpResponseMessage>> requester)
        {
            this.Api = ep;
            this.CancelToken = cancelToken;
            this.requester = requester;
        }

        /// <summary>APIエンドポイント</summary>
        public ApiEndpoint Api { get; }

        /// <summary>キャンセルトークン</summary>
        public CancellationToken CancelToken { get; }

        /// <summary>API要求を行い応答をJSONとして解釈して型にマッピングする</summary>
        /// <typeparam name="TResult">応答結果データ型</typeparam>
        /// <returns>API応答データ</returns>
        public Task<TResult> JsonResponseAsync<TResult>()
            => interpretResponseAsync(async (rsp) =>
            {
                // 空の応答の場合もある。空応答を期待する場合はデコードせずに。
                if (typeof(EmptyResult).Equals(typeof(TResult))) return default!;

                // JSON応答を取得
                var json = await rsp.Content.ReadFromJsonAsync<JsonDocument>(options: null, this.CancelToken).ConfigureAwait(false)
                    ?? throw new ResponseInterpretException("Response is not JSON.");

                // エラー応答かチェック
                if (json.RootElement.TryGetProperty("error", out var errElem))
                {
                    var code = errElem.TryGetInt32(out var c) ? c : throw new ResponseInterpretException("Cannot detect error code.");
                    var msg = errElem.GetString() ?? throw new ResponseInterpretException("Cannot detect error msg.");
                    throw new ErrorResponseException(code, msg);
                }

                // 応答をデコードして返却
                return json.RootElement.Deserialize<TResult>() ?? throw new ResponseInterpretException($"Cannot decode JSON to {typeof(TResult).Name}.");
            });

        /// <summary>API要求を行い応答をテキストとして解釈する</summary>
        /// <returns>API応答データ</returns>
        public Task<string> TextResponseAsync()
            => interpretResponseAsync((rsp) => rsp.Content.ReadAsStringAsync(this.CancelToken));

        /// <summary>API要求を行い応答をバイナリとして解釈する</summary>
        /// <returns>API応答データ</returns>
        public Task<byte[]> BinaryResponseAsync()
            => interpretResponseAsync((rsp) => rsp.Content.ReadAsByteArrayAsync(this.CancelToken));

        /// <summary>API要求デリゲート</summary>
        private Func<RequestContext, Task<HttpResponseMessage>> requester;

        /// <summary>API要求を行いJSON応答を解釈する</summary>
        /// <typeparam name="TResult">応答データ型</typeparam>
        /// <param name="interpreter">応答の解釈デリゲート</param>
        /// <returns>応答を得るタスク</returns>
        private async Task<TResult> interpretResponseAsync<TResult>(Func<HttpResponseMessage, Task<TResult>> interpreter)
        {
            // API要求
            using var response = await this.requester(this).ConfigureAwait(false);

            // 要求が成功レスポンスを示すかを確認
            if (!response.IsSuccessStatusCode)
            {
                throwRequestException(response);
            }

            // 応答の解釈
            try
            {
                // 応答を解釈
                return await interpreter(response).ConfigureAwait(false);
            }
            catch (BookStackClientException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ResponseInterpretException("An error occurred in the interpretation of the response data.", ex);
            }
        }

        /// <summary>エラーに対するレスポンス内容に応じた例外を送出する</summary>
        /// <param name="response">HTTP応答</param>
        [DoesNotReturn]
        private void throwRequestException(HttpResponseMessage response)
        {
            // API要求数の制限によるエラーの場合は専用の型を送出
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                var limit = tryGetHeaderInt(response, "X-RateLimit-Limit");
                var remain = tryGetHeaderInt(response, "X-RateLimit-Remaining");
                if (limit.HasValue && remain == 0)
                {
                    throw new ApiLimitResponseException(limit.Value, response.ReasonPhrase ?? $"HTTP {(int)response.StatusCode}");
                }
            }

            // 特定のエラー出ない場合は汎用の応答エラー例外
            throw new UnexpectedResponseException(response.ReasonPhrase ?? $"HTTP {(int)response.StatusCode}");
        }

        /// <summary>HTTP応答ヘッダの指定されたヘッダの値を整数として取得を試みる</summary>
        /// <param name="response">HTTP応答</param>
        /// <param name="name">ヘッダ名</param>
        /// <returns>取得できた場合はその整数値。取得できなかった場合は null を返却。</returns>
        private long? tryGetHeaderInt(HttpResponseMessage response, string name)
        {
            if (response.Headers.TryGetValues(name, out var limits))
            {
                foreach (var field in limits)
                {
                    if (long.TryParse(field, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                    {
                        return value;
                    }
                }
            }
            return null;
        }
    }

    /// <summary>API要求時に送信するコンテンツ生成デリゲート型</summary>
    /// <returns>HTTPコンテンツ</returns>
    private delegate HttpContent RequestContentGenerator();

    /// <summary>API要求時に送信するファイルコンテンツ生成デリゲート型</summary>
    /// <returns>ファイルHTTPコンテンツとファイル名称</returns>
    private delegate (HttpContent content, string name) FileContentGenerator();
    #endregion

    // 非公開フィールド
    #region 定数：シリアル化
    /// <summary>値がnullのプロパティを無視するシリアライズオプション</summary>
    private static readonly JsonSerializerOptions JsonIgnoreNulls = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, };
    #endregion

    #region リソース
    /// <summary>HTTPクライアント</summary>
    private readonly HttpClient http;
    #endregion

    #region 状態フラグ
    /// <summary>破棄済みフラグ</summary>
    private bool isDisposed;
    #endregion

    // 非公開メソッド
    #region ユーティリティ
    /// <summary>引数がnull出ないことを検証する</summary>
    /// <typeparam name="T">引数の型</typeparam>
    /// <param name="args">引数</param>
    /// <param name="argsName">引数の名称。通常は指定を省略する。</param>
    /// <returns>引数がnullでなければ渡した引数。nullの場合は例外を送出する。</returns>
    private T notNullArgs<T>(T? args, [CallerArgumentExpression(nameof(args))] string? argsName = null) where T : class
    {
        if (args == null) throw new ArgumentNullException(argsName);
        return args;
    }

    /// <summary>パスを元にHTTP要求用コンテンツの生成デリゲートを作成する</summary>
    /// <param name="imgPath">ファイルパス</param>
    /// <returns>HTTP要求用コンテンツの生成デリゲートと名称のタプル</returns>
    private FileContentGenerator? pathFileContentGenerator(string? imgPath)
    {
        if (imgPath == null) return null;
        return () => (new StreamContent(new FileStream(imgPath, FileMode.Open, FileAccess.Read, FileShare.Read)), Path.GetFileName(imgPath));
    }

    /// <summary>ファイル内容バイナリを元にHTTP要求用コンテンツの生成デリゲートを作成する</summary>
    /// <param name="imgContent">ファイル内容バイナリ</param>
    /// <param name="imgName">ファイル名称</param>
    /// <returns>HTTP要求用コンテンツの生成デリゲートと名称のタプル</returns>
    private FileContentGenerator? binaryFileContentGenerator(byte[]? imgContent, string imgName)
    {
        if (imgContent == null) return null;
        return () => (new ByteArrayContent(imgContent), imgName);
    }
    #endregion

    #region API-Endpoint
    /// <summary>API要求URIを作成する</summary>
    /// <param name="api">APIパス</param>
    /// <returns>APIエンドポイント情報</returns>
    private ApiEndpoint apiEp(string api)
        => new(api, new Uri(this.BaseUri, api));

    /// <summary>リスト要求用のAPI要求URIを作成する</summary>
    /// <param name="api">APIパス</param>
    /// <param name="listing">リスト要求オプション</param>
    /// <returns>APIエンドポイント情報</returns>
    private ApiEndpoint apiEp(string api, ListingOptions? listing)
    {
        // オプション指定がなければパスのみで生成
        if (listing == null)
        {
            return apiEp(api);
        }

        // 基本APIパス
        var query = api;

        // リストオプションがあればそれをURIに構築
        // 何らかのオプションが指定されている場合のみ構築するため、最初はインスタンスを作らない
        var builder = default(StringBuilder);

        // 取得位置
        if (listing.offset.HasValue)
        {
            if (builder == null) builder = new StringBuilder(api).Append('?'); else builder.Append('&');
            builder.Append("offset=").Append(listing.offset.Value);
        }

        // 最大数
        if (listing.count.HasValue)
        {
            if (builder == null) builder = new StringBuilder(api).Append('?'); else builder.Append('&');
            builder.Append("count=").Append(listing.count.Value);
        }

        // ソート
        if (listing.sorts != null)
        {
            if (builder == null) builder = new StringBuilder(api).Append('?'); else builder.Append('&');
            var delimiter = "";
            foreach (var sort in listing.sorts)
            {
                builder.Append(delimiter);
                builder.Append("sort=").Append(sort);
                delimiter = "&";
            }
        }

        // フィルタ
        if (listing.filters != null)
        {
            if (builder == null) builder = new StringBuilder(api).Append('?'); else builder.Append('&');
            var delimiter = "";
            foreach (var filter in listing.filters)
            {
                builder.Append(delimiter);
                builder.Append("filter[").Append(filter.field).Append("]=").Append(filter.expr);
                delimiter = "&";
            }
        }

        // 何らかのオプションを負荷した場合にAPIパスを置き換え
        if (builder != null)
        {
            query = builder.ToString();
        }

        // ベースURIと連結したURIを返却
        var uri = new Uri(this.BaseUri, query);

        return new(api, uri);
    }

    /// <summary>検索用のAPI要求URIを作成する</summary>
    /// <param name="api">APIパス</param>
    /// <param name="search">検索オプション</param>
    /// <returns>APIエンドポイント情報</returns>
    private ApiEndpoint apiEp(string api, SearchArgs search)
    {
        var builder = new StringBuilder(api).Append('?');

        builder.Append("query=").Append(search.query);

        if (search.page.HasValue)
        {
            builder.Append("&page=").Append(search.page.Value);
        }

        if (search.count.HasValue)
        {
            builder.Append("&count=").Append(search.count.Value);
        }

        // ベースURIと連結したURIを返却
        var uri = new Uri(this.BaseUri, builder.ToString());

        return new(api, uri);
    }
    #endregion

    #region API-HTTP
    /// <summary>APIへのGETアクセスコンテキストを生成する。</summary>
    /// <param name="ep">APIエンドポイント</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>要求コンテキスト</returns>
    private RequestContext contextGetRequest(ApiEndpoint ep, CancellationToken cancelToken)
        => new RequestContext(ep, cancelToken, c => this.http.GetAsync(c.Api.Uri, c.CancelToken));

    /// <summary>APIへのPOSTアクセスコンテキストを生成する</summary>
    /// <typeparam name="TArgs">要求パラメータ型</typeparam>
    /// <param name="ep">APIエンドポイント</param>
    /// <param name="args">要求コンテンツとしてJSON形式で送信するパラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <param name="options">シリアライズオプション</param>
    /// <returns>要求コンテキスト</returns>
    private RequestContext contextPostRequest<TArgs>(ApiEndpoint ep, TArgs? args, CancellationToken cancelToken, JsonSerializerOptions? options = null) where TArgs : class
        => new RequestContext(ep, cancelToken, c => this.http.PostAsJsonAsync(c.Api.Uri, args, options, c.CancelToken));

    /// <summary>指定のコンテンツによるAPIへのPOSTアクセスコンテキストを生成する</summary>
    /// <param name="ep">APIエンドポイント</param>
    /// <param name="generator">コンテンツ生成デリゲート</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>要求コンテキスト</returns>
    private RequestContext contextPostContentRequest(ApiEndpoint ep, RequestContentGenerator generator, CancellationToken cancelToken)
        => new RequestContext(ep, cancelToken, async c =>
        {
            using var content = generator();
            return await this.http.PostAsync(c.Api.Uri, content, c.CancelToken).ConfigureAwait(false);
        });

    /// <summary>APIへのPUTアクセスコンテキストを生成する</summary>
    /// <typeparam name="TArgs">要求パラメータ型</typeparam>
    /// <param name="ep">APIエンドポイント</param>
    /// <param name="args">要求コンテンツとしてJSON形式で送信するパラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <param name="options">シリアライズオプション</param>
    /// <returns>要求コンテキスト</returns>
    private RequestContext contextPutRequest<TArgs>(ApiEndpoint ep, TArgs? args, CancellationToken cancelToken, JsonSerializerOptions? options = null) where TArgs : class
        => new RequestContext(ep, cancelToken, c => this.http.PutAsJsonAsync(c.Api.Uri, args, options, c.CancelToken));

    /// <summary>APIへのDELETEアクセスコンテキストを生成する</summary>
    /// <param name="ep">APIエンドポイント</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>要求コンテキスト</returns>
    private RequestContext contextDeleteRequest(ApiEndpoint ep, CancellationToken cancelToken)
        => new RequestContext(ep, cancelToken, c => this.http.DeleteAsync(c.Api.Uri, c.CancelToken));
    #endregion

    #region API-固有
    /// <summary>ファイル添付要求共通処理</summary>
    /// <param name="ep">APIエンドポイント</param>
    /// <param name="args">ファイル添付共通パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <param name="fileContentGenerator">ファイル内容コンテンツ生成デリゲート</param>
    /// <returns>要求コンテキスト</returns>
    private RequestContext createFileAttachmentAsync(ApiEndpoint ep, CreateAttachmentArgs args, FileContentGenerator fileContentGenerator, CancellationToken cancelToken)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(fileContentGenerator);

        return contextPostContentRequest(ep, () =>
        {
            var attach = fileContentGenerator();
            var context = new MultipartFormDataContent();
            context.Add(attach.content, "file", attach.name);
            context.Add(new StringContent(args.name), nameof(args.name));
            context.Add(new StringContent(args.uploaded_to.ToString()), nameof(args.uploaded_to));
            return context;
        }, cancelToken);
    }

    /// <summary>ファイル添付更新要求共通処理</summary>
    /// <param name="ep">APIエンドポイント</param>
    /// <param name="args">ファイル添付共通パラメータ</param>
    /// <param name="fileContentGenerator">ファイル内容コンテンツ生成デリゲート</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>要求コンテキスト</returns>
    private RequestContext updateFileAttachmentAsync(ApiEndpoint ep, UpdateAttachmentArgs args, FileContentGenerator? fileContentGenerator, CancellationToken cancelToken)
    {
        ArgumentNullException.ThrowIfNull(args);

        return contextPostContentRequest(ep, () =>
        {
            var context = new MultipartFormDataContent();
            context.Add(new StringContent("PUT"), "_method");   // contextUpdateBook() 内コメントも参照
            if (fileContentGenerator != null)
            {
                var attach = fileContentGenerator();
                context.Add(attach.content, "file", attach.name);
            }
            if (!string.IsNullOrWhiteSpace(args.name)) context.Add(new StringContent(args.name), nameof(args.name));
            if (args.uploaded_to.HasValue) context.Add(new StringContent(args.uploaded_to.Value.ToString()), nameof(args.uploaded_to));
            return context;
        }, cancelToken);
    }

    /// <summary>ブック作成要求共通処理</summary>
    /// <param name="ep">APIエンドポイント</param>
    /// <param name="args">ブック作成パラメータ</param>
    /// <param name="fileContentGenerator">ブックカバー画像コンテンツ生成デリゲート</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>要求コンテキスト</returns>
    private RequestContext contextCreateBook(ApiEndpoint ep, CreateBookArgs args, FileContentGenerator? fileContentGenerator, CancellationToken cancelToken)
    {
        ArgumentNullException.ThrowIfNull(args);

        return contextPostContentRequest(ep, () =>
        {
            var context = new MultipartFormDataContent();
            context.Add(new StringContent(args.name), nameof(args.name));
            if (args.description != null) context.Add(new StringContent(args.description), nameof(args.description));
            if (args.tags != null)
            {
                for (var i = 0; i < args.tags.Length; i++)
                {
                    var tag = args.tags[i];
                    context.Add(new StringContent(tag.name), $"{nameof(args.tags)}[{i}][{nameof(tag.name)}]");
                    context.Add(new StringContent(tag.value), $"{nameof(args.tags)}[{i}][{nameof(tag.value)}]");
                }
            }
            if (fileContentGenerator != null)
            {
                var image = fileContentGenerator();
                context.Add(image.content, "image", image.name);
            }
            return context;
        }, cancelToken);
    }

    /// <summary>ブック更新要求共通処理</summary>
    /// <param name="ep">APIエンドポイント</param>
    /// <param name="args">ブック更新パラメータ</param>
    /// <param name="fileContentGenerator">ブックカバー画像コンテンツ生成デリゲート</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>要求コンテキスト</returns>
    private RequestContext contextUpdateBook(ApiEndpoint ep, UpdateBookArgs args, FileContentGenerator? fileContentGenerator, CancellationToken cancelToken)
    {
        ArgumentNullException.ThrowIfNull(args);

        // PHP には POST 以外のリクエストで multipart/form-data を解釈しない問題があるらしい。
        // https://github.com/laravel/framework/issues/13457
        // そのため PUT リクエストで送っても正しく動作しない。
        // 問題の回避策としては POST リクエストで '_method' フィールドに要求メソッド種別を指定する方法があるようなので、それを使う。
        // (BookStackのAPIドキュメントにもこの回避策について書かれている)

        return contextPostContentRequest(ep, () =>
        {
            var context = new MultipartFormDataContent();
            context.Add(new StringContent("PUT"), "_method");
            if (!string.IsNullOrWhiteSpace(args.name)) context.Add(new StringContent(args.name), nameof(args.name));
            if (args.description != null) context.Add(new StringContent(args.description), nameof(args.description));
            if (args.tags != null)
            {
                for (var i = 0; i < args.tags.Length; i++)
                {
                    var tag = args.tags[i];
                    context.Add(new StringContent(tag.name), $"{nameof(args.tags)}[{i}][{nameof(tag.name)}]");
                    context.Add(new StringContent(tag.value), $"{nameof(args.tags)}[{i}][{nameof(tag.value)}]");
                }
            }
            if (fileContentGenerator != null)
            {
                var image = fileContentGenerator();
                context.Add(image.content, "image", image.name);
            }
            return context;
        }, cancelToken);
    }

    /// <summary>ギャラリ画像作成要求共通処理</summary>
    /// <param name="ep">APIエンドポイント</param>
    /// <param name="args">ギャラリ画像作成パラメータ</param>
    /// <param name="fileContentGenerator">画像コンテンツ生成デリゲート</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>要求コンテキスト</returns>
    private RequestContext createImageAsync(ApiEndpoint ep, CreateImageArgs args, FileContentGenerator fileContentGenerator, CancellationToken cancelToken)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(fileContentGenerator);

        return contextPostContentRequest(ep, () =>
        {
            var context = new MultipartFormDataContent();
            context.Add(new StringContent(args.uploaded_to.ToString()), nameof(args.uploaded_to));
            context.Add(new StringContent(args.type), nameof(args.type));
            context.Add(new StringContent(args.name), nameof(args.name));
            var image = fileContentGenerator();
            context.Add(image.content, "image", image.name);
            return context;
        }, cancelToken);
    }

    /// <summary>棚作成要求共通処理</summary>
    /// <param name="ep">APIエンドポイント</param>
    /// <param name="args">棚作成パラメータ</param>
    /// <param name="fileContentGenerator">ブックカバー画像コンテンツ生成デリゲート</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>要求コンテキスト</returns>
    private RequestContext contextCreateShelve(ApiEndpoint ep, CreateShelfArgs args, FileContentGenerator? fileContentGenerator, CancellationToken cancelToken)
    {
        ArgumentNullException.ThrowIfNull(args);

        return contextPostContentRequest(ep, () =>
        {
            var context = new MultipartFormDataContent();
            if (!string.IsNullOrWhiteSpace(args.name)) context.Add(new StringContent(args.name), nameof(args.name));
            if (args.description != null) context.Add(new StringContent(args.description), nameof(args.description));
            if (args.tags != null)
            {
                for (var i = 0; i < args.tags.Length; i++)
                {
                    var tag = args.tags[i];
                    context.Add(new StringContent(tag.name), $"{nameof(args.tags)}[{i}][{nameof(tag.name)}]");
                    context.Add(new StringContent(tag.value), $"{nameof(args.tags)}[{i}][{nameof(tag.value)}]");
                }
            }
            if (args.books != null)
            {
                for (var i = 0; i < args.books.Length; i++)
                {
                    var book_id = args.books[i];
                    context.Add(new StringContent(book_id.ToString()), $"{nameof(args.books)}[{i}]");
                }
            }
            if (fileContentGenerator != null)
            {
                var image = fileContentGenerator();
                context.Add(image.content, "image", image.name);
            }
            return context;
        }, cancelToken);
    }

    /// <summary>棚更新要求共通処理</summary>
    /// <param name="ep">APIエンドポイント</param>
    /// <param name="args">棚更新パラメータ</param>
    /// <param name="fileContentGenerator">ブックカバー画像コンテンツ生成デリゲート</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>要求コンテキスト</returns>
    private RequestContext contextUpdateShelve(ApiEndpoint ep, UpdateShelfArgs args, FileContentGenerator? fileContentGenerator, CancellationToken cancelToken)
    {
        ArgumentNullException.ThrowIfNull(args);

        return contextPostContentRequest(ep, () =>
        {
            var context = new MultipartFormDataContent();
            context.Add(new StringContent("PUT"), "_method");
            if (!string.IsNullOrWhiteSpace(args.name)) context.Add(new StringContent(args.name), nameof(args.name));
            if (args.description != null) context.Add(new StringContent(args.description), nameof(args.description));
            if (args.tags != null)
            {
                for (var i = 0; i < args.tags.Length; i++)
                {
                    var tag = args.tags[i];
                    context.Add(new StringContent(tag.name), $"{nameof(args.tags)}[{i}][{nameof(tag.name)}]");
                    context.Add(new StringContent(tag.value), $"{nameof(args.tags)}[{i}][{nameof(tag.value)}]");
                }
            }
            if (args.books != null)
            {
                for (var i = 0; i < args.books.Length; i++)
                {
                    var book_id = args.books[i];
                    context.Add(new StringContent(book_id.ToString()), $"{nameof(args.books)}[{i}]");
                }
            }
            if (fileContentGenerator != null)
            {
                var image = fileContentGenerator();
                context.Add(image.content, "image", image.name);
            }
            return context;
        }, cancelToken);
    }
    #endregion
}
