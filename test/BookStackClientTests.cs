namespace BookStackApiClient.Tests;

[TestClass()]
public class BookStackClientTests : BookStackClientTestsBase
{
    #region 状態確認
    [TestMethod()]
    public async Task EntitiesAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        var attachments = await client.ListAttachmentsAsync();
        var books = await client.ListBooksAsync();
        var chapters = await client.ListChaptersAsync();
        var pages = await client.ListPagesAsync();
        var images = await client.ListImagesAsync();
        var users = await client.ListUsersAsync();

        // エンティティが多すぎると1回の検索APIで取得できなかったりするので、
        // テスト安定性のためにテスト内で作成したエンティティを削除しているはずだが、
        // 削除漏れなどがあった場合に検出できるようにチェックをしている。
        attachments.data.Should().BeEmpty();
        books.data.Should().BeEmpty();
        chapters.data.Should().BeEmpty();
        pages.data.Should().BeEmpty();
        images.data.Should().BeEmpty();
        users.data.Where(u => u.name.ToLowerInvariant() is (not "admin") and (not "guest")).Should().BeEmpty();
    }
    #endregion

    #region クリーンナップ
    [TestMethod()]
    public async Task CleanupRecycleBinAsync()
    {
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // テストを繰り返すとゴミ箱に削除アイテムが溜まるのでクリーンナップしておく。
        // ゴミ箱APIのテストでは多数アイテムがあっても通るようにテストを作成しているが、
        // あまり数が多すぎるとパフォーマンスが下がるので、これを用意している。

        while (true)
        {
            var items = await client.ListRecycleBinAsync();
            if (items.data.Length <= 0) break;

            foreach (var item in items.data)
            {
                await client.DestroyRecycleItemAsync(item.id);
            }
        }
    }
    #endregion
}
