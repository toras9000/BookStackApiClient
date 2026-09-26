namespace BookStackApiClient.Tests;

[TestClass()]
public class BookStackClientSearchTests : BookStackClientTestsBase
{
    #region search
    [TestMethod()]
    public async Task SearchAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        await using var container = new TestResourceContainer(client);
        var guid = Guid.NewGuid().ToString();
        for (var i = 0; i < 10; i++)
        {
            await client.CreateBookAsync(new($"{TestName()}_book_{guid}_N{i:D3}", $"book_{guid}_N{i:D3}_desc", tags: [new($"bt{i}", $"btv{i}")])).WillBeDiscarded(container);
        }
        for (var i = 0; i < 10; i++)
        {
            var book = container.Books[i / 3];
            await client.CreateChapterAsync(new(book.id, $"{TestName()}_chapter_{guid}_N{i:D3}", $"chapter_{guid}_N{i:D3}_desc", priority: i, tags: [new($"ct{i}", $"ctv{i}")])).AddTo(container);
        }
        for (var i = 0; i < 10; i++)
        {
            if (i < 5)
            {
                var chapter = container.Chapters[i / 2];
                await client.CreatePageAsync(new(chapter_id: chapter.id, name: $"{TestName()}_page_{guid}_N{i:D3}", markdown: "md", tags: [new($"pt{i}", $"ptv{i}")])).AddTo(container);
            }
            else
            {
                var book = container.Books[i / 2];
                await client.CreatePageAsync(new(book_id: book.id, name: $"{TestName()}_page_{guid}_N{i:D3}", markdown: "md", tags: [new($"pt{i}", $"ptv{i}")])).AddTo(container);
            }
        }
        for (var i = 0; i < 10; i++)
        {
            await client.CreateShelfAsync(new($"{TestName()}_shelf_{guid}_N{i:D3}", $"shelf_{guid}_N{i:D3}_desc", tags: [new($"st{i}", $"stv{i}"),])).WillBeDiscarded(container);
        }


        // test call & validate
        {
            for (var i = 0; i < 10; i++)
            {
                var numpart = $"{guid}_N{i:D3}";
                var results = await client.SearchAsync(new($"{{in_name:{numpart}}}"));
                results.books().Select(b => b.id).Should().BeEquivalentTo(container.Books.Where(b => b.name.Contains(numpart)).Select(b => b.id));
                results.chapters().Select(c => c.id).Should().BeEquivalentTo(container.Chapters.Where(c => c.name.Contains(numpart)).Select(c => c.id));
                results.pages().Select(p => p.id).Should().BeEquivalentTo(container.Pages.Where(p => p.name.Contains(numpart)).Select(p => p.id));
                results.shelves().Select(s => s.id).Should().BeEquivalentTo(container.Shelves.Where(s => s.name.Contains(numpart)).Select(s => s.id));
            }
        }
        {
            for (var i = 0; i < 10; i++)
            {
                var name = $"book_{guid}_N{i:D3}";
                var expect = container.Books.First(b => b.name.Contains(name));
                var actual = await client.SearchAsync(new($"{{in_name:{name}}}")).ContinueWith(t => t.Result.books().FirstOrDefault());
                Assert.IsNotNull(actual);
                actual.id.Should().Be(expect.id);
                actual.name.Should().Be(expect.name);
                actual.slug.Should().Be(expect.slug);
                actual.created_at.Should().Be(expect.created_at);
                actual.updated_at.Should().Be(expect.updated_at);
                actual.tags.Should().BeEquivalentTo((Tag[])[new($"bt{i}", $"btv{i}")]);
            }
        }
        {
            for (var i = 0; i < 10; i++)
            {
                var name = $"chapter_{guid}_N{i:D3}";
                var expect = container.Chapters.First(b => b.name.Contains(name));
                var actual = await client.SearchAsync(new($"{{in_name:{name}}}")).ContinueWith(t => t.Result.chapters().FirstOrDefault());
                Assert.IsNotNull(actual);
                actual.id.Should().Be(expect.id);
                actual.book_id.Should().Be(expect.book_id);
                actual.name.Should().Be(expect.name);
                actual.slug.Should().Be(expect.slug);
                actual.created_at.Should().Be(expect.created_at);
                actual.updated_at.Should().Be(expect.updated_at);
                actual.tags.Should().BeEquivalentTo((Tag[])[new($"ct{i}", $"ctv{i}")]);
            }
        }
        {
            for (var i = 0; i < 10; i++)
            {
                var name = $"page_{guid}_N{i:D3}";
                var expect = container.Pages.First(b => b.name.Contains(name));
                var actual = await client.SearchAsync(new($"{{in_name:{name}}}")).ContinueWith(t => t.Result.pages().FirstOrDefault());
                Assert.IsNotNull(actual);
                actual.id.Should().Be(expect.id);
                actual.book_id.Should().Be(expect.book_id);
                actual.name.Should().Be(expect.name);
                actual.slug.Should().Be(expect.slug);
                actual.created_at.Should().Be(expect.created_at);
                actual.updated_at.Should().Be(expect.updated_at);
                actual.tags.Should().BeEquivalentTo((Tag[])[new($"pt{i}", $"ptv{i}")]);
            }
        }
        {
            for (var i = 0; i < 10; i++)
            {
                var name = $"shelf_{guid}_N{i:D3}";
                var expect = container.Shelves.First(b => b.name.Contains(name));
                var actual = await client.SearchAsync(new($"{{in_name:{name}}}")).ContinueWith(t => t.Result.shelves().FirstOrDefault());
                Assert.IsNotNull(actual);
                actual.id.Should().Be(expect.id);
                actual.name.Should().Be(expect.name);
                actual.slug.Should().Be(expect.slug);
                actual.created_at.Should().Be(expect.created_at);
                actual.updated_at.Should().Be(expect.updated_at);
                actual.tags.Should().BeEquivalentTo((Tag[])[new($"st{i}", $"stv{i}")]);
            }
        }
    }

    [TestMethod()]
    public async Task SearchAsync_paging()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        await using var container = new TestResourceContainer(client);
        var guid = Guid.NewGuid().ToString();
        for (var i = 0; i < 10; i++)
        {
            await client.CreateBookAsync(new($"{TestName()}_book_{guid}_N{i:D3}", $"book_{guid}_N{i:D3}_desc", tags: [new($"bt{i}", $"btv{i}")])).WillBeDiscarded(container);
        }


        // test call & validate
        {
            var paging1 = await client.SearchAsync(new($"{{in_name:{guid}}}", count: 3, page: 1));
            var paging2 = await client.SearchAsync(new($"{{in_name:{guid}}}", count: 3, page: 2));

            paging1.data.Select(d => d.id).Should().NotIntersectWith(paging2.data.Select(d => d.id));
        }
    }

    [TestMethod()]
    public async Task SearchBookContentsAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        await using var container = new TestResourceContainer(client);

        var book1 = await client.CreateBookAsync(new($"{TestName()}_book1", $"book1_desc", tags: [new($"b1_t", $"b1_tv")])).WillBeDiscarded(container);
        var book1chapter1 = await client.CreateChapterAsync(new(book1.id, $"{TestName()}_b1_chapter1", $"b1_chapter1_desc", tags: [new($"b1c1_t", $"b1c1_tv")])).AddTo(container);
        var book1chapter1page1 = await client.CreateMarkdownPageInChapterAsync(new(book1chapter1.id, $"{TestName()}_b1c1_page1", $"b1c1_page1_desc", tags: [new($"b1c1p1_t", $"b1c1p1_tv")])).AddTo(container);
        var book1chapter1page2 = await client.CreateMarkdownPageInChapterAsync(new(book1chapter1.id, $"{TestName()}_b1c1_page2", $"b1c1_page2_desc", tags: [new($"b1c1p2_t", $"b1c1p2_tv")])).AddTo(container);
        var book1chapter2 = await client.CreateChapterAsync(new(book1.id, $"{TestName()}_b1_chapter2", $"b1_chapter2_desc", tags: [new($"b1c2_t", $"b1c2_tv")])).AddTo(container);
        var book1chapter2page1 = await client.CreateMarkdownPageInChapterAsync(new(book1chapter2.id, $"{TestName()}_b1c2_page1", $"b1c2_page1_desc", tags: [new($"b1c2p1_t", $"b1c2p1_tv")])).AddTo(container);
        var book1chapter2page2 = await client.CreateMarkdownPageInChapterAsync(new(book1chapter2.id, $"{TestName()}_b1c2_page2", $"b1c2_page2_desc", tags: [new($"b1c2p2_t", $"b1c2p2_tv")])).AddTo(container);

        var book2 = await client.CreateBookAsync(new($"{TestName()}_book2", $"book2_desc", tags: [new($"b2_t", $"b2_tv")])).WillBeDiscarded(container);
        var book2chapter1 = await client.CreateChapterAsync(new(book2.id, $"{TestName()}_b2_chapter1", $"b2_chapter1_desc", tags: [new($"b2c1_t", $"b2c1_tv")])).AddTo(container);
        var book2chapter1page1 = await client.CreateMarkdownPageInChapterAsync(new(book2chapter1.id, $"{TestName()}_b2c1_page1", $"b2c1_page1_desc", tags: [new($"b2c1p1_t", $"b2c1p1_tv")])).AddTo(container);
        var book2chapter1page2 = await client.CreateMarkdownPageInChapterAsync(new(book2chapter1.id, $"{TestName()}_b2c1_page2", $"b2c1_page2_desc", tags: [new($"b2c1p2_t", $"b2c1p2_tv")])).AddTo(container);
        var book2chapter2 = await client.CreateChapterAsync(new(book2.id, $"{TestName()}_b2_chapter2", $"b2_chapter2_desc", tags: [new($"b2c2_t", $"b2c2_tv")])).AddTo(container);
        var book2chapter2page1 = await client.CreateMarkdownPageInChapterAsync(new(book2chapter2.id, $"{TestName()}_b2c2_page1", $"b2c2_page1_desc", tags: [new($"b2c2p1_t", $"b2c2p1_tv")])).AddTo(container);
        var book2chapter2page2 = await client.CreateMarkdownPageInChapterAsync(new(book2chapter2.id, $"{TestName()}_b2c2_page2", $"b2c2_page2_desc", tags: [new($"b2c2p2_t", $"b2c2p2_tv")])).AddTo(container);

        // test call & validate
        var results = await client.SearchBookContentsAsync(book2.id, new("{in_name:page2}"));
        results.books().Select(b => b.id).Should().BeEmpty();
        results.chapters().Select(c => c.id).Should().BeEmpty();
        results.pages().Select(p => p.id).Should().BeEquivalentTo([book2chapter1page2.id, book2chapter2page2.id]);
        results.shelves().Select(s => s.id).Should().BeEmpty();
    }

    [TestMethod()]
    public async Task SearchChapterContentsAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        await using var container = new TestResourceContainer(client);

        var book1 = await client.CreateBookAsync(new($"{TestName()}_book1", $"book1_desc", tags: [new($"b1_t", $"b1_tv")])).WillBeDiscarded(container);
        var book1chapter1 = await client.CreateChapterAsync(new(book1.id, $"{TestName()}_b1_chapter1", $"b1_chapter1_desc", tags: [new($"b1c1_t", $"b1c1_tv")])).AddTo(container);
        var book1chapter1page1 = await client.CreateMarkdownPageInChapterAsync(new(book1chapter1.id, $"{TestName()}_b1c1_page1", $"b1c1_page1_desc", tags: [new($"b1c1p1_t", $"b1c1p1_tv")])).AddTo(container);
        var book1chapter1page2 = await client.CreateMarkdownPageInChapterAsync(new(book1chapter1.id, $"{TestName()}_b1c1_page2", $"b1c1_page2_desc", tags: [new($"b1c1p2_t", $"b1c1p2_tv")])).AddTo(container);
        var book1chapter2 = await client.CreateChapterAsync(new(book1.id, $"{TestName()}_b1_chapter2", $"b1_chapter2_desc", tags: [new($"b1c2_t", $"b1c2_tv")])).AddTo(container);
        var book1chapter2page1 = await client.CreateMarkdownPageInChapterAsync(new(book1chapter2.id, $"{TestName()}_b1c2_page1", $"b1c2_page1_desc", tags: [new($"b1c2p1_t", $"b1c2p1_tv")])).AddTo(container);
        var book1chapter2page2 = await client.CreateMarkdownPageInChapterAsync(new(book1chapter2.id, $"{TestName()}_b1c2_page2", $"b1c2_page2_desc", tags: [new($"b1c2p2_t", $"b1c2p2_tv")])).AddTo(container);

        var book2 = await client.CreateBookAsync(new($"{TestName()}_book2", $"book2_desc", tags: [new($"b2_t", $"b2_tv")])).WillBeDiscarded(container);
        var book2chapter1 = await client.CreateChapterAsync(new(book2.id, $"{TestName()}_b2_chapter1", $"b2_chapter1_desc", tags: [new($"b2c1_t", $"b2c1_tv")])).AddTo(container);
        var book2chapter1page1 = await client.CreateMarkdownPageInChapterAsync(new(book2chapter1.id, $"{TestName()}_b2c1_page1", $"b2c1_page1_desc", tags: [new($"b2c1p1_t", $"b2c1p1_tv")])).AddTo(container);
        var book2chapter1page2 = await client.CreateMarkdownPageInChapterAsync(new(book2chapter1.id, $"{TestName()}_b2c1_page2", $"b2c1_page2_desc", tags: [new($"b2c1p2_t", $"b2c1p2_tv")])).AddTo(container);
        var book2chapter2 = await client.CreateChapterAsync(new(book2.id, $"{TestName()}_b2_chapter2", $"b2_chapter2_desc", tags: [new($"b2c2_t", $"b2c2_tv")])).AddTo(container);
        var book2chapter2page1 = await client.CreateMarkdownPageInChapterAsync(new(book2chapter2.id, $"{TestName()}_b2c2_page1", $"b2c2_page1_desc", tags: [new($"b2c2p1_t", $"b2c2p1_tv")])).AddTo(container);
        var book2chapter2page2 = await client.CreateMarkdownPageInChapterAsync(new(book2chapter2.id, $"{TestName()}_b2c2_page2", $"b2c2_page2_desc", tags: [new($"b2c2p2_t", $"b2c2p2_tv")])).AddTo(container);

        // test call & validate
        var results = await client.SearchChapterContentsAsync(book2chapter1.id, new("{in_name:page2}"));
        results.books().Select(b => b.id).Should().BeEmpty();
        results.chapters().Select(c => c.id).Should().BeEmpty();
        results.pages().Select(p => p.id).Should().BeEquivalentTo([book2chapter1page2.id]);
        results.shelves().Select(s => s.id).Should().BeEmpty();
    }
    #endregion
}
