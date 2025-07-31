using Lestaly;

namespace BookStackApiClient.Tests;

[TestClass()]
public class BookStackClientImportsTests : BookStackClientTestsBase
{
    #region imports
    [TestMethod()]
    public async Task ImportsScenario()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var book = await client.CreateBookAsync(new(testName("testbook"), "book-desc", tags: [new("book-tag", "book-tag-val")])).WillBeDiscarded(container);
        var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter"), "chapt-desc", tags: [new("chapt-tag", "chapt-tag-val")])).WillBeDiscarded(container);
        var cpage = await client.CreatePageAsync(new(testName("test-cpage"), book_id: book.id, chapter.id, markdown: "md-cpage", tags: [new("cpage-tag", "cpage-tag-val")]));
        var cimage = await client.CreateImageAsync(new(cpage.id, "gallery", testName("image-cpage")), testResPath("images/pd001.png")).WillBeDiscarded(container);
        var cattach = await client.CreateFileAttachmentAsync(new(testName("attach-cpage"), cpage.id), testResPath("images/pd002.png")).WillBeDiscarded(container);
        cpage = await client.UpdatePageAsync(cpage.id, new(markdown: $"[![cimage]({cimage.url})"));
        var bpage = await client.CreatePageAsync(new(testName("test-bpage"), book_id: book.id, markdown: "md-bpage", tags: [new("bpage-tag", "bpage-tag-val")]));
        var bimage = await client.CreateImageAsync(new(bpage.id, "gallery", testName("image-bpage")), testResPath("images/pd003.png")).WillBeDiscarded(container);
        var battach = await client.CreateFileAttachmentAsync(new(testName("attach-bpage"), bpage.id), testResPath("images/pd004.jpg")).WillBeDiscarded(container);
        bpage = await client.UpdatePageAsync(bpage.id, new(markdown: $"[![bimage]({bimage.url})"));

        // temp dir
        using var tempDir = new TempDir();

        // export
        var bookExportFile = await client.ExportBookZipAsync(book.id).WriteToFileAsync(tempDir.Info.RelativeFile("book-export.zip"));
        var chapterExportFile = await client.ExportChapterZipAsync(chapter.id).WriteToFileAsync(tempDir.Info.RelativeFile("chapter-export.zip"));
        var cpageExportFile = await client.ExportPageZipAsync(cpage.id).WriteToFileAsync(tempDir.Info.RelativeFile("cpage-export.zip"));

        // import
        var bookImports = await client.CreateImportsAsync(bookExportFile.FullName);
        var chapterImports = await client.CreateImportsAsync(chapterExportFile.FullName);
        var cpageImports = await client.CreateImportsAsync(cpageExportFile.FullName);
        bookImports.name.Should().Be(book.name);
        chapterImports.name.Should().Be(chapter.name);
        cpageImports.name.Should().Be(cpage.name);

        // details
        var bookImportDetails = await client.ReadImportsAsync(bookImports.id);
        var chapterImportDetails = await client.ReadImportsAsync(chapterImports.id);
        var cpageImportDetails = await client.ReadImportsAsync(cpageImports.id);
        bookImportDetails.name.Should().Be(book.name);
        chapterImportDetails.name.Should().Be(chapter.name);
        cpageImportDetails.name.Should().Be(cpage.name);

        // list
        var imports = await client.ListImportsAsync();
        imports.total.Should().BeGreaterThanOrEqualTo(3);
        imports.data.Should().HaveCountGreaterThanOrEqualTo(3);

        // delete
        await client.DeleteImportsAsync(bookImports.id);
        await client.DeleteImportsAsync(chapterImports.id);
        await client.DeleteImportsAsync(cpageImports.id);

    }

    [TestMethod()]
    public async Task TryExport()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);
        await using var container = new TestResourceContainer(client);

        // get exists book
        var result = await client.SearchAsync(new("TestBook"));
        var book = result.data.FirstOrDefault();
        if (book == null) return;
        var bookDetails = await client.ReadBookAsync(book.id);

        // temp dir
        using var tempDir = new TempDir();

        // book
        if (book is not null)
        {
            var exportFile = await client.ExportBookZipAsync(book.id).WriteToFileAsync(tempDir.Info.RelativeFile("book-export.zip"));
            var imports = await client.CreateImportsAsync(exportFile.FullName).WillBeDiscarded(container);
            var details = await client.ReadImportsAsync(imports.id);
        }

        // chapter
        if (bookDetails.chapters().FirstOrDefault() is var chapter && chapter is not null)
        {
            var exportFile = await client.ExportChapterZipAsync(chapter.id).WriteToFileAsync(tempDir.Info.RelativeFile("chapter-export.zip"));
            var imports = await client.CreateImportsAsync(exportFile.FullName).WillBeDiscarded(container);
            var details = await client.ReadImportsAsync(imports.id);
        }

        // page
        if (bookDetails.pages().FirstOrDefault() is var page && page is not null)
        {
            var exportFile = await client.ExportPageZipAsync(page.id).WriteToFileAsync(tempDir.Info.RelativeFile("page-export.zip"));
            var imports = await client.CreateImportsAsync(exportFile.FullName).WillBeDiscarded(container);
            var details = await client.ReadImportsAsync(imports.id);
        }


    }
    #endregion

}