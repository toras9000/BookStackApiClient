namespace BookStackApiClient.Tests;

[TestClass()]
public class BookStackClientTagsTests : BookStackClientTestsBase
{
    #region tags
    [TestMethod()]
    public async Task ListTagNamesAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var shelf = await client.CreateShelfAsync(new(testName("shelve1"), tags: [new("TagName_shelf1", "TagValue_shelf1"),])).WillBeDiscarded(container);
        var book = await client.CreateBookAsync(new(testName("book1"), tags: [new("TagName_book1", "TagValue_book1")])).WillBeDiscarded(container);
        var chapter = await client.CreateChapterAsync(new(book.id, testName("chapter1"), tags: [new("TagName_chapter1", "TagValue_chapter1"),])).WillBeDiscarded(container);
        var page = await client.CreateMarkdownPageInBookAsync(new(book.id, testName("page1"), "asd", tags: [new("TagName_page1", "TagValue_page1"),])).WillBeDiscarded(container);

        var tags = await client.ListTagNamesAsync();

        var shelf_tag = tags.data.FirstOrDefault(t => t.name == "TagName_shelf1");
        {
            shelf_tag.Should().NotBeNull();
            shelf_tag.values.Should().BeGreaterThanOrEqualTo(1);
            shelf_tag.usages.Should().BeGreaterThanOrEqualTo(1);
            shelf_tag.shelf_count.Should().BeGreaterThanOrEqualTo(1);
            shelf_tag.book_count.Should().Be(0);
            shelf_tag.chapter_count.Should().Be(0);
            shelf_tag.page_count.Should().Be(0);
        }

        var book_tag = tags.data.FirstOrDefault(t => t.name == "TagName_book1");
        {
            book_tag.Should().NotBeNull();
            book_tag.values.Should().BeGreaterThanOrEqualTo(1);
            book_tag.usages.Should().BeGreaterThanOrEqualTo(1);
            book_tag.book_count.Should().BeGreaterThanOrEqualTo(1);
            book_tag.shelf_count.Should().Be(0);
            book_tag.chapter_count.Should().Be(0);
            book_tag.page_count.Should().Be(0);
        }

        var chapter_tag = tags.data.FirstOrDefault(t => t.name == "TagName_chapter1");
        {
            chapter_tag.Should().NotBeNull();
            chapter_tag.values.Should().BeGreaterThanOrEqualTo(1);
            chapter_tag.usages.Should().BeGreaterThanOrEqualTo(1);
            chapter_tag.chapter_count.Should().BeGreaterThanOrEqualTo(1);
            chapter_tag.book_count.Should().Be(0);
            chapter_tag.shelf_count.Should().Be(0);
            chapter_tag.page_count.Should().Be(0);
        }

        var page_tag = tags.data.FirstOrDefault(t => t.name == "TagName_page1");
        {
            page_tag.Should().NotBeNull();
            page_tag.values.Should().BeGreaterThanOrEqualTo(1);
            page_tag.usages.Should().BeGreaterThanOrEqualTo(1);
            page_tag.page_count.Should().BeGreaterThanOrEqualTo(1);
            page_tag.book_count.Should().Be(0);
            page_tag.chapter_count.Should().Be(0);
            page_tag.shelf_count.Should().Be(0);
        }
    }

    [TestMethod()]
    public async Task ListTagValuesAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var shelf = await client.CreateShelfAsync(new(testName("shelve1"), tags: [new("TagName_shelf1", "TagValue_shelf1"),])).WillBeDiscarded(container);
        var book = await client.CreateBookAsync(new(testName("book1"), tags: [new("TagName_book1", "TagValue_book1")])).WillBeDiscarded(container);
        var chapter = await client.CreateChapterAsync(new(book.id, testName("chapter1"), tags: [new("TagName_chapter1", "TagValue_chapter1"),])).WillBeDiscarded(container);
        var page = await client.CreateMarkdownPageInBookAsync(new(book.id, testName("page1"), "asd", tags: [new("TagName_page1", "TagValue_page1"),])).WillBeDiscarded(container);

        var shelf_tags = await client.ListTagValuesAsync("TagName_shelf1");
        var shelf_tag = shelf_tags.data.Should().HaveCount(1).And.Subject.First();
        {
            shelf_tag.Should().NotBeNull();
            shelf_tag.value.Should().Be("TagValue_shelf1");
            shelf_tag.usages.Should().BeGreaterThanOrEqualTo(1);
            shelf_tag.shelf_count.Should().BeGreaterThanOrEqualTo(1);
            shelf_tag.book_count.Should().Be(0);
            shelf_tag.chapter_count.Should().Be(0);
            shelf_tag.page_count.Should().Be(0);
        }

        var book_tags = await client.ListTagValuesAsync("TagName_book1");
        var book_tag = book_tags.data.Should().HaveCount(1).And.Subject.First();
        {
            book_tag.Should().NotBeNull();
            book_tag.value.Should().Be("TagValue_book1");
            book_tag.usages.Should().BeGreaterThanOrEqualTo(1);
            book_tag.book_count.Should().BeGreaterThanOrEqualTo(1);
            book_tag.shelf_count.Should().Be(0);
            book_tag.chapter_count.Should().Be(0);
            book_tag.page_count.Should().Be(0);
        }

        var chapter_tags = await client.ListTagValuesAsync("TagName_chapter1");
        var chapter_tag = chapter_tags.data.Should().HaveCount(1).And.Subject.First();
        {
            chapter_tag.Should().NotBeNull();
            chapter_tag.value.Should().Be("TagValue_chapter1");
            chapter_tag.usages.Should().BeGreaterThanOrEqualTo(1);
            chapter_tag.chapter_count.Should().BeGreaterThanOrEqualTo(1);
            chapter_tag.book_count.Should().Be(0);
            chapter_tag.shelf_count.Should().Be(0);
            chapter_tag.page_count.Should().Be(0);
        }

        var page_tags = await client.ListTagValuesAsync("TagName_page1");
        var page_tag = page_tags.data.Should().HaveCount(1).And.Subject.First();
        {
            page_tag.Should().NotBeNull();
            page_tag.value.Should().Be("TagValue_page1");
            page_tag.usages.Should().BeGreaterThanOrEqualTo(1);
            page_tag.page_count.Should().BeGreaterThanOrEqualTo(1);
            page_tag.book_count.Should().Be(0);
            page_tag.chapter_count.Should().Be(0);
            page_tag.shelf_count.Should().Be(0);
        }
    }
    #endregion
}