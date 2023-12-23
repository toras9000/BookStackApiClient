namespace BookStackApiClient.Tests;

[TestClass()]
public class BookStackClientPagesTests : BookStackClientTestsBase
{
    #region pages
    [TestMethod()]
    public async Task ListPagesAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
        await client.CreateMarkdownPageInBookAsync(new(book.id, testName($"page_{Guid.NewGuid()}"), "- aaa", priority: 4, tags: [new("tp1", "vp1"), new("tp2", "vp2"),]));
        await client.CreateHtmlPageInBookAsync(new(book.id, testName($"page_{Guid.NewGuid()}"), "<b>aa</b>", priority: 5, tags: [new("tp3", "vp3"), new("tp4", "vp4"),]));

        var pages = await client.ListPagesAsync();
        foreach (var created in container.Pages)
        {
            var actual = pages.data.Should().Contain(i => i.id == created.id).Subject;
            var expect = created;
            actual.Should().BeEquivalentTo(expect, o => o.ExcludingMissingMembers());
        }
    }

    [TestMethod()]
    public async Task ListPagesAsync_options()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);

        var prefix1 = testName($"page_{Guid.NewGuid()}_");
        for (var i = 0; i < 10; i++)
        {
            await client.CreateMarkdownPageInBookAsync(new(book.id, $"{prefix1}_{i:D2}", "markdown")).AddTo(container);
        }
        var prefix2 = testName($"page_{Guid.NewGuid()}_");
        for (var i = 0; i < 10; i++)
        {
            await client.CreateHtmlPageInBookAsync(new(book.id, $"{prefix2}_{i:D2}", "html")).AddTo(container);
        }

        {// range
            var pages1 = await client.ListPagesAsync(new(offset: 0, count: 5));
            pages1.data.Should().HaveCount(5);
            var pages2 = await client.ListPagesAsync(new(offset: 5, count: 5));
            pages2.data.Should().HaveCount(5);

            pages1.data.Select(d => d.id).Should().NotIntersectWith(pages2.data.Select(d => d.id));
        }
        {// filter
            var pages = await client.ListPagesAsync(new(filters: [new($"name:like", $"{prefix1}%")]));
            pages.data.Should().AllSatisfy(d => d.name.StartsWith(prefix1));
        }
        {// filter & sort (asc)
            var offset = 0;
            var count = 4;
            var pages = await client.ListPagesAsync(new(offset, count, sorts: [nameof(PageSummary.name),], filters: [new($"name:like", $"{prefix1}%")]));
            var expects = container.Pages.Where(c => c.name.StartsWith(prefix1)).Select(c => c.id).Skip(offset).Take(count);
            pages.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (desc)
            var offset = 0;
            var count = 4;
            var pages = await client.ListPagesAsync(new(offset, count, sorts: [$"-{nameof(PageSummary.name)}",], filters: [new($"name:like", $"{prefix1}%")]));
            var expects = container.Pages.AsEnumerable().Reverse().Where(c => c.name.StartsWith(prefix1)).Select(c => c.id).Skip(offset).Take(count);
            pages.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (asc) & range
            var offset = 2;
            var count = 5;
            var pages = await client.ListPagesAsync(new(offset, count, sorts: [nameof(PageSummary.name),], filters: [new($"name:like", $"{prefix2}%")]));
            var expects = container.Pages.Where(c => c.name.StartsWith(prefix2)).Select(c => c.id).Skip(offset).Take(count);
            pages.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (desc) & range
            var offset = 3;
            var count = 4;
            var pages = await client.ListPagesAsync(new(offset, count, sorts: [$"-{nameof(PageSummary.name)}",], filters: [new($"name:like", $"{prefix2}%")]));
            var expects = container.Pages.AsEnumerable().Reverse().Where(c => c.name.StartsWith(prefix2)).Select(c => c.id).Skip(offset).Take(count);
            pages.data.Select(d => d.id).Should().Equal(expects);
        }
    }

    [TestMethod()]
    public async Task CreatePageAsync_forBook()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {// name & markdown
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreatePageAsync(new(testName("aaa"), book_id: book.id, markdown: "aaa"));
            page.book_id.Should().Be(book.id);
            page.chapter_id.Should().Be(0);
            page.name.Should().Be(testName("aaa"));
            page.editor.Should().Be("markdown");
            page.draft.Should().BeFalse();
            page.template.Should().BeFalse();
            page.created_at.Should().BeCloseTo(now, 10.Seconds());
            page.updated_at.Should().BeCloseTo(now, 10.Seconds());
            page.created_by.id.Should().Be(book.created_by);
            page.updated_by.id.Should().Be(book.updated_by);
            page.owned_by.id.Should().Be(book.owned_by);
            page.tags.Should().BeEmpty();
        }
        {// name & html
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreatePageAsync(new(testName("aaa"), book_id: book.id, html: "aaa"));
            page.book_id.Should().Be(book.id);
            page.chapter_id.Should().Be(0);
            page.name.Should().Be(testName("aaa"));
            page.editor.Should().BeEmpty(); // html
            page.draft.Should().BeFalse();
            page.template.Should().BeFalse();
            page.created_at.Should().BeCloseTo(now, 10.Seconds());
            page.updated_at.Should().BeCloseTo(now, 10.Seconds());
            page.created_by.id.Should().Be(book.created_by);
            page.updated_by.id.Should().Be(book.updated_by);
            page.owned_by.id.Should().Be(book.owned_by);
            page.tags.Should().BeEmpty();
        }
        {//  tags
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreatePageAsync(new(testName("aaa"), book_id: book.id, markdown: "aaa", tags: [new("tpv1", "tpv1"), new("tpv2", "tpv2"),]));
            page.book_id.Should().Be(book.id);
            page.chapter_id.Should().Be(0);
            page.name.Should().Be(testName("aaa"));
            page.editor.Should().Be("markdown");
            page.draft.Should().BeFalse();
            page.template.Should().BeFalse();
            page.created_at.Should().BeCloseTo(now, 10.Seconds());
            page.updated_at.Should().BeCloseTo(now, 10.Seconds());
            page.created_by.id.Should().Be(book.created_by);
            page.updated_by.id.Should().Be(book.updated_by);
            page.owned_by.id.Should().Be(book.owned_by);
            page.tags.Should().BeEquivalentTo((Tag[])[new("tpv1", "tpv1"), new("tpv2", "tpv2"),]);
        }
    }

    [TestMethod()]
    public async Task CreatePageAsync_forChapter()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {// name & markdown
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter")));
            var page = await client.CreatePageAsync(new(testName("aaa"), chapter_id: chapter.id, markdown: "aaa"));
            page.book_id.Should().Be(book.id);
            page.chapter_id.Should().Be(chapter.id);
            page.name.Should().Be(testName("aaa"));
            page.editor.Should().Be("markdown");
            page.draft.Should().BeFalse();
            page.template.Should().BeFalse();
            page.created_at.Should().BeCloseTo(now, 10.Seconds());
            page.updated_at.Should().BeCloseTo(now, 10.Seconds());
            page.created_by.id.Should().Be(book.created_by);
            page.updated_by.id.Should().Be(book.updated_by);
            page.owned_by.id.Should().Be(book.owned_by);
            page.tags.Should().BeEmpty();
        }
        {// name & html
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter")));
            var page = await client.CreatePageAsync(new(testName("aaa"), chapter_id: chapter.id, html: "aaa"));
            page.book_id.Should().Be(book.id);
            page.chapter_id.Should().Be(chapter.id);
            page.name.Should().Be(testName("aaa"));
            page.editor.Should().BeEmpty(); // html
            page.draft.Should().BeFalse();
            page.template.Should().BeFalse();
            page.created_at.Should().BeCloseTo(now, 10.Seconds());
            page.updated_at.Should().BeCloseTo(now, 10.Seconds());
            page.created_by.id.Should().Be(book.created_by);
            page.updated_by.id.Should().Be(book.updated_by);
            page.owned_by.id.Should().Be(book.owned_by);
            page.tags.Should().BeEmpty();
        }
        {//  tags
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter")));
            var page = await client.CreatePageAsync(new(testName("aaa"), chapter_id: chapter.id, markdown: "aaa", tags: [new("tpv1", "tpv1"), new("tpv2", "tpv2"),]));
            page.book_id.Should().Be(book.id);
            page.chapter_id.Should().Be(chapter.id);
            page.name.Should().Be(testName("aaa"));
            page.editor.Should().Be("markdown");
            page.draft.Should().BeFalse();
            page.template.Should().BeFalse();
            page.created_at.Should().BeCloseTo(now, 10.Seconds());
            page.updated_at.Should().BeCloseTo(now, 10.Seconds());
            page.created_by.id.Should().Be(book.created_by);
            page.updated_by.id.Should().Be(book.updated_by);
            page.owned_by.id.Should().Be(book.owned_by);
            page.tags.Should().BeEquivalentTo((Tag[])[new("tpv1", "tpv1"), new("tpv2", "tpv2"),]);
        }
    }

    [TestMethod()]
    public async Task CreateMarkdownPageInBookAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreateMarkdownPageInBookAsync(new(book.id, testName("aaa"), "mdmd"));
            page.book_id.Should().Be(book.id);
            page.chapter_id.Should().Be(0);
            page.name.Should().Be(testName("aaa"));
            page.editor.Should().Be("markdown");
            page.draft.Should().BeFalse();
            page.template.Should().BeFalse();
            page.created_at.Should().BeCloseTo(now, 10.Seconds());
            page.updated_at.Should().BeCloseTo(now, 10.Seconds());
            page.created_by.id.Should().Be(book.created_by);
            page.updated_by.id.Should().Be(book.updated_by);
            page.owned_by.id.Should().Be(book.owned_by);
            page.tags.Should().BeEmpty();
        }
        {
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreateMarkdownPageInBookAsync(new(book.id, testName("bbb"), "mdmd", priority: 13, tags: [new("tpv1", "tpv1"), new("tpv2", "tpv2"),]));
            page.book_id.Should().Be(book.id);
            page.chapter_id.Should().Be(0);
            page.name.Should().Be(testName("bbb"));
            page.editor.Should().Be("markdown");
            page.draft.Should().BeFalse();
            page.template.Should().BeFalse();
            page.priority.Should().Be(13);
            page.created_at.Should().BeCloseTo(now, 10.Seconds());
            page.updated_at.Should().BeCloseTo(now, 10.Seconds());
            page.created_by.id.Should().Be(book.created_by);
            page.updated_by.id.Should().Be(book.updated_by);
            page.owned_by.id.Should().Be(book.owned_by);
            page.tags.Should().BeEquivalentTo((Tag[])[new("tpv1", "tpv1"), new("tpv2", "tpv2"),]);
        }
    }

    [TestMethod()]
    public async Task CreateMarkdownPageInChapterAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter")));
            var page = await client.CreateMarkdownPageInChapterAsync(new(chapter.id, testName("aaa"), "mdmd"));
            page.book_id.Should().Be(book.id);
            page.chapter_id.Should().Be(chapter.id);
            page.name.Should().Be(testName("aaa"));
            page.editor.Should().Be("markdown");
            page.draft.Should().BeFalse();
            page.template.Should().BeFalse();
            page.created_at.Should().BeCloseTo(now, 10.Seconds());
            page.updated_at.Should().BeCloseTo(now, 10.Seconds());
            page.created_by.id.Should().Be(book.created_by);
            page.updated_by.id.Should().Be(book.updated_by);
            page.owned_by.id.Should().Be(book.owned_by);
            page.tags.Should().BeEmpty();
        }
        {
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter")));
            var page = await client.CreateMarkdownPageInChapterAsync(new(chapter.id, testName("bbb"), "mdmd", priority: 22, tags: [new("tpv1", "tpv1"), new("tpv2", "tpv2"),]));
            page.book_id.Should().Be(book.id);
            page.chapter_id.Should().Be(chapter.id);
            page.name.Should().Be(testName("bbb"));
            page.editor.Should().Be("markdown");
            page.draft.Should().BeFalse();
            page.template.Should().BeFalse();
            page.priority.Should().Be(22);
            page.created_at.Should().BeCloseTo(now, 10.Seconds());
            page.updated_at.Should().BeCloseTo(now, 10.Seconds());
            page.created_by.id.Should().Be(book.created_by);
            page.updated_by.id.Should().Be(book.updated_by);
            page.owned_by.id.Should().Be(book.owned_by);
            page.tags.Should().BeEquivalentTo((Tag[])[new("tpv1", "tpv1"), new("tpv2", "tpv2"),]);
        }
    }

    [TestMethod()]
    public async Task CreateHtmlPageInBookAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreateHtmlPageInBookAsync(new(book.id, testName("aaa"), "htht"));
            page.book_id.Should().Be(book.id);
            page.chapter_id.Should().Be(0);
            page.name.Should().Be(testName("aaa"));
            page.editor.Should().BeEmpty(); // html
            page.draft.Should().BeFalse();
            page.template.Should().BeFalse();
            page.created_at.Should().BeCloseTo(now, 10.Seconds());
            page.updated_at.Should().BeCloseTo(now, 10.Seconds());
            page.created_by.id.Should().Be(book.created_by);
            page.updated_by.id.Should().Be(book.updated_by);
            page.owned_by.id.Should().Be(book.owned_by);
            page.tags.Should().BeEmpty();
        }
        {
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreateHtmlPageInBookAsync(new(book.id, testName("bbb"), "htht", priority: 1, tags: [new("tpv1", "tpv1"), new("tpv2", "tpv2"),]));
            page.book_id.Should().Be(book.id);
            page.chapter_id.Should().Be(0);
            page.name.Should().Be(testName("bbb"));
            page.editor.Should().BeEmpty(); // html
            page.draft.Should().BeFalse();
            page.template.Should().BeFalse();
            page.priority.Should().Be(1);
            page.created_at.Should().BeCloseTo(now, 10.Seconds());
            page.updated_at.Should().BeCloseTo(now, 10.Seconds());
            page.created_by.id.Should().Be(book.created_by);
            page.updated_by.id.Should().Be(book.updated_by);
            page.owned_by.id.Should().Be(book.owned_by);
            page.tags.Should().BeEquivalentTo((Tag[])[new("tpv1", "tpv1"), new("tpv2", "tpv2"),]);
        }
    }

    [TestMethod()]
    public async Task CreateHtmlPageInChapterAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter")));
            var page = await client.CreateHtmlPageInChapterAsync(new(chapter.id, testName("aaa"), "htht"));
            page.book_id.Should().Be(book.id);
            page.chapter_id.Should().Be(chapter.id);
            page.name.Should().Be(testName("aaa"));
            page.editor.Should().BeEmpty(); // html
            page.draft.Should().BeFalse();
            page.template.Should().BeFalse();
            page.created_at.Should().BeCloseTo(now, 10.Seconds());
            page.updated_at.Should().BeCloseTo(now, 10.Seconds());
            page.created_by.id.Should().Be(book.created_by);
            page.updated_by.id.Should().Be(book.updated_by);
            page.owned_by.id.Should().Be(book.owned_by);
            page.tags.Should().BeEmpty();
        }
        {
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter")));
            var page = await client.CreateHtmlPageInChapterAsync(new(chapter.id, testName("bbb"), "htht", priority: 2, tags: [new("tpv1", "tpv1"), new("tpv2", "tpv2"),]));
            page.book_id.Should().Be(book.id);
            page.chapter_id.Should().Be(chapter.id);
            page.name.Should().Be(testName("bbb"));
            page.editor.Should().BeEmpty(); // html
            page.draft.Should().BeFalse();
            page.template.Should().BeFalse();
            page.priority.Should().Be(2);
            page.created_at.Should().BeCloseTo(now, 10.Seconds());
            page.updated_at.Should().BeCloseTo(now, 10.Seconds());
            page.created_by.id.Should().Be(book.created_by);
            page.updated_by.id.Should().Be(book.updated_by);
            page.owned_by.id.Should().Be(book.owned_by);
            page.tags.Should().BeEquivalentTo((Tag[])[new("tpv1", "tpv1"), new("tpv2", "tpv2"),]);
        }
    }

    [TestMethod()]
    public async Task ReadPageAsync_forBook()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {// markdown
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreateMarkdownPageInBookAsync(new(book.id, testName("aaa"), "mdmd"));
            var detail = await client.ReadPageAsync(page.id);
            detail.book_id.Should().Be(book.id);
            detail.chapter_id.Should().Be(0);
            detail.priority.Should().BeGreaterThan(0);
            detail.name.Should().Be(testName("aaa"));
            detail.slug.Should().NotBeNullOrEmpty();
            detail.revision_count.Should().BeGreaterThan(0);
            detail.editor.Should().Be("markdown");
            detail.markdown.Should().Be("mdmd");
            detail.html.Should().NotBeNullOrEmpty();
            detail.draft.Should().BeFalse();
            detail.template.Should().BeFalse();
            detail.tags.Should().BeEmpty();
            detail.created_at.Should().BeCloseTo(now, 10.Seconds());
            detail.updated_at.Should().BeCloseTo(now, 10.Seconds());
            detail.created_by.id.Should().Be(book.created_by);
            detail.created_by.name.Should().Be("Admin");
            detail.updated_by.id.Should().Be(book.created_by);
            detail.updated_by.name.Should().Be("Admin");
            detail.owned_by.id.Should().Be(book.created_by);
            detail.owned_by.name.Should().Be("Admin");
        }
        {// html
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreateHtmlPageInBookAsync(new(book.id, testName("bbb"), "<b>asd</b><script>def</script>"));
            var detail = await client.ReadPageAsync(page.id);
            detail.book_id.Should().Be(book.id);
            detail.chapter_id.Should().Be(0);
            detail.priority.Should().BeGreaterThan(0);
            detail.name.Should().Be(testName("bbb"));
            detail.slug.Should().NotBeNullOrEmpty();
            detail.revision_count.Should().BeGreaterThan(0);
            detail.editor.Should().BeEmpty();    // html
            detail.markdown.Should().BeEmpty();
            detail.html.Should().Contain("asd").And.NotContain("def");
            detail.raw_html.Should().Contain("asd").And.Contain("def");
            detail.draft.Should().BeFalse();
            detail.template.Should().BeFalse();
            detail.tags.Should().BeEmpty();
            detail.created_at.Should().BeCloseTo(now, 10.Seconds());
            detail.updated_at.Should().BeCloseTo(now, 10.Seconds());
            detail.created_by.id.Should().Be(book.created_by);
            detail.created_by.name.Should().Be("Admin");
            detail.updated_by.id.Should().Be(book.created_by);
            detail.updated_by.name.Should().Be("Admin");
            detail.owned_by.id.Should().Be(book.created_by);
            detail.owned_by.name.Should().Be("Admin");
        }
        {//  tags
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreateMarkdownPageInBookAsync(new(book.id, testName("aaa"), "mdmd", priority: 4, tags: [new("tpv1", "tpv1"), new("tpv2", "tpv2"),]));
            var detail = await client.ReadPageAsync(page.id);
            detail.priority.Should().Be(4);
            detail.tags.Should().BeEquivalentTo((Tag[])[new("tpv1", "tpv1"), new("tpv2", "tpv2"),]);
        }
    }

    [TestMethod()]
    public async Task ReadPageAsync_forChapter()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {// markdown
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter")));
            var page = await client.CreateMarkdownPageInChapterAsync(new(chapter.id, testName("aaa"), "mdmd"));
            var detail = await client.ReadPageAsync(page.id);
            detail.book_id.Should().Be(book.id);
            detail.chapter_id.Should().Be(chapter.id);
            detail.priority.Should().BeGreaterThan(0);
            detail.name.Should().Be(testName("aaa"));
            detail.slug.Should().NotBeNullOrEmpty();
            detail.revision_count.Should().BeGreaterThan(0);
            detail.editor.Should().Be("markdown");
            detail.markdown.Should().Be("mdmd");
            detail.html.Should().NotBeNullOrEmpty();
            detail.draft.Should().BeFalse();
            detail.template.Should().BeFalse();
            detail.tags.Should().BeEmpty();
            detail.created_at.Should().BeCloseTo(now, 10.Seconds());
            detail.updated_at.Should().BeCloseTo(now, 10.Seconds());
            detail.created_by.id.Should().Be(book.created_by);
            detail.created_by.name.Should().Be("Admin");
            detail.updated_by.id.Should().Be(book.created_by);
            detail.updated_by.name.Should().Be("Admin");
            detail.owned_by.id.Should().Be(book.created_by);
            detail.owned_by.name.Should().Be("Admin");
        }
        {// html
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter")));
            var page = await client.CreateHtmlPageInChapterAsync(new(chapter.id, testName("bbb"), "<b>asd</b><script>def</script>"));
            var detail = await client.ReadPageAsync(page.id);
            detail.book_id.Should().Be(book.id);
            detail.chapter_id.Should().Be(chapter.id);
            detail.priority.Should().BeGreaterThan(0);
            detail.name.Should().Be(testName("bbb"));
            detail.slug.Should().NotBeNullOrEmpty();
            detail.revision_count.Should().BeGreaterThan(0);
            detail.editor.Should().BeEmpty();    // html
            detail.markdown.Should().BeEmpty();
            detail.html.Should().Contain("asd").And.NotContain("def");
            detail.raw_html.Should().Contain("asd").And.Contain("def");
            detail.draft.Should().BeFalse();
            detail.template.Should().BeFalse();
            detail.tags.Should().BeEmpty();
            detail.created_at.Should().BeCloseTo(now, 10.Seconds());
            detail.updated_at.Should().BeCloseTo(now, 10.Seconds());
            detail.created_by.id.Should().Be(book.created_by);
            detail.created_by.name.Should().Be("Admin");
            detail.updated_by.id.Should().Be(book.created_by);
            detail.updated_by.name.Should().Be("Admin");
            detail.owned_by.id.Should().Be(book.created_by);
            detail.owned_by.name.Should().Be("Admin");
        }
        {//  tags
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter")));
            var page = await client.CreateMarkdownPageInChapterAsync(new(chapter.id, testName("aaa"), "mdmd", priority: 5, tags: [new("tpv1", "tpv1"), new("tpv2", "tpv2"),]));
            var detail = await client.ReadPageAsync(page.id);
            detail.priority.Should().Be(5);
            detail.tags.Should().BeEquivalentTo((Tag[])[new("tpv1", "tpv1"), new("tpv2", "tpv2"),]);
        }
    }

    [TestMethod()]
    public async Task UpdatePageAsync_forBook()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {// update markdown to markdown
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var created = await client.CreatePageAsync(new(testName("aaa"), book_id: book.id, markdown: "m1"));
            created.name.Should().Be(testName("aaa"));
            created.editor.Should().Be("markdown");
            created.markdown.Should().Be("m1");
            await Task.Delay(3 * 1000);
            var updated = await client.UpdatePageAsync(created.id, new(testName("bbb"), markdown: "m2"));
            updated.name.Should().Be(testName("bbb"));
            updated.editor.Should().Be("markdown");
            updated.markdown.Should().Be("m2");
            updated.revision_count.Should().BeGreaterThan(created.revision_count);
            updated.created_at.Should().Be(created.created_at);
            updated.updated_at.Should().BeAfter(created.updated_at);
            updated.created_by.id.Should().Be(book.created_by);
            updated.updated_by.id.Should().Be(book.updated_by);
            updated.owned_by.id.Should().Be(book.owned_by);
        }
        {// update html to markdown
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var created = await client.CreatePageAsync(new(testName("ccc"), book_id: book.id, html: "h1"));
            created.name.Should().Be(testName("ccc"));
            created.editor.Should().BeEmpty();
            created.html.Should().Contain("h1");
            var updated = await client.UpdatePageAsync(created.id, new(testName("ddd"), html: "h2"));
            updated.name.Should().Be(testName("ddd"));
            updated.editor.Should().BeEmpty();
            updated.html.Should().Contain("h2");
        }
        {// update markdown to html
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var created = await client.CreatePageAsync(new(testName("eee"), book_id: book.id, markdown: "mdmd"));
            created.name.Should().Be(testName("eee"));
            created.editor.Should().Be("markdown");
            created.markdown.Should().Be("mdmd");
            var updated = await client.UpdatePageAsync(created.id, new(testName("fff"), html: "htht"));
            updated.name.Should().Be(testName("fff"));
            updated.editor.Should().Be("wysiwyg");  // markdown -> html に update するとこうなるみたい？
            updated.html.Should().Contain("htht");
        }
        {// update html to markdown
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var created = await client.CreatePageAsync(new(testName("ggg"), book_id: book.id, html: "htht"));
            created.name.Should().Be(testName("ggg"));
            created.editor.Should().BeEmpty();
            created.html.Should().Contain("htht");
            var updated = await client.UpdatePageAsync(created.id, new(testName("hhh"), markdown: "mdmd"));
            updated.name.Should().Be(testName("hhh"));
            updated.editor.Should().Be("markdown");
            updated.markdown.Should().Be("mdmd");
        }
    }

    [TestMethod()]
    public async Task UpdatePageAsync_forChapter()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {// update markdown to markdown
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter")));
            var created = await client.CreatePageAsync(new(testName("aaa"), chapter_id: chapter.id, markdown: "m1"));
            created.name.Should().Be(testName("aaa"));
            created.editor.Should().Be("markdown");
            created.markdown.Should().Be("m1");
            await Task.Delay(3 * 1000);
            var updated = await client.UpdatePageAsync(created.id, new(testName("bbb"), markdown: "m2"));
            updated.name.Should().Be(testName("bbb"));
            updated.editor.Should().Be("markdown");
            updated.markdown.Should().Be("m2");
            updated.created_at.Should().Be(created.created_at);
            updated.updated_at.Should().BeAfter(created.updated_at);
            updated.created_by.id.Should().Be(book.created_by);
            updated.updated_by.id.Should().Be(book.updated_by);
            updated.owned_by.id.Should().Be(book.owned_by);
        }
        {// update html to markdown
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter")));
            var created = await client.CreatePageAsync(new(testName("ccc"), chapter_id: chapter.id, html: "h1"));
            created.name.Should().Be(testName("ccc"));
            created.editor.Should().BeEmpty();
            created.html.Should().Contain("h1");
            var updated = await client.UpdatePageAsync(created.id, new(testName("ddd"), html: "h2"));
            updated.name.Should().Be(testName("ddd"));
            updated.editor.Should().BeEmpty();
            updated.html.Should().Contain("h2");
        }
        {// update markdown to html
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter")));
            var created = await client.CreatePageAsync(new(testName("eee"), chapter_id: chapter.id, markdown: "mdmd"));
            created.name.Should().Be(testName("eee"));
            created.editor.Should().Be("markdown");
            created.markdown.Should().Be("mdmd");
            var updated = await client.UpdatePageAsync(created.id, new(testName("fff"), html: "htht"));
            updated.name.Should().Be(testName("fff"));
            updated.editor.Should().Be("wysiwyg");  // markdown -> html に update するとこうなるみたい？
            updated.html.Should().Contain("htht");
        }
        {// update html to markdown
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter")));
            var created = await client.CreatePageAsync(new(testName("ggg"), chapter_id: chapter.id, html: "htht"));
            created.name.Should().Be(testName("ggg"));
            created.editor.Should().BeEmpty();
            created.html.Should().Contain("htht");
            var updated = await client.UpdatePageAsync(created.id, new(testName("hhh"), markdown: "mdmd"));
            updated.name.Should().Be(testName("hhh"));
            updated.editor.Should().Be("markdown");
            updated.markdown.Should().Be("mdmd");
        }
    }

    [TestMethod()]
    public async Task UpdatePageAsync_move()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {// move book to book
            var book1 = await client.CreateBookAsync(new(testName("testbook1"))).WillBeDiscarded(container);
            var book2 = await client.CreateBookAsync(new(testName("testbook2"))).WillBeDiscarded(container);
            var created = await client.CreatePageAsync(new(testName("aaa"), book_id: book1.id, markdown: "m1"));
            created.book_id.Should().Be(book1.id);
            created.chapter_id.Should().Be(0);
            created.name.Should().Be(testName("aaa"));
            created.editor.Should().Be("markdown");
            created.markdown.Should().Be("m1");
            var updated = await client.UpdatePageAsync(created.id, new(testName("bbb"), book_id: book2.id, markdown: "m2"));
            updated.book_id.Should().Be(book2.id);
            updated.chapter_id.Should().Be(0);
            updated.name.Should().Be(testName("bbb"));
            updated.editor.Should().Be("markdown");
            updated.markdown.Should().Be("m2");
        }
        {// move chapter to chapter
            var book1 = await client.CreateBookAsync(new(testName("testbook1"))).WillBeDiscarded(container);
            var book2 = await client.CreateBookAsync(new(testName("testbook2"))).WillBeDiscarded(container);
            var chapter1 = await client.CreateChapterAsync(new(book1.id, testName("testchapter1")));
            var chapter2 = await client.CreateChapterAsync(new(book2.id, testName("testchapter2")));
            var created = await client.CreatePageAsync(new(testName("aaa"), chapter_id: chapter1.id, markdown: "m1"));
            created.book_id.Should().Be(book1.id);
            created.chapter_id.Should().Be(chapter1.id);
            created.name.Should().Be(testName("aaa"));
            created.editor.Should().Be("markdown");
            created.markdown.Should().Be("m1");
            var updated = await client.UpdatePageAsync(created.id, new(testName("bbb"), chapter_id: chapter2.id, markdown: "m2"));
            updated.book_id.Should().Be(book2.id);
            updated.chapter_id.Should().Be(chapter2.id);
            updated.name.Should().Be(testName("bbb"));
            updated.editor.Should().Be("markdown");
            updated.markdown.Should().Be("m2");
        }
        {// move book to chapter
            var book1 = await client.CreateBookAsync(new(testName("testbook1"))).WillBeDiscarded(container);
            var book2 = await client.CreateBookAsync(new(testName("testbook2"))).WillBeDiscarded(container);
            var chapter1 = await client.CreateChapterAsync(new(book1.id, testName("testchapter1")));
            var chapter2 = await client.CreateChapterAsync(new(book2.id, testName("testchapter2")));
            var created = await client.CreatePageAsync(new(testName("aaa"), book_id: book1.id, markdown: "m1"));
            created.book_id.Should().Be(book1.id);
            created.chapter_id.Should().Be(0);
            created.name.Should().Be(testName("aaa"));
            created.editor.Should().Be("markdown");
            created.markdown.Should().Be("m1");
            var updated = await client.UpdatePageAsync(created.id, new(testName("bbb"), chapter_id: chapter2.id, markdown: "m2"));
            updated.book_id.Should().Be(book2.id);
            updated.chapter_id.Should().Be(chapter2.id);
            updated.name.Should().Be(testName("bbb"));
            updated.editor.Should().Be("markdown");
            updated.markdown.Should().Be("m2");
        }
        {// move chapter to book
            var book1 = await client.CreateBookAsync(new(testName("testbook1"))).WillBeDiscarded(container);
            var book2 = await client.CreateBookAsync(new(testName("testbook2"))).WillBeDiscarded(container);
            var chapter1 = await client.CreateChapterAsync(new(book1.id, testName("testchapter1")));
            var chapter2 = await client.CreateChapterAsync(new(book2.id, testName("testchapter2")));
            var created = await client.CreatePageAsync(new(testName("aaa"), chapter_id: chapter1.id, markdown: "m1"));
            created.book_id.Should().Be(book1.id);
            created.chapter_id.Should().Be(chapter1.id);
            created.name.Should().Be(testName("aaa"));
            created.editor.Should().Be("markdown");
            created.markdown.Should().Be("m1");
            var updated = await client.UpdatePageAsync(created.id, new(testName("bbb"), book_id: book2.id, markdown: "m2"));
            updated.book_id.Should().Be(book2.id);
            updated.chapter_id.Should().Be(0);
            updated.name.Should().Be(testName("bbb"));
            updated.editor.Should().Be("markdown");
            updated.markdown.Should().Be("m2");
        }
    }

    [TestMethod()]
    public async Task DeletePageAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var name = testName($"page_{Guid.NewGuid()}");
            var page = await client.CreatePageAsync(new(name, book_id: book.id, markdown: "aaa"));
            (await client.ListPagesAsync(new(filters: [new("name", name)]))).data.Should().Contain(d => d.id == page.id);
            await client.DeletePageAsync(page.id);
            (await client.ListPagesAsync(new(filters: [new("name", name)]))).data.Should().NotContain(d => d.id == page.id);
        }
        {
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter")));
            var name = testName($"page_{Guid.NewGuid()}");
            var page = await client.CreatePageAsync(new(name, chapter_id: chapter.id, markdown: "aaa"));
            (await client.ListPagesAsync(new(filters: [new("name", name)]))).data.Should().Contain(d => d.id == page.id);
            await client.DeletePageAsync(page.id);
            (await client.ListPagesAsync(new(filters: [new("name", name)]))).data.Should().NotContain(d => d.id == page.id);
        }
    }

    [TestMethod()]
    public async Task ExportPageHtmlAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("aaa"))).WillBeDiscarded(container);
        var chapter = await client.CreateChapterAsync(new(book.id, testName("bbb")));
        var page = await client.CreatePageAsync(new(testName("ccc"), book_id: book.id, markdown: "aaa"));
        var html = await client.ExportPageHtmlAsync(page.id);
        html.Should().NotBeNullOrEmpty();
    }

    [TestMethod()]
    public async Task ExportPagePlainAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("aaa"))).WillBeDiscarded(container);
        var chapter = await client.CreateChapterAsync(new(book.id, testName("bbb")));
        var page = await client.CreatePageAsync(new(testName("ccc"), book_id: book.id, markdown: "aaa"));
        var text = await client.ExportPagePlainAsync(page.id);
        text.Should().NotBeNullOrEmpty();
    }

    [TestMethod()]
    public async Task ExportPageMarkdownAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("aaa"))).WillBeDiscarded(container);
        var chapter = await client.CreateChapterAsync(new(book.id, testName("bbb")));
        var page = await client.CreatePageAsync(new(testName("ccc"), book_id: book.id, markdown: "aaa"));
        var markdown = await client.ExportPageMarkdownAsync(page.id);
        markdown.Should().NotBeNullOrEmpty();
    }

    [TestMethod()]
    public async Task ExportPagePdfAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("aaa"))).WillBeDiscarded(container);
        var chapter = await client.CreateChapterAsync(new(book.id, testName("bbb")));
        var page = await client.CreatePageAsync(new(testName("ccc"), book_id: book.id, markdown: "aaa"));
        var pdf = await client.ExportPagePdfAsync(page.id);
        pdf.Should().NotBeNullOrEmpty();
    }
    #endregion
}
