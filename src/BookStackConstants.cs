namespace BookStackApiClient;

#pragma warning disable CS1591 // 公開されている型またはメンバーの XML コメントがありません

/// <summary>
/// ロール権限定数
/// </summary>
public static class RolePermissions
{
    public static string SettingsManage { get; } = "settings-manage";
    public static string UsersManage { get; } = "users-manage";
    public static string UserRolesManage { get; } = "user-roles-manage";
    public static string RestrictionsManageAll { get; } = "restrictions-manage-all";
    public static string RestrictionsManageOwn { get; } = "restrictions-manage-own";
    public static string BookCreateAll { get; } = "book-create-all";
    public static string BookCreateOwn { get; } = "book-create-own";
    public static string BookUpdateAll { get; } = "book-update-all";
    public static string BookUpdateOwn { get; } = "book-update-own";
    public static string BookDeleteAll { get; } = "book-delete-all";
    public static string BookDeleteOwn { get; } = "book-delete-own";
    public static string PageCreateAll { get; } = "page-create-all";
    public static string PageCreateOwn { get; } = "page-create-own";
    public static string PageUpdateAll { get; } = "page-update-all";
    public static string PageUpdateOwn { get; } = "page-update-own";
    public static string PageDeleteAll { get; } = "page-delete-all";
    public static string PageDeleteOwn { get; } = "page-delete-own";
    public static string ChapterCreateAll { get; } = "chapter-create-all";
    public static string ChapterCreateOwn { get; } = "chapter-create-own";
    public static string ChapterUpdateAll { get; } = "chapter-update-all";
    public static string ChapterUpdateOwn { get; } = "chapter-update-own";
    public static string ChapterDeleteAll { get; } = "chapter-delete-all";
    public static string ChapterDeleteOwn { get; } = "chapter-delete-own";
    public static string ImageCreateAll { get; } = "image-create-all";
    public static string ImageCreateOwn { get; } = "image-create-own";
    public static string ImageUpdateAll { get; } = "image-update-all";
    public static string ImageUpdateOwn { get; } = "image-update-own";
    public static string ImageDeleteAll { get; } = "image-delete-all";
    public static string ImageDeleteOwn { get; } = "image-delete-own";
    public static string BookViewAll { get; } = "book-view-all";
    public static string BookViewOwn { get; } = "book-view-own";
    public static string PageViewAll { get; } = "page-view-all";
    public static string PageViewOwn { get; } = "page-view-own";
    public static string ChapterViewAll { get; } = "chapter-view-all";
    public static string ChapterViewOwn { get; } = "chapter-view-own";
    public static string AttachmentCreateAll { get; } = "attachment-create-all";
    public static string AttachmentCreateOwn { get; } = "attachment-create-own";
    public static string AttachmentUpdateAll { get; } = "attachment-update-all";
    public static string AttachmentUpdateOwn { get; } = "attachment-update-own";
    public static string AttachmentDeleteAll { get; } = "attachment-delete-all";
    public static string AttachmentDeleteOwn { get; } = "attachment-delete-own";
    public static string CommentCreateAll { get; } = "comment-create-all";
    public static string CommentCreateOwn { get; } = "comment-create-own";
    public static string CommentUpdateAll { get; } = "comment-update-all";
    public static string CommentUpdateOwn { get; } = "comment-update-own";
    public static string CommentDeleteAll { get; } = "comment-delete-all";
    public static string CommentDeleteOwn { get; } = "comment-delete-own";
    public static string BookshelfViewAll { get; } = "bookshelf-view-all";
    public static string BookshelfViewOwn { get; } = "bookshelf-view-own";
    public static string BookshelfCreateAll { get; } = "bookshelf-create-all";
    public static string BookshelfCreateOwn { get; } = "bookshelf-create-own";
    public static string BookshelfUpdateAll { get; } = "bookshelf-update-all";
    public static string BookshelfUpdateOwn { get; } = "bookshelf-update-own";
    public static string BookshelfDeleteAll { get; } = "bookshelf-delete-all";
    public static string BookshelfDeleteOwn { get; } = "bookshelf-delete-own";
    public static string TemplatesManage { get; } = "templates-manage";
    public static string AccessApi { get; } = "access-api";
    public static string ContentExport { get; } = "content-export";
    public static string EditorChange { get; } = "editor-change";
    public static string ReceiveNotifications { get; } = "receive-notifications";
    public static string ContentImport { get; } = "content-import";
}