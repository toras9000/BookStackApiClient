using BookStackApiClient.Tests;
using R3;

namespace BookStackApiClient.Utility.Tests;

[TestClass()]
public class BookStackClientHelperTests : BookStackClientTestsBase
{
    [TestMethod()]
    public async Task EnumerateAllShelvesAsync()
    {
        using var helper = new BookStackClientHelper(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret);

        await using var container = new TestResourceContainer(helper.Client);
        var testShelves = await Observable.Range(1, 10)
            .SelectAwait(async (n, c) => await helper.Client.CreateShelfAsync(new(testName($"shelve{n}"))).WillBeDiscarded(container))
            .ToArrayAsync();

        var actualShelves = await helper.EnumerateAllShelvesAsync(batchCount: 2).ToObservable().ToArrayAsync();
        testShelves.Should().AllSatisfy(i => actualShelves.Any(s => s.id == i.id));
    }

    [TestMethod()]
    public async Task EnumerateAllBooksAsync()
    {
        using var helper = new BookStackClientHelper(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret);

        await using var container = new TestResourceContainer(helper.Client);
        var testBooks = await Observable.Range(1, 10)
            .SelectAwait(async (n, c) => await helper.Client.CreateBookAsync(new(testName($"book{n}"))).WillBeDiscarded(container))
            .ToArrayAsync();

        var allBooks = await helper.EnumerateAllBooksAsync(batchCount: 2).ToObservable().ToArrayAsync();
        testBooks.Should().AllSatisfy(i => allBooks.Any(b => b.id == i.id));
    }

    [TestMethod()]
    public async Task EnumerateAllChaptersAsync()
    {
        using var helper = new BookStackClientHelper(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret);

        await using var container = new TestResourceContainer(helper.Client);
        var testBook = await helper.Client.CreateBookAsync(new(testName($"book"))).WillBeDiscarded(container);
        var testChapters = await Observable.Range(1, 10)
            .SelectAwait(async (n, c) => await helper.Client.CreateChapterAsync(new(testBook.id, testName($"chapter{n}"))).WillBeDiscarded(container))
            .ToArrayAsync();

        var allChapters = await helper.EnumerateAllChaptersAsync(batchCount: 2).ToObservable().ToArrayAsync();
        testChapters.Should().AllSatisfy(i => allChapters.Any(b => b.id == i.id));
    }

    [TestMethod()]
    public async Task EnumerateAllPagesAsync()
    {
        using var helper = new BookStackClientHelper(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret);

        await using var container = new TestResourceContainer(helper.Client);
        var testBook = await helper.Client.CreateBookAsync(new(testName($"book"))).WillBeDiscarded(container);
        var testPages = await Observable.Range(1, 10)
            .SelectAwait(async (n, c) => await helper.Client.CreateMarkdownPageInBookAsync(new(testBook.id, testName($"page{n}"), $"# page{n}")).WillBeDiscarded(container))
            .ToArrayAsync();

        var allPages = await helper.EnumerateAllPagesAsync(batchCount: 2).ToObservable().ToArrayAsync();
        testPages.Should().AllSatisfy(i => allPages.Any(b => b.id == i.id));
    }

    [TestMethod()]
    public async Task EnumerateAllUsersAsync()
    {
        using var helper = new BookStackClientHelper(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret);

        await using var container = new TestResourceContainer(helper.Client);
        var testUsers = await Observable.Range(1, 10)
            .SelectAwait(async (n, c) => await helper.Client.CreateUserAsync(new(testName($"user{n}"), $"user{n}-{DateTime.Now.Ticks:X16}@example.com")).WillBeDiscarded(container))
            .ToArrayAsync();

        var allUsers = await helper.EnumerateAllUsersAsync(batchCount: 2).ToObservable().ToArrayAsync();
        testUsers.Should().AllSatisfy(i => allUsers.Any(u => u.id == i.id));
    }

    [TestMethod()]
    public async Task EnumerateAllRolesAsync()
    {
        using var helper = new BookStackClientHelper(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret);

        await using var container = new TestResourceContainer(helper.Client);
        var testUser = await helper.Client.CreateUserAsync(new(testName($"user"), $"user-{DateTime.Now.Ticks:X16}@example.com")).WillBeDiscarded(container);
        var testRoles = await Observable.Range(1, 10)
            .SelectAwait(async (n, c) => await helper.Client.CreateRoleAsync(new(testName($"role{n}"))).WillBeDiscarded(container))
            .ToArrayAsync();

        var allRoles = await helper.EnumerateAllRolesAsync(batchCount: 2).ToObservable().ToArrayAsync();
        testRoles.Should().AllSatisfy(i => allRoles.Any(u => u.id == i.id));
    }

    [TestMethod()]
    public async Task EnumerateAllAttachmentsAsync()
    {
        using var helper = new BookStackClientHelper(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret);

        await using var container = new TestResourceContainer(helper.Client);
        var testBook = await helper.Client.CreateBookAsync(new(testName($"book"))).WillBeDiscarded(container);
        var testPage = await helper.Client.CreateMarkdownPageInBookAsync(new(testBook.id, testName($"page"), $"# page")).WillBeDiscarded(container);
        var testAttachments = await Observable.Range(1, 10)
            .SelectAwait(async (n, c) => await helper.Client.CreateFileAttachmentAsync(new(testName($"attach{n}"), testPage.id), [0x01, 0x02], $"attach{n}").WillBeDiscarded(container))
            .ToArrayAsync();

        var allAttaches = await helper.EnumerateAllAttachmentsAsync(batchCount: 2).ToObservable().ToArrayAsync();
        testAttachments.Should().AllSatisfy(i => allAttaches.Any(b => b.id == i.id));

        var pageAttaches = await helper.EnumeratePageAttachmentsAsync(testPage.id, batchCount: 2).ToObservable().ToArrayAsync();
        testAttachments.Should().AllSatisfy(i => pageAttaches.Any(b => b.id == i.id));
    }

    [TestMethod()]
    public async Task EnumerateAllImagesAsync()
    {
        using var helper = new BookStackClientHelper(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret);

        await using var container = new TestResourceContainer(helper.Client);
        var testBook = await helper.Client.CreateBookAsync(new(testName($"book"))).WillBeDiscarded(container);
        var testPage = await helper.Client.CreateMarkdownPageInBookAsync(new(testBook.id, testName($"page"), $"# page")).WillBeDiscarded(container);
        var testImages = await Observable.Range(1, 10)
            .SelectAwait(async (n, c) => await helper.Client.CreateImageAsync(new(testPage.id, "gallery", testName($"iamge{n}")), [0x01, 0x02], $"image{n}").WillBeDiscarded(container))
            .ToArrayAsync();

        var allImages = await helper.EnumerateAllImagesAsync(batchCount: 2).ToObservable().ToArrayAsync();
        testImages.Should().AllSatisfy(i => allImages.Any(b => b.id == i.id));

        var pageImages = await helper.EnumeratePageImagesAsync(testPage.id, batchCount: 2).ToObservable().ToArrayAsync();
        testImages.Should().AllSatisfy(i => pageImages.Any(b => b.id == i.id));
    }

    [TestMethod()]
    public async Task EnumerateAllRecycleItemsAsync()
    {
        using var helper = new BookStackClientHelper(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret);

        await using var container = new TestResourceContainer(helper.Client);
        var testBooks = await Observable.Range(1, 10)
            .SelectAwait(async (n, c) => await helper.Client.CreateBookAsync(new(testName($"book{n}"))).WillBeDiscarded(container))
            .ToArrayAsync();
        await container.DisposeAsync();

        var allItems = await helper.EnumerateAllRecycleItemsAsync(batchCount: 2).ToObservable().ToArrayAsync();
        testBooks.Should().AllSatisfy(i => allItems.Any(t => t.deletable_type == "book" && t.deletable_id == i.id));
    }

    [TestMethod()]
    public async Task EnumerateAllAuditLogsAsync()
    {
        using var helper = new BookStackClientHelper(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret);

        await using var container = new TestResourceContainer(helper.Client);
        var testBook = await helper.Client.CreateBookAsync(new(testName($"book"))).WillBeDiscarded(container);
        var allLogs = await helper.EnumerateAllAuditLogsAsync(batchCount: 500).ToObservable().ToArrayAsync();
        allLogs.Should().NotBeEmpty();
    }

    [TestMethod()]
    public async Task EnumerateAllSearchAsync()
    {
        using var helper = new BookStackClientHelper(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret);

        await using var container = new TestResourceContainer(helper.Client);
        var testBooks = await Observable.Range(1, 10)
            .SelectAwait(async (n, c) => await helper.Client.CreateBookAsync(new(testName($"book{n}"))).WillBeDiscarded(container))
            .ToArrayAsync();
        var results = await helper.EnumerateAllSearchAsync(new("{in_name:book}", count: 1)).ToObservable().ToArrayAsync();
        results.Select(r => r.id).Should().Contain(testBooks.Select(b => b.id));
    }

    [TestMethod()]
    public async Task GetMeAsync()
    {
        using var helper = new BookStackClientHelper(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret);

        await using var container = new TestResourceContainer(helper.Client);
        var testBook = await helper.Client.CreateBookAsync(new(testName($"book"))).WillBeDiscarded(container);
        var me = await helper.GetMeAsync();
        me.Should().NotBeNull();
    }

}
