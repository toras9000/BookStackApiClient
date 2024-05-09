namespace BookStackApiClient.Tests;

[TestClass()]
public class BookStackClientAuditLogTests : BookStackClientTestsBase
{
    [TestMethod()]
    public async Task ListAuditLogAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // テスト用のオブジェクトを作成
        await using var container = new TestResourceContainer(client);
        var now = DateTime.UtcNow;
        var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
        var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter"))).WillBeDiscarded(container);
        var page_in_book = await client.CreateMarkdownPageInBookAsync(new(book.id, testName("testpage_in_book"), "in_book")).WillBeDiscarded(container);
        var page_in_chapter = await client.CreateMarkdownPageInChapterAsync(new(chapter.id, testName("testpage_in_chapter"), "in_chapter")).WillBeDiscarded(container);
        var shelf_has_book = await client.CreateShelfAsync(new(testName("testshelf_has_book"), books: new[] { book.id, })).WillBeDiscarded(container);
        var shelf_no_book = await client.CreateShelfAsync(new(testName("testshelf_no_book"))).WillBeDiscarded(container);

        // 更新する
        book = await client.UpdateBookAsync(book.id, new(testName("testbook-renamed")));
        chapter = await client.UpdateChapterAsync(chapter.id, new(testName("testchapter-renamed")));
        page_in_book = await client.UpdatePageAsync(page_in_book.id, new(testName("testpage_in_book-renamed")));
        page_in_chapter = await client.UpdatePageAsync(page_in_chapter.id, new(testName("testpage_in_chapter-renamed")));

        // 削除する
        await container.DisposeAsync();

        // 監査ログをすべて取得
        var items = await client.ListAllAuditLogAsync();

        // 取得結果を検証
        items.Should().ContainEquivalentOf(new { user_id = this.ApiUserID, type = "book_create", loggable_type = "book", loggable_id = book.id, });
        items.Should().ContainEquivalentOf(new { user_id = this.ApiUserID, type = "book_update", loggable_type = "book", loggable_id = book.id, });
        items.Should().ContainEquivalentOf(new { user_id = this.ApiUserID, type = "book_delete", loggable_type = "book", loggable_id = book.id, });
        items.Should().ContainEquivalentOf(new { user_id = this.ApiUserID, type = "chapter_create", loggable_type = "chapter", loggable_id = chapter.id, });
        items.Should().ContainEquivalentOf(new { user_id = this.ApiUserID, type = "chapter_update", loggable_type = "chapter", loggable_id = chapter.id, });
        items.Should().ContainEquivalentOf(new { user_id = this.ApiUserID, type = "chapter_delete", loggable_type = "chapter", loggable_id = chapter.id, });
        items.Should().ContainEquivalentOf(new { user_id = this.ApiUserID, type = "page_create", loggable_type = "page", loggable_id = page_in_book.id, });
        items.Should().ContainEquivalentOf(new { user_id = this.ApiUserID, type = "page_update", loggable_type = "page", loggable_id = page_in_book.id, });
        items.Should().ContainEquivalentOf(new { user_id = this.ApiUserID, type = "page_delete", loggable_type = "page", loggable_id = page_in_book.id, });
        items.Should().ContainEquivalentOf(new { user_id = this.ApiUserID, type = "page_create", loggable_type = "page", loggable_id = page_in_chapter.id, });
        items.Should().ContainEquivalentOf(new { user_id = this.ApiUserID, type = "page_update", loggable_type = "page", loggable_id = page_in_chapter.id, });
        items.Should().ContainEquivalentOf(new { user_id = this.ApiUserID, type = "page_delete", loggable_type = "page", loggable_id = page_in_chapter.id, });
        items.Should().ContainEquivalentOf(new { user_id = this.ApiUserID, type = "bookshelf_create", loggable_type = "bookshelf", loggable_id = shelf_has_book.id, });
        items.Should().ContainEquivalentOf(new { user_id = this.ApiUserID, type = "bookshelf_delete", loggable_type = "bookshelf", loggable_id = shelf_has_book.id, });
        items.Should().ContainEquivalentOf(new { user_id = this.ApiUserID, type = "bookshelf_create", loggable_type = "bookshelf", loggable_id = shelf_no_book.id, });
        items.Should().ContainEquivalentOf(new { user_id = this.ApiUserID, type = "bookshelf_delete", loggable_type = "bookshelf", loggable_id = shelf_no_book.id, });
    }
}
