# BookStackApiClient

[![NugetShield]][NugetPackage]

[NugetPackage]: https://www.nuget.org/packages/BookStackApiClient
[NugetShield]: https://img.shields.io/nuget/v/BookStackApiClient

This is [BookStack](https://www.bookstackapp.com/) API client library for .NET. (unofficial)  
BookStack is a platform for organizing and storing information.


Since this library is a relatively simple mapper for the BookStack API, it should be easy to tie the endpoints and methods described in the BookStack [API documentation](https://demo.bookstackapp.com/api/docs).  
Sorry, IntelliSense messages (documentation comments) for types and members are provided in Japanese. This is because I currently think that the main users are me and the people around me.  

## Package and API version 

Although the BookStack API specification may change from version to version, this library targets only a single version.  
If the version targeted by the library does not match the server version, there is a large possibility that it will not work properly.  
The server and client versions must be combined correctly.  

Package versions are in semantic versioning format, but are numbered according to the following arrangement.  
The version number of this package always uses the pre-release versioning format.   
The core version part represents the version of the target server.  
The version (lib.XX) portion of the pre-release is used to indicate the version of the library, not as a pre-release.  
Therefore, differences in pre-release version numbers are not necessarily trivial changes.  

## Examples

Some samples are shown below.  
These use C#12 or later syntax.  

### Create books, chapters, and pages. and attach file

```csharp
var apiEntry = new Uri(@"http://<your-hosting-server>/api/");
var apiToken = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
var apiSecret = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
using var client = new BookStackClient(apiEntry, apiToken, apiSecret);
var cover = "path/to/image.png";
var book = await client.CreateBookAsync(new("TestBook", tags: [new("test"),]), imgPath: cover);
var chapter = await client.CreateChapterAsync(new(book.id, "TestChapter"));
var page1 = await client.CreateMarkdownPageInBookAsync(new(book.id, "TestPage", "# Test page in book"));
var page2 = await client.CreateMarkdownPageInChapterAsync(new(chapter.id, "TestPage", "# Test page in chapter"));

var filePath = "path/to/file";
var attach1 = await client.CreateFileAttachmentAsync(new("attach from path", page1.id), filePath);

var contents = new byte[] { xxxx };
var attach2 = await client.CreateFileAttachmentAsync(new("attach from binary", page1.id), contents, "test.bin");
```

### Display a list of books

Note the limit on the number of API requests to issue many requests.  

```csharp
using var client = new BookStackClient(apiEntry, apiToken, apiSecret);
try
{
    var offset = 0;
    while (true)
    {
        var books = await client.ListBooksAsync(new(offset, sorts: ["id",]));
        foreach (var book in books.data)
        {
            var detail = await client.ReadBookAsync(book.id);
            var chapters = detail.contents.OfType<BookContentChapter>().Count();
            var pages = detail.contents.OfType<BookContentPage>().Count();
            Console.WriteLine($"{book.id,4}: {book.name}, chapters={chapters}, pages={pages}");
        }

        offset += books.data.Length;
        if (books.data.Length <= 0 || books.total <= offset) break;
    }
}
catch (ApiLimitResponseException ex)
{
    Console.WriteLine($"Api Limit: Limit={ex.RequestsPerMin}, RetryAfter={ex.RetryAfter}");
}
```

### Upload image gallery

```csharp
using var client = new BookStackClient(apiEntry, apiToken, apiSecret);
var book = await client.CreateBookAsync(new("book"));
var page = await client.CreateMarkdownPageInBookAsync(new(book.id, "page", "body"));

var filePath = "path/to/image.png";
await client.CreateImageAsync(new(page.id, "gallery", "image1"), filePath, "upload.png");

var image = await File.ReadAllBytesAsync(@"path/to/image.jpg");
await client.CreateImageAsync(new(page.id, "gallery", "image2"), image, "upload.jpg");
```

### Content Search

```csharp
using var client = new BookStackClient(apiEntry, apiToken, apiSecret);

// search query (see https://www.bookstackapp.com/docs/user/searching/)
var found = await client.SearchAsync(new("search query"));

// list of pages (see https://demo.bookstackapp.com/api/docs#listing-endpoints)
var pages = await client.ListPagesAsync(new(filters: [new("filter", "expression"),]));
```

### Helper class

```csharp
using var client = new BookStackClient(apiEntry, apiToken, apiSecret);
using var helper = new BookStackClientHelper(client);

// Handling when API limits are reached.
helper.LimitHandler += async args => await Task.Delay(TimeSpan.FromSeconds(args.Exception.RetryAfter));

// Be prepared to stop halfway if necessary.
using var timeout = new CancellationTokenSource();
timeout.CancelAfter(TimeSpan.FromMinutes(30));

// List everything. If the limit is reached, the handler is called.
await foreach (var book in helper.EnumerateAllBooksAsync(timeout.Token))
{
    Console.WriteLine($"{book.id}: {book.name}");
}
```

