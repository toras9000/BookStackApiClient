namespace BookStackApiClient;

#pragma warning disable CS1591 // 公開されている型またはメンバーの XML コメントがありません

/// <summary>
/// ロール権限定数
/// </summary>
public static class RolePermissions
{
    public static string ManageSettings { get; } = "settings-manage";
    public static string ManageUsers { get; } = "users-manage";
    public static string ManageRolesAndPermissions { get; } = "user-roles-manage";
    public static string ManageAllEntityPermissions { get; } = "restrictions-manage-all";
    public static string ManageEntityPermissionsOnOwnContent { get; } = "restrictions-manage-own";
    public static string CreateAllBooks { get; } = "book-create-all";
    public static string CreateOwnBooks { get; } = "book-create-own";
    public static string UpdateAllBooks { get; } = "book-update-all";
    public static string UpdateOwnBooks { get; } = "book-update-own";
    public static string DeleteAllBooks { get; } = "book-delete-all";
    public static string DeleteOwnBooks { get; } = "book-delete-own";
    public static string CreateAllPages { get; } = "page-create-all";
    public static string CreateOwnPages { get; } = "page-create-own";
    public static string UpdateAllPages { get; } = "page-update-all";
    public static string UpdateOwnPages { get; } = "page-update-own";
    public static string DeleteAllPages { get; } = "page-delete-all";
    public static string DeleteOwnPages { get; } = "page-delete-own";
    public static string CreateAllChapters { get; } = "chapter-create-all";
    public static string CreateOwnChapters { get; } = "chapter-create-own";
    public static string UpdateAllChapters { get; } = "chapter-update-all";
    public static string UpdateOwnChapters { get; } = "chapter-update-own";
    public static string DeleteAllChapters { get; } = "chapter-delete-all";
    public static string DeleteOwnChapters { get; } = "chapter-delete-own";
    public static string CreateAllImages { get; } = "image-create-all";
    public static string CreateOwnImages { get; } = "image-create-own";
    public static string UpdateAllImages { get; } = "image-update-all";
    public static string UpdateOwnImages { get; } = "image-update-own";
    public static string DeleteAllImages { get; } = "image-delete-all";
    public static string DeleteOwnImages { get; } = "image-delete-own";
    public static string ViewAllBooks { get; } = "book-view-all";
    public static string ViewOwnBooks { get; } = "book-view-own";
    public static string ViewAllPages { get; } = "page-view-all";
    public static string ViewOwnPages { get; } = "page-view-own";
    public static string ViewAllChapters { get; } = "chapter-view-all";
    public static string ViewOwnChapters { get; } = "chapter-view-own";
    public static string CreateAllAttachments { get; } = "attachment-create-all";
    public static string CreateOwnAttachments { get; } = "attachment-create-own";
    public static string UpdateAllAttachments { get; } = "attachment-update-all";
    public static string UpdateOwnAttachments { get; } = "attachment-update-own";
    public static string DeleteAllAttachments { get; } = "attachment-delete-all";
    public static string DeleteOwnAttachments { get; } = "attachment-delete-own";
    public static string CreateAllComments { get; } = "comment-create-all";
    public static string CreateOwnComments { get; } = "comment-create-own";
    public static string UpdateAllComments { get; } = "comment-update-all";
    public static string UpdateOwnComments { get; } = "comment-update-own";
    public static string DeleteAllComments { get; } = "comment-delete-all";
    public static string DeleteOwnComments { get; } = "comment-delete-own";
    public static string ViewAllBookShelves { get; } = "bookshelf-view-all";
    public static string ViewOwnBookShelves { get; } = "bookshelf-view-own";
    public static string CreateAllBookShelves { get; } = "bookshelf-create-all";
    public static string CreateOwnBookShelves { get; } = "bookshelf-create-own";
    public static string UpdateAllBookShelves { get; } = "bookshelf-update-all";
    public static string UpdateOwnBookShelves { get; } = "bookshelf-update-own";
    public static string DeleteAllBookShelves { get; } = "bookshelf-delete-all";
    public static string DeleteOwnBookShelves { get; } = "bookshelf-delete-own";
    public static string ManagePageTemplates { get; } = "templates-manage";
    public static string AccessSystemAPI { get; } = "access-api";
    public static string ExportContent { get; } = "content-export";
    public static string ChangePageEditor { get; } = "editor-change";
}
