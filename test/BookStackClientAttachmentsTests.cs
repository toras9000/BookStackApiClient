namespace BookStackApiClient.Tests;

[TestClass()]
public class BookStackClientAttachmentsTests : BookStackClientTestsBase
{
    #region attachments
    [TestMethod()]
    public async Task ListAttachmentsAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new($"{TestName()}_testbook")).WillBeDiscarded(container);
        var page = await client.CreatePageAsync(new($"{TestName()}_testpage", book_id: book.id, markdown: "aaa"));
        await client.CreateFileAttachmentAsync(new($"{TestName()}_aaa", page.id), TestResPath("images/pd001.png")).WillBeDiscarded(container);
        await client.CreateFileAttachmentAsync(new($"{TestName()}_bbb", page.id), await TestResContentAsync("images/pd002.png"), "file.ext").WillBeDiscarded(container);
        await client.CreateLinkAttachmentAsync(new($"{TestName()}_ccc", page.id, "https://www.google.com")).WillBeDiscarded(container);

        var attatchments = await client.ListAttachmentsAsync();
        foreach (var attachment in container.Attachments)
        {
            var actual = attatchments.data.Should().Contain(i => i.id == attachment.id).Subject;
            var expect = attachment;
            actual.Should().BeEquivalentTo(expect, o => o.ExcludingMissingMembers());
        }
    }

    [TestMethod()]
    public async Task ListAttachmentsAsync_options()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new($"{TestName()}_testbook")).WillBeDiscarded(container);
        var page = await client.CreatePageAsync(new($"{TestName()}_testpage", book_id: book.id, markdown: "aaa"));

        var prefix1 = $"{TestName()}_attachment_{Guid.NewGuid()}_";
        for (var i = 0; i < 10; i++)
        {
            await client.CreateFileAttachmentAsync(new($"{prefix1}_file{i}", page.id), TestResPath("images/pd001.png")).WillBeDiscarded(container);
        }
        var prefix2 = $"{TestName()}_attachment_{Guid.NewGuid()}_";
        for (var i = 0; i < 10; i++)
        {
            await client.CreateLinkAttachmentAsync(new($"{prefix2}_link{i}", page.id, "https://www.google.com")).WillBeDiscarded(container);
        }

        {// range
            var attachments1 = await client.ListAttachmentsAsync(new(offset: 0, count: 5));
            attachments1.data.Should().HaveCount(5);
            var attachments2 = await client.ListAttachmentsAsync(new(offset: 5, count: 5));
            attachments2.data.Should().HaveCount(5);

            attachments1.data.Select(d => d.id).Should().NotIntersectWith(attachments2.data.Select(d => d.id));
        }
        {// filter
            var attachments = await client.ListAttachmentsAsync(new(filters: [new($"name:like", $"{prefix1}%")]));
            attachments.data.Should().AllSatisfy(d => d.name.Should().StartWith(prefix1));
        }
        {// filter & sort (asc)
            var offset = 0;
            var count = 4;
            var attachments = await client.ListAttachmentsAsync(new(offset, count, sorts: [nameof(AttachmentItem.name),], filters: [new($"name:like", $"{prefix1}%")]));
            var expects = container.Attachments.Where(a => a.name.StartsWith(prefix1)).Select(a => a.id).Skip(offset).Take(count);
            attachments.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (desc)
            var offset = 0;
            var count = 4;
            var attachments = await client.ListAttachmentsAsync(new(offset, count, sorts: [$"-{nameof(BookSummary.name)}",], filters: [new($"name:like", $"{prefix1}%")]));
            var expects = container.Attachments.Reverse().Where(a => a.name.StartsWith(prefix1)).Select(a => a.id).Skip(offset).Take(count);
            attachments.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (asc) & range
            var offset = 2;
            var count = 5;
            var attachments = await client.ListAttachmentsAsync(new(offset, count, sorts: [nameof(AttachmentItem.name),], filters: [new($"name:like", $"{prefix2}%")]));
            var expects = container.Attachments.Where(a => a.name.StartsWith(prefix2)).Select(a => a.id).Skip(offset).Take(count);
            attachments.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (desc) & range
            var offset = 3;
            var count = 4;
            var attachments = await client.ListAttachmentsAsync(new(offset, count, sorts: [$"-{nameof(BookSummary.name)}",], filters: [new($"name:like", $"{prefix2}%")]));
            var expects = container.Attachments.Reverse().Where(a => a.name.StartsWith(prefix2)).Select(a => a.id).Skip(offset).Take(count);
            attachments.data.Select(d => d.id).Should().Equal(expects);
        }
    }

    [TestMethod()]
    public async Task CreateFileAttachmentAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new($"{TestName()}_testbook")).WillBeDiscarded(container);
        var page = await client.CreatePageAsync(new($"{TestName()}_testpage", book_id: book.id, markdown: "aaa"));
        {
            var now = DateTime.UtcNow;
            var path = TestResPath("images/pd001.png");
            var attachment = await client.CreateFileAttachmentAsync(new($"{TestName()}_aaa", page.id), path).WillBeDiscarded(container);
            attachment.name.Should().Be($"{TestName()}_aaa");
            attachment.extension.Should().Be("png");
            attachment.uploaded_to.Should().Be(page.id);
            attachment.external.Should().BeFalse();
            attachment.created_at.Should().BeCloseTo(now, 10.Seconds());
            attachment.updated_at.Should().BeCloseTo(now, 10.Seconds());
            attachment.created_by.Should().Be(book.created_by);
            attachment.updated_by.Should().Be(book.updated_by);
        }
        {
            var now = DateTime.UtcNow;
            var path = TestResPath("images/pd001.png");
            var attachment = await client.CreateFileAttachmentAsync(new($"{TestName()}_aaa", page.id), path, "aaa.test").WillBeDiscarded(container);
            attachment.name.Should().Be($"{TestName()}_aaa");
            attachment.extension.Should().Be("test");
            attachment.uploaded_to.Should().Be(page.id);
            attachment.external.Should().BeFalse();
            attachment.created_at.Should().BeCloseTo(now, 10.Seconds());
            attachment.updated_at.Should().BeCloseTo(now, 10.Seconds());
            attachment.created_by.Should().Be(book.created_by);
            attachment.updated_by.Should().Be(book.updated_by);
        }
        {
            var now = DateTime.UtcNow;
            var image = await TestResContentAsync("images/pd002.png");
            var attachment = await client.CreateFileAttachmentAsync(new($"{TestName()}_bbb", page.id), image, "image").WillBeDiscarded(container);
            attachment.name.Should().Be($"{TestName()}_bbb");
            attachment.extension.Should().BeEmpty();
            attachment.uploaded_to.Should().Be(page.id);
            attachment.external.Should().BeFalse();
            attachment.created_at.Should().BeCloseTo(now, 10.Seconds());
            attachment.updated_at.Should().BeCloseTo(now, 10.Seconds());
            attachment.created_by.Should().Be(book.created_by);
            attachment.updated_by.Should().Be(book.updated_by);
        }
        {
            var now = DateTime.UtcNow;
            var image = await TestResContentAsync("images/pd003.png");
            var attachment = await client.CreateFileAttachmentAsync(new($"{TestName()}_ccc", page.id), image, "a.txt").WillBeDiscarded(container);
            attachment.name.Should().Be($"{TestName()}_ccc");
            attachment.extension.Should().Be("txt");
            attachment.uploaded_to.Should().Be(page.id);
            attachment.external.Should().BeFalse();
            attachment.created_at.Should().BeCloseTo(now, 10.Seconds());
            attachment.updated_at.Should().BeCloseTo(now, 10.Seconds());
            attachment.created_by.Should().Be(book.created_by);
            attachment.updated_by.Should().Be(book.updated_by);
        }
    }

    [TestMethod()]
    public async Task CreateLinkAttachmentAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new($"{TestName()}_testbook")).WillBeDiscarded(container);
        var page = await client.CreatePageAsync(new($"{TestName()}_testpage", book_id: book.id, markdown: "aaa"));
        {
            var now = DateTime.UtcNow;
            var attachment = await client.CreateLinkAttachmentAsync(new($"{TestName()}_bbb", page.id, "https://www.google.com")).WillBeDiscarded(container);
            attachment.name.Should().Be($"{TestName()}_bbb");
            attachment.extension.Should().BeEmpty();
            attachment.uploaded_to.Should().Be(page.id);
            attachment.external.Should().BeTrue();
            attachment.created_at.Should().BeCloseTo(now, 10.Seconds());
            attachment.updated_at.Should().BeCloseTo(now, 10.Seconds());
            attachment.created_by.Should().Be(book.created_by);
            attachment.updated_by.Should().Be(book.updated_by);
        }
        {
            var now = DateTime.UtcNow;
            var attachment = await client.CreateLinkAttachmentAsync(new($"{TestName()}_bbb", page.id, $"{this.ApiBaseUri.GetLeftPart(UriPartial.Authority)}/logo.png")).WillBeDiscarded(container);
            attachment.name.Should().Be($"{TestName()}_bbb");
            attachment.extension.Should().BeEmpty();
            attachment.uploaded_to.Should().Be(page.id);
            attachment.external.Should().BeTrue();
            attachment.created_at.Should().BeCloseTo(now, 10.Seconds());
            attachment.updated_at.Should().BeCloseTo(now, 10.Seconds());
            attachment.created_by.Should().Be(book.created_by);
            attachment.updated_by.Should().Be(book.updated_by);
        }
    }

    [TestMethod()]
    public async Task ReadAttachmentAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new($"{TestName()}_testbook")).WillBeDiscarded(container);
        var page = await client.CreatePageAsync(new($"{TestName()}_testpage", book_id: book.id, markdown: "aaa"));
        {
            var now = DateTime.UtcNow;
            var image = await TestResContentAsync("images/pd003.png");
            var attachment = await client.CreateFileAttachmentAsync(new($"{TestName()}_aaa", page.id), image, "a.txt").WillBeDiscarded(container);
            var detail = await client.ReadAttachmentAsync(attachment.id);
            detail.name.Should().Be($"{TestName()}_aaa");
            detail.extension.Should().Be("txt");
            detail.uploaded_to.Should().Be(page.id);
            detail.external.Should().BeFalse();
            detail.created_at.Should().BeCloseTo(now, 10.Seconds());
            detail.updated_at.Should().BeCloseTo(now, 10.Seconds());
            detail.created_by.id.Should().Be(book.created_by);
            detail.updated_by.id.Should().Be(book.updated_by);
            Convert.FromBase64String(detail.content).Should().Equal(image);
            detail.links.html.Should().NotBeNullOrEmpty();
            detail.links.markdown.Should().NotBeNullOrEmpty();
        }
        {
            var now = DateTime.UtcNow;
            var url = "https://www.google.com";
            var attachment = await client.CreateLinkAttachmentAsync(new($"{TestName()}_bbb", page.id, url)).WillBeDiscarded(container);
            var detail = await client.ReadAttachmentAsync(attachment.id);
            detail.name.Should().Be($"{TestName()}_bbb");
            detail.extension.Should().BeNullOrEmpty();
            detail.uploaded_to.Should().Be(page.id);
            detail.external.Should().BeTrue();
            detail.created_at.Should().BeCloseTo(now, 10.Seconds());
            detail.updated_at.Should().BeCloseTo(now, 10.Seconds());
            detail.created_by.id.Should().Be(book.created_by);
            detail.updated_by.id.Should().Be(book.updated_by);
            detail.content.Should().Be(url);
            detail.links.html.Should().NotBeNullOrEmpty();
            detail.links.markdown.Should().NotBeNullOrEmpty();
        }

    }

    [TestMethod()]
    public async Task UpdateFileAttachmentAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new($"{TestName()}_testbook")).WillBeDiscarded(container);
        var page = await client.CreatePageAsync(new($"{TestName()}_testpage", book_id: book.id, markdown: "aaa"));
        {// update path to path
            var path1 = TestResPath("images/pd001.png");
            var created = await client.CreateFileAttachmentAsync(new($"{TestName()}_aaa", page.id), path1).WillBeDiscarded(container);
            await Task.Delay(2 * 1000);     // for update timestamp
            var path2 = TestResPath("images/pd004.jpg");
            var updated = await client.UpdateFileAttachmentAsync(created.id, new($"{TestName()}_bbb", page.id), path2);
            updated.id.Should().Be(created.id);
            updated.name.Should().Be($"{TestName()}_bbb");
            updated.extension.Should().Be("jpg");
            updated.uploaded_to.Should().Be(page.id);
            updated.external.Should().BeFalse();
            updated.created_at.Should().Be(created.created_at);
            updated.updated_at.Should().BeAfter(created.updated_at);
            updated.created_by.Should().Be(created.created_by);
            updated.updated_by.Should().Be(book.created_by);
            var detail = await client.ReadAttachmentAsync(updated.id);
            Convert.FromBase64String(detail.content).Should().Equal(await TestResContentAsync(path2));
        }
        {// update path to path with name
            var path1 = TestResPath("images/pd001.png");
            var created = await client.CreateFileAttachmentAsync(new($"{TestName()}_aaa", page.id), path1).WillBeDiscarded(container);
            var path2 = TestResPath("images/pd004.jpg");
            var updated = await client.UpdateFileAttachmentAsync(created.id, new($"{TestName()}_bbb", page.id), path2, "bbb.hoge");
            updated.id.Should().Be(created.id);
            updated.name.Should().Be($"{TestName()}_bbb");
            updated.extension.Should().Be("hoge");
            updated.uploaded_to.Should().Be(page.id);
            updated.external.Should().BeFalse();
            updated.created_at.Should().Be(created.created_at);
            updated.updated_at.Should().BeOnOrAfter(created.updated_at);
            updated.created_by.Should().Be(created.created_by);
            updated.updated_by.Should().Be(book.created_by);
            var detail = await client.ReadAttachmentAsync(updated.id);
            Convert.FromBase64String(detail.content).Should().Equal(await TestResContentAsync(path2));
        }
        {// update path to binary
            var path1 = TestResPath("images/pd001.png");
            var created = await client.CreateFileAttachmentAsync(new($"{TestName()}_ccc", page.id), path1).WillBeDiscarded(container);
            var image2 = await TestResContentAsync("images/pd004.jpg");
            var updated = await client.UpdateFileAttachmentAsync(created.id, new($"{TestName()}_ddd", page.id), image2, "abc.txt");
            updated.id.Should().Be(created.id);
            updated.name.Should().Be($"{TestName()}_ddd");
            updated.extension.Should().Be("txt");
            updated.uploaded_to.Should().Be(page.id);
            updated.external.Should().BeFalse();
            updated.created_at.Should().Be(created.created_at);
            updated.updated_at.Should().BeOnOrAfter(created.updated_at);
            updated.created_by.Should().Be(created.created_by);
            updated.updated_by.Should().Be(book.created_by);
            var detail = await client.ReadAttachmentAsync(updated.id);
            Convert.FromBase64String(detail.content).Should().Equal(image2);
        }
        {// update binary to binary
            var image1 = await TestResContentAsync("images/pd005.jpg");
            var created = await client.CreateFileAttachmentAsync(new($"{TestName()}_eee", page.id), image1, "image1.ext1").WillBeDiscarded(container);
            var image2 = await TestResContentAsync("images/pd002.png");
            var updated = await client.UpdateFileAttachmentAsync(created.id, new($"{TestName()}_fff", page.id), image2, "image2.ext2");
            updated.id.Should().Be(created.id);
            updated.name.Should().Be($"{TestName()}_fff");
            updated.extension.Should().Be("ext2");
            updated.uploaded_to.Should().Be(page.id);
            updated.external.Should().BeFalse();
            updated.created_at.Should().Be(created.created_at);
            updated.updated_at.Should().BeOnOrAfter(created.updated_at);
            updated.created_by.Should().Be(created.created_by);
            updated.updated_by.Should().Be(book.created_by);
            var detail = await client.ReadAttachmentAsync(updated.id);
            Convert.FromBase64String(detail.content).Should().Equal(image2);
        }
        {// update binary to path
            var image1 = await TestResContentAsync("images/pd005.jpg");
            var created = await client.CreateFileAttachmentAsync(new($"{TestName()}_ggg", page.id), image1, "image1.ext1").WillBeDiscarded(container);
            var path2 = TestResPath("images/pd001.png");
            var updated = await client.UpdateFileAttachmentAsync(created.id, new($"{TestName()}_hhh", page.id), path2);
            updated.id.Should().Be(created.id);
            updated.name.Should().Be($"{TestName()}_hhh");
            updated.extension.Should().Be("png");
            updated.uploaded_to.Should().Be(page.id);
            updated.external.Should().BeFalse();
            updated.created_at.Should().Be(created.created_at);
            updated.updated_at.Should().BeOnOrAfter(created.updated_at);
            updated.created_by.Should().Be(created.created_by);
            updated.updated_by.Should().Be(book.created_by);
            var detail = await client.ReadAttachmentAsync(updated.id);
            Convert.FromBase64String(detail.content).Should().Equal(await TestResContentAsync(path2));
        }
        {// update link to binary
            var url = "https://www.google.com";
            var created = await client.CreateLinkAttachmentAsync(new($"{TestName()}_iii", page.id, url)).WillBeDiscarded(container);
            var image = await TestResContentAsync("images/pd002.png");
            var updated = await client.UpdateFileAttachmentAsync(created.id, new($"{TestName()}_jjj", page.id), image, "image.ext");
            updated.id.Should().Be(created.id);
            updated.name.Should().Be($"{TestName()}_jjj");
            updated.extension.Should().Be("ext");
            updated.uploaded_to.Should().Be(page.id);
            updated.external.Should().BeFalse();
            updated.created_at.Should().Be(created.created_at);
            updated.updated_at.Should().BeOnOrAfter(created.updated_at);
            updated.created_by.Should().Be(created.created_by);
            updated.updated_by.Should().Be(book.created_by);
            var detail = await client.ReadAttachmentAsync(updated.id);
            Convert.FromBase64String(detail.content).Should().Equal(image);
        }
    }

    [TestMethod()]
    public async Task UpdateLinkAttachmentAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new($"{TestName()}_testbook")).WillBeDiscarded(container);
        var page = await client.CreatePageAsync(new($"{TestName()}_testpage", book_id: book.id, markdown: "aaa"));
        {// update link to link
            var url1 = "https://server1.home";
            var created = await client.CreateLinkAttachmentAsync(new($"{TestName()}_aaa", page.id, url1)).WillBeDiscarded(container);
            await Task.Delay(2 * 1000);     // for update timestamp
            var url2 = "https://server2.home";
            var updated = await client.UpdateLinkAttachmentAsync(created.id, new($"{TestName()}_bbb", page.id, url2));
            updated.id.Should().Be(created.id);
            updated.name.Should().Be($"{TestName()}_bbb");
            updated.extension.Should().BeNullOrEmpty();
            updated.uploaded_to.Should().Be(page.id);
            updated.external.Should().BeTrue();
            updated.created_at.Should().Be(created.created_at);
            updated.updated_at.Should().BeAfter(created.updated_at);
            updated.created_by.Should().Be(created.created_by);
            updated.updated_by.Should().Be(book.created_by);
            var detail = await client.ReadAttachmentAsync(updated.id);
            detail.content.Should().Be(url2);
        }
        {// update binary to link
            var image = await TestResContentAsync("images/pd005.jpg");
            var created = await client.CreateFileAttachmentAsync(new($"{TestName()}_ccc", page.id), image, "image.ext").WillBeDiscarded(container);
            var url = "https://server3.home";
            var updated = await client.UpdateLinkAttachmentAsync(created.id, new($"{TestName()}_ddd", page.id, url));
            updated.id.Should().Be(created.id);
            updated.name.Should().Be($"{TestName()}_ddd");
            updated.extension.Should().BeNullOrEmpty();
            updated.uploaded_to.Should().Be(page.id);
            updated.external.Should().BeTrue();
            updated.created_at.Should().Be(created.created_at);
            updated.updated_at.Should().BeOnOrAfter(created.updated_at);
            updated.created_by.Should().Be(created.created_by);
            updated.updated_by.Should().Be(book.created_by);
            var detail = await client.ReadAttachmentAsync(updated.id);
            detail.content.Should().Be(url);
        }
    }

    [TestMethod()]
    public async Task DeleteAttachmentAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new($"{TestName()}_testbook")).WillBeDiscarded(container);
        var page = await client.CreatePageAsync(new($"{TestName()}_testpage", book_id: book.id, markdown: "aaa"));
        {// binary
            var name = $"{TestName()}_file_{Guid.NewGuid()}";
            var image = await TestResContentAsync("images/pd002.png");
            var attachment = await client.CreateFileAttachmentAsync(new(name, page.id), image, "img.png");
            (await client.ListAttachmentsAsync(new(filters: [new("name", name)]))).data.Should().Contain(d => d.id == attachment.id);
            await client.DeleteAttachmentAsync(attachment.id);
            (await client.ListAttachmentsAsync(new(filters: [new("name", name)]))).data.Should().NotContain(d => d.id == attachment.id);
        }
        {// link
            var name = $"{TestName()}_file_{Guid.NewGuid()}";
            var url = "https://server.home";
            var attachment = await client.CreateLinkAttachmentAsync(new(name, page.id, url));
            (await client.ListAttachmentsAsync(new(filters: [new("name", name)]))).data.Should().Contain(d => d.id == attachment.id);
            await client.DeleteAttachmentAsync(attachment.id);
            (await client.ListAttachmentsAsync(new(filters: [new("name", name)]))).data.Should().NotContain(d => d.id == attachment.id);
        }
    }
    #endregion

}
