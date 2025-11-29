namespace BookStackApiClient.Tests;

[TestClass()]
public class BookStackClientBooksTests : BookStackClientTestsBase
{
    #region books
    [TestMethod()]
    public async Task ListBooksAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        await client.CreateBookAsync(new(testName($"book_{Guid.NewGuid()}"), "aaa")).WillBeDiscarded(container);
        await client.CreateBookAsync(new(testName($"book_{Guid.NewGuid()}"), "bbb")).WillBeDiscarded(container);

        var book3cover = testResFile("images/pd001.png");
        var book3 = await client.CreateBookAsync(new(testName($"book_{Guid.NewGuid()}"), "bbb"), imgPath: book3cover.FullName).WillBeDiscarded(container);

        var books = await client.ListBooksAsync();
        foreach (var created in container.Books)
        {
            var actual = books.data.Should().Contain(i => i.id == created.id).Subject;
            var expect = created;
            actual.Should().BeEquivalentTo(expect, o => o.ExcludingMissingMembers());   // 一覧取得時には取得されないものもあるので、見つからないメンバを期待値から除外する

            if (actual.id == book3.id)
            {
                Assert.IsNotNull(book3.cover);
                book3.cover.name.Should().Be(book3cover.Name);
            }
        }
    }

    [TestMethod()]
    public async Task ListBooksAsync_options()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);

        var prefix1 = testName($"book_{Guid.NewGuid()}_");
        for (var i = 0; i < 10; i++)
        {
            await client.CreateBookAsync(new($"{prefix1}_{i:D2}")).WillBeDiscarded(container);
        }
        var prefix2 = testName($"book_{Guid.NewGuid()}_");
        for (var i = 0; i < 10; i++)
        {
            await client.CreateBookAsync(new($"{prefix2}_{i:D2}")).WillBeDiscarded(container);
        }

        {// range
            var books1 = await client.ListBooksAsync(new(offset: 0, count: 5));
            books1.data.Should().HaveCount(5);
            var books2 = await client.ListBooksAsync(new(offset: 5, count: 5));
            books2.data.Should().HaveCount(5);

            books1.data.Select(d => d.id).Should().NotIntersectWith(books2.data.Select(d => d.id));
        }
        {// filter
            var books = await client.ListBooksAsync(new(filters: [new($"name:like", $"{prefix1}%")]));
            books.data.Should().AllSatisfy(d => d.name.Should().StartWith(prefix1));
        }
        {// filter & sort (asc)
            var offset = 0;
            var count = 4;
            var books = await client.ListBooksAsync(new(offset, count, sorts: [nameof(BookSummary.name),], filters: [new($"name:like", $"{prefix1}%")]));
            var expects = container.Books.Where(b => b.name.StartsWith(prefix1)).Select(b => b.id).Skip(offset).Take(count);
            books.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (desc)
            var offset = 0;
            var count = 4;
            var books = await client.ListBooksAsync(new(offset, count, sorts: [$"-{nameof(BookSummary.name)}",], filters: [new($"name:like", $"{prefix1}%")]));
            var expects = container.Books.Reverse().Where(b => b.name.StartsWith(prefix1)).Select(b => b.id).Skip(offset).Take(count);
            books.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (asc) & range
            var offset = 2;
            var count = 5;
            var books = await client.ListBooksAsync(new(offset, count, sorts: [nameof(BookSummary.name),], filters: [new($"name:like", $"{prefix2}%")]));
            var expects = container.Books.Where(b => b.name.StartsWith(prefix2)).Select(b => b.id).Skip(offset).Take(count);
            books.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (desc) & range
            var offset = 3;
            var count = 4;
            var books = await client.ListBooksAsync(new(offset, count, sorts: [$"-{nameof(BookSummary.name)}",], filters: [new($"name:like", $"{prefix2}%")]));
            var expects = container.Books.Reverse().Where(b => b.name.StartsWith(prefix2)).Select(b => b.id).Skip(offset).Take(count);
            books.data.Select(d => d.id).Should().Equal(expects);
        }
    }

    [TestMethod()]
    public async Task CreateBookAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {// only name
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("abc"))).WillBeDiscarded(container);
            book.name.Should().Be(testName("abc"));
            book.description.Should().BeNullOrEmpty();
            book.slug.Should().NotBeNullOrEmpty();
            book.created_at.Should().BeCloseTo(now, 10.Seconds());
            book.updated_at.Should().BeCloseTo(now, 10.Seconds());
            book.created_by.Should().NotBe(0);
            book.updated_by.Should().NotBe(0);
            book.owned_by.Should().NotBe(0);
            var detail = await client.ReadBookAsync(book.id);
            detail.name.Should().Be(book.name);
            detail.tags.Should().BeNullOrEmpty();
            detail.cover.Should().BeNull();
            detail.image_id.Should().BeNull();
        }
        {// name & desc
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("def"), description: "ghi")).WillBeDiscarded(container);
            book.name.Should().Be(testName("def"));
            book.description.Should().Be("ghi");
            book.slug.Should().NotBeNullOrEmpty();
            book.created_at.Should().BeCloseTo(now, 10.Seconds());
            book.updated_at.Should().BeCloseTo(now, 10.Seconds());
            book.created_by.Should().NotBe(0);
            book.updated_by.Should().NotBe(0);
            book.owned_by.Should().NotBe(0);
            var detail = await client.ReadBookAsync(book.id);
            detail.name.Should().Be(book.name);
            detail.tags.Should().BeNullOrEmpty();
            detail.cover.Should().BeNull();
            detail.image_id.Should().BeNull();
        }
        {// desc_html
            var book = await client.CreateBookAsync(new(testName("jkl"), description_html: "mno")).WillBeDiscarded(container);
            book.description.Should().Be("mno");
            book.description_html.Should().Contain("mno");
            var detail = await client.ReadBookAsync(book.id);
            detail.description.Should().Be("mno");
            detail.description_html.Should().Contain("mno");
        }
        {// name & desc & tags
            var book = await client.CreateBookAsync(new(testName("aaa"), "bbb", tags: [new("t1", "v1"), new("t2", "v2"),])).WillBeDiscarded(container);
            book.name.Should().Be(testName("aaa"));
            book.description.Should().Be("bbb");
            book.tags.Should().BeEquivalentTo((Tag[])[new("t1", "v1"), new("t2", "v2"),]);
            var detail = await client.ReadBookAsync(book.id);
            detail.name.Should().Be(book.name);
            detail.description.Should().Be("bbb");
            detail.tags.Should().BeEquivalentTo((Tag[])[new("t1", "v1"), new("t2", "v2"),]);
            detail.cover.Should().BeNull();
            detail.image_id.Should().BeNull();
        }
        {// image from path
            var path = testResPath("images/pd001.png");
            var book = await client.CreateBookAsync(new(testName("aaa")), path).WillBeDiscarded(container);
            Assert.IsNotNull(book.cover);
            book.cover.name.Should().Be("pd001.png");
            book.image_id.Should().NotBeNull();
            var detail = await client.ReadBookAsync(book.id);
            Assert.IsNotNull(detail.cover);
            detail.cover.name.Should().Be("pd001.png");
            detail.image_id.Should().NotBeNull();
        }
        {// image from path & name
            var path = testResPath("images/pd001.png");
            var book = await client.CreateBookAsync(new(testName("aaa")), path, "aaaaaa.jpg").WillBeDiscarded(container);
            Assert.IsNotNull(book.cover);
            book.cover.name.Should().Be("aaaaaa.jpg");
            book.image_id.Should().NotBeNull();
            var detail = await client.ReadBookAsync(book.id);
            Assert.IsNotNull(detail.cover);
            detail.cover.name.Should().Be("aaaaaa.jpg");
            detail.image_id.Should().NotBeNull();
        }
        {// image from content
            var image = await testResContentAsync("images/pd001.png");
            var book = await client.CreateBookAsync(new(testName("aaa")), image, "testimage.png").WillBeDiscarded(container);
            Assert.IsNotNull(book.cover);
            book.cover.name.Should().Be("testimage.png");
            book.image_id.Should().NotBeNull();
            var detail = await client.ReadBookAsync(book.id);
            Assert.IsNotNull(detail.cover);
            detail.cover.name.Should().Be("testimage.png");
            detail.image_id.Should().NotBeNull();
        }
        {// default_template_id
            var template_book = await client.CreateBookAsync(new(testName("template-page-container"))).WillBeDiscarded(container);
            var template_page = await client.CreateMarkdownPageInBookAsync(new(template_book.id, "template-page", "template-body")).WillBeDiscarded(container);

            await using var adapter = new TestBackendAdapter();
            await adapter.SetPagaTemplateFlag(template_page.id, true);

            var book = await client.CreateBookAsync(new(testName("test-book"), default_template_id: template_page.id)).WillBeDiscarded(container);
            book.default_template_id.Should().Be(template_page.id);
            var detail = await client.ReadBookAsync(book.id);
            detail.default_template_id.Should().Be(template_page.id);
        }
    }

    [TestMethod()]
    public async Task ReadBookAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {
            var now = DateTime.UtcNow;
            var path = testResPath("images/pd001.png");
            var book = await client.CreateBookAsync(new(testName("aaa"), "bbb", tags: [new("t1", "v1"), new("t2", "v2"),]), path).WillBeDiscarded(container);
            var book_id = book.id;
            var chapter1 = await client.CreateChapterAsync(new(book_id, "c1"));
            var chapter2 = await client.CreateChapterAsync(new(book_id, "c2"));
            var page1 = await client.CreateMarkdownPageInBookAsync(new(book_id, "p1", "ppp"));
            var page2 = await client.CreateMarkdownPageInBookAsync(new(book_id, "p2", "ppp"));
            var page3 = await client.CreateMarkdownPageInChapterAsync(new(chapter1.id, "p1", "ppp"));
            var page4 = await client.CreateMarkdownPageInChapterAsync(new(chapter1.id, "p2", "ppp"));

            var detail = await client.ReadBookAsync(book.id);
            detail.name.Should().Be(testName("aaa"));
            detail.description.Should().Be("bbb");
            detail.description_html.Should().Contain("bbb");
            detail.slug.Should().NotBeNullOrEmpty();
            detail.tags.Should().BeEquivalentTo((Tag[])[new("t1", "v1"), new("t2", "v2"),]);
            detail.created_at.Should().BeCloseTo(now, 10.Seconds());
            detail.updated_at.Should().BeCloseTo(now, 10.Seconds());
            detail.created_by.name.Should().Be("Admin");
            detail.updated_by.name.Should().Be("Admin");
            detail.owned_by.name.Should().Be("Admin");
            Assert.IsNotNull(detail.cover);
            detail.cover.name.Should().Be("pd001.png");
            detail.cover.type.Should().Be("cover_book");
            detail.cover.created_at.Should().BeCloseTo(now, 10.Seconds());
            detail.cover.updated_at.Should().BeCloseTo(now, 10.Seconds());
            detail.cover.created_by.Should().Be(detail.created_by.id);
            detail.cover.updated_by.Should().Be(detail.updated_by.id);
            detail.cover.uploaded_to.Should().Be(detail.id);
            detail.contents.OfType<BookContentChapter>().Should().BeEquivalentTo(new[]
                {
                    new { chapter1.id, chapter1.name, },
                    new { chapter2.id, chapter2.name, },
                });
            detail.contents.OfType<BookContentPage>().Should().BeEquivalentTo(new[]
                {
                    new { page1.id, page1.name, },
                    new { page2.id, page2.name, },
                });

            var bookChapter1 = detail.chapters().First(c => c.id == chapter1.id);
            bookChapter1.pages.Should().BeEquivalentTo(new[]
                {
                    new { page3.id, page3.name, },
                    new { page4.id, page4.name, },
                });
        }
    }

    [TestMethod()]
    public async Task UpdateBookAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {// update name & desc
            var image = await testResContentAsync("images/pd001.png");
            var created = await client.CreateBookAsync(new(testName("aaa"), "bbb", tags: [new("t1", "v1"), new("t2", "v2"),]), image, "test.png").WillBeDiscarded(container);
            await Task.Delay(2 * 1000);     // for update timestamp
            var updated = await client.UpdateBookAsync(created.id, new(name: testName("ccc"), description: "ddd"));
            updated.name.Should().Be(testName("ccc"));
            updated.description.Should().Be("ddd");
            updated.description_html.Should().Contain("ddd");
            updated.slug.Should().NotBeNullOrEmpty();
            updated.created_at.Should().Be(created.created_at);
            updated.updated_at.Should().BeAfter(created.updated_at);
            updated.created_by.Should().Be(created.created_by);
            updated.updated_by.Should().Be(created.updated_by);
            updated.owned_by.Should().Be(created.owned_by);
        }
        {// description_html
            var created = await client.CreateBookAsync(new(testName("aaa"), "bbb")).WillBeDiscarded(container);
            var updated = await client.UpdateBookAsync(created.id, new(name: testName("ccc"), description_html: "ddd"));
            updated.description.Should().Contain("ddd");
            updated.description_html.Should().Contain("ddd");
        }
        {// update tags
            var image = await testResContentAsync("images/pd001.png");
            var created = await client.CreateBookAsync(new(testName("aaa"), "bbb", tags: [new("t1", "v1"), new("t2", "v2"),]), image, "test.png").WillBeDiscarded(container);
            var updated = await client.UpdateBookAsync(created.id, new(tags: [new("t1", "v1new"), new("t3", "v3")]));
            updated.name.Should().Be(testName("aaa"));
            updated.description.Should().Be("bbb");
            updated.tags.Should().BeEquivalentTo((Tag[])[new("t1", "v1new"), new("t3", "v3"),]);
            var detail = await client.ReadBookAsync(updated.id);
            detail.tags.Should().BeEquivalentTo((Tag[])[new("t1", "v1new"), new("t3", "v3"),]);
        }
        {// update image from path
            var image = await testResContentAsync("images/pd001.png");
            var created = await client.CreateBookAsync(new(testName("ccc"), "ddd", tags: [new("t1", "v1"), new("t2", "v2"),]), image, "test.png").WillBeDiscarded(container);
            var path = testResPath("images/pd001.png");
            var updated = await client.UpdateBookAsync(created.id, new(), path, "aaa.jpg");
            Assert.IsNotNull(updated.cover);
            updated.cover.name.Should().Be("aaa.jpg");
            var detail = await client.ReadBookAsync(updated.id);
            Assert.IsNotNull(detail.cover);
            detail.cover.name.Should().Be("aaa.jpg");
        }
        {// update image from content
            var image = await testResContentAsync("images/pd001.png");
            var created = await client.CreateBookAsync(new(testName("ccc"), "ddd", tags: [new("t1", "v1"), new("t2", "v2"),]), image, "test.png").WillBeDiscarded(container);
            var newimage = await testResContentAsync("images/pd002.png");
            var updated = await client.UpdateBookAsync(created.id, new(), newimage, "new.png");
            Assert.IsNotNull(updated.cover);
            updated.cover.name.Should().Be("new.png");
            var detail = await client.ReadBookAsync(updated.id);
            Assert.IsNotNull(detail.cover);
            detail.cover.name.Should().Be("new.png");
        }
        {// default_template_id
            var created = await client.CreateBookAsync(new(testName("test-book"))).WillBeDiscarded(container);
            created.default_template_id.Should().BeNull();

            var template_page = await client.CreateMarkdownPageInBookAsync(new(created.id, "template-page", "template-body")).WillBeDiscarded(container);
            await using var adapter = new TestBackendAdapter();
            await adapter.SetPagaTemplateFlag(template_page.id, true);

            var updated = await client.UpdateBookAsync(created.id, new(default_template_id: template_page.id));
            updated.default_template_id.Should().Be(template_page.id);
            var detail = await client.ReadBookAsync(created.id);
            detail.default_template_id.Should().Be(template_page.id);
        }
    }

    [TestMethod()]
    public async Task DeleteBookAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        var name = testName($"book_{Guid.NewGuid()}");
        var book = await client.CreateBookAsync(new(name));
        (await client.ListBooksAsync(new(filters: [new("name", name)]))).data.Should().Contain(d => d.id == book.id);
        await client.DeleteBookAsync(book.id);
        (await client.ListBooksAsync(new(filters: [new("name", name)]))).data.Should().NotContain(d => d.id == book.id);

    }

    [TestMethod()]
    public async Task ExportBookHtmlAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("aaa"), "bbb", tags: [new("t1", "v1"), new("t2", "v2"),])).WillBeDiscarded(container);
        var html = await client.ExportBookHtmlAsync(book.id);
        html.Should().NotBeNullOrEmpty();

    }

    [TestMethod()]
    public async Task ExportBookPlainAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("aaa"), "bbb", tags: [new("t1", "v1"), new("t2", "v2"),])).WillBeDiscarded(container);
        var text = await client.ExportBookPlainAsync(book.id);
        text.Should().NotBeNullOrEmpty();
    }

    [TestMethod()]
    public async Task ExportBookMarkdownAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("aaa"), "bbb", tags: [new("t1", "v1"), new("t2", "v2"),])).WillBeDiscarded(container);
        var markdown = await client.ExportBookMarkdownAsync(book.id);
        markdown.Should().NotBeNullOrEmpty();
    }

    [TestMethod()]
    public async Task ExportBookPdfAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("aaa"), "bbb", tags: [new("t1", "v1"), new("t2", "v2"),])).WillBeDiscarded(container);
        using var pdf = await client.ExportBookPdfAsync(book.id);
        pdf.Stream.Should().BeReadable();
    }

    [TestMethod()]
    public async Task ExportBookZipAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("aaa"), "bbb", tags: [new("t1", "v1"), new("t2", "v2"),])).WillBeDiscarded(container);
        using var zip = await client.ExportBookZipAsync(book.id);
        zip.Stream.Should().BeReadable();
    }
    #endregion
}
