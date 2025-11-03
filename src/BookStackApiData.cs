using System.Text.Json.Serialization;
using BookStackApiClient.Converters;

namespace BookStackApiClient;

// ここで定義するものは API の JSONインタフェースに利用する型なので、言語の命名標準には従わない。
#pragma warning disable IDE1006

/// <summary>リスト取得時のフィルタ条件</summary>
/// <param name="field">
/// フィルタ対象フィールド名。
/// "{field}:{operator}" の形式で比較演算子を指定することも可能。
/// 比較演算子には eq/ne/gt/lt/gte/lte/like を利用できる。詳細はAPIドキュメントを参照。
/// </param>
/// <param name="expr">フィルタ式</param>
public record struct Filter(string field, string expr);

/// <summary>リスト型情報取得時の共通オプション</summary>
/// <param name="offset">取得対象のオフセット位置(対象内のスキップするデータ数)</param>
/// <param name="count">取得するデータの最大数</param>
/// <param name="sorts">応答結果のソートキー。プレフィクス'+'/'-'により昇順または降順を指定可能。</param>
/// <param name="filters">応答のフィルタ条件。</param>
public record ListingOptions(int? offset = null, int? count = null, IReadOnlyList<string>? sorts = null, IReadOnlyList<Filter>? filters = null);

/// <summary>タグ情報</summary>
/// <param name="name">タグ名</param>
/// <param name="value">タグ値</param>
public record Tag(string name, string value = "");

/// <summary>コンテンツに付与されたタグ情報</summary>
/// <param name="name">タグ名</param>
/// <param name="value">タグ値</param>
/// <param name="order">順序</param>
public record ContentTag(string name, string value, long order) : Tag(name, value);

/// <summary>ユーザ情報</summary>
/// <param name="id">ユーザID</param>
/// <param name="name">ユーザ名</param>
/// <param name="slug">ユーザスラグ</param>
public record User(long id, string name, string? slug);

#region docs
/// <summary>APIドキュメントデータ</summary>
/// <param name="name">API名</param>
/// <param name="uri">APIパス</param>
/// <param name="method">HTTPメソッド</param>
/// <param name="controller">コントローラパス</param>
/// <param name="controller_method">メソッド名</param>
/// <param name="controller_method_kebab">ケバブケースメソッド名</param>
/// <param name="description">API説明</param>
/// <param name="body_params">APIパラメータ</param>
/// <param name="example_request">API要求サンプル</param>
/// <param name="example_response">API応答サンプル</param>
public record ApiDoc(
    string name, string uri, string method,
    string controller, string controller_method, string controller_method_kebab,
    string description, Dictionary<string, string[]> body_params,
    string example_request, string example_response
);

/// <summary>APIドキュメント取得結果</summary>
public class ApiDocResult : Dictionary<string, ApiDoc[]> { }
#endregion

#region system
/// <summary>システム情報</summary>
/// <param name="version">バージョン</param>
/// <param name="instance_id">インスタンスID</param>
/// <param name="app_name">アプリケーション名称</param>
/// <param name="app_logo">ロゴURL</param>
/// <param name="base_url">ベースURL</param>
public record SystemInfo(
    string version,
    string instance_id, string app_name,
    string app_logo, string base_url
);
#endregion

#region attachments
/// <summary>添付ファイル/リンク情報</summary>
/// <param name="id">添付ファイルID</param>
/// <param name="name">添付ファイル名称</param>
/// <param name="extension">添付ファイル拡張子</param>
/// <param name="uploaded_to">添付先ページID</param>
/// <param name="external">外部リンクであるか否か</param>
/// <param name="order">順序</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
public record AttachmentItem(
    long id, string name, string extension,
    long uploaded_to, bool external, long order,
    DateTime created_at, DateTime updated_at,
    long created_by, long updated_by
);

/// <summary>添付ファイル/リンク一覧取得結果</summary>
/// <param name="data">添付ファイル一覧</param>
/// <param name="total">添付ファイル総数</param>
public record ListAttachmentsResult(AttachmentItem[] data, long total);

/// <summary>添付ファイル/リンク追加要求共通パラメータ</summary>
/// <param name="name">ファイル名</param>
/// <param name="uploaded_to">添付先ページID</param>
public record CreateAttachmentArgs(string name, long uploaded_to);

/// <summary>添付リンクの追加要求パラメータ</summary>
/// <param name="name">添付名</param>
/// <param name="uploaded_to">添付先ページID</param>
/// <param name="link">外部リンク</param>
public record CreateLinkAttachmentArgs(string name, long uploaded_to, string link)
{
    /// <summary>共通パラメータとの組み合わせを指定するコンストラクタ</summary>
    /// <param name="args">追加要求共通パラメータ</param>
    /// <param name="link">外部リンク</param>
    public CreateLinkAttachmentArgs(CreateAttachmentArgs args, string link) : this(args?.name ?? throw new ArgumentNullException(nameof(args)), args.uploaded_to, link) { }
}

/// <summary>添付リンク情報</summary>
/// <param name="html">リンクのHTML表現</param>
/// <param name="markdown">リンクのMarkdown表現</param>
public record AttachmentLink(string html, string markdown);

/// <summary>添付ファイル/リンクの詳細情報</summary>
/// <param name="id">添付ファイルID</param>
/// <param name="name">添付名称</param>
/// <param name="extension">添付ファイル拡張子</param>
/// <param name="uploaded_to">添付先ページID</param>
/// <param name="external">外部リンクであるか否か</param>
/// <param name="order">順序</param>
/// <param name="links">添付リンク情報</param>
/// <param name="content">ファイル添付の場合はファイル内容(Base64)。外部リンクの場合はリンク先URL</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
public record ReadAttachmentResult(
    long id, string name, string extension,
    long uploaded_to, bool external, long order,
    AttachmentLink links, string content,
    User created_by, User updated_by,
    DateTime created_at, DateTime updated_at
);

/// <summary>添付ファイル/リンク更新要求共通パラメータ</summary>
/// <param name="name">添付名称</param>
/// <param name="uploaded_to">添付先ページID</param>
public record UpdateAttachmentArgs(string? name = null, long? uploaded_to = null);

/// <summary>添付リンクの更新要求パラメータ</summary>
/// <param name="name">添付名</param>
/// <param name="uploaded_to">添付先ページID</param>
/// <param name="link">外部リンク</param>
public record UpdateLinkAttachmentArgs(string? name = null, long? uploaded_to = null, string? link = null)
{
    /// <summary>共通パラメータとの組み合わせを指定するコンストラクタ</summary>
    /// <param name="args">更新要求共通パラメータ</param>
    /// <param name="link">外部リンク</param>
    public UpdateLinkAttachmentArgs(UpdateAttachmentArgs args, string? link) : this(args?.name ?? throw new ArgumentNullException(nameof(args)), args.uploaded_to, link) { }
}
#endregion

#region books
/// <summary>ブックカバー画像情報</summary>
/// <param name="id">ブックカバー画像ID</param>
/// <param name="name">ブックカバー画像名称</param>
/// <param name="url">ブックカバー画像URL</param>
public record BookCoverSummary(
    long id, string name, string url
);

/// <summary>ブック情報</summary>
/// <param name="id">ブックID</param>
/// <param name="name">ブックの名前</param>
/// <param name="slug">ブックのスラグ</param>
/// <param name="description">ブックの概要</param>
/// <param name="cover">ブックカバー画像</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
/// <param name="owned_by">オーナーユーザ</param>
public record BookSummary(
    long id, string name, string slug, string description, BookCoverSummary? cover,
    DateTime created_at, DateTime updated_at,
    long created_by, long updated_by, long owned_by
);

/// <summary>ブックカバー画像サムネイル情報</summary>
/// <param name="display"></param>
/// <param name="gallery"></param>
public record BookCoverThumbs(string display, string gallery);

/// <summary>ブックカバー画像情報</summary>
/// <param name="id">ブックカバー画像ID</param>
/// <param name="name">ブックカバー画像名称</param>
/// <param name="type">ブックカバー画像種別</param>
/// <param name="uploaded_to">対象ブックID</param>
/// <param name="path">ブックカバー画像パス</param>
/// <param name="url">ブックカバー画像URL</param>
/// <param name="thumbs">ブックカバー画像サムネイル情報</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
public record BookCover(
    long id, string name, string type, long uploaded_to,
    string path, string url, BookCoverThumbs? thumbs,
    DateTime created_at, DateTime updated_at,
    long created_by, long updated_by
);

/// <summary>ブック情報</summary>
/// <param name="id">ブックID</param>
/// <param name="name">ブックの名前</param>
/// <param name="slug">ブックのスラグ</param>
/// <param name="description">ブックの概要</param>
/// <param name="description_html">ブックの概要(HTML表現)</param>
/// <param name="default_template_id">デフォルトテンプレートID</param>
/// <param name="sort_rule_id">ソートルールID</param>
/// <param name="tags">タグ一覧</param>
/// <param name="cover">ブックカバー画像</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
/// <param name="owned_by">オーナーユーザ</param>
public record BookItem(
    long id, string name, string slug,
    string description, string description_html, long? default_template_id, long? sort_rule_id,
    ContentTag[]? tags, BookCover? cover,
    DateTime created_at, DateTime updated_at,
    long created_by, long updated_by, long owned_by
);

/// <summary>ブック一覧取得結果</summary>
/// <param name="data">ブック一覧</param>
/// <param name="total">ブック総数</param>
public record ListBooksResult(BookSummary[] data, long total);

/// <summary>ブック作成要求パラメータ</summary>
/// <param name="name">ブック名</param>
/// <param name="description">ブック概要</param>
/// <param name="description_html">ブック概要(HTML表現)</param>
/// <param name="default_template_id">デフォルトテンプレートID</param>
/// <param name="tags">付与するタグ一覧</param>
public record CreateBookArgs(
    string name, string? description = null, string? description_html = null,
    long? default_template_id = null, IReadOnlyList<Tag>? tags = null
);

/// <summary>ブック内コンテンツ基本クラス</summary>
/// <param name="id">コンテンツID</param>
/// <param name="name">コンテンツ名</param>
/// <param name="slug">コンテンツスラグ</param>
/// <param name="type">コンテンツ種別</param>
/// <param name="book_id">対象ブックID</param>
/// <param name="priority">順序</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
[JsonConverter(typeof(BookContentJsonConverter))]
public abstract record BookContent(long id, string name, string slug, string type, long book_id, long priority, DateTime created_at, DateTime updated_at);

/// <summary>ブック内ページコンテンツ</summary>
/// <param name="id">ページID</param>
/// <param name="name">ページ名</param>
/// <param name="slug">ページスラグ</param>
/// <param name="book_id">所属ブックID</param>
/// <param name="chapter_id">所属チャプタID</param>
/// <param name="type">コンテンツ種別</param>
/// <param name="draft">ドラフトであるか</param>
/// <param name="template">テンプレートであるか</param>
/// <param name="url">ページURL</param>
/// <param name="priority">順序</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
public record BookContentPage(
    long id, string name, string slug,
    string type, long book_id, long? chapter_id,
    bool draft, bool template, string url, long priority,
    DateTime created_at, DateTime updated_at
) : BookContent(id, name, slug, type, book_id, priority, created_at, updated_at);

/// <summary>ブック内チャプタコンテンツ</summary>
/// <param name="id">チャプタID</param>
/// <param name="name">チャプタ名</param>
/// <param name="slug">チャプタスラグ</param>
/// <param name="book_id">所属ブックID</param>
/// <param name="type">コンテンツ種別</param>
/// <param name="url">チャプタURL</param>
/// <param name="pages">チャプタ内ページ</param>
/// <param name="priority">順序</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
public record BookContentChapter(
    long id, string name, string slug,
    string type, long book_id,
    string url, BookContentPage[]? pages, long priority,
    DateTime created_at, DateTime updated_at
) : BookContent(id, name, slug, type, book_id, priority, created_at, updated_at);

/// <summary>ブック詳細情報</summary>
/// <param name="id">ブックID</param>
/// <param name="name">ブック名</param>
/// <param name="slug">ブックスラグ</param>
/// <param name="description">ブック概要</param>
/// <param name="description_html">ブック概要(HTML表現)</param>
/// <param name="default_template_id">デフォルトテンプレートID</param>
/// <param name="sort_rule_id">ソートルールID</param>
/// <param name="owned_by">オーナーユーザ</param>
/// <param name="contents">
/// ブック内コンテンツ。
/// このプロパティのコレクション内にはページとチャプタの両方が含まれる。
/// ページ/チャプタの型付きインスタンスは<see cref="chapters"/>および<see cref="pages"/>プロパティで参照可能。
/// </param>
/// <param name="tags">タグ一覧</param>
/// <param name="cover">ブックカバー画像</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
public record ReadBookResult(
    long id, string name, string slug,
    string description, string description_html, long? default_template_id, long? sort_rule_id,
    BookContent[] contents, ContentTag[]? tags, BookCover? cover,
    DateTime created_at, DateTime updated_at,
    User created_by, User updated_by, User owned_by
)
{
    /// <summary>ブック内チャプタを列挙する</summary>
    /// <returns>ブック内チャプタシーケンス</returns>
    public IEnumerable<BookContentChapter> chapters() => this.contents.OfType<BookContentChapter>();

    /// <summary>ブック内ページを列挙する</summary>
    /// <returns>ブック内ページシーケンス</returns>
    public IEnumerable<BookContentPage> pages() => this.contents.OfType<BookContentPage>();
}

/// <summary>ブック更新要求パラメータ</summary>
/// <param name="name">ブック名</param>
/// <param name="description">ブック概要</param>
/// <param name="description_html">ブック概要(HTML表現)</param>
/// <param name="default_template_id">デフォルトテンプレートID</param>
/// <param name="tags">更新するタグ一覧</param>
public record UpdateBookArgs(
    string? name = null, string? description = null, string? description_html = null,
    long? default_template_id = null, IReadOnlyList<Tag>? tags = null
);
#endregion

#region chapters
/// <summary>チャプタ情報</summary>
/// <param name="id">チャプタID</param>
/// <param name="name">チャプタ名</param>
/// <param name="slug">チャプタスラグ</param>
/// <param name="description">チャプタ概要</param>
/// <param name="book_id">ブックID</param>
/// <param name="book_slug">ブックスラグ</param>
/// <param name="priority">順序</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
/// <param name="owned_by">オーナーユーザ</param>
public record ChapterSummary(
    long id, string name, string slug, string description,
    long book_id, string book_slug, long priority,
    DateTime created_at, DateTime updated_at,
    long created_by, long updated_by, long owned_by
);

/// <summary>チャプタ情報</summary>
/// <param name="id">チャプタID</param>
/// <param name="name">チャプタ名</param>
/// <param name="slug">チャプタスラグ</param>
/// <param name="description">チャプタ概要</param>
/// <param name="description_html">チャプタ概要(HTML表現)</param>
/// <param name="book_id">ブックID</param>
/// <param name="book_slug">ブックスラグ</param>
/// <param name="default_template_id">デフォルトテンプレートID</param>
/// <param name="priority">順序</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
/// <param name="owned_by">オーナーユーザ</param>
/// <param name="tags">タグ一覧</param>
public record ChapterItem(
    long id, string name, string slug, string description, string description_html,
    long book_id, string? book_slug, long? default_template_id, long priority,
    DateTime created_at, DateTime updated_at,
    long created_by, long updated_by, long owned_by,
    ContentTag[] tags
);

/// <summary>チャプタ一覧取得結果</summary>
/// <param name="data">チャプタ一覧</param>
/// <param name="total">チャプタ総数</param>
public record ListChaptersResult(ChapterSummary[] data, long total);

/// <summary>チャプタ作成要求パラメータ</summary>
/// <param name="book_id">ブックID</param>
/// <param name="name">チャプタ名</param>
/// <param name="description">チャプタ概要</param>
/// <param name="description_html">チャプタ概要(HTML表現)</param>
/// <param name="default_template_id">デフォルトテンプレートID</param>
/// <param name="priority">順序</param>
/// <param name="tags">タグ</param>
public record CreateChapterArgs(
    long book_id, string name, string? description = null, string? description_html = null,
    long? default_template_id = null, long? priority = null, IReadOnlyList<Tag>? tags = null
 );

/// <summary>チャプタ内ページコンテンツ</summary>
/// <param name="id">ページID</param>
/// <param name="name">ページ名</param>
/// <param name="slug">ページスラグ</param>
/// <param name="revision_count">リビジョン番号</param>
/// <param name="draft">ドラフトであるか</param>
/// <param name="template">テンプレートであるか</param>
/// <param name="book_id">ブックID</param>
/// <param name="chapter_id">チャプタID</param>
/// <param name="priority">順序</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
public record ChapterContentPage(
    long id, string name, string slug,
    long revision_count, bool draft, bool template,
    long book_id, long chapter_id, long priority,
    DateTime created_at, DateTime updated_at,
    long created_by, long updated_by
);

/// <summary>チャプタ詳細情報</summary>
/// <param name="id">チャプタID</param>
/// <param name="name">チャプタ名</param>
/// <param name="slug">チャプタスラグ</param>
/// <param name="description">チャプタ概要</param>
/// <param name="description_html">チャプタ概要(HTML表現)</param>
/// <param name="default_template_id">デフォルトテンプレートID</param>
/// <param name="book_id">ブックID</param>
/// <param name="book_slug">ブックスラグ</param>
/// <param name="priority">順序</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
/// <param name="owned_by">オーナーユーザ</param>
/// <param name="tags">タグ一覧</param>
/// <param name="pages">ページ一覧</param>
public record ReadChapterResult(
    long id, string name, string slug, string description, string description_html,
    long? default_template_id, long book_id, string book_slug, long priority,
    DateTime created_at, DateTime updated_at,
    User created_by, User updated_by, User owned_by,
    ContentTag[]? tags, ChapterContentPage[] pages
);

/// <summary>チャプタ更新要求パラメータ</summary>
/// <param name="name">チャプタ名</param>
/// <param name="description">チャプタ概要</param>
/// <param name="description_html">チャプタ概要(HTML表現)</param>
/// <param name="default_template_id">デフォルトテンプレートID</param>
/// <param name="priority">順序</param>
/// <param name="tags">タグ</param>
/// <param name="book_id">ブックID</param>
public record UpdateChapterArgs(
    string? name = null, string? description = null, string? description_html = null,
    long? default_template_id = null, long? priority = null, IReadOnlyList<Tag>? tags = null, long? book_id = null
);
#endregion

#region pages
/// <summary>ページ情報</summary>
/// <param name="id">ページID</param>
/// <param name="name">ページ名</param>
/// <param name="slug">ページスラグ</param>
/// <param name="editor">エディタ種別</param>
/// <param name="revision_count">リビジョン番号</param>
/// <param name="draft">ドラフトであるか</param>
/// <param name="template">テンプレートであるか</param>
/// <param name="book_id">ブックID</param>
/// <param name="book_slug">ブックスラグ</param>
/// <param name="chapter_id">チャプタID</param>
/// <param name="priority">順序</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
/// <param name="owned_by">オーナーユーザ</param>
public record PageSummary(
    long id, string name, string slug,
    string editor, long revision_count, bool draft, bool template,
    long book_id, string book_slug, long? chapter_id, long priority,
    DateTime created_at, DateTime updated_at,
    long created_by, long updated_by, long owned_by
);

/// <summary>ページ情報</summary>
/// <param name="id">ページID</param>
/// <param name="name">ページ名</param>
/// <param name="slug">ページスラグ</param>
/// <param name="editor">エディタ種別</param>
/// <param name="markdown">ページ内容Markdown</param>
/// <param name="html">ページ内容HTML</param>
/// <param name="revision_count">リビジョン番号</param>
/// <param name="draft">ドラフトであるか</param>
/// <param name="template">テンプレートであるか</param>
/// <param name="book_id">ブックID</param>
/// <param name="chapter_id">チャプタID</param>
/// <param name="priority">順序</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
/// <param name="owned_by">オーナーユーザ</param>
/// <param name="tags">タグ一覧</param>
public record PageItem(
    long id, string name, string slug,
    string editor, string markdown, string html,
    long revision_count, bool draft, bool template,
    long book_id, long? chapter_id, long priority,
    DateTime created_at, DateTime updated_at,
    User created_by, User updated_by, User owned_by,
    ContentTag[] tags
);

/// <summary>ページ一覧取得結果</summary>
/// <param name="data">ページ一覧</param>
/// <param name="total">ページ総数</param>
public record ListPagesResult(PageSummary[] data, long total);

/// <summary>ページ作成要求パラメータ</summary>
/// <param name="name">ページ名</param>
/// <param name="book_id">ブックID</param>
/// <param name="chapter_id">チャプタID</param>
/// <param name="html">ページ内容HTML</param>
/// <param name="markdown">ページ内容Markdown</param>
/// <param name="priority">順序</param>
/// <param name="tags">タグ</param>
public record CreatePageArgs(string name, long? book_id = null, long? chapter_id = null, string? html = null, string? markdown = null, long? priority = null, IReadOnlyList<Tag>? tags = null);

/// <summary>ページ作成(Markdown/ブック内)要求パラメータ</summary>
/// <param name="book_id">作成先ブックID</param>
/// <param name="name">ページ名</param>
/// <param name="markdown">ページ内容Markdown</param>
/// <param name="priority">順序</param>
/// <param name="tags">タグ</param>
public record CreateMarkdownPageInBookArgs(long book_id, string name, string markdown, long? priority = null, IReadOnlyList<Tag>? tags = null);

/// <summary>ページ作成(Markdown/チャプタ内)要求パラメータ</summary>
/// <param name="chapter_id">作成先チャプタID</param>
/// <param name="name">ページ名</param>
/// <param name="markdown">ページ内容Markdown</param>
/// <param name="priority">順序</param>
/// <param name="tags">タグ</param>
public record CreateMarkdownPageInChapterArgs(long chapter_id, string name, string markdown, long? priority = null, IReadOnlyList<Tag>? tags = null);

/// <summary>ページ作成(HTML/ブック内)要求パラメータ</summary>
/// <param name="book_id">作成先ブックID</param>
/// <param name="name">ページ名</param>
/// <param name="html">ページ内容HTML</param>
/// <param name="priority">順序</param>
/// <param name="tags">タグ</param>
public record CreateHtmlPageInBookArgs(long book_id, string name, string html, long? priority = null, IReadOnlyList<Tag>? tags = null);

/// <summary>ページ作成(HTML/ブック内)要求パラメータ</summary>
/// <param name="chapter_id">作成先チャプタID</param>
/// <param name="name">ページ名</param>
/// <param name="html">ページ内容HTML</param>
/// <param name="priority">順序</param>
/// <param name="tags">タグ</param>
public record CreateHtmlPageInChapterArgs(long chapter_id, string name, string html, long? priority = null, IReadOnlyList<Tag>? tags = null);

/// <summary>ページ詳細情報</summary>
/// <param name="id">ページID</param>
/// <param name="name">ページ名</param>
/// <param name="slug">ページスラグ</param>
/// <param name="editor">エディタ種別</param>
/// <param name="markdown">ページ内容Markdown</param>
/// <param name="html">レンダリング済みページ内容HTML</param>
/// <param name="raw_html">ページ内容HTML</param>
/// <param name="revision_count">リビジョン番号</param>
/// <param name="draft">ドラフトであるか</param>
/// <param name="template">テンプレートであるか</param>
/// <param name="book_id">ブックID</param>
/// <param name="chapter_id">チャプタID</param>
/// <param name="priority">順序</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
/// <param name="owned_by">オーナーユーザ</param>
/// <param name="tags">タグ一覧</param>
public record ReadPageResult(
    long id, string name, string slug,
    string editor, string markdown, string html, string raw_html,
    long revision_count, bool draft, bool template,
    long book_id, long? chapter_id, long priority,
    DateTime created_at, DateTime updated_at,
    User created_by, User updated_by, User owned_by,
    ContentTag[] tags
);

/// <summary>ページ更新要求パラメータ</summary>
/// <param name="name">ページ名</param>
/// <param name="book_id">ブックID</param>
/// <param name="chapter_id">チャプタID</param>
/// <param name="html">ページ内容HTML</param>
/// <param name="markdown">ページ内容Markdown</param>
/// <param name="priority">順序</param>
/// <param name="tags">タグ</param>
public record UpdatePageArgs(string? name = null, long? book_id = null, long? chapter_id = null, string? html = null, string? markdown = null, long? priority = null, IReadOnlyList<Tag>? tags = null);
#endregion

#region shelves
/// <summary>棚カバー画像情報</summary>
/// <param name="id">棚カバー画像ID</param>
/// <param name="name">棚カバー画像名称</param>
/// <param name="url">棚カバー画像URL</param>
public record ShelfCoverSummary(
    long id, string name, string url
);

/// <summary>棚情報</summary>
/// <param name="id">棚ID</param>
/// <param name="name">棚名</param>
/// <param name="slug">棚スラグ</param>
/// <param name="description">棚概要</param>
/// <param name="cover">棚カバー画像情報</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
/// <param name="owned_by">オーナーユーザ</param>
public record ShelfSummary(
    long id, string name, string slug, string description, ShelfCoverSummary? cover,
    DateTime created_at, DateTime updated_at,
    long created_by, long updated_by, long owned_by
);

/// <summary>棚カバー画像情報</summary>
/// <param name="id">棚カバー画像ID</param>
/// <param name="name">棚カバー画像名称</param>
/// <param name="type">棚カバー画像種別</param>
/// <param name="uploaded_to">対象棚ID</param>
/// <param name="path">棚カバー画像パス</param>
/// <param name="url">棚カバー画像URL</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
public record ShelfCover(
    long id, string name, string type, long uploaded_to,
    string path, string url,
    DateTime created_at, DateTime updated_at,
    long created_by, long updated_by
);

/// <summary>棚情報</summary>
/// <param name="id">棚ID</param>
/// <param name="name">棚名</param>
/// <param name="slug">棚スラグ</param>
/// <param name="description">棚概要</param>
/// <param name="description_html">棚概要(HTML表現)</param>
/// <param name="tags">タグ一覧</param>
/// <param name="cover">棚カバー画像情報</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
/// <param name="owned_by">オーナーユーザ</param>
public record ShelfItem(
    long id, string name, string slug, string description, string description_html,
    ContentTag[]? tags, ShelfCover? cover,
    DateTime created_at, DateTime updated_at,
    long created_by, long updated_by, long owned_by
);

/// <summary>棚一覧の取得結果</summary>
/// <param name="data">棚一覧</param>
/// <param name="total">棚総数</param>
public record ListShelvesResult(ShelfSummary[] data, long total);

/// <summary>棚作成要求パラメータ</summary>
/// <param name="name">棚名</param>
/// <param name="description">棚概要</param>
/// <param name="description_html">棚概要(HTML表現)</param>
/// <param name="books">棚に含むブックIDの配列</param>
/// <param name="tags">付与するタグ一覧</param>
public record CreateShelfArgs(
    string name, string? description = null, string? description_html = null,
    IReadOnlyList<long>? books = null, IReadOnlyList<Tag>? tags = null
);

/// <summary>棚更新要求パラメータ</summary>
/// <param name="name">棚名</param>
/// <param name="description">棚概要</param>
/// <param name="description_html">棚概要(HTML表現)</param>
/// <param name="books">棚に含むブックIDの配列</param>
/// <param name="tags">付与するタグ一覧</param>
public record UpdateShelfArgs(
    string? name = null, string? description = null, string? description_html = null,
    IReadOnlyList<long>? books = null, IReadOnlyList<Tag>? tags = null
);

/// <summary>棚内ブックコンテンツ</summary>
/// <param name="id">ブックID</param>
/// <param name="name">ブック名</param>
/// <param name="slug">ブックスラグ</param>
/// <param name="description">ブック概要</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
/// <param name="owned_by">オーナーユーザ</param>
public record ShelfContentBook(
    long id, string name, string slug, string description,
    DateTime created_at, DateTime updated_at,
    long created_by, long updated_by, long owned_by
);

/// <summary>棚詳細情報</summary>
/// <param name="id">棚ID</param>
/// <param name="name">棚名</param>
/// <param name="slug">棚スラグ</param>
/// <param name="description">棚概要</param>
/// <param name="description_html">棚概要(HTML表現)</param>
/// <param name="books">棚に含むブックの配列</param>
/// <param name="tags">タグ一覧</param>
/// <param name="cover">棚カバー画像情報</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
/// <param name="owned_by">オーナーユーザ</param>
public record ReadShelfResult(
    long id, string name, string slug, string description, string description_html,
    ShelfContentBook[] books, ContentTag[]? tags, ShelfCover? cover,
    DateTime created_at, DateTime updated_at,
    User created_by, User updated_by, User owned_by
);
#endregion

#region image-gallery
/// <summary>ギャラリー画像情報</summary>
/// <param name="id">画像ID</param>
/// <param name="name">画像名</param>
/// <param name="url">画像のURL</param>
/// <param name="path">画像のパス</param>
/// <param name="type">ギャラリー画像種別("gallery" or "drawio")</param>
/// <param name="uploaded_to">アップロード先ページID</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
public record ImageSummary(
    long id, string name, string url, string path,
    string type, long uploaded_to,
    DateTime created_at,
    DateTime updated_at,
    long created_by, long updated_by
);

/// <summary>ギャラリー画像サムネイル情報</summary>
/// <param name="gallery">ギャラリ一覧用サムネイル画像URL</param>
/// <param name="display">ギャラリ画像表示用画像URL</param>
public record ImageThumbs(string gallery, string display);

/// <summary>画像参照書式情報</summary>
/// <param name="html">HTMLでの画像参照マークアップ</param>
/// <param name="markdown">Markdownでの画像参照マークアップ</param>
public record ImageRef(string html, string markdown);

/// <summary>ギャラリー画像情報</summary>
/// <param name="id">画像ID</param>
/// <param name="name">画像名</param>
/// <param name="url">画像のURL</param>
/// <param name="path">画像のパス</param>
/// <param name="type">ギャラリー画像種別("gallery" or "drawio")</param>
/// <param name="uploaded_to">アップロード先ページID</param>
/// <param name="thumbs">サムネイル画像情報</param>
/// <param name="content">画像参照情報</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
public record ImageItem(
    long id, string name, string url, string path,
    string type, long uploaded_to,
    ImageThumbs thumbs, ImageRef content,
    [property: JsonConverter(typeof(AmbiguousTimeJsonConverter))] DateTime created_at,
    [property: JsonConverter(typeof(AmbiguousTimeJsonConverter))] DateTime updated_at,
    User created_by, User updated_by
);

/// <summary>画像一覧取得結果</summary>
/// <param name="data">画像一覧</param>
/// <param name="total">画像総数</param>
public record ListImagesResult(ImageSummary[] data, long total);

/// <summary>ギャラリ画像の作成要求パラメータ</summary>
/// <param name="uploaded_to">アップロード先ページID</param>
/// <param name="type">ギャラリー画像種別("gallery" or "drawio")</param>
/// <param name="name">画像名</param>
public record CreateImageArgs(long uploaded_to, string type, string name);

/// <summary>ギャラリ画像の更新要求パラメータ</summary>
/// <param name="name">画像名</param>
public record UpdateImageArgs(string? name = null);
#endregion

#region search
/// <summary>検索要求パラメータ</summary>
/// <param name="query">
/// 検索クエリ。詳細は以下を参照。
/// https://www.bookstackapp.com/docs/user/searching/
/// </param>
/// <param name="count">取得する検索</param>
/// <param name="page">取得する検索結果ページ番号(1ベース)</param>
public record SearchArgs(string query, int? count = null, int? page = null);

/// <summary>検索結果コンテンツプレビュー情報</summary>
/// <param name="name">名称</param>
/// <param name="content">内容</param>
public record SearchContentPreview(string name, string content);

/// <summary>検索結果コンテンツの親オブジェクト情報</summary>
/// <param name="id">コンテンツID</param>
/// <param name="name">コンテンツ名</param>
/// <param name="slug">コンテンツスラグ</param>
public record SearchContentEnvelope(long id, string name, string slug);

/// <summary>検索結果コンテンツ基本クラス</summary>
/// <param name="id">コンテンツID</param>
/// <param name="name">コンテンツ名</param>
/// <param name="slug">コンテンツスラグ</param>
/// <param name="type">コンテンツ種別</param>
/// <param name="url">コンテンツURL</param>
/// <param name="tags">コンテンツタグ</param>
/// <param name="preview_html">コンテンツプレビュー</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
[JsonConverter(typeof(SearchContentJsonConverter))]
public abstract record SearchContent(
    long id, string name, string slug, string type,
    string url, ContentTag[]? tags, SearchContentPreview? preview_html,
    DateTime created_at, DateTime updated_at
);

/// <summary>検索結果ブックコンテンツ</summary>
/// <param name="id">コンテンツID</param>
/// <param name="name">コンテンツ名</param>
/// <param name="slug">コンテンツスラグ</param>
/// <param name="type">コンテンツ種別</param>
/// <param name="url">コンテンツURL</param>
/// <param name="tags">コンテンツタグ</param>
/// <param name="preview_html">コンテンツプレビュー</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
public record SearchContentBook(
    long id, string name, string slug, string type,
    string url, ContentTag[]? tags, SearchContentPreview? preview_html,
    DateTime created_at, DateTime updated_at
) : SearchContent(id, name, slug, type, url, tags, preview_html, created_at, updated_at);

/// <summary>検索結果チャプタコンテンツ</summary>
/// <param name="id">チャプタID</param>
/// <param name="name">チャプタ名</param>
/// <param name="slug">チャプタスラグ</param>
/// <param name="type">コンテンツ種別</param>
/// <param name="url">チャプタURL</param>
/// <param name="tags">タグ一覧</param>
/// <param name="preview_html">コンテンツプレビュー</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="book_id">所属ブックID</param>
/// <param name="priority">順序</param>
/// <param name="book">親ブック情報</param>
public record SearchContentChapter(
    long id, string name, string slug, string type,
    string url, ContentTag[]? tags, SearchContentPreview? preview_html,
    DateTime created_at, DateTime updated_at,
    long book_id, long priority, SearchContentEnvelope? book
) : SearchContent(id, name, slug, type, url, tags, preview_html, created_at, updated_at);

/// <summary>検索結果ページコンテンツ</summary>
/// <param name="id">コンテンツID</param>
/// <param name="name">コンテンツ名</param>
/// <param name="slug">コンテンツスラグ</param>
/// <param name="type">コンテンツ種別</param>
/// <param name="url">コンテンツURL</param>
/// <param name="tags">コンテンツタグ</param>
/// <param name="preview_html">コンテンツプレビュー</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="book_id">ブックID</param>
/// <param name="chapter_id">チャプタID</param>
/// <param name="draft">ドラフトであるか</param>
/// <param name="template">テンプレートであるか</param>
/// <param name="priority">順序</param>
/// <param name="book">親ブック情報</param>
/// <param name="chapter">親チャプタ情報</param>
public record SearchContentPage(
    long id, string name, string slug, string type,
    string url, ContentTag[]? tags, SearchContentPreview? preview_html,
    DateTime created_at, DateTime updated_at,
    long book_id, long? chapter_id, bool draft, bool template, long priority,
    SearchContentEnvelope? book, SearchContentEnvelope? chapter
) : SearchContent(id, name, slug, type, url, tags, preview_html, created_at, updated_at);

/// <summary>検索結果棚コンテンツ</summary>
/// <param name="id">棚ID</param>
/// <param name="name">棚名</param>
/// <param name="slug">棚スラグ</param>
/// <param name="type">コンテンツ種別</param>
/// <param name="url">棚URL</param>
/// <param name="tags">タグ一覧</param>
/// <param name="preview_html">コンテンツプレビュー</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
public record SearchContentShelf(
    long id, string name, string slug, string type,
    string url, ContentTag[]? tags, SearchContentPreview? preview_html,
    DateTime created_at, DateTime updated_at
) : SearchContent(id, name, slug, type, url, tags, preview_html, created_at, updated_at);

/// <summary>検索結果情報</summary>
/// <param name="data">検索結果一覧</param>
/// <param name="total">検索結果総数</param>
public record SearchResult(SearchContent[] data, long total)
{
    /// <summary>検索結果ブック一覧を列挙する</summary>
    /// <returns>検索結果ブック一覧シーケンス</returns>
    public IEnumerable<SearchContentBook> books() => this.data.OfType<SearchContentBook>();

    /// <summary>検索結果チャプタ一覧を列挙する</summary>
    /// <returns>検索結果チャプタ一覧シーケンス</returns>
    public IEnumerable<SearchContentChapter> chapters() => this.data.OfType<SearchContentChapter>();

    /// <summary>検索結果ページ一覧を列挙する</summary>
    /// <returns>検索結果ページ一覧シーケンス</returns>
    public IEnumerable<SearchContentPage> pages() => this.data.OfType<SearchContentPage>();

    /// <summary>検索結果棚一覧を列挙する</summary>
    /// <returns>検索結果棚一覧シーケンス</returns>
    public IEnumerable<SearchContentShelf> shelves() => this.data.OfType<SearchContentShelf>();
};
#endregion

#region users
/// <summary>ユーザ情報</summary>
/// <param name="id">ユーザID</param>
/// <param name="name">ユーザ名</param>
/// <param name="slug">ユーザスラグ</param>
/// <param name="email">ユーザメールアドレス</param>
/// <param name="external_auth_id">external_auth_id</param>
/// <param name="profile_url">プロフィールページURL</param>
/// <param name="edit_url">ユーザ編集ページURL</param>
/// <param name="avatar_url">ユーザアバターURL</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="last_activity_at">最終アクティビティ日時</param>
public record UserSummary(
    long id, string name, string slug, string email,
    string? external_auth_id,
    string profile_url, string edit_url, string avatar_url,
    DateTime? created_at, DateTime? updated_at, DateTime? last_activity_at
);

/// <summary>ユーザ情報</summary>
/// <param name="id">ユーザID</param>
/// <param name="name">ユーザ名</param>
/// <param name="slug">ユーザスラグ</param>
/// <param name="email">ユーザメールアドレス</param>
/// <param name="external_auth_id">external_auth_id</param>
/// <param name="roles">ロール一覧</param>
/// <param name="profile_url">プロフィールページURL</param>
/// <param name="edit_url">ユーザ編集ページURL</param>
/// <param name="avatar_url">ユーザアバターURL</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
public record UserItem(
    long id, string name, string slug, string email,
    string? external_auth_id, UserRole[]? roles,
    string profile_url, string edit_url, string avatar_url,
    DateTime created_at, DateTime updated_at
);

/// <summary>ユーザ一覧取得結果</summary>
/// <param name="data">ユーザ一覧</param>
/// <param name="total">ユーザ総数</param>
public record ListUsersResult(UserSummary[] data, long total);

/// <summary>ユーザ作成要求パラメータ</summary>
/// <param name="name">ユーザ名</param>
/// <param name="email">ユーザメールアドレス</param>
/// <param name="external_auth_id">external_auth_id</param>
/// <param name="language">ユーザの表示言語</param>
/// <param name="password">ユーザパスワード</param>
/// <param name="roles">ユーザのロール</param>
/// <param name="send_invite">send_invite</param>
public record CreateUserArgs(
    string name, string email, string? external_auth_id = null,
    string? language = null, string? password = null,
    IReadOnlyList<long>? roles = null, bool? send_invite = null
);

/// <summary>ユーザロール</summary>
/// <param name="id">ロールID</param>
/// <param name="display_name">ロール名</param>
public record UserRole(long id, string display_name);

/// <summary>ユーザ更新要求パラメータ</summary>
/// <param name="name">ユーザ名</param>
/// <param name="email">ユーザメールアドレス</param>
/// <param name="external_auth_id">external_auth_id</param>
/// <param name="language">ユーザの表示言語</param>
/// <param name="password">ユーザパスワード</param>
/// <param name="roles">ユーザのロール</param>
/// <param name="send_invite">send_invite</param>
public record UpdateUserArgs(
    string? name = null, string? email = null, string? external_auth_id = null,
    string? language = null, string? password = null,
    IReadOnlyList<long>? roles = null, bool? send_invite = null
);
#endregion

#region roles
/// <summary>ロール情報</summary>
/// <param name="id">ロールID</param>
/// <param name="display_name">ロール名</param>
/// <param name="system_name">ロールシステム名</param>
/// <param name="description">ロール説明</param>
/// <param name="permissions_count">ロールが含む権限数</param>
/// <param name="users_count">ロールを持つユーザ数</param>
/// <param name="mfa_enforced">多要素認証を要求するか否か</param>
/// <param name="external_auth_id">external_auth_id</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
public record RoleSummary(
    long id, string display_name, string? system_name,
    string? description, long permissions_count, long users_count,
    bool mfa_enforced, string? external_auth_id,
    DateTime created_at, DateTime updated_at
);

/// <summary>ロール情報</summary>
/// <param name="id">ロールID</param>
/// <param name="display_name">ロール名</param>
/// <param name="description">ロール説明</param>
/// <param name="mfa_enforced">多要素認証を要求するか否か</param>
/// <param name="permissions">権限一覧</param>
/// <param name="users">パーミッションが付与されたユーザ</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
public record RoleItem(
    long id, string display_name, string? description,
    bool mfa_enforced, string[] permissions, User[] users,
    DateTime created_at, DateTime updated_at
);

/// <summary>ロール一覧</summary>
/// <param name="data">ロール一覧</param>
/// <param name="total">ロール総数</param>
public record ListRolesResult(RoleSummary[] data, long total);

/// <summary>ロール作成要求パラメータ</summary>
/// <param name="display_name">ロール名</param>
/// <param name="description">ロール説明</param>
/// <param name="mfa_enforced">多要素認証を要求するか否か</param>
/// <param name="external_auth_id">external_auth_id</param>
/// <param name="permissions">含める権限</param>
public record CreateRoleArgs(
    string display_name, string? description = null,
    bool? mfa_enforced = null, string? external_auth_id = null,
    IReadOnlyList<string>? permissions = null
);

/// <summary>ロール更新要求パラメータ</summary>
/// <param name="display_name">ロール名</param>
/// <param name="description">ロール説明</param>
/// <param name="mfa_enforced">多要素認証を要求するか否か</param>
/// <param name="external_auth_id">external_auth_id</param>
/// <param name="permissions">含める権限</param>
public record UpdateRoleArgs(
    string? display_name = null, string? description = null,
    bool? mfa_enforced = null, string? external_auth_id = null,
    IReadOnlyList<string>? permissions = null
);
#endregion

#region content-permissions
/// <summary>ロール情報(一部)</summary>
/// <param name="id">ロールID</param>
/// <param name="display_name">ロール名</param>
public record RoleShort(long id, string display_name);

/// <summary>ロールに対する権限</summary>
/// <param name="role_id">対象ロールID</param>
/// <param name="view">表示権限</param>
/// <param name="create">作成権限</param>
/// <param name="update">更新権限</param>
/// <param name="delete">削除権限</param>
public record RolePermission(long role_id, bool view, bool create, bool update, bool delete);

/// <summary>ロールに対する権限</summary>
/// <param name="role_id">対象ロールID</param>
/// <param name="role">ロール情報(一部)</param>
/// <param name="view">表示権限</param>
/// <param name="create">作成権限</param>
/// <param name="update">更新権限</param>
/// <param name="delete">削除権限</param>
public record RolePermissionEx(long role_id, RoleShort role, bool view, bool create, bool update, bool delete)
    : RolePermission(role_id, view, create, update, delete);

/// <summary>フォールバック権限</summary>
/// <param name="inheriting">デフォルトを継承するか否か</param>
/// <param name="view">表示権限</param>
/// <param name="create">作成権限</param>
/// <param name="update">更新権限</param>
/// <param name="delete">削除権限</param>
public record FallbackPermission(bool inheriting, bool? view, bool? create, bool? update, bool? delete)
{
    /// <summary>デフォルトを継承するフォールバック権限値</summary>
    public static FallbackPermission Inherit { get; } = new FallbackPermission(true, false, false, false, false);

    /// <summary>明示的に指定するフォールバック権限値を作成する</summary>
    /// <param name="view">表示権限</param>
    /// <param name="create">作成権限</param>
    /// <param name="update">更新権限</param>
    /// <param name="delete">削除権限</param>
    /// <returns>フォールバック権限値</returns>
    public static FallbackPermission Appoint(bool view, bool create, bool update, bool delete) => new FallbackPermission(false, view, create, update, delete);
}

/// <summary>コンテンツの権限情報</summary>
/// <param name="owner">所有ユーザ</param>
/// <param name="role_permissions">ロールへの権限一覧</param>
/// <param name="fallback_permissions">フォールバック権限</param>
public record ContentPermissionsItem(User owner, RolePermissionEx[] role_permissions, FallbackPermission fallback_permissions);

/// <summary>コンテンツの権限更新要求パラメータ</summary>
/// <param name="owner_id">所有ユーザID</param>
/// <param name="role_permissions">ロールへの権限</param>
/// <param name="fallback_permissions">フォールバック権限</param>
public record UpdateContentPermissionsArgs(long? owner_id = null, RolePermission[]? role_permissions = null, FallbackPermission? fallback_permissions = null);
#endregion

#region imports
/// <summary>ZIPインポート情報</summary>
/// <param name="id">インポートID</param>
/// <param name="name">インポート名</param>
/// <param name="size">インポートサイズ</param>
/// <param name="type">インポート種別("book" or "chapter" or "page")</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
public record ImportsSummary(
    long id, string name, long size, string type,
    DateTime created_at, DateTime updated_at,
    long created_by
);

/// <summary>ZIPインポート一覧取得結果</summary>
/// <param name="data">インポート一覧</param>
/// <param name="total">インポート総数</param>
public record ListImportsResult(ImportsSummary[] data, long total);

/// <summary>ZIPインポート情報</summary>
/// <param name="id">インポートID</param>
/// <param name="name">インポート名</param>
/// <param name="size">インポートサイズ</param>
/// <param name="path">インポートファイルアップロードパス</param>
/// <param name="type">ギャラリー画像種別("book" or "chapter" or "page")</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
public record ImportsItem(
    long id, string name, long size, string type, string path,
    DateTime created_at, DateTime updated_at,
    long created_by
);

/// <summary>ZIPインポートタグ詳細</summary>
/// <param name="name">インポートタグ名</param>
public record ImportsTag(string name);

/// <summary>ZIPインポート添付詳細</summary>
/// <param name="id">インポート添付ID</param>
/// <param name="name">インポート添付名</param>
public record ImportsAttachment(long id, string name);

/// <summary>ZIPインポート画像詳細</summary>
/// <param name="id">インポート画像ID</param>
/// <param name="name">インポート画像名</param>
/// <param name="type">インポート画像種別</param>
/// <param name="file">インポート画像ファイル名</param>
public record ImportsImage(long id, string name, string type, string file);

/// <summary>ZIPインポートページ詳細</summary>
/// <param name="id">ページインポートID</param>
/// <param name="name">ページインポート名</param>
/// <param name="priority">順序</param>
/// <param name="attachments">添付インポート情報</param>
/// <param name="images">画像インポート情報</param>
/// <param name="tags">タグインポート情報</param>
public record ImportsPageDetails(
    long id, string name, long? priority,
    ImportsAttachment[] attachments, ImportsImage[] images,
    ImportsTag[] tags
);

/// <summary>ZIPインポートチャプタ詳細</summary>
/// <param name="id">チャプタインポートID</param>
/// <param name="name">チャプタインポート名</param>
/// <param name="priority">順序</param>
/// <param name="pages">ページインポート情報</param>
/// <param name="tags">タグインポート情報</param>
public record ImportsChapterDetails(
    long id, string name, long? priority,
    ImportsPageDetails[] pages,
    ImportsTag[] tags
);

/// <summary>ZIPインポートコンテンツ詳細</summary>
/// <param name="id">ブックインポートID</param>
/// <param name="name">ブックインポート名</param>
/// <param name="chapters">チャプタインポート情報</param>
/// <param name="pages">ページインポート情報</param>
/// <param name="tags">タグインポート情報</param>
public record ImportsContentDetails(
    long id, string name,
    ImportsChapterDetails[]? chapters,
    ImportsPageDetails[]? pages,
    ImportsTag[] tags
);

/// <summary>ZIPインポート詳細</summary>
/// <param name="id">インポートID</param>
/// <param name="name">インポート名</param>
/// <param name="size">インポートサイズ</param>
/// <param name="type">ギャラリー画像種別("book" or "chapter" or "page")</param>
/// <param name="path">インポートファイルアップロードパス</param>
/// <param name="details">詳細情報</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
public record ImportsItemDetails(
    long id, string name, long size, string type, string path,
    ImportsContentDetails details,
    DateTime created_at, DateTime updated_at,
    long created_by
);

/// <summary>インポート実行パラメータ</summary>
/// <param name="parent_type">インポート先種別("book" or "chapter")</param>
/// <param name="parent_id">インポート先ID</param>
public record RunImportsArgs(string parent_type, long parent_id);

/// <summary>インポート実行結果</summary>
/// <param name="id">インポートアイテムID</param>
/// <param name="book_id">ブックID</param>
/// <param name="name">インポートアイテム名</param>
/// <param name="slug">インポートアイテムスラグ</param>
/// <param name="description">インポートアイテム概要</param>
/// <param name="priority">インポートアイテム順序</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
/// <param name="owned_by">所有ユーザ</param>
/// <param name="default_template_id">デフォルトテンプレートID</param>
public record RunImportsResult(
    long id, string name, string slug, long book_id,
    string description, long priority,
    DateTime created_at, DateTime updated_at,
    long created_by, long updated_by, long owned_by,
    long? default_template_id
);
#endregion

#region recycle-bin
/// <summary>削除コンテンツの親情報</summary>
/// <param name="id">コンテンツID</param>
/// <param name="name">コンテンツ名称</param>
/// <param name="slug">コンテンツスラグ</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
/// <param name="owned_by">オーナーユーザ</param>
/// <param name="type">コンテンツ種別</param>
/// <param name="description">コンテンツ説明</param>
[JsonConverter(typeof(DeletableContentParentJsonConverter))]
public record DeletableContentParent(
    long id, string name, string slug,
    DateTime created_at, DateTime updated_at,
    long created_by, long updated_by, long owned_by,
    string type, string description
);

/// <summary>削除コンテンツの親ブック情報</summary>
/// <param name="id">ブックID</param>
/// <param name="name">ブック名称</param>
/// <param name="slug">ブックスラグ</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
/// <param name="owned_by">オーナーユーザ</param>
/// <param name="type">コンテンツ種別</param>
/// <param name="description">ブック説明</param>
/// <param name="default_template_id">デフォルトテンプレートID</param>
public record DeletableContentParentBook(
    long id, string name, string slug,
    DateTime created_at, DateTime updated_at,
    long created_by, long updated_by, long owned_by,
    string type, string description, long? default_template_id
) : DeletableContentParent(id, name, slug, created_at, updated_at, created_by, updated_by, owned_by, type, description);

/// <summary>削除コンテンツの親チャプタ情報</summary>
/// <param name="id">チャプタID</param>
/// <param name="name">チャプタ名称</param>
/// <param name="slug">チャプタスラグ</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
/// <param name="owned_by">オーナーユーザ</param>
/// <param name="type">コンテンツ種別</param>
/// <param name="description">チャプタ説明</param>
/// <param name="default_template_id">デフォルトテンプレートID</param>
/// <param name="book_id">所属ブックID</param>
/// <param name="priority">順序</param>
public record DeletableContentParentChapter(
    long id, string name, string slug,
    DateTime created_at, DateTime updated_at,
    long created_by, long updated_by, long owned_by,
    string type, string description, long? default_template_id,
    long book_id, long priority
) : DeletableContentParent(id, name, slug, created_at, updated_at, created_by, updated_by, owned_by, type, description);

/// <summary>削除コンテンツ情報</summary>
/// <param name="id">コンテンツID</param>
/// <param name="name">コンテンツ名称</param>
/// <param name="slug">コンテンツスラグ</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
/// <param name="owned_by">オーナーユーザ</param>
[JsonConverter(typeof(DeletableContentJsonConverter))]
public record DeletableContent(
    long id, string name, string slug,
    DateTime created_at, DateTime updated_at,
    long created_by, long updated_by, long? owned_by
);

/// <summary>削除棚情報</summary>
/// <param name="id">棚ID</param>
/// <param name="name">棚名称</param>
/// <param name="slug">棚スラグ</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
/// <param name="owned_by">オーナーユーザ</param>
/// <param name="description">棚説明</param>
public record DeletableContentShelf(
    long id, string name, string slug,
    DateTime created_at, DateTime updated_at,
    long created_by, long updated_by, long? owned_by,
    string description
) : DeletableContent(id, name, slug, created_at, updated_at, created_by, updated_by, owned_by);

/// <summary>削除ブック情報</summary>
/// <param name="id">ブックID</param>
/// <param name="name">ブック名称</param>
/// <param name="slug">ブックスラグ</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
/// <param name="owned_by">オーナーユーザ</param>
/// <param name="description">ブック説明</param>
/// <param name="default_template_id">デフォルトテンプレートID</param>
/// <param name="chapters_count">含むチャプタ数</param>
/// <param name="pages_count">含むページ数</param>
public record DeletableContentBook(
    long id, string name, string slug,
    DateTime created_at, DateTime updated_at,
    long created_by, long updated_by, long? owned_by,
    string description, long? default_template_id, long chapters_count, long pages_count
) : DeletableContent(id, name, slug, created_at, updated_at, created_by, updated_by, owned_by);

/// <summary>削除チャプタ情報</summary>
/// <param name="id">チャプタID</param>
/// <param name="name">チャプタ名称</param>
/// <param name="slug">チャプタスラグ</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
/// <param name="owned_by">オーナーユーザ</param>
/// <param name="description">チャプタ説明</param>
/// <param name="default_template_id">デフォルトテンプレートID</param>
/// <param name="book_id">所属ブックID</param>
/// <param name="parent">所属ブック情報</param>
/// <param name="priority">順序</param>
/// <param name="pages_count">含むページ数</param>
public record DeletableContentChapter(
    long id, string name, string slug,
    DateTime created_at, DateTime updated_at,
    long created_by, long updated_by, long? owned_by,
    string description, long? default_template_id, long book_id, DeletableContentParentBook parent,
    long priority, long pages_count
) : DeletableContent(id, name, slug, created_at, updated_at, created_by, updated_by, owned_by);

/// <summary>削除ページ情報</summary>
/// <param name="id">ページID</param>
/// <param name="name">ページ名称</param>
/// <param name="slug">ページスラグ</param>
/// <param name="created_at">作成日時</param>
/// <param name="updated_at">更新日時</param>
/// <param name="created_by">作成したユーザ</param>
/// <param name="updated_by">更新したユーザ</param>
/// <param name="owned_by">オーナーユーザ</param>
/// <param name="book_id">所属ブックID</param>
/// <param name="chapter_id">所属チャプタID</param>
/// <param name="parent">所属ブックまたはチャプタ情報</param>
/// <param name="draft">ドラフトであるか</param>
/// <param name="template">テンプレートであるか</param>
/// <param name="editor">エディタ種別</param>
/// <param name="priority">順序</param>
/// <param name="revision_count">リビジョン番号</param>
public record DeletableContentPage(
    long id, string name, string slug,
    DateTime created_at, DateTime updated_at,
    long created_by, long updated_by, long? owned_by,
    long book_id, long? chapter_id, DeletableContentParent parent,
    bool draft, bool template, string editor, long priority, long revision_count
) : DeletableContent(id, name, slug, created_at, updated_at, created_by, updated_by, owned_by);

/// <summary>ゴミ箱のアイテム</summary>
/// <param name="id">ゴミ箱のアイテムID</param>
/// <param name="deletable_type">削除コンテンツ種別</param>
/// <param name="deletable_id">削除コンテンツID</param>
/// <param name="deleted_by">削除ユーザ</param>
/// <param name="deletable">削除コンテンツ</param>
/// <param name="created_at">削除日時</param>
/// <param name="updated_at">削除日時</param>
[JsonConverter(typeof(RecycleItemJsonConverter))]
public record RecycleItem(
    long id, string deletable_type, long deletable_id, long deleted_by, DeletableContent deletable,
    DateTime created_at, DateTime updated_at
);

/// <summary>ゴミ箱のアイテム一覧取得結果</summary>
/// <param name="data">ゴミ箱のアイテム一覧</param>
/// <param name="total">ゴミ箱のアイテム総数</param>
public record ListRecycleBinResult(RecycleItem[] data, long total);

/// <summary>ゴミ箱からの復元結果情報</summary>
/// <param name="restore_count">復元アイテム数</param>
public record RestoreRecycleItemResult(long restore_count);
#endregion

#region audit-log
/// <summary>監査ログ情報</summary>
/// <param name="id">監査ログID</param>
/// <param name="type">イベント種別種別</param>
/// <param name="detail">詳細情報</param>
/// <param name="loggable_id">関連オブジェクトID</param>
/// <param name="loggable_type">関連オブジェクト種別</param>
/// <param name="user_id">対象ユーザID</param>
/// <param name="ip">IPアドレス</param>
/// <param name="created_at">アクティビティ日時</param>
/// <param name="user">対象ユーザ情報</param>
public record AuditLogItem(
    long id, string type, string detail,
    long? loggable_id, string? loggable_type, long user_id, string ip,
    DateTime created_at, User user
);

/// <summary>監査ログ一覧取得結果</summary>
/// <param name="data">ゴミ箱のアイテム一覧</param>
/// <param name="total">ゴミ箱のアイテム総数</param>
public record ListAuditLogResult(AuditLogItem[] data, long total);
#endregion
