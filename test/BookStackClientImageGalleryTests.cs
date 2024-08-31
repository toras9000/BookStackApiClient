namespace BookStackApiClient.Tests;

[TestClass()]
public class BookStackClientImageGalleryTests : BookStackClientTestsBase
{
    #region image-gallery
    [TestMethod()]
    public async Task ListImagesAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
        var page = await client.CreatePageAsync(new(testName("testpage"), book_id: book.id, markdown: "aaa"));
        await client.CreateImageAsync(new(page.id, "gallery", testName("aaa")), testResPath("images/pd001.png")).WillBeDiscarded(container);
        await client.CreateImageAsync(new(page.id, "drawio", testName("bbb")), await testResContentAsync("images/draw001.png"), "tttt.png").WillBeDiscarded(container);

        var images = await client.ListImagesAsync();
        foreach (var image in container.Images)
        {
            var actual = images.data.Should().Contain(i => i.id == image.id).Subject;
            var expect = image;
            actual.Should().BeEquivalentTo(expect, o => o.ExcludingMissingMembers());
        }
    }

    [TestMethod()]
    public async Task ListImagesAsync_options()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
        var page = await client.CreatePageAsync(new(testName("testpage"), book_id: book.id, markdown: "aaa"));

        var prefix1 = testName($"image_{Guid.NewGuid()}_");
        for (var i = 0; i < 10; i++)
        {
            await client.CreateImageAsync(new(page.id, "gallery", $"{prefix1}_{i:D2}"), testResPath("images/pd001.png")).WillBeDiscarded(container);
        }
        var prefix2 = testName($"image_{Guid.NewGuid()}_");
        for (var i = 0; i < 10; i++)
        {
            await client.CreateImageAsync(new(page.id, "drawio", $"{prefix2}_{i:D2}"), await testResContentAsync("images/draw001.png"), $"img{i:D3}.png").WillBeDiscarded(container);
        }

        {// range
            var images1 = await client.ListImagesAsync(new(offset: 0, count: 5));
            images1.data.Should().HaveCount(5);
            var images2 = await client.ListImagesAsync(new(offset: 5, count: 5));
            images2.data.Should().HaveCount(5);

            images1.data.Select(d => d.id).Should().NotIntersectWith(images2.data.Select(d => d.id));
        }
        {// filter
            var images = await client.ListImagesAsync(new(filters: [new($"name:like", $"{prefix1}%")]));
            images.data.Should().AllSatisfy(d => d.name.StartsWith(prefix1));
        }
        {// filter & sort (asc)
            var offset = 0;
            var count = 4;
            var images = await client.ListImagesAsync(new(offset, count, sorts: [nameof(ImageSummary.name),], filters: [new($"name:like", $"{prefix1}%")]));
            var expects = container.Images.Where(c => c.name.StartsWith(prefix1)).Select(c => c.id).Skip(offset).Take(count);
            images.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (desc)
            var offset = 0;
            var count = 4;
            var images = await client.ListImagesAsync(new(offset, count, sorts: [$"-{nameof(ImageSummary.name)}",], filters: [new($"name:like", $"{prefix1}%")]));
            var expects = container.Images.AsEnumerable().Reverse().Where(c => c.name.StartsWith(prefix1)).Select(c => c.id).Skip(offset).Take(count);
            images.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (asc) & range
            var offset = 2;
            var count = 5;
            var images = await client.ListImagesAsync(new(offset, count, sorts: [nameof(ImageSummary.name),], filters: [new($"name:like", $"{prefix2}%")]));
            var expects = container.Images.Where(c => c.name.StartsWith(prefix2)).Select(c => c.id).Skip(offset).Take(count);
            images.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (desc) & range
            var offset = 3;
            var count = 4;
            var images = await client.ListImagesAsync(new(offset, count, sorts: [$"-{nameof(ImageSummary.name)}",], filters: [new($"name:like", $"{prefix2}%")]));
            var expects = container.Images.AsEnumerable().Reverse().Where(c => c.name.StartsWith(prefix2)).Select(c => c.id).Skip(offset).Take(count);
            images.data.Select(d => d.id).Should().Equal(expects);
        }
    }

    [TestMethod()]
    public async Task CreateImageArgs_gallery()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreatePageAsync(new(testName("testpage"), book_id: book.id, markdown: "aaa"));
            var path = testResPath("images/pd001.png");
            var image = await client.CreateImageAsync(new(page.id, "gallery", testName("aaa")), path).WillBeDiscarded(container);
            image.uploaded_to.Should().Be(page.id);
            image.name.Should().Be(testName("aaa"));
            image.type.Should().Be("gallery");
            image.path.Should().Contain("pd001.png");
            image.url.Should().NotBeNullOrEmpty();
            image.thumbs.gallery.Should().NotBeNullOrEmpty();
            image.thumbs.display.Should().NotBeNullOrEmpty();
            image.content.html.Should().NotBeNullOrEmpty();
            image.content.markdown.Should().NotBeNullOrEmpty();
            image.created_at.Should().BeCloseTo(now, 10.Seconds());
            image.updated_at.Should().BeCloseTo(now, 10.Seconds());
            image.created_by.id.Should().Be(book.created_by);
            image.updated_by.id.Should().Be(book.updated_by);
        }
        {
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreatePageAsync(new(testName("testpage"), book_id: book.id, markdown: "aaa"));
            var path = testResPath("images/pd001.png");
            var image = await client.CreateImageAsync(new(page.id, "gallery", testName("aaa")), path, "aaa-image.png").WillBeDiscarded(container);
            image.uploaded_to.Should().Be(page.id);
            image.name.Should().Be(testName("aaa"));
            image.type.Should().Be("gallery");
            image.path.Should().Contain("aaa-image.png");
            image.url.Should().NotBeNullOrEmpty();
            image.thumbs.gallery.Should().NotBeNullOrEmpty();
            image.thumbs.display.Should().NotBeNullOrEmpty();
            image.content.html.Should().NotBeNullOrEmpty();
            image.content.markdown.Should().NotBeNullOrEmpty();
            image.created_at.Should().BeCloseTo(now, 10.Seconds());
            image.updated_at.Should().BeCloseTo(now, 10.Seconds());
            image.created_by.id.Should().Be(book.created_by);
            image.updated_by.id.Should().Be(book.updated_by);
        }
        {
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreatePageAsync(new(testName("testpage"), book_id: book.id, markdown: "aaa"));
            var binary = await testResContentAsync("images/pd003.png");
            var image = await client.CreateImageAsync(new(page.id, "gallery", testName("bbb")), binary, "img.png").WillBeDiscarded(container);
            image.uploaded_to.Should().Be(page.id);
            image.name.Should().Be(testName("bbb"));
            image.type.Should().Be("gallery");
            image.path.Should().NotBeNullOrEmpty();
            image.url.Should().NotBeNullOrEmpty();
            image.thumbs.gallery.Should().NotBeNullOrEmpty();
            image.thumbs.display.Should().NotBeNullOrEmpty();
            image.content.html.Should().NotBeNullOrEmpty();
            image.content.markdown.Should().NotBeNullOrEmpty();
            image.created_at.Should().BeCloseTo(now, 10.Seconds());
            image.updated_at.Should().BeCloseTo(now, 10.Seconds());
            image.created_by.id.Should().Be(book.created_by);
            image.updated_by.id.Should().Be(book.updated_by);
        }
    }

    [TestMethod()]
    public async Task CreateImageArgs_drawio()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreatePageAsync(new(testName("testpage"), book_id: book.id, markdown: "aaa"));
            var path = testResPath("images/draw001.png");
            var image = await client.CreateImageAsync(new(page.id, "drawio", testName("bbb")), path).WillBeDiscarded(container);
            image.uploaded_to.Should().Be(page.id);
            image.name.Should().Be(testName("bbb"));
            image.type.Should().Be("drawio");
            image.path.Should().NotBeNullOrEmpty();
            image.url.Should().NotBeNullOrEmpty();
            image.thumbs.gallery.Should().NotBeNullOrEmpty();
            image.thumbs.display.Should().NotBeNullOrEmpty();
            image.content.html.Should().NotBeNullOrEmpty();
            image.content.markdown.Should().NotBeNullOrEmpty();
            image.created_at.Should().BeCloseTo(now, 10.Seconds());
            image.updated_at.Should().BeCloseTo(now, 10.Seconds());
            image.created_by.id.Should().Be(book.created_by);
            image.updated_by.id.Should().Be(book.updated_by);
        }
        {
            var now = DateTime.UtcNow;
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreatePageAsync(new(testName("testpage"), book_id: book.id, markdown: "aaa"));
            var binary = await testResContentAsync("images/draw001.png");
            var image = await client.CreateImageAsync(new(page.id, "drawio", testName("bbb")), binary, "img.png").WillBeDiscarded(container);
            image.uploaded_to.Should().Be(page.id);
            image.name.Should().Be(testName("bbb"));
            image.type.Should().Be("drawio");
            image.path.Should().NotBeNullOrEmpty();
            image.url.Should().NotBeNullOrEmpty();
            image.thumbs.gallery.Should().NotBeNullOrEmpty();
            image.thumbs.display.Should().NotBeNullOrEmpty();
            image.content.html.Should().NotBeNullOrEmpty();
            image.content.markdown.Should().NotBeNullOrEmpty();
            image.created_at.Should().BeCloseTo(now, 10.Seconds());
            image.updated_at.Should().BeCloseTo(now, 10.Seconds());
            image.created_by.id.Should().Be(book.created_by);
            image.updated_by.id.Should().Be(book.updated_by);
        }
    }

    [TestMethod()]
    public async Task ReadImageAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreatePageAsync(new(testName("testpage"), book_id: book.id, markdown: "aaa"));
            var path = testResPath("images/pd001.png");
            var image = await client.CreateImageAsync(new(page.id, "gallery", testName("aaa")), path).WillBeDiscarded(container);
            var detail = await client.ReadImageAsync(image.id);
            detail.Should().BeEquivalentTo(image);
        }
        {
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreatePageAsync(new(testName("testpage"), book_id: book.id, markdown: "aaa"));
            var path = testResPath("images/draw001.png");
            var image = await client.CreateImageAsync(new(page.id, "drawio", testName("bbb")), path).WillBeDiscarded(container);
            var detail = await client.ReadImageAsync(image.id);
            detail.Should().BeEquivalentTo(image);
        }

    }

    [TestMethod()]
    public async Task UpdateImageAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {// name
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreatePageAsync(new(testName("testpage"), book_id: book.id, markdown: "aaa"));
            var path = testResPath("images/pd001.png");
            var image = await client.CreateImageAsync(new(page.id, "gallery", testName("aaa")), path).WillBeDiscarded(container);
            await Task.Delay(2 * 1000);     // for update timestamp
            var detail = await client.UpdateImageAsync(image.id, new(testName("bbb")));
            detail.uploaded_to.Should().Be(image.uploaded_to);
            detail.name.Should().Be(testName("bbb"));
            detail.type.Should().Be(image.type);
            detail.path.Should().Be(image.path);
            detail.url.Should().Be(image.url);
            detail.thumbs.gallery.Should().Be(image.thumbs.gallery);
            detail.thumbs.display.Should().Be(image.thumbs.display);
            detail.content.html.Should().NotBe(image.content.html);             // name が反映されるので変わる
            detail.content.markdown.Should().NotBe(image.content.markdown);     // name が反映されるので変わる
            detail.created_at.Should().Be(image.created_at);
            detail.updated_at.Should().BeAfter(image.updated_at);
            detail.created_by.Should().Be(image.created_by);
            detail.updated_by.Should().Be(image.updated_by);
            var dlimage = await this.Client.GetByteArrayAsync(image.url);
            dlimage.Should().Equal(await File.ReadAllBytesAsync(path));
        }
        {// image from path
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreatePageAsync(new(testName("testpage"), book_id: book.id, markdown: "aaa"));
            var path = testResPath("images/pd001.png");
            var image = await client.CreateImageAsync(new(page.id, "gallery", testName("aaa")), path).WillBeDiscarded(container);
            var newpath = testResPath("images/pd002.png");
            var detail = await client.UpdateImageAsync(image.id, new(), newpath, "newimg.png");
            detail.uploaded_to.Should().Be(image.uploaded_to);
            detail.name.Should().Be(testName("aaa"));
            detail.type.Should().Be(image.type);
            detail.path.Should().Be(image.path);
            detail.url.Should().Be(image.url);
            detail.thumbs.gallery.Should().Be(image.thumbs.gallery);
            detail.thumbs.display.Should().Be(image.thumbs.display);
            detail.content.html.Should().Be(image.content.html);
            detail.content.markdown.Should().Be(image.content.markdown);
            detail.created_at.Should().Be(image.created_at);
            detail.updated_at.Should().BeOnOrAfter(image.updated_at);
            detail.created_by.Should().Be(image.created_by);
            detail.updated_by.Should().Be(image.updated_by);
            var dlimage = await this.Client.GetByteArrayAsync(image.url);
            dlimage.Should().Equal(await File.ReadAllBytesAsync(newpath));
        }
        {// image from content
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreatePageAsync(new(testName("testpage"), book_id: book.id, markdown: "aaa"));
            var path = testResPath("images/pd001.png");
            var image = await client.CreateImageAsync(new(page.id, "gallery", testName("aaa")), path).WillBeDiscarded(container);
            var binary = await testResContentAsync("images/pd003.png");
            var detail = await client.UpdateImageAsync(image.id, new(), binary, "newimg.png");
            detail.uploaded_to.Should().Be(image.uploaded_to);
            detail.name.Should().Be(testName("aaa"));
            detail.type.Should().Be(image.type);
            detail.path.Should().Be(image.path);
            detail.url.Should().Be(image.url);
            detail.thumbs.gallery.Should().Be(image.thumbs.gallery);
            detail.thumbs.display.Should().Be(image.thumbs.display);
            detail.content.html.Should().Be(image.content.html);
            detail.content.markdown.Should().Be(image.content.markdown);
            detail.created_at.Should().Be(image.created_at);
            detail.updated_at.Should().BeOnOrAfter(image.updated_at);
            detail.created_by.Should().Be(image.created_by);
            detail.updated_by.Should().Be(image.updated_by);
            var dlimage = await this.Client.GetByteArrayAsync(image.url);
            dlimage.Should().Equal(binary);
        }

    }

    [TestMethod()]
    public async Task DeleteImageAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreatePageAsync(new(testName("testpage"), book_id: book.id, markdown: "aaa"));
            var path = testResPath("images/pd001.png");
            var name = testName($"image_{Guid.NewGuid()}");
            var image = await client.CreateImageAsync(new(page.id, "gallery", name), path);
            (await client.ListImagesAsync(new(filters: [new("name", name)]))).data.Should().Contain(i => i.id == image.id);
            await client.DeleteImageAsync(image.id);
            (await client.ListImagesAsync(new(filters: [new("name", name)]))).data.Should().NotContain(i => i.id == image.id);
        }
        {
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreatePageAsync(new(testName("testpage"), book_id: book.id, markdown: "aaa"));
            var path = testResPath("images/draw001.png");
            var name = testName($"image_{Guid.NewGuid()}");
            var image = await client.CreateImageAsync(new(page.id, "drawio", name), path);
            (await client.ListImagesAsync(new(filters: [new("name", name)]))).data.Should().Contain(i => i.id == image.id);
            await client.DeleteImageAsync(image.id);
            (await client.ListImagesAsync(new(filters: [new("name", name)]))).data.Should().NotContain(i => i.id == image.id);
        }

    }
    #endregion

}