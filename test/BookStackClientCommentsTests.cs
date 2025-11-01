namespace BookStackApiClient.Tests;

[TestClass()]
public class BookStackClientCommentsTests : BookStackClientTestsBase
{
    #region comments
    [TestMethod()]
    public async Task ListCommentsAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
        var page = await client.CreateMarkdownPageInBookAsync(new(book.id, testName($"page_{Guid.NewGuid()}"), "- aaa", priority: 4, tags: [new("tp1", "vp1"), new("tp2", "vp2"),]));

        var comment1 = await client.CreateCommentAsync(new(page.id, "<p>aaa</p>", content_ref: "")).WillBeDiscarded(container);
        var comment2 = await client.CreateCommentAsync(new(page.id, "<p>bbb</p>", content_ref: "", reply_to: comment1.id)).WillBeDiscarded(container);

        var comments = await client.ListCommentsAsync();
        foreach (var created in container.Comments)
        {
            var actual = comments.data.Should().Contain(i => i.id == created.id).Subject;
            var expect = created;
            actual.Should().BeEquivalentTo(expect, o => o.ExcludingMissingMembers());
        }
    }

    [TestMethod()]
    public async Task CreateCommentAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
        var page = await client.CreateMarkdownPageInBookAsync(new(book.id, testName($"page_{Guid.NewGuid()}"), "- aaa", priority: 4, tags: [new("tp1", "vp1"), new("tp2", "vp2"),]));

        var now = DateTime.UtcNow;
        var comment1 = await client.CreateCommentAsync(new(page.id, "<p>aaa</p>", content_ref: "")).WillBeDiscarded(container);
        comment1.commentable_id.Should().Be(page.id);
        comment1.commentable_type.Should().Be("page");
        comment1.created_at.Should().BeCloseTo(now, 10.Seconds());
        comment1.updated_at.Should().BeCloseTo(now, 10.Seconds());
        comment1.created_by.Should().Be(page.owned_by.id);
        comment1.updated_by.Should().Be(page.owned_by.id);

        now = DateTime.UtcNow;
        var comment2 = await client.CreateCommentAsync(new(page.id, "<p>bbb</p>", content_ref: "bkmrk-page-title:0:0-1", reply_to: comment1.local_id)).WillBeDiscarded(container);
        comment2.parent_id.Should().Be(comment1.local_id);
        comment2.commentable_id.Should().Be(page.id);
        comment2.commentable_type.Should().Be("page");
        comment2.content_ref.Should().Be("bkmrk-page-title:0:0-1");
        comment2.created_at.Should().BeCloseTo(now, 10.Seconds());
        comment2.updated_at.Should().BeCloseTo(now, 10.Seconds());
        comment2.created_by.Should().Be(page.owned_by.id);
        comment2.updated_by.Should().Be(page.owned_by.id);
    }

    [TestMethod()]
    public async Task ReadCommentAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
        var page = await client.CreateMarkdownPageInBookAsync(new(book.id, testName($"page_{Guid.NewGuid()}"), "- aaa", priority: 4, tags: [new("tp1", "vp1"), new("tp2", "vp2"),]));

        var now = DateTime.UtcNow;
        var created = await client.CreateCommentAsync(new(page.id, "<p>aaa</p>", content_ref: "")).WillBeDiscarded(container);
        created.commentable_id.Should().Be(page.id);
        created.commentable_type.Should().Be("page");
        created.created_at.Should().BeCloseTo(now, 10.Seconds());
        created.updated_at.Should().BeCloseTo(now, 10.Seconds());
        created.created_by.Should().Be(page.owned_by.id);
        created.updated_by.Should().Be(page.owned_by.id);

        var comment = await client.ReadCommentAsync(created.id);
        comment.commentable_id.Should().Be(page.id);
        comment.commentable_type.Should().Be("page");
        comment.html.Should().Be("<p>aaa</p>");
        comment.created_at.Should().BeCloseTo(now, 10.Seconds());
        comment.updated_at.Should().BeCloseTo(now, 10.Seconds());
        comment.created_by.id.Should().Be(page.owned_by.id);
        comment.updated_by?.id.Should().Be(page.owned_by.id);

    }

    [TestMethod()]
    public async Task UpdateCommentAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
        var page = await client.CreateMarkdownPageInBookAsync(new(book.id, testName($"page_{Guid.NewGuid()}"), "- aaa", priority: 4, tags: [new("tp1", "vp1"), new("tp2", "vp2"),]));

        var now = DateTime.UtcNow;
        var created = await client.CreateCommentAsync(new(page.id, "<p>aaa</p>", content_ref: "")).WillBeDiscarded(container);
        var updated = await client.UpdateCommentAsync(created.id, new(html: "<p>xxx</p>", archived: false));
        updated.created_by.Should().Be(page.owned_by.id);
        updated.updated_by.Should().Be(page.owned_by.id);

        var comment = await client.ReadCommentAsync(created.id);
        comment.commentable_id.Should().Be(page.id);
        comment.commentable_type.Should().Be("page");
        comment.html.Should().Be("<p>xxx</p>");
        comment.created_at.Should().BeCloseTo(now, 10.Seconds());
        comment.updated_at.Should().BeCloseTo(now, 10.Seconds());
        comment.created_by.id.Should().Be(page.owned_by.id);
        comment.updated_by?.id.Should().Be(page.owned_by.id);

    }

    [TestMethod()]
    public async Task DeleteCommentAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
        var page = await client.CreateMarkdownPageInBookAsync(new(book.id, testName($"page_{Guid.NewGuid()}"), "- aaa", priority: 4, tags: [new("tp1", "vp1"), new("tp2", "vp2"),]));

        var comment = await client.CreateCommentAsync(new(page.id, "<p>aaa</p>", content_ref: ""));

        await client.DeleteCommentAsync(comment.id);

        var comments = await client.ListCommentsAsync();
        comments.data.Where(c => c.id == comment.id).Should().BeEmpty();

    }

    [TestMethod()]
    public async Task PageCommentsAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
        var page = await client.CreateMarkdownPageInBookAsync(new(book.id, testName($"page_{Guid.NewGuid()}"), "- aaa", priority: 4, tags: [new("tp1", "vp1"), new("tp2", "vp2"),]));

        var comment1 = await client.CreateCommentAsync(new(page.id, "<p>aaa</p>", content_ref: "")).WillBeDiscarded(container);
        var comment2 = await client.CreateCommentAsync(new(page.id, "<p>bbb</p>", content_ref: "", reply_to: comment1.id)).WillBeDiscarded(container);

        var pageDetail = await client.ReadPageAsync(page.id);
        pageDetail.comments.Should().NotBeNull();
        pageDetail.comments.active[0].comment.id.Should().Be(comment1.id);
        pageDetail.comments.active[0].comment.html.Should().Be("<p>aaa</p>");

    }
    #endregion
}
