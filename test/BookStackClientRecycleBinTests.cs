using System.Linq;

namespace BookStackApiClient.Tests;

[TestClass()]
public class BookStackClientRecycleBinTests : BookStackClientTestsBase
{
    #region recycle-bin
    [TestMethod()]
    public async Task ListRecycleBinAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var now = DateTime.UtcNow;
        var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
        var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter"))).WillBeDiscarded(container);
        var page_in_book = await client.CreateMarkdownPageInBookAsync(new(book.id, testName("testpage_in_book"), "in_book")).WillBeDiscarded(container);
        var page_in_chapter = await client.CreateMarkdownPageInChapterAsync(new(chapter.id, testName("testpage_in_chapter"), "in_chapter")).WillBeDiscarded(container);
        var shelf_has_book = await client.CreateShelfAsync(new(testName("testshelf_has_book"), books: new[] { book.id, })).WillBeDiscarded(container);
        var shelf_no_book = await client.CreateShelfAsync(new(testName("testshelf_no_book"))).WillBeDiscarded(container);
        await container.DisposeAsync();

        // すべて取得
        var items = new List<RecycleItem>();
        var offset = 0;
        while (true)
        {
            var result = await client.ListRecycleBinAsync(new(offset));
            if (result.data.Length <= 0) break;

            items.AddRange(result.data);
            offset += result.data.Length;
            if (result.total <= offset) break;
        }

        // 取得内容の検証
        {
            var expect = book;
            var item = items.Should().Contain(i => i.deletable_type == "book" && i.deletable_id == expect.id).Subject;
            item.id.Should().NotBe(0);
            item.deleted_by.Should().Be(this.ApiUserID);
            item.created_at.Should().BeCloseTo(now, 10.Seconds());
            item.updated_at.Should().BeCloseTo(now, 10.Seconds());

            var deletable = item.deletable.Should().BeOfType<DeletableContentBook>().Subject;
            deletable.id.Should().Be(expect.id);
            deletable.name.Should().Be(expect.name);
            deletable.slug.Should().Be(expect.slug);
            deletable.created_at.Should().BeCloseTo(expect.created_at, 10.Seconds());
            deletable.updated_at.Should().BeCloseTo(expect.updated_at, 10.Seconds());
            deletable.created_by.Should().Be(expect.created_by);
            deletable.updated_by.Should().Be(expect.updated_by);
            deletable.owned_by.Should().Be(expect.owned_by);
            deletable.description.Should().Be(expect.description);
            deletable.chapters_count.Should().Be(1);
            deletable.pages_count.Should().Be(2);
        }
        {
            var expect = chapter;
            var item = items.Should().Contain(i => i.deletable_type == "chapter" && i.deletable_id == expect.id).Subject;
            item.id.Should().NotBe(0);
            item.deleted_by.Should().Be(this.ApiUserID);
            item.created_at.Should().BeCloseTo(now, 10.Seconds());
            item.updated_at.Should().BeCloseTo(now, 10.Seconds());

            var deletable = item.deletable.Should().BeOfType<DeletableContentChapter>().Subject;
            deletable.id.Should().Be(expect.id);
            deletable.name.Should().Be(expect.name);
            deletable.slug.Should().Be(expect.slug);
            deletable.created_at.Should().BeCloseTo(expect.created_at, 10.Seconds());
            deletable.updated_at.Should().BeCloseTo(expect.updated_at, 10.Seconds());
            deletable.created_by.Should().Be(expect.created_by);
            deletable.updated_by.Should().Be(expect.updated_by);
            deletable.owned_by.Should().Be(expect.owned_by);
            deletable.description.Should().Be(expect.description);
            deletable.book_id.Should().Be(expect.book_id);
            deletable.priority.Should().NotBe(0);
            deletable.pages_count.Should().Be(1);

            var parent = deletable.parent.Should().BeOfType<DeletableContentParentBook>().Subject;
            parent.id.Should().Be(book.id);
            parent.name.Should().Be(book.name);
            parent.slug.Should().Be(book.slug);
            parent.created_at.Should().BeCloseTo(book.created_at, 10.Seconds());
            parent.updated_at.Should().BeCloseTo(book.updated_at, 10.Seconds());
            parent.created_by.Should().Be(book.created_by);
            parent.updated_by.Should().Be(book.updated_by);
            parent.owned_by.Should().Be(book.owned_by);
            parent.type.Should().Be("book");
            parent.description.Should().Be(book.description);
        }
        {
            var expect = page_in_book;
            var item = items.Should().Contain(i => i.deletable_type == "page" && i.deletable_id == expect.id).Subject;
            item.id.Should().NotBe(0);
            item.deleted_by.Should().Be(this.ApiUserID);
            item.created_at.Should().BeCloseTo(now, 10.Seconds());
            item.updated_at.Should().BeCloseTo(now, 10.Seconds());

            var deletable = item.deletable.Should().BeOfType<DeletableContentPage>().Subject;
            deletable.id.Should().Be(expect.id);
            deletable.name.Should().Be(expect.name);
            deletable.slug.Should().Be(expect.slug);
            deletable.created_at.Should().BeCloseTo(expect.created_at, 10.Seconds());
            deletable.updated_at.Should().BeCloseTo(expect.updated_at, 10.Seconds());
            deletable.created_by.Should().Be(expect.created_by.id);
            deletable.updated_by.Should().Be(expect.updated_by.id);
            deletable.owned_by.Should().Be(expect.owned_by.id);
            deletable.book_id.Should().Be(expect.book_id);
            deletable.book_slug.Should().NotBeEmpty();
            deletable.chapter_id.Should().Be(expect.chapter_id);
            deletable.draft.Should().Be(expect.draft);
            deletable.template.Should().Be(expect.template);
            deletable.editor.Should().Be(expect.editor);
            deletable.priority.Should().Be(expect.priority);
            deletable.revision_count.Should().Be(expect.revision_count);

            var parent = deletable.parent.Should().BeOfType<DeletableContentParentBook>().Subject;
            parent.id.Should().Be(book.id);
            parent.name.Should().Be(book.name);
            parent.slug.Should().Be(book.slug);
            parent.created_at.Should().BeCloseTo(book.created_at, 10.Seconds());
            parent.updated_at.Should().BeCloseTo(book.updated_at, 10.Seconds());
            parent.created_by.Should().Be(book.created_by);
            parent.updated_by.Should().Be(book.updated_by);
            parent.owned_by.Should().Be(book.owned_by);
            parent.type.Should().Be("book");
            parent.description.Should().Be(book.description);

        }
        {
            var expect = page_in_chapter;
            var item = items.Should().Contain(i => i.deletable_type == "page" && i.deletable_id == expect.id).Subject;
            item.id.Should().NotBe(0);
            item.deleted_by.Should().Be(this.ApiUserID);
            item.created_at.Should().BeCloseTo(now, 10.Seconds());
            item.updated_at.Should().BeCloseTo(now, 10.Seconds());

            var deletable = item.deletable.Should().BeOfType<DeletableContentPage>().Subject;
            deletable.id.Should().Be(expect.id);
            deletable.name.Should().Be(expect.name);
            deletable.slug.Should().Be(expect.slug);
            deletable.created_at.Should().BeCloseTo(expect.created_at, 10.Seconds());
            deletable.updated_at.Should().BeCloseTo(expect.updated_at, 10.Seconds());
            deletable.created_by.Should().Be(expect.created_by.id);
            deletable.updated_by.Should().Be(expect.updated_by.id);
            deletable.owned_by.Should().Be(expect.owned_by.id);
            deletable.book_id.Should().Be(expect.book_id);
            deletable.book_slug.Should().NotBeNullOrEmpty();
            deletable.chapter_id.Should().Be(expect.chapter_id);
            deletable.draft.Should().Be(expect.draft);
            deletable.template.Should().Be(expect.template);
            deletable.editor.Should().Be(expect.editor);
            deletable.priority.Should().Be(expect.priority);
            deletable.revision_count.Should().Be(expect.revision_count);

            var parent = deletable.parent.Should().BeOfType<DeletableContentParentChapter>().Subject;
            parent.id.Should().Be(chapter.id);
            parent.name.Should().Be(chapter.name);
            parent.slug.Should().Be(chapter.slug);
            parent.created_at.Should().BeCloseTo(chapter.created_at, 10.Seconds());
            parent.updated_at.Should().BeCloseTo(chapter.updated_at, 10.Seconds());
            parent.created_by.Should().Be(chapter.created_by);
            parent.updated_by.Should().Be(chapter.updated_by);
            parent.owned_by.Should().Be(chapter.owned_by);
            parent.type.Should().Be("chapter");
            parent.description.Should().Be(chapter.description);
            parent.book_id.Should().Be(chapter.book_id);
            parent.book_slug.Should().NotBeNullOrEmpty();
            parent.priority.Should().Be(chapter.priority);
        }
        {
            var expect = shelf_has_book;
            var item = items.Should().Contain(i => i.deletable_type == "bookshelf" && i.deletable_id == expect.id).Subject;
            item.id.Should().NotBe(0);
            item.deleted_by.Should().Be(this.ApiUserID);
            item.created_at.Should().BeCloseTo(now, 10.Seconds());
            item.updated_at.Should().BeCloseTo(now, 10.Seconds());

            var deletable = item.deletable.Should().BeOfType<DeletableContentShelf>().Subject;
            deletable.id.Should().Be(expect.id);
            deletable.name.Should().Be(expect.name);
            deletable.slug.Should().Be(expect.slug);
            deletable.created_at.Should().BeCloseTo(expect.created_at, 10.Seconds());
            deletable.updated_at.Should().BeCloseTo(expect.updated_at, 10.Seconds());
            deletable.created_by.Should().Be(expect.created_by);
            deletable.updated_by.Should().Be(expect.updated_by);
            deletable.owned_by.Should().Be(expect.owned_by);
            deletable.description.Should().Be(expect.description);
        }
        {
            var expect = shelf_no_book;
            var item = items.Should().Contain(i => i.deletable_type == "bookshelf" && i.deletable_id == expect.id).Subject;
            item.id.Should().NotBe(0);
            item.deleted_by.Should().Be(this.ApiUserID);
            item.created_at.Should().BeCloseTo(now, 10.Seconds());
            item.updated_at.Should().BeCloseTo(now, 10.Seconds());

            var deletable = item.deletable.Should().BeOfType<DeletableContentShelf>().Subject;
            deletable.id.Should().Be(expect.id);
            deletable.name.Should().Be(expect.name);
            deletable.slug.Should().Be(expect.slug);
            deletable.created_at.Should().BeCloseTo(expect.created_at, 10.Seconds());
            deletable.updated_at.Should().BeCloseTo(expect.updated_at, 10.Seconds());
            deletable.created_by.Should().Be(expect.created_by);
            deletable.updated_by.Should().Be(expect.updated_by);
            deletable.owned_by.Should().Be(expect.owned_by);
            deletable.description.Should().Be(expect.description);
        }
    }

    [TestMethod()]
    public async Task RestoreRecycleItemAsync_shelf()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var shelf = await client.CreateShelfAsync(new(testName("testshelf"))).WillBeDiscarded(container);
        await client.DeleteShelfAsync(shelf.id);

        var recycles = await client.ListRecycleBinAsync(new(filters: [new("deletable_type", "bookshelf"), new("deletable_id", $"{shelf.id}"),]));
        var item = recycles.data[0];

        (await client.ListShelvesAsync(new(filters: [new("id", $"{shelf.id}")]))).data.Should().BeEmpty();
        var restored = await client.RestoreRecycleItemAsync(item.id);
        restored.restore_count.Should().NotBe(0);
        (await client.ListShelvesAsync(new(filters: [new("id", $"{shelf.id}")]))).data.Should().NotBeEmpty();
    }

    [TestMethod()]
    public async Task RestoreRecycleItemAsync_book()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
        await client.DeleteBookAsync(book.id);

        var recycles = await client.ListRecycleBinAsync(new(filters: [new("deletable_type", "book"), new("deletable_id", $"{book.id}"),]));
        var item = recycles.data[0];

        (await client.ListBooksAsync(new(filters: [new("id", $"{book.id}")]))).data.Should().BeEmpty();
        var restored = await client.RestoreRecycleItemAsync(item.id);
        restored.restore_count.Should().NotBe(0);
        (await client.ListBooksAsync(new(filters: [new("id", $"{book.id}")]))).data.Should().NotBeEmpty();
    }

    [TestMethod()]
    public async Task RestoreRecycleItemAsync_chapter()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
        var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter"))).WillBeDiscarded(container);
        await client.DeleteChapterAsync(chapter.id);

        var recycles = await client.ListRecycleBinAsync(new(filters: [new("deletable_type", "chapter"), new("deletable_id", $"{chapter.id}"),]));
        var item = recycles.data[0];

        (await client.ListChaptersAsync(new(filters: [new("id", $"{chapter.id}")]))).data.Should().BeEmpty();
        var restored = await client.RestoreRecycleItemAsync(item.id);
        restored.restore_count.Should().NotBe(0);
        (await client.ListChaptersAsync(new(filters: [new("id", $"{chapter.id}")]))).data.Should().NotBeEmpty();
    }

    [TestMethod()]
    public async Task RestoreRecycleItemAsync_page()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
        var page = await client.CreateMarkdownPageInBookAsync(new(book.id, testName("testpage"), "in_book")).WillBeDiscarded(container);
        await client.DeletePageAsync(page.id);

        var recycles = await client.ListRecycleBinAsync(new(filters: [new("deletable_type", "page"), new("deletable_id", $"{page.id}"),]));
        var item = recycles.data[0];

        (await client.ListPagesAsync(new(filters: [new("id", $"{page.id}")]))).data.Should().BeEmpty();
        var restored = await client.RestoreRecycleItemAsync(item.id);
        restored.restore_count.Should().NotBe(0);
        (await client.ListPagesAsync(new(filters: [new("id", $"{page.id}")]))).data.Should().NotBeEmpty();
    }

    [TestMethod()]
    public async Task DestroyRecycleItemAsync_shelf()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var shelf = await client.CreateShelfAsync(new(testName("testshelf"))).WillBeDiscarded(container);
        await client.DeleteShelfAsync(shelf.id);

        var recycles = await client.ListRecycleBinAsync(new(filters: [new("deletable_type", "bookshelf"), new("deletable_id", $"{shelf.id}"),]));
        var item = recycles.data[0];

        await client.DestroyRecycleItemAsync(item.id);

        recycles = await client.ListRecycleBinAsync(new(filters: [new("deletable_type", "bookshelf"), new("deletable_id", $"{shelf.id}"),]));
        recycles.data.Should().BeEmpty();
    }

    [TestMethod()]
    public async Task DestroyRecycleItemAsync_book()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
        await client.DeleteBookAsync(book.id);

        var recycles = await client.ListRecycleBinAsync(new(filters: [new("deletable_type", "book"), new("deletable_id", $"{book.id}"),]));
        var item = recycles.data[0];

        await client.DestroyRecycleItemAsync(item.id);

        recycles = await client.ListRecycleBinAsync(new(filters: [new("deletable_type", "book"), new("deletable_id", $"{book.id}"),]));
        recycles.data.Should().BeEmpty();
    }

    [TestMethod()]
    public async Task DestroyRecycleItemAsync_chapter()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
        var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter"))).WillBeDiscarded(container);
        await client.DeleteChapterAsync(chapter.id);

        var recycles = await client.ListRecycleBinAsync(new(filters: [new("deletable_type", "chapter"), new("deletable_id", $"{chapter.id}"),]));
        var item = recycles.data[0];

        await client.DestroyRecycleItemAsync(item.id);

        recycles = await client.ListRecycleBinAsync(new(filters: [new("deletable_type", "chapter"), new("deletable_id", $"{chapter.id}"),]));
        recycles.data.Should().BeEmpty();
    }

    [TestMethod()]
    public async Task DestroyRecycleItemAsync_page()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
        var page = await client.CreateMarkdownPageInBookAsync(new(book.id, testName("testpage"), "in_book")).WillBeDiscarded(container);
        await client.DeletePageAsync(page.id);

        var recycles = await client.ListRecycleBinAsync(new(filters: [new("deletable_type", "page"), new("deletable_id", $"{page.id}"),]));
        var item = recycles.data[0];

        await client.DestroyRecycleItemAsync(item.id);

        recycles = await client.ListRecycleBinAsync(new(filters: [new("deletable_type", "page"), new("deletable_id", $"{page.id}"),]));
        recycles.data.Should().BeEmpty();
    }
    #endregion
}
