namespace BookStackApiClient.Tests.helper;

public class TestResourceContainer : IAsyncDisposable
{
    public TestResourceContainer(BookStackClient client)
    {
        this.resources = new List<TestResource>();
        this.attachments = new List<AttachmentItem>();
        this.books = new List<BookItem>();
        this.chapters = new List<ChapterItem>();
        this.pages = new List<PageItem>();
        this.shelves = new List<ShelfItem>();
        this.images = new List<ImageItem>();
        this.comments = new List<CommentItem>();
        this.users = new List<UserItem>();
        this.roles = new List<RoleItem>();
        this.imports = new List<ImportsItem>();

        this.Client = client;
        this.Attachments = this.attachments.AsReadOnly();
        this.Books = this.books.AsReadOnly();
        this.Chapters = this.chapters.AsReadOnly();
        this.Pages = this.pages.AsReadOnly();
        this.Shelves = this.shelves.AsReadOnly();
        this.Images = this.images.AsReadOnly();
        this.Comments = this.comments.AsReadOnly();
        this.Users = this.users.AsReadOnly();
        this.Roles = this.roles.AsReadOnly();
        this.Imports = this.imports.AsReadOnly();
    }

    public BookStackClient Client { get; }
    public IReadOnlyList<AttachmentItem> Attachments { get; }
    public IReadOnlyList<BookItem> Books { get; }
    public IReadOnlyList<ChapterItem> Chapters { get; }
    public IReadOnlyList<PageItem> Pages { get; }
    public IReadOnlyList<ShelfItem> Shelves { get; }
    public IReadOnlyList<ImageItem> Images { get; }
    public IReadOnlyList<CommentItem> Comments { get; }
    public IReadOnlyList<UserItem> Users { get; }
    public IReadOnlyList<RoleItem> Roles { get; }
    public IReadOnlyList<ImportsItem> Imports { get; }


    public AttachmentItem ToBeDiscarded(AttachmentItem attachment)
    {
        var resource = new TestResource(() => this.Client.DeleteAttachmentAsync(attachment.id));
        this.resources.Add(resource);
        return this.AddTo(attachment);
    }

    public BookItem ToBeDiscarded(BookItem book)
    {
        var resource = new TestResource(() => this.Client.DeleteBookAsync(book.id));
        this.resources.Add(resource);
        return this.AddTo(book);
    }

    public ChapterItem ToBeDiscarded(ChapterItem chapter)
    {
        var resource = new TestResource(() => this.Client.DeleteChapterAsync(chapter.id));
        this.resources.Add(resource);
        return this.AddTo(chapter);
    }

    public PageItem ToBeDiscarded(PageItem page)
    {
        var resource = new TestResource(() => this.Client.DeletePageAsync(page.id));
        this.resources.Add(resource);
        return this.AddTo(page);
    }

    public ShelfItem ToBeDiscarded(ShelfItem shelf)
    {
        var resource = new TestResource(() => this.Client.DeleteShelfAsync(shelf.id));
        this.resources.Add(resource);
        return this.AddTo(shelf);
    }

    public ImageItem ToBeDiscarded(ImageItem image)
    {
        var resource = new TestResource(() => this.Client.DeleteImageAsync(image.id));
        this.resources.Add(resource);
        return this.AddTo(image);
    }

    public CommentItem ToBeDiscarded(CommentItem comment)
    {
        var resource = new TestResource(() => this.Client.DeleteCommentAsync(comment.id));
        this.resources.Add(resource);
        return this.AddTo(comment);
    }

    public UserItem ToBeDiscarded(UserItem user)
    {
        var resource = new TestResource(() => this.Client.DeleteUserAsync(user.id));
        this.resources.Add(resource);
        return this.AddTo(user);
    }

    public RoleItem ToBeDiscarded(RoleItem role)
    {
        var resource = new TestResource(() => this.Client.DeleteRoleAsync(role.id));
        this.resources.Add(resource);
        return this.AddTo(role);
    }

    public ImportsItem ToBeDiscarded(ImportsItem imports)
    {
        var resource = new TestResource(() => this.Client.DeleteImportsAsync(imports.id));
        this.resources.Add(resource);
        return this.AddTo(imports);
    }

    public AttachmentItem AddTo(AttachmentItem attachment)
    {
        this.attachments.Add(attachment);
        return attachment;
    }

    public BookItem AddTo(BookItem book)
    {
        this.books.Add(book);
        return book;
    }

    public ChapterItem AddTo(ChapterItem chapter)
    {
        this.chapters.Add(chapter);
        return chapter;
    }

    public PageItem AddTo(PageItem page)
    {
        this.pages.Add(page);
        return page;
    }

    public ShelfItem AddTo(ShelfItem shelf)
    {
        this.shelves.Add(shelf);
        return shelf;
    }

    public ImageItem AddTo(ImageItem image)
    {
        this.images.Add(image);
        return image;
    }

    public CommentItem AddTo(CommentItem comment)
    {
        this.comments.Add(comment);
        return comment;
    }

    public UserItem AddTo(UserItem user)
    {
        this.users.Add(user);
        return user;
    }

    public RoleItem AddTo(RoleItem role)
    {
        this.roles.Add(role);
        return role;
    }

    public ImportsItem AddTo(ImportsItem imports)
    {
        this.imports.Add(imports);
        return imports;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var res in this.resources.AsEnumerable().Reverse())
        {
            try { await res.DisposeAsync().ConfigureAwait(false); } catch { }
        }
        this.resources.Clear();
        this.attachments.Clear();
        this.books.Clear();
        this.chapters.Clear();
        this.pages.Clear();
        this.shelves.Clear();
        this.images.Clear();
        this.comments.Clear();
        this.users.Clear();
    }

    private class TestResource : IAsyncDisposable
    {
        public TestResource(Func<Task> disposer)
        {
            this.disposer = disposer;
        }

        public async ValueTask DisposeAsync()
        {
            await this.disposer().ConfigureAwait(false);
        }

        private Func<Task> disposer;
    }

    private List<TestResource> resources;
    private List<AttachmentItem> attachments;
    private List<BookItem> books;
    private List<ChapterItem> chapters;
    private List<PageItem> pages;
    private List<ShelfItem> shelves;
    private List<ImageItem> images;
    private List<CommentItem> comments;
    private List<UserItem> users;
    private List<RoleItem> roles;
    private List<ImportsItem> imports;
}

public static class TestResourceContainerExtensions
{
    public static async Task<AttachmentItem> WillBeDiscarded(this Task<AttachmentItem> self, TestResourceContainer container)
        => container.ToBeDiscarded(await self.ConfigureAwait(false));

    public static async Task<BookItem> WillBeDiscarded(this Task<BookItem> self, TestResourceContainer container)
        => container.ToBeDiscarded(await self.ConfigureAwait(false));

    public static async Task<ChapterItem> WillBeDiscarded(this Task<ChapterItem> self, TestResourceContainer container)
        => container.ToBeDiscarded(await self.ConfigureAwait(false));

    public static async Task<PageItem> WillBeDiscarded(this Task<PageItem> self, TestResourceContainer container)
        => container.ToBeDiscarded(await self.ConfigureAwait(false));

    public static async Task<ShelfItem> WillBeDiscarded(this Task<ShelfItem> self, TestResourceContainer container)
        => container.ToBeDiscarded(await self.ConfigureAwait(false));

    public static async Task<ImageItem> WillBeDiscarded(this Task<ImageItem> self, TestResourceContainer container)
        => container.ToBeDiscarded(await self.ConfigureAwait(false));

    public static async Task<CommentItem> WillBeDiscarded(this Task<CommentItem> self, TestResourceContainer container)
        => container.ToBeDiscarded(await self.ConfigureAwait(false));

    public static async Task<UserItem> WillBeDiscarded(this Task<UserItem> self, TestResourceContainer container)
        => container.ToBeDiscarded(await self.ConfigureAwait(false));

    public static async Task<RoleItem> WillBeDiscarded(this Task<RoleItem> self, TestResourceContainer container)
        => container.ToBeDiscarded(await self.ConfigureAwait(false));

    public static async Task<ImportsItem> WillBeDiscarded(this Task<ImportsItem> self, TestResourceContainer container)
        => container.ToBeDiscarded(await self.ConfigureAwait(false));

    public static async Task<AttachmentItem> AddTo(this Task<AttachmentItem> self, TestResourceContainer container)
        => container.AddTo(await self.ConfigureAwait(false));

    public static async Task<BookItem> AddTo(this Task<BookItem> self, TestResourceContainer container)
        => container.AddTo(await self.ConfigureAwait(false));

    public static async Task<ChapterItem> AddTo(this Task<ChapterItem> self, TestResourceContainer container)
        => container.AddTo(await self.ConfigureAwait(false));

    public static async Task<PageItem> AddTo(this Task<PageItem> self, TestResourceContainer container)
        => container.AddTo(await self.ConfigureAwait(false));

    public static async Task<ShelfItem> AddTo(this Task<ShelfItem> self, TestResourceContainer container)
        => container.AddTo(await self.ConfigureAwait(false));

    public static async Task<ImageItem> AddTo(this Task<ImageItem> self, TestResourceContainer container)
        => container.AddTo(await self.ConfigureAwait(false));

    public static async Task<CommentItem> AddTo(this Task<CommentItem> self, TestResourceContainer container)
        => container.AddTo(await self.ConfigureAwait(false));

    public static async Task<UserItem> AddTo(this Task<UserItem> self, TestResourceContainer container)
        => container.AddTo(await self.ConfigureAwait(false));

    public static async Task<RoleItem> AddTo(this Task<RoleItem> self, TestResourceContainer container)
        => container.AddTo(await self.ConfigureAwait(false));

    public static async Task<ImportsItem> AddTo(this Task<ImportsItem> self, TestResourceContainer container)
        => container.AddTo(await self.ConfigureAwait(false));
}
