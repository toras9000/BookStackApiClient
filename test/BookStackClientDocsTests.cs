namespace BookStackApiClient.Tests;

[TestClass()]
public class BookStackClientDocsTests : BookStackClientTestsBase
{
    #region docs
    [TestMethod()]
    public async Task DocsAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        var result = await client.DocsAsync();
        result.Should().ContainKeys(new[]
        {
            "docs",
            "attachments",
            "books",
            "chapters",
            "pages",
            "shelves",
            "users",
        });
    }
    #endregion
}
