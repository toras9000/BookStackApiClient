# BookStackApiClient

[![NugetShield]][NugetPackage]

[NugetPackage]: https://www.nuget.org/packages/BookStackApiClient
[NugetShield]: https://img.shields.io/nuget/v/BookStackApiClient

This is [BookStack](https://www.bookstackapp.com/) API client library for .NET.  
BookStack is a platform for organizing and storing information.


Since this library is a relatively simple mapper for the BookStack API, it should be easy to tie the endpoints and methods described in the BookStack [API documentation](https://demo.bookstackapp.com/api/docs).  
Sorry, IntelliSense messages (documentation comments) for types and members are provided in Japanese. This is because I currently think that the main users are me and the people around me.  

## Package and API version 

Although the BookStack API specification may change from version to version, this library targets only a single version.  
If the version targeted by the library does not match the server version, there is a large possibility that it will not work properly.  

The package version represents the corresponding server version.  
The first two parts of the version match the first two parts of the target BookStack version.  
More detailed versions are considered to have no effect on the API specification.  

The third and fourth numbers in the version are specific to this library.  
The third number will be changed if the library specification has been changed (not binary compatible), even for the same target version.  
The fourth number is changed for bug fixes and other cases where binary compatibility is maintained.  

## Examples

Some samples are shown below.  
These use C#9 or later syntax.  

### Create books, chapters, and pages.

```csharp
var apiEntry = new Uri(@"http://<your-hosting-server>/api/"),
var apiToken  = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
var apiSecret = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
using var client = new BookStackClient(apiEntry, apiToken, apiSecret);
var book = await client.CreateBookAsync(new("TestBook", tags: new Tag[] { new("test") }));
var chapter = await client.CreateChapterAsync(new(book.id, "TestChapter"));
var page1 = await client.CreateMarkdownPageInBookAsync(new(book.id, "TestPage", "# Test page in book"));
var page2 = await client.CreateMarkdownPageInChapterAsync(new(chapter.id, "TestPage", "# Test page in chapter"));
```

### Display a list of books.

Note the limit on the number of API requests to issue many requests.  

```csharp
using var client = new BookStackClient(apiEntry, apiToken, apiSecret);
var offset = 0;
while (true)
{
    var books = await client.ListBooksAsync(new(offset, sorts: new[] { "id", }));
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
```

### Content Search

```csharp
using var client = new BookStackClient(apiEntry, apiToken, apiSecret);

// search query (see https://www.bookstackapp.com/docs/user/searching/)
var found = await client.SearchAsync(new("search query"));

// list of pages (see https://demo.bookstackapp.com/api/docs#listing-endpoints)
var pages = await client.ListPagesAsync(new(filters: new[] { new Filter("filter", "expression") }));
```