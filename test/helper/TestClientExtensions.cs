using Lestaly;

namespace BookStackApiClient.Tests.helper;

public static class TestClientExtensions
{
    public static async ValueTask<List<BookSummary>> ListAllBooksAsync(this BookStackClient self, IReadOnlyList<Filter>? filters = default, CancellationToken cancelToken = default)
    {
        var items = new List<BookSummary>();
        var offset = 0;
        while (true)
        {
            var books = await self.ListBooksAsync(new(offset, count: 500, filters: filters), cancelToken);
            items.AddRange(books.data);

            offset += books.data.Length;
            var finished = (books.data.Length <= 0) || (books.total <= offset);
            if (finished) break;
        }
        return items;
    }

    public static async ValueTask<List<RecycleItem>> ListAllRecycleBinAsync(this BookStackClient self, IReadOnlyList<Filter>? filters = default, CancellationToken cancelToken = default)
    {
        var items = new List<RecycleItem>();
        var offset = 0;
        while (true)
        {
            var bins = await self.ListRecycleBinAsync(new(offset, count: 500, filters: filters), cancelToken);
            items.AddRange(bins.data);

            offset += bins.data.Length;
            var finished = (bins.data.Length <= 0) || (bins.total <= offset);
            if (finished) break;
        }
        return items;
    }

    public static async ValueTask<List<AuditLogItem>> ListAllAuditLogAsync(this BookStackClient self, IReadOnlyList<Filter>? filters = default, CancellationToken cancelToken = default)
    {
        var items = new List<AuditLogItem>();
        var offset = 0;
        while (true)
        {
            var logs = await self.ListAuditLogAsync(new(offset, count: 500, filters: filters), cancelToken);
            items.AddRange(logs.data);

            offset += logs.data.Length;
            var finished = (logs.data.Length <= 0) || (logs.total <= offset);
            if (finished) break;
        }
        return items;
    }

    public static async ValueTask<FileInfo> WriteToFileAsync(this Task<DownloadResult> self, FileInfo file, CancellationToken cancelToken = default)
    {
        using var download = await self;
        return await download.Stream.WriteToFileAsync(file, cancelToken);
    }
}
