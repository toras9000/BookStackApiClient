namespace BookStackApiClient.Tests;

[TestClass()]
public class BookStackClientShelvesTests : BookStackClientTestsBase
{
    #region shelves
    [TestMethod()]
    public async Task ListShelvesAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book1 = await client.CreateBookAsync(new(testName("book1"))).WillBeDiscarded(container);
        var book2 = await client.CreateBookAsync(new(testName("book2"))).WillBeDiscarded(container);
        var book3 = await client.CreateBookAsync(new(testName("book3"))).WillBeDiscarded(container);

        await client.CreateShelfAsync(new(testName("testshelve1"), "desc1", books: [book1.id, book3.id,], tags: [new("ts1", "tv1"),])).WillBeDiscarded(container);
        await client.CreateShelfAsync(new(testName("testshelve2"), "desc2", books: [book2.id, book3.id,])).WillBeDiscarded(container);
        await client.CreateShelfAsync(new(testName("testshelve3"), "desc3", tags: [new("ts2", "tv2"),])).WillBeDiscarded(container);
        await client.CreateShelfAsync(new(testName("testshelve4"), "desc4")).WillBeDiscarded(container);

        var shelves = await client.ListShelvesAsync();
        foreach (var shelf in container.Shelves)
        {
            var actual = shelves.data.Should().Contain(i => i.id == shelf.id).Subject;
            var expect = shelf;
            actual.Should().BeEquivalentTo(shelf, o => o.ExcludingMissingMembers());
        }
    }

    [TestMethod()]
    public async Task ListShelvesAsync_options()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);

        var prefix1 = testName($"shelve_{Guid.NewGuid()}_");
        for (var i = 0; i < 10; i++)
        {
            await client.CreateShelfAsync(new($"{prefix1}_{i:D2}")).WillBeDiscarded(container);
        }
        var prefix2 = testName($"shelve_{Guid.NewGuid()}_");
        for (var i = 0; i < 10; i++)
        {
            await client.CreateShelfAsync(new($"{prefix2}_{i:D2}")).WillBeDiscarded(container);
        }

        {// range
            var shelves1 = await client.ListShelvesAsync(new(offset: 0, count: 5));
            shelves1.data.Should().HaveCount(5);
            var shelves2 = await client.ListShelvesAsync(new(offset: 5, count: 5));
            shelves2.data.Should().HaveCount(5);

            shelves1.data.Select(d => d.id).Should().NotIntersectWith(shelves2.data.Select(d => d.id));
        }
        {// filter
            var shelves = await client.ListShelvesAsync(new(filters: [new($"name:like", $"{prefix1}%")]));
            shelves.data.Should().AllSatisfy(d => d.name.StartsWith(prefix1));
        }
        {// filter & sort (asc)
            var offset = 0;
            var count = 4;
            var shelves = await client.ListShelvesAsync(new(offset, count, sorts: [nameof(ShelfItem.name),], filters: [new($"name:like", $"{prefix1}%")]));
            var expects = container.Shelves.Where(b => b.name.StartsWith(prefix1)).Select(b => b.id).Skip(offset).Take(count);
            shelves.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (desc)
            var offset = 0;
            var count = 4;
            var shelves = await client.ListShelvesAsync(new(offset, count, sorts: [$"-{nameof(ShelfItem.name)}",], filters: [new($"name:like", $"{prefix1}%")]));
            var expects = container.Shelves.Reverse().Where(b => b.name.StartsWith(prefix1)).Select(b => b.id).Skip(offset).Take(count);
            shelves.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (asc) & range
            var offset = 2;
            var count = 5;
            var shelves = await client.ListShelvesAsync(new(offset, count, sorts: [nameof(ShelfItem.name),], filters: [new($"name:like", $"{prefix2}%")]));
            var expects = container.Shelves.Where(b => b.name.StartsWith(prefix2)).Select(b => b.id).Skip(offset).Take(count);
            shelves.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (desc) & range
            var offset = 3;
            var count = 4;
            var shelves = await client.ListShelvesAsync(new(offset, count, sorts: [$"-{nameof(ShelfItem.name)}",], filters: [new($"name:like", $"{prefix2}%")]));
            var expects = container.Shelves.Reverse().Where(b => b.name.StartsWith(prefix2)).Select(b => b.id).Skip(offset).Take(count);
            shelves.data.Select(d => d.id).Should().Equal(expects);
        }
    }

    [TestMethod()]
    public async Task CreateShelfAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book1 = await client.CreateBookAsync(new(testName("book1"))).WillBeDiscarded(container);
        var book2 = await client.CreateBookAsync(new(testName("book2"))).WillBeDiscarded(container);
        var book3 = await client.CreateBookAsync(new(testName("book3"))).WillBeDiscarded(container);

        {// name & desc
            var now = DateTime.UtcNow;
            var shelf = await client.CreateShelfAsync(new(testName("aaa"), "bbb")).WillBeDiscarded(container);
            shelf.name.Should().Be(testName("aaa"));
            shelf.description.Should().Be("bbb");
            shelf.slug.Should().NotBeNullOrEmpty();
            shelf.created_at.Should().BeCloseTo(now, 10.Seconds());
            shelf.updated_at.Should().BeCloseTo(now, 10.Seconds());
            shelf.created_by.Should().Be(shelf.created_by);
            shelf.updated_by.Should().Be(shelf.updated_by);
            shelf.owned_by.Should().Be(shelf.owned_by);
        }
        {// books
            var shelf = await client.CreateShelfAsync(new(testName("ccc"), books: new[] { book2.id, book3.id, })).WillBeDiscarded(container);
            shelf.name.Should().Be(testName("ccc"));
            var detail = await client.ReadShelfAsync(shelf.id);
            detail.tags.Should().BeNullOrEmpty();
            detail.books.Should().BeEquivalentTo(new[] { book2, book3 }, o => o.ExcludingMissingMembers());
            detail.cover.Should().BeNull();
        }
        {// tags
            var shelf = await client.CreateShelfAsync(new(testName("ddd"), tags: [new("ts1", "vs1"), new("ts2", "vs2"),])).WillBeDiscarded(container);
            shelf.name.Should().Be(testName("ddd"));
            shelf.description.Should().BeEmpty();
            shelf.slug.Should().NotBeNullOrEmpty();
            var detail = await client.ReadShelfAsync(shelf.id);
            detail.tags.Should().BeEquivalentTo((Tag[])[new("ts1", "vs1"), new("ts2", "vs2"),]);
            detail.books.Should().BeNullOrEmpty();
            detail.cover.Should().BeNull();
        }
        {// cover (path)
            var now = DateTime.UtcNow;
            var path = testResPath("images/pd001.png");
            var shelf = await client.CreateShelfAsync(new(testName("eee")), path).WillBeDiscarded(container);
            shelf.name.Should().Be(testName("eee"));
            var detail = await client.ReadShelfAsync(shelf.id);
            detail.tags.Should().BeNullOrEmpty();
            detail.books.Should().BeNullOrEmpty();
            Assert.IsNotNull(detail.cover);
            detail.cover.name.Should().Be("pd001.png");
            detail.cover.type.Should().Be("cover_bookshelf");
            detail.cover.uploaded_to.Should().Be(shelf.id);
            detail.cover.path.Should().NotBeNullOrEmpty();
            detail.cover.url.Should().NotBeNullOrEmpty();
            detail.cover.created_at.Should().BeCloseTo(now, 10.Seconds());
            detail.cover.updated_at.Should().BeCloseTo(now, 10.Seconds());
            detail.cover.created_by.Should().Be(shelf.created_by);
            detail.cover.updated_by.Should().Be(shelf.updated_by);
        }
        {// cover (path & name)
            var now = DateTime.UtcNow;
            var path = testResPath("images/pd001.png");
            var shelf = await client.CreateShelfAsync(new(testName("eee")), path, "xxx.png").WillBeDiscarded(container);
            shelf.name.Should().Be(testName("eee"));
            var detail = await client.ReadShelfAsync(shelf.id);
            detail.tags.Should().BeNullOrEmpty();
            detail.books.Should().BeNullOrEmpty();
            Assert.IsNotNull(detail.cover);
            detail.cover.name.Should().Be("xxx.png");
            detail.cover.type.Should().Be("cover_bookshelf");
            detail.cover.uploaded_to.Should().Be(shelf.id);
            detail.cover.path.Should().NotBeNullOrEmpty();
            detail.cover.url.Should().NotBeNullOrEmpty();
            detail.cover.created_at.Should().BeCloseTo(now, 10.Seconds());
            detail.cover.updated_at.Should().BeCloseTo(now, 10.Seconds());
            detail.cover.created_by.Should().Be(shelf.created_by);
            detail.cover.updated_by.Should().Be(shelf.updated_by);
        }
        {// cover (binary)
            var now = DateTime.UtcNow;
            var binary = await testResContentAsync("images/pd001.png");
            var shelf = await client.CreateShelfAsync(new(testName("eee")), binary, "img.png").WillBeDiscarded(container);
            shelf.name.Should().Be(testName("eee"));
            var detail = await client.ReadShelfAsync(shelf.id);
            detail.tags.Should().BeNullOrEmpty();
            detail.books.Should().BeNullOrEmpty();
            Assert.IsNotNull(detail.cover);
            detail.cover.name.Should().Be("img.png");
            detail.cover.type.Should().Be("cover_bookshelf");
            detail.cover.uploaded_to.Should().Be(shelf.id);
            detail.cover.path.Should().NotBeNullOrEmpty();
            detail.cover.url.Should().NotBeNullOrEmpty();
            detail.cover.created_at.Should().BeCloseTo(now, 10.Seconds());
            detail.cover.updated_at.Should().BeCloseTo(now, 10.Seconds());
            detail.cover.created_by.Should().Be(shelf.created_by);
            detail.cover.updated_by.Should().Be(shelf.updated_by);
        }
    }

    [TestMethod()]
    public async Task ReadShelfAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book1 = await client.CreateBookAsync(new(testName("book1"), "desc1")).WillBeDiscarded(container);
        var book2 = await client.CreateBookAsync(new(testName("book2"), "desc2", tags: [new("bt1", "bv1"), new("bt2", "bv2"),])).WillBeDiscarded(container);
        var book3 = await client.CreateBookAsync(new(testName("book3"), "desc3"), testResPath("images/pd001.png")).WillBeDiscarded(container);

        {
            var books = new[] { book1.id, book2.id, book3.id };
            var tags = (Tag[])[new("st1", "sv1"), new("st2", "sv2"),];
            var path = testResPath("images/pd002.png");
            var now = DateTime.UtcNow;
            var shelf = await client.CreateShelfAsync(new(testName("shelf"), "desc", books, tags), path).WillBeDiscarded(container);
            var detail = await client.ReadShelfAsync(shelf.id);
            detail.name.Should().Be(testName("shelf"));
            detail.description.Should().Be("desc");
            detail.slug.Should().Be(shelf.slug);
            foreach (var book in container.Books)
            {
                var actual = detail.books.Should().Contain(b => b.id == book.id).Subject;
                var expect = book;
                actual.Should().BeEquivalentTo(expect, o => o.ExcludingMissingMembers());
            }
            detail.tags.Should().BeEquivalentTo(tags);
            detail.created_at.Should().BeCloseTo(now, 10.Seconds());
            detail.updated_at.Should().BeCloseTo(now, 10.Seconds());
            detail.created_by.id.Should().Be(shelf.created_by);
            detail.updated_by.id.Should().Be(shelf.updated_by);
            detail.owned_by.id.Should().Be(shelf.owned_by);
            Assert.IsNotNull(detail.cover);
            detail.cover.name.Should().Be("pd002.png");
            detail.cover.type.Should().Be("cover_bookshelf");
            detail.cover.created_at.Should().BeCloseTo(now, 10.Seconds());
            detail.cover.updated_at.Should().BeCloseTo(now, 10.Seconds());
            detail.cover.created_by.Should().Be(detail.created_by.id);
            detail.cover.updated_by.Should().Be(detail.updated_by.id);
            detail.cover.uploaded_to.Should().Be(detail.id);

        }
    }

    [TestMethod()]
    public async Task UpdateShelfAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book1 = await client.CreateBookAsync(new(testName("book1"))).WillBeDiscarded(container);
        var book2 = await client.CreateBookAsync(new(testName("book2"))).WillBeDiscarded(container);
        var book3 = await client.CreateBookAsync(new(testName("book3"))).WillBeDiscarded(container);

        {// name & desc
            var now = DateTime.UtcNow;
            var created = await client.CreateShelfAsync(new(testName("aaa"), "bbb")).WillBeDiscarded(container);
            await Task.Delay(3 * 1000);
            var updated = await client.UpdateShelfAsync(created.id, new(testName("ccc"), "ddd"));
            updated.name.Should().Be(testName("ccc"));
            updated.description.Should().Be("ddd");
            updated.slug.Should().NotBe(created.slug);
            updated.created_at.Should().Be(created.created_at);
            updated.updated_at.Should().BeAfter(created.updated_at);
            updated.created_by.Should().Be(created.created_by);
            updated.updated_by.Should().Be(created.updated_by);
            updated.owned_by.Should().Be(created.owned_by);
        }
        {// books
            var created = await client.CreateShelfAsync(new(testName("aaa"), "bbb", books: new[] { book2.id, book3.id, })).WillBeDiscarded(container);
            created.name.Should().Be(testName("aaa"));
            created.description.Should().Be("bbb");
            await Task.Delay(3 * 1000);
            var updated = await client.UpdateShelfAsync(created.id, new(books: new[] { book1.id, }));
            updated.name.Should().Be(testName("aaa"));
            updated.description.Should().Be("bbb");
            var detail = await client.ReadShelfAsync(created.id);
            detail.tags.Should().BeNullOrEmpty();
            detail.books.Should().BeEquivalentTo(new[] { book1 }, o => o.ExcludingMissingMembers());
            detail.cover.Should().BeNull();
        }
        {// tags
            var created = await client.CreateShelfAsync(new(testName("aaa"), tags: [new("ts1", "vs1"), new("ts2", "vs2"),])).WillBeDiscarded(container);
            created.name.Should().Be(testName("aaa"));
            created.description.Should().BeEmpty();
            created.slug.Should().NotBeNullOrEmpty();
            await Task.Delay(3 * 1000);
            var updated = await client.UpdateShelfAsync(created.id, new(tags: [new("ts3", "vs3"), new("ts4", "vs4"),]));
            var detail = await client.ReadShelfAsync(created.id);
            detail.tags.Should().BeEquivalentTo((Tag[])[new("ts3", "vs3"), new("ts4", "vs4"),]);
            detail.books.Should().BeNullOrEmpty();
            detail.cover.Should().BeNull();
        }
        {// cover
            var now = DateTime.UtcNow;
            var binary = await testResContentAsync("images/pd001.png");
            var created = await client.CreateShelfAsync(new(testName("aaa")), binary, "img.png").WillBeDiscarded(container);
            created.name.Should().Be(testName("aaa"));
            await Task.Delay(3 * 1000);
            var path = testResPath("images/pd001.png");
            var updated = await client.UpdateShelfAsync(created.id, new(), path, "ttt.png");
            var detail = await client.ReadShelfAsync(created.id);
            detail.tags.Should().BeNullOrEmpty();
            detail.books.Should().BeNullOrEmpty();
            Assert.IsNotNull(detail.cover);
            detail.cover.name.Should().Be("ttt.png");
            detail.cover.type.Should().Be("cover_bookshelf");
        }
    }

    [TestMethod()]
    public async Task DeleteShelfAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        var name = testName($"shelf_{Guid.NewGuid()}");
        var shelf = await client.CreateShelfAsync(new(name));
        (await client.ListShelvesAsync(new(filters: [new("name", name)]))).data.Should().Contain(d => d.id == shelf.id);
        await client.DeleteShelfAsync(shelf.id);
        (await client.ListShelvesAsync(new(filters: [new("name", name)]))).data.Should().NotContain(d => d.id == shelf.id);

    }
    #endregion
}