namespace BookStackApiClient.Tests;

[TestClass()]
public class BookStackClientContentPermissionsTests : BookStackClientTestsBase
{
    #region content-permissions
    [TestMethod()]
    public async Task ReadShelfPermissionsAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {
            var guid = Guid.NewGuid().ToString();
            var shelf = await client.CreateShelfAsync(new(testName("testshelf"))).WillBeDiscarded(container);

            var before = await client.ReadShelfPermissionsAsync(shelf.id);
            before.owner.id.Should().Be(shelf.owned_by);
            before.role_permissions.Should().BeNullOrEmpty();
            before.fallback_permissions.inheriting.Should().BeTrue();

            var user = await client.CreateUserAsync(new(testName($"user_{guid}"), $"user_{guid}@example.com")).WillBeDiscarded(container);
            var role1 = await client.CreateRoleAsync(new(testName($"role1_{guid}"))).WillBeDiscarded(container);
            var role2 = await client.CreateRoleAsync(new(testName($"role2_{guid}"))).WillBeDiscarded(container);
            var newperms = new RolePermission[]
            {
                new(role1.id, false, true,  true,  false),
                new(role2.id, true,  false, false, true),
            };
            await client.UpdateShelfPermissionsAsync(shelf.id, new(user.id, newperms));

            var after = await client.ReadShelfPermissionsAsync(shelf.id);
            after.owner.id.Should().Be(user.id);
            after.role_permissions.Should().BeEquivalentTo(newperms);
            after.fallback_permissions.inheriting.Should().BeTrue();

            var newfallback = FallbackPermission.Appoint(false, false, true, true);
            await client.UpdateShelfPermissionsAsync(shelf.id, new(fallback_permissions: newfallback));

            var after2 = await client.ReadShelfPermissionsAsync(shelf.id);
            after2.owner.id.Should().Be(user.id);
            after2.role_permissions.Should().BeEquivalentTo(newperms);
            after2.fallback_permissions.Should().Be(newfallback);
        }
    }

    [TestMethod()]
    public async Task ReadBookPermissionsAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {
            var guid = Guid.NewGuid().ToString();
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);

            var before = await client.ReadBookPermissionsAsync(book.id);
            before.owner.id.Should().Be(book.owned_by);
            before.role_permissions.Should().BeNullOrEmpty();
            before.fallback_permissions.inheriting.Should().BeTrue();

            var user = await client.CreateUserAsync(new(testName($"user_{guid}"), $"user_{guid}@example.com")).WillBeDiscarded(container);
            var role1 = await client.CreateRoleAsync(new(testName($"role1_{guid}"))).WillBeDiscarded(container);
            var role2 = await client.CreateRoleAsync(new(testName($"role2_{guid}"))).WillBeDiscarded(container);
            var newperms = new RolePermission[]
            {
                new(role1.id, false, true,  true,  false),
                new(role2.id, true,  false, false, true),
            };
            await client.UpdateBookPermissionsAsync(book.id, new(user.id, newperms));

            var after = await client.ReadBookPermissionsAsync(book.id);
            after.owner.id.Should().Be(user.id);
            after.role_permissions.Should().BeEquivalentTo(newperms);
            after.fallback_permissions.inheriting.Should().BeTrue();

            var newfallback = FallbackPermission.Appoint(false, false, true, true);
            await client.UpdateBookPermissionsAsync(book.id, new(fallback_permissions: newfallback));

            var after2 = await client.ReadBookPermissionsAsync(book.id);
            after2.owner.id.Should().Be(user.id);
            after2.role_permissions.Should().BeEquivalentTo(newperms);
            after2.fallback_permissions.Should().Be(newfallback);
        }
    }

    [TestMethod()]
    public async Task ReadChapterPermissionsAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {
            var guid = Guid.NewGuid().ToString();
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter"))).WillBeDiscarded(container);

            var before = await client.ReadChapterPermissionsAsync(chapter.id);
            before.owner.id.Should().Be(chapter.owned_by);
            before.role_permissions.Should().BeNullOrEmpty();
            before.fallback_permissions.inheriting.Should().BeTrue();

            var user = await client.CreateUserAsync(new(testName($"user_{guid}"), $"user_{guid}@example.com")).WillBeDiscarded(container);
            var role1 = await client.CreateRoleAsync(new(testName($"role1_{guid}"))).WillBeDiscarded(container);
            var role2 = await client.CreateRoleAsync(new(testName($"role2_{guid}"))).WillBeDiscarded(container);
            var newperms = new RolePermission[]
            {
                new(role1.id, false, true,  true,  false),
                new(role2.id, true,  false, false, true),
            };
            await client.UpdateChapterPermissionsAsync(chapter.id, new(user.id, newperms));

            var after = await client.ReadChapterPermissionsAsync(chapter.id);
            after.owner.id.Should().Be(user.id);
            after.role_permissions.Should().BeEquivalentTo(newperms);
            after.fallback_permissions.inheriting.Should().BeTrue();

            var newfallback = FallbackPermission.Appoint(false, false, true, true);
            await client.UpdateChapterPermissionsAsync(chapter.id, new(fallback_permissions: newfallback));

            var after2 = await client.ReadChapterPermissionsAsync(chapter.id);
            after2.owner.id.Should().Be(user.id);
            after2.role_permissions.Should().BeEquivalentTo(newperms);
            after2.fallback_permissions.Should().Be(newfallback);
        }
    }

    [TestMethod()]
    public async Task ReadPagePermissionsAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {
            var guid = Guid.NewGuid().ToString();
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreateMarkdownPageInBookAsync(new(book.id, testName("testpage"), markdown: "mdmd")).WillBeDiscarded(container);

            var before = await client.ReadPagePermissionsAsync(page.id);
            before.owner.id.Should().Be(page.owned_by.id);
            before.role_permissions.Should().BeNullOrEmpty();
            before.fallback_permissions.inheriting.Should().BeTrue();

            var user = await client.CreateUserAsync(new(testName($"user_{guid}"), $"user_{guid}@example.com")).WillBeDiscarded(container);
            var role1 = await client.CreateRoleAsync(new(testName($"role1_{guid}"))).WillBeDiscarded(container);
            var role2 = await client.CreateRoleAsync(new(testName($"role2_{guid}"))).WillBeDiscarded(container);
            var newperms = new RolePermission[]
            {
                new(role1.id, false, true,  true,  false),
                new(role2.id, true,  false, false, true),
            };
            await client.UpdatePagePermissionsAsync(page.id, new(user.id, newperms));

            var after = await client.ReadPagePermissionsAsync(page.id);
            after.owner.id.Should().Be(user.id);
            after.role_permissions.Should().BeEquivalentTo(newperms);
            after.fallback_permissions.inheriting.Should().BeTrue();

            var newfallback = FallbackPermission.Appoint(false, false, true, true);
            await client.UpdatePagePermissionsAsync(page.id, new(fallback_permissions: newfallback));

            var after2 = await client.ReadPagePermissionsAsync(page.id);
            after2.owner.id.Should().Be(user.id);
            after2.role_permissions.Should().BeEquivalentTo(newperms);
            after2.fallback_permissions.Should().Be(newfallback);
        }
    }

    [TestMethod()]
    public async Task UpdateShelfPermissionsAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {// owner_id
            var guid = Guid.NewGuid().ToString();
            var shelf = await client.CreateShelfAsync(new(testName("testshelf"))).WillBeDiscarded(container);
            var user = await client.CreateUserAsync(new(testName($"user_{guid}"), $"user_{guid}@example.com")).WillBeDiscarded(container);

            var before = await client.ReadShelfPermissionsAsync(shelf.id);
            before.owner.id.Should().Be(shelf.owned_by);
            before.role_permissions.Should().BeNullOrEmpty();
            before.fallback_permissions.inheriting.Should().BeTrue();

            var after = await client.UpdateShelfPermissionsAsync(shelf.id, new(owner_id: user.id));
            after.owner.id.Should().Be(user.id);
            after.role_permissions.Should().BeNullOrEmpty();
            after.fallback_permissions.inheriting.Should().BeTrue();
        }
        {// role_permissions
            var guid = Guid.NewGuid().ToString();
            var shelf = await client.CreateShelfAsync(new(testName("testshelf"))).WillBeDiscarded(container);
            var user = await client.CreateUserAsync(new(testName($"user_{guid}"), $"user_{guid}@example.com")).WillBeDiscarded(container);
            var role1 = await client.CreateRoleAsync(new(testName($"role1_{guid}"))).WillBeDiscarded(container);
            var role2 = await client.CreateRoleAsync(new(testName($"role2_{guid}"))).WillBeDiscarded(container);

            var before = await client.ReadShelfPermissionsAsync(shelf.id);
            before.owner.id.Should().Be(shelf.owned_by);
            before.role_permissions.Should().BeNullOrEmpty();
            before.fallback_permissions.inheriting.Should().BeTrue();

            var newperms = new RolePermission[]
            {
                new(role1.id, true, false, true,  false),
                new(role2.id, true, true,  false, false),
            };

            var after = await client.UpdateShelfPermissionsAsync(shelf.id, new(role_permissions: newperms));
            after.owner.id.Should().Be(shelf.owned_by);
            after.role_permissions.Should().BeEquivalentTo(newperms);
            after.fallback_permissions.inheriting.Should().BeTrue();
        }
        {// role_permissions
            var guid = Guid.NewGuid().ToString();
            var shelf = await client.CreateShelfAsync(new(testName("testshelf"))).WillBeDiscarded(container);
            var user = await client.CreateUserAsync(new(testName($"user_{guid}"), $"user_{guid}@example.com")).WillBeDiscarded(container);

            var before = await client.ReadShelfPermissionsAsync(shelf.id);
            before.owner.id.Should().Be(shelf.owned_by);
            before.role_permissions.Should().BeNullOrEmpty();
            before.fallback_permissions.inheriting.Should().BeTrue();

            var newfallback = FallbackPermission.Appoint(true, true, false, false);
            var after = await client.UpdateShelfPermissionsAsync(shelf.id, new(fallback_permissions: newfallback));
            after.owner.id.Should().Be(shelf.owned_by);
            after.role_permissions.Should().BeNullOrEmpty();
            after.fallback_permissions.Should().Be(newfallback);
        }
    }

    [TestMethod()]
    public async Task UpdateBookPermissionsAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {// owner_id
            var guid = Guid.NewGuid().ToString();
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var user = await client.CreateUserAsync(new(testName($"user_{guid}"), $"user_{guid}@example.com")).WillBeDiscarded(container);

            var before = await client.ReadBookPermissionsAsync(book.id);
            before.owner.id.Should().Be(book.owned_by);
            before.role_permissions.Should().BeNullOrEmpty();
            before.fallback_permissions.inheriting.Should().BeTrue();

            var after = await client.UpdateBookPermissionsAsync(book.id, new(owner_id: user.id));
            after.owner.id.Should().Be(user.id);
            after.role_permissions.Should().BeNullOrEmpty();
            after.fallback_permissions.inheriting.Should().BeTrue();
        }
        {// role_permissions
            var guid = Guid.NewGuid().ToString();
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var user = await client.CreateUserAsync(new(testName($"user_{guid}"), $"user_{guid}@example.com")).WillBeDiscarded(container);
            var role1 = await client.CreateRoleAsync(new(testName($"role1_{guid}"))).WillBeDiscarded(container);
            var role2 = await client.CreateRoleAsync(new(testName($"role2_{guid}"))).WillBeDiscarded(container);

            var before = await client.ReadBookPermissionsAsync(book.id);
            before.owner.id.Should().Be(book.owned_by);
            before.role_permissions.Should().BeNullOrEmpty();
            before.fallback_permissions.inheriting.Should().BeTrue();

            var newperms = new RolePermission[]
            {
                new(role1.id, true, false, true,  false),
                new(role2.id, true, true,  false, false),
            };

            var after = await client.UpdateBookPermissionsAsync(book.id, new(role_permissions: newperms));
            after.owner.id.Should().Be(book.owned_by);
            after.role_permissions.Should().BeEquivalentTo(newperms);
            after.fallback_permissions.inheriting.Should().BeTrue();
        }
        {// role_permissions
            var guid = Guid.NewGuid().ToString();
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var user = await client.CreateUserAsync(new(testName($"user_{guid}"), $"user_{guid}@example.com")).WillBeDiscarded(container);

            var before = await client.ReadBookPermissionsAsync(book.id);
            before.owner.id.Should().Be(book.owned_by);
            before.role_permissions.Should().BeNullOrEmpty();
            before.fallback_permissions.inheriting.Should().BeTrue();

            var newfallback = FallbackPermission.Appoint(true, true, false, false);
            var after = await client.UpdateBookPermissionsAsync(book.id, new(fallback_permissions: newfallback));
            after.owner.id.Should().Be(book.owned_by);
            after.role_permissions.Should().BeNullOrEmpty();
            after.fallback_permissions.Should().Be(newfallback);
        }
    }

    [TestMethod()]
    public async Task UpdateChapterPermissionsAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {// owner_id
            var guid = Guid.NewGuid().ToString();
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter"))).WillBeDiscarded(container);
            var user = await client.CreateUserAsync(new(testName($"user_{guid}"), $"user_{guid}@example.com")).WillBeDiscarded(container);

            var before = await client.ReadChapterPermissionsAsync(chapter.id);
            before.owner.id.Should().Be(chapter.owned_by);
            before.role_permissions.Should().BeNullOrEmpty();
            before.fallback_permissions.inheriting.Should().BeTrue();

            var after = await client.UpdateChapterPermissionsAsync(chapter.id, new(owner_id: user.id));
            after.owner.id.Should().Be(user.id);
            after.role_permissions.Should().BeNullOrEmpty();
            after.fallback_permissions.inheriting.Should().BeTrue();
        }
        {// role_permissions
            var guid = Guid.NewGuid().ToString();
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter"))).WillBeDiscarded(container);
            var user = await client.CreateUserAsync(new(testName($"user_{guid}"), $"user_{guid}@example.com")).WillBeDiscarded(container);
            var role1 = await client.CreateRoleAsync(new(testName($"role1_{guid}"))).WillBeDiscarded(container);
            var role2 = await client.CreateRoleAsync(new(testName($"role2_{guid}"))).WillBeDiscarded(container);

            var before = await client.ReadChapterPermissionsAsync(chapter.id);
            before.owner.id.Should().Be(chapter.owned_by);
            before.role_permissions.Should().BeNullOrEmpty();
            before.fallback_permissions.inheriting.Should().BeTrue();

            var newperms = new RolePermission[]
            {
                new(role1.id, true, false, true,  false),
                new(role2.id, true, true,  false, false),
            };

            var after = await client.UpdateChapterPermissionsAsync(chapter.id, new(role_permissions: newperms));
            after.owner.id.Should().Be(book.owned_by);
            after.role_permissions.Should().BeEquivalentTo(newperms);
            after.fallback_permissions.inheriting.Should().BeTrue();
        }
        {// role_permissions
            var guid = Guid.NewGuid().ToString();
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var chapter = await client.CreateChapterAsync(new(book.id, testName("testchapter"))).WillBeDiscarded(container);
            var user = await client.CreateUserAsync(new(testName($"user_{guid}"), $"user_{guid}@example.com")).WillBeDiscarded(container);

            var before = await client.ReadChapterPermissionsAsync(chapter.id);
            before.owner.id.Should().Be(chapter.owned_by);
            before.role_permissions.Should().BeNullOrEmpty();
            before.fallback_permissions.inheriting.Should().BeTrue();

            var newfallback = FallbackPermission.Appoint(true, true, false, false);
            var after = await client.UpdateChapterPermissionsAsync(chapter.id, new(fallback_permissions: newfallback));
            after.owner.id.Should().Be(chapter.owned_by);
            after.role_permissions.Should().BeNullOrEmpty();
            after.fallback_permissions.Should().Be(newfallback);
        }
    }

    [TestMethod()]
    public async Task UpdatePagePermissionsAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {// owner_id
            var guid = Guid.NewGuid().ToString();
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreateMarkdownPageInBookAsync(new(book.id, testName("testpage"), markdown: "mdmd")).WillBeDiscarded(container);
            var user = await client.CreateUserAsync(new(testName($"user_{guid}"), $"user_{guid}@example.com")).WillBeDiscarded(container);

            var before = await client.ReadPagePermissionsAsync(page.id);
            before.owner.id.Should().Be(page.owned_by.id);
            before.role_permissions.Should().BeNullOrEmpty();
            before.fallback_permissions.inheriting.Should().BeTrue();

            var after = await client.UpdatePagePermissionsAsync(page.id, new(owner_id: user.id));
            after.owner.id.Should().Be(user.id);
            after.role_permissions.Should().BeNullOrEmpty();
            after.fallback_permissions.inheriting.Should().BeTrue();
        }
        {// role_permissions
            var guid = Guid.NewGuid().ToString();
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreateMarkdownPageInBookAsync(new(book.id, testName("testpage"), markdown: "mdmd")).WillBeDiscarded(container);
            var user = await client.CreateUserAsync(new(testName($"user_{guid}"), $"user_{guid}@example.com")).WillBeDiscarded(container);
            var role1 = await client.CreateRoleAsync(new(testName($"role1_{guid}"))).WillBeDiscarded(container);
            var role2 = await client.CreateRoleAsync(new(testName($"role2_{guid}"))).WillBeDiscarded(container);

            var before = await client.ReadPagePermissionsAsync(page.id);
            before.owner.id.Should().Be(page.owned_by.id);
            before.role_permissions.Should().BeNullOrEmpty();
            before.fallback_permissions.inheriting.Should().BeTrue();

            var newperms = new RolePermission[]
            {
                new(role1.id, true, false, true,  false),
                new(role2.id, true, true,  false, false),
            };

            var after = await client.UpdatePagePermissionsAsync(page.id, new(role_permissions: newperms));
            after.owner.id.Should().Be(book.owned_by);
            after.role_permissions.Should().BeEquivalentTo(newperms);
            after.fallback_permissions.inheriting.Should().BeTrue();
        }
        {// role_permissions
            var guid = Guid.NewGuid().ToString();
            var book = await client.CreateBookAsync(new(testName("testbook"))).WillBeDiscarded(container);
            var page = await client.CreateMarkdownPageInBookAsync(new(book.id, testName("testpage"), markdown: "mdmd")).WillBeDiscarded(container);
            var user = await client.CreateUserAsync(new(testName($"user_{guid}"), $"user_{guid}@example.com")).WillBeDiscarded(container);

            var before = await client.ReadPagePermissionsAsync(page.id);
            before.owner.id.Should().Be(page.owned_by.id);
            before.role_permissions.Should().BeNullOrEmpty();
            before.fallback_permissions.inheriting.Should().BeTrue();

            var newfallback = FallbackPermission.Appoint(true, true, false, false);
            var after = await client.UpdatePagePermissionsAsync(page.id, new(fallback_permissions: newfallback));
            after.owner.id.Should().Be(page.owned_by.id);
            after.role_permissions.Should().BeNullOrEmpty();
            after.fallback_permissions.Should().Be(newfallback);
        }
    }
    #endregion
}
