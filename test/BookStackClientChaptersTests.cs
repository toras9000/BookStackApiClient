namespace BookStackApiClient.Tests;

[TestClass()]
public class BookStackClientChaptersTests : BookStackClientTestsBase
{
    #region chapters
    [TestMethod()]
    public async Task ListChaptersAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
        await client.CreateChapterAsync(new(book.id, testName($"chapter_{Guid.NewGuid()}"), "aaa"));
        await client.CreateChapterAsync(new(book.id, testName($"chapter_{Guid.NewGuid()}"), "bbb"));

        var chapters = await client.ListChaptersAsync();
        foreach (var created in container.Chapters)
        {
            var actual = chapters.data.Should().Contain(i => i.id == created.id).Subject;
            var expect = created;
            actual.Should().BeEquivalentTo(expect);
        }
    }

    [TestMethod()]
    public async Task ListChaptersAsync_options()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);

        var prefix1 = testName($"chapter_{Guid.NewGuid()}_");
        for (var i = 0; i < 10; i++)
        {
            await client.CreateChapterAsync(new(book.id, $"{prefix1}_{i:D2}")).AddTo(container);
        }
        var prefix2 = testName($"chapter_{Guid.NewGuid()}_");
        for (var i = 0; i < 10; i++)
        {
            await client.CreateChapterAsync(new(book.id, $"{prefix2}_{i:D2}")).AddTo(container);
        }

        {// range
            var chapters1 = await client.ListChaptersAsync(new(offset: 0, count: 5));
            chapters1.data.Should().HaveCount(5);
            var chapters2 = await client.ListChaptersAsync(new(offset: 5, count: 5));
            chapters2.data.Should().HaveCount(5);

            chapters1.data.Select(d => d.id).Should().NotIntersectWith(chapters2.data.Select(d => d.id));
        }
        {// filter
            var chapters = await client.ListChaptersAsync(new(filters: [new($"name:like", $"{prefix1}%")]));
            chapters.data.Should().AllSatisfy(d => d.name.StartsWith(prefix1));
        }
        {// filter & sort (asc)
            var offset = 0;
            var count = 4;
            var chapters = await client.ListChaptersAsync(new(offset, count, sorts: [nameof(ChapterSummary.name),], filters: [new($"name:like", $"{prefix1}%")]));
            var expects = container.Chapters.Where(c => c.name.StartsWith(prefix1)).Select(c => c.id).Skip(offset).Take(count);
            chapters.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (desc)
            var offset = 0;
            var count = 4;
            var chapters = await client.ListChaptersAsync(new(offset, count, sorts: [$"-{nameof(ChapterSummary.name)}",], filters: [new($"name:like", $"{prefix1}%")]));
            var expects = container.Chapters.AsEnumerable().Reverse().Where(c => c.name.StartsWith(prefix1)).Select(c => c.id).Skip(offset).Take(count);
            chapters.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (asc) & range
            var offset = 2;
            var count = 5;
            var chapters = await client.ListChaptersAsync(new(offset, count, sorts: [nameof(ChapterSummary.name),], filters: [new($"name:like", $"{prefix2}%")]));
            var expects = container.Chapters.Where(c => c.name.StartsWith(prefix2)).Select(c => c.id).Skip(offset).Take(count);
            chapters.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (desc) & range
            var offset = 3;
            var count = 4;
            var chapters = await client.ListChaptersAsync(new(offset, count, sorts: [$"-{nameof(ChapterSummary.name)}",], filters: [new($"name:like", $"{prefix2}%")]));
            var expects = container.Chapters.AsEnumerable().Reverse().Where(c => c.name.StartsWith(prefix2)).Select(c => c.id).Skip(offset).Take(count);
            chapters.data.Select(d => d.id).Should().Equal(expects);
        }
    }

    [TestMethod()]
    public async Task CreateChapterAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {// name only
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("aaa")));
            chapter.book_id.Should().Be(book.id);
            chapter.book_slug.Should().Be(book.slug);
            chapter.name.Should().Be(testName("aaa"));
            chapter.description.Should().BeEmpty();
            chapter.slug.Should().NotBeNullOrEmpty();
            chapter.tags.Should().BeEmpty();
            chapter.created_at.Should().BeCloseTo(now, 10.Seconds());
            chapter.updated_at.Should().BeCloseTo(now, 10.Seconds());
            chapter.created_by.Should().Be(book.created_by);
            chapter.updated_by.Should().Be(book.updated_by);
            chapter.owned_by.Should().Be(book.owned_by);
        }
        {// name & description
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("aaa"), "bbb"));
            chapter.book_id.Should().Be(book.id);
            chapter.book_slug.Should().Be(book.slug);
            chapter.name.Should().Be(testName("aaa"));
            chapter.description.Should().Be(chapter.description);
            chapter.slug.Should().NotBeNullOrEmpty();
            chapter.tags.Should().BeEmpty();
            chapter.created_at.Should().BeCloseTo(now, 10.Seconds());
            chapter.updated_at.Should().BeCloseTo(now, 10.Seconds());
            chapter.created_by.Should().Be(book.created_by);
            chapter.updated_by.Should().Be(book.updated_by);
            chapter.owned_by.Should().Be(book.owned_by);
        }
        {// description_html
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("aaa"), description_html: "bbb"));
            chapter.description.Should().Be("bbb");
            chapter.description_html.Should().Contain("bbb");
        }
        {// name & description & tags
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("aaa"), "bbb", priority: 5, tags: [new("tc1", "tv1"), new("tc2", "tv2"),]));
            chapter.book_id.Should().Be(book.id);
            chapter.book_slug.Should().Be(book.slug);
            chapter.name.Should().Be(testName("aaa"));
            chapter.description.Should().Be(chapter.description);
            chapter.slug.Should().NotBeNullOrEmpty();
            chapter.priority.Should().Be(5);
            chapter.tags.Should().BeEquivalentTo((Tag[])[new("tc1", "tv1"), new("tc2", "tv2"),]);
            chapter.created_at.Should().BeCloseTo(now, 10.Seconds());
            chapter.updated_at.Should().BeCloseTo(now, 10.Seconds());
            chapter.created_by.Should().Be(book.created_by);
            chapter.updated_by.Should().Be(book.updated_by);
            chapter.owned_by.Should().Be(book.owned_by);
        }
        {// default_template_id
            var template_book = await client.CreateBookAsync(new(testName("template-page-container"))).WillBeDiscarded(container);
            var template_page = await client.CreateMarkdownPageInBookAsync(new(template_book.id, "template-page", "template-body")).WillBeDiscarded(container);

            await using var adapter = new TestBackendAdapter();
            await adapter.SetPagaTemplateFlag(template_page.id, true);

            var book = await client.CreateBookAsync(new(testName("test-book"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("aaa"), default_template_id: template_page.id));
            chapter.default_template_id.Should().Be(template_page.id);
            var detail = await client.ReadChapterAsync(chapter.id);
            detail.default_template_id.Should().Be(template_page.id);
        }
    }

    [TestMethod()]
    public async Task ReadChapterAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("aaa"), "bbb", priority: 5, tags: [new("tc1", "tcv1"), new("tc2", "tcv2"),]));
            var page1 = await client.CreateMarkdownPageInChapterAsync(new(chapter.id, testName("p1"), "ppp", priority: 2, tags: [new("tp1", "tpv1"), new("tp2", "tpv2"),]));
            var page2 = await client.CreateMarkdownPageInChapterAsync(new(chapter.id, testName("p2"), "ppp", priority: 3, tags: [new("tp1", "tpv1"), new("tp3", "tpv3"),]));
            var detail = await client.ReadChapterAsync(chapter.id);
            detail.book_id.Should().Be(book.id);
            detail.name.Should().Be(testName("aaa"));
            detail.description.Should().Be("bbb");
            detail.description_html.Should().Contain("bbb");
            detail.slug.Should().NotBeNullOrEmpty();
            detail.tags.Should().BeEquivalentTo((Tag[])[new("tc1", "tcv1"), new("tc2", "tcv2"),]);
            detail.created_at.Should().BeCloseTo(now, 10.Seconds());
            detail.updated_at.Should().BeCloseTo(now, 10.Seconds());
            detail.created_by.name.Should().Be("Admin");
            detail.updated_by.name.Should().Be("Admin");
            detail.owned_by.name.Should().Be("Admin");
            detail.pages.Should().BeEquivalentTo(new[]
                {
                    new { page1.id, page1.name, page1.priority, },
                    new { page2.id, page2.name, page2.priority, },
                });
        }
    }

    [TestMethod()]
    public async Task UpdateChapterAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {// update name & desc
            var image = await testResContentAsync("images/pd001.png");
            var book = await client.CreateBookAsync(new(testName("testbook")), image, "test.png").WillBeDiscarded(container);
            var created = await client.CreateChapterAsync(new(book.id, testName("aaa"), "bbb", priority: 3, tags: [new("t1", "v1"), new("t2", "v2"),]));
            await Task.Delay(2 * 1000);     // for update timestamp
            var updated = await client.UpdateChapterAsync(created.id, new(name: testName("ccc"), description: "ddd", priority: 7));
            updated.book_id.Should().Be(book.id);
            updated.book_slug.Should().Be(book.slug);
            updated.name.Should().Be(testName("ccc"));
            updated.description.Should().Be("ddd");
            updated.description_html.Should().Contain("ddd");
            updated.slug.Should().NotBeNullOrEmpty();
            updated.priority.Should().Be(7);
            updated.tags.Should().BeEquivalentTo((Tag[])[new("t1", "v1"), new("t2", "v2"),]);
            updated.created_at.Should().Be(created.created_at);
            updated.updated_at.Should().BeAfter(created.updated_at);
            updated.created_by.Should().Be(book.created_by);
            updated.updated_by.Should().Be(book.updated_by);
            updated.owned_by.Should().Be(book.owned_by);
        }
        {// description_html
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var created = await client.CreateChapterAsync(new(book.id, testName("aaa"), "bbb"));
            var updated = await client.UpdateChapterAsync(created.id, new(name: testName("ccc"), description_html: "ddd"));
            updated.description.Should().Contain("ddd");
            updated.description_html.Should().Contain("ddd");
        }
        {// update tags
            var image = await testResContentAsync("images/pd001.png");
            var book = await client.CreateBookAsync(new(testName("aaa"), "bbb", tags: [new("t1", "v1"), new("t2", "v2"),]), image, "test.png").WillBeDiscarded(container);
            var created = await client.CreateChapterAsync(new(book.id, testName("aaa"), "bbb", tags: [new("t1", "v1"), new("t2", "v2"),]));
            var updated = await client.UpdateChapterAsync(created.id, new(tags: [new("t1", "v1new"), new("t3", "v3")]));
            updated.name.Should().Be(testName("aaa"));
            updated.description.Should().Be("bbb");
            updated.tags.Should().BeEquivalentTo((Tag[])[new("t1", "v1new"), new("t3", "v3"),]);
        }
        {// default_template_id
            var book = await client.CreateBookAsync(new(testName("test-book"))).WillBeDiscarded(container);
            book.default_template_id.Should().BeNull();
            var created = await client.CreateChapterAsync(new(book.id, testName("aaa")));
            created.default_template_id.Should().BeNull();

            var template_page = await client.CreateMarkdownPageInBookAsync(new(book.id, "template-page", "template-body")).WillBeDiscarded(container);
            await using var adapter = new TestBackendAdapter();
            await adapter.SetPagaTemplateFlag(template_page.id, true);

            var chapter = await client.UpdateChapterAsync(created.id, new(default_template_id: template_page.id));
            chapter.default_template_id.Should().Be(template_page.id);
            var detail = await client.ReadChapterAsync(chapter.id);
            detail.default_template_id.Should().Be(template_page.id);
        }
    }

    [TestMethod()]
    public async Task UpdateChapterAsync_move()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {// move
            var image = await testResContentAsync("images/pd001.png");
            var book1 = await client.CreateBookAsync(new(testName("testbook1"))).WillBeDiscarded(container);
            var book2 = await client.CreateBookAsync(new(testName("testbook2"))).WillBeDiscarded(container);
            var created = await client.CreateChapterAsync(new(book1.id, testName("aaa"), description: "bbb"));
            var updated = await client.UpdateChapterAsync(created.id, new(name: testName("ccc"), description: "ddd", book_id: book2.id));
            updated.book_id.Should().Be(book2.id);
            updated.name.Should().Be(testName("ccc"));
            updated.description.Should().Be("ddd");
        }
    }

    [TestMethod()]
    public async Task DeleteChapterAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
        var name = testName($"chapter_{Guid.NewGuid()}");
        var chapter = await client.CreateChapterAsync(new(book.id, name));
        (await client.ListChaptersAsync(new(filters: [new("name", name)]))).data.Should().Contain(d => d.id == chapter.id);
        await client.DeleteChapterAsync(chapter.id);
        (await client.ListChaptersAsync(new(filters: [new("name", name)]))).data.Should().NotContain(d => d.id == chapter.id);

    }

    [TestMethod()]
    public async Task ExportChapterHtmlAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("aaa"), "bbb", tags: [new("tb1", "vb1"), new("tb2", "vb2"),])).WillBeDiscarded(container);
        var chapter = await client.CreateChapterAsync(new(book.id, testName("ccc"), "ddd", tags: [new("tc1", "vc1"), new("tc2", "vc2"),]));
        var html = await client.ExportChapterHtmlAsync(chapter.id);
        html.Should().NotBeNullOrEmpty();

    }

    [TestMethod()]
    public async Task ExportChapterPlainAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("aaa"), "bbb", tags: [new("tb1", "vb1"), new("tb2", "vb2"),])).WillBeDiscarded(container);
        var chapter = await client.CreateChapterAsync(new(book.id, testName("ccc"), "ddd", tags: [new("tc1", "vc1"), new("tc2", "vc2"),]));
        var text = await client.ExportChapterPlainAsync(chapter.id);
        text.Should().NotBeNullOrEmpty();
    }

    [TestMethod()]
    public async Task ExportChapterMarkdownAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("aaa"), "bbb", tags: [new("tb1", "vb1"), new("tb2", "vb2"),])).WillBeDiscarded(container);
        var chapter = await client.CreateChapterAsync(new(book.id, testName("ccc"), "ddd", tags: [new("tc1", "vc1"), new("tc2", "vc2"),]));
        var markdown = await client.ExportChapterMarkdownAsync(chapter.id);
        markdown.Should().NotBeNullOrEmpty();
    }

    [TestMethod()]
    public async Task ExportChapterPdfAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("aaa"), "bbb", tags: [new("tb1", "vb1"), new("tb2", "vb2"),])).WillBeDiscarded(container);
        var chapter = await client.CreateChapterAsync(new(book.id, testName("ccc"), "ddd", tags: [new("tc1", "vc1"), new("tc2", "vc2"),]));
        using var pdf = await client.ExportChapterPdfAsync(chapter.id);
        pdf.Should().BeReadable();
    }

    [TestMethod()]
    public async Task ExportChapterZipAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("aaa"), "bbb", tags: [new("tb1", "vb1"), new("tb2", "vb2"),])).WillBeDiscarded(container);
        var chapter = await client.CreateChapterAsync(new(book.id, testName("ccc"), "ddd", tags: [new("tc1", "vc1"), new("tc2", "vc2"),]));
        using var zip = await client.ExportChapterZipAsync(chapter.id);
        zip.Should().BeReadable();
    }
    #endregion
}
