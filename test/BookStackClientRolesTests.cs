namespace BookStackApiClient.Tests;

[TestClass()]
public class BookStackClientRolesTests : BookStackClientTestsBase
{
    #region roles
    [TestMethod()]
    public async Task ListRolesAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var guid = Guid.NewGuid().ToString();
        var perms1 = new[] { RolePermissions.CreateOwnBooks, RolePermissions.DeleteOwnComments, };
        var perms2 = new string[0] { };
        var perms3 = new[] { RolePermissions.AccessSystemAPI, RolePermissions.CreateAllChapters, RolePermissions.CreateOwnComments, };

        var role1 = await client.CreateRoleAsync(new(testName("role1"), "desc1", permissions: perms1)).WillBeDiscarded(container);
        var role2 = await client.CreateRoleAsync(new(testName("role2"), "desc2", permissions: perms2)).WillBeDiscarded(container);
        var role3 = await client.CreateRoleAsync(new(testName("role3"), "desc3", permissions: perms3)).WillBeDiscarded(container);

        var user1 = await client.CreateUserAsync(new(testName("user1"), $"user1-{guid}@example.com", roles: Array.Empty<long>())).WillBeDiscarded(container);
        var user2 = await client.CreateUserAsync(new(testName("user2"), $"user2-{guid}@example.com", roles: new[] { role2.id, })).WillBeDiscarded(container);
        var user3 = await client.CreateUserAsync(new(testName("user3"), $"user3-{guid}@example.com", roles: new[] { role1.id, role2.id, })).WillBeDiscarded(container);

        var roles = await client.ListRolesAsync();

        var actual1 = roles.data.Should().Contain(i => i.id == role1.id).Subject;
        var actual2 = roles.data.Should().Contain(i => i.id == role2.id).Subject;
        var actual3 = roles.data.Should().Contain(i => i.id == role3.id).Subject;

        actual1.Should().BeEquivalentTo(role1, o => o.ExcludingMissingMembers());
        actual2.Should().BeEquivalentTo(role2, o => o.ExcludingMissingMembers());
        actual3.Should().BeEquivalentTo(role3, o => o.ExcludingMissingMembers());

        actual1.permissions_count.Should().Be(perms1.Length);
        actual2.permissions_count.Should().Be(perms2.Length);
        actual3.permissions_count.Should().Be(perms3.Length);

        actual1.users_count.Should().Be(1);
        actual2.users_count.Should().Be(2);
        actual3.users_count.Should().Be(0);
    }

    [TestMethod()]
    public async Task ListRolesAsync_options()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);

        var prefix1 = testName($"role_{Guid.NewGuid()}_");
        for (var i = 0; i < 10; i++)
        {
            await client.CreateRoleAsync(new(testName($"{prefix1}_U{i:D3}"), $"{prefix1}_U{i:D3}@example.com")).WillBeDiscarded(container);
        }
        var prefix2 = testName($"role_{Guid.NewGuid()}_");
        for (var i = 0; i < 10; i++)
        {
            await client.CreateRoleAsync(new(testName($"{prefix2}_U{i:D3}"), $"{prefix2}_U{i:D3}@example.com")).WillBeDiscarded(container);
        }

        {// range
            var roles1 = await client.ListRolesAsync(new(offset: 0, count: 5));
            roles1.data.Should().HaveCount(5);
            var roles2 = await client.ListRolesAsync(new(offset: 5, count: 5));
            roles2.data.Should().HaveCount(5);

            roles1.data.Select(d => d.id).Should().NotIntersectWith(roles2.data.Select(d => d.id));
        }
        {// filter
            var roles = await client.ListRolesAsync(new(filters: [new($"name:like", $"{prefix1}%")]));
            roles.data.Should().AllSatisfy(d => d.display_name.StartsWith(prefix1));
        }
        {// filter & sort (asc)
            var offset = 0;
            var count = 4;
            var roles = await client.ListRolesAsync(new(offset, count, sorts: [nameof(RoleSummary.display_name),], filters: [new($"{nameof(RoleSummary.display_name)}:like", $"{prefix1}%")]));
            var expects = container.Roles.Where(c => c.display_name.StartsWith(prefix1)).Select(c => c.id).Skip(offset).Take(count);
            roles.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (desc)
            var offset = 0;
            var count = 4;
            var roles = await client.ListRolesAsync(new(offset, count, sorts: [$"-{nameof(RoleSummary.display_name)}",], filters: [new($"{nameof(RoleSummary.display_name)}:like", $"{prefix1}%")]));
            var expects = container.Roles.AsEnumerable().Reverse().Where(c => c.display_name.StartsWith(prefix1)).Select(c => c.id).Skip(offset).Take(count);
            roles.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (asc) & range
            var offset = 2;
            var count = 5;
            var roles = await client.ListRolesAsync(new(offset, count, sorts: [nameof(RoleSummary.display_name),], filters: [new($"{nameof(RoleSummary.display_name)}:like", $"{prefix2}%")]));
            var expects = container.Roles.Where(c => c.display_name.StartsWith(prefix2)).Select(c => c.id).Skip(offset).Take(count);
            roles.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (desc) & range
            var offset = 3;
            var count = 4;
            var roles = await client.ListRolesAsync(new(offset, count, sorts: [$"-{nameof(RoleSummary.display_name)}",], filters: [new($"{nameof(RoleSummary.display_name)}:like", $"{prefix2}%")]));
            var expects = container.Roles.AsEnumerable().Reverse().Where(c => c.display_name.StartsWith(prefix2)).Select(c => c.id).Skip(offset).Take(count);
            roles.data.Select(d => d.id).Should().Equal(expects);
        }
    }

    [TestMethod()]
    public async Task CreateRoleAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {// name 
            var now = DateTime.UtcNow;
            var role = await client.CreateRoleAsync(new(testName("aaa"))).WillBeDiscarded(container);
            role.id.Should().NotBe(0);
            role.display_name.Should().Be(testName("aaa"));
            role.description.Should().BeNullOrEmpty();
            role.mfa_enforced.Should().BeFalse();
            role.permissions.Should().BeNullOrEmpty();
            role.users.Should().BeNullOrEmpty();
            role.created_at.Should().BeCloseTo(now, 10.Seconds());
            role.updated_at.Should().BeCloseTo(now, 10.Seconds());
        }
        {// description 
            var now = DateTime.UtcNow;
            var role = await client.CreateRoleAsync(new(testName("role"), "desc")).WillBeDiscarded(container);
            role.description.Should().Be("desc");
        }
        {// mfa_enforced 
            var now = DateTime.UtcNow;
            var role = await client.CreateRoleAsync(new(testName("role"), mfa_enforced: true)).WillBeDiscarded(container);
            role.mfa_enforced.Should().BeTrue();
        }
        {// permissions 
            var now = DateTime.UtcNow;
            var permissions = new[] { RolePermissions.CreateAllBooks, RolePermissions.ViewOwnChapters, };
            var role = await client.CreateRoleAsync(new(testName("role"), permissions: permissions)).WillBeDiscarded(container);
            role.permissions.Should().BeEquivalentTo(permissions);
        }
    }

    [TestMethod()]
    public async Task ReadRoleAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {
            var guid = Guid.NewGuid().ToString();
            var perms = new[] { RolePermissions.AccessSystemAPI, RolePermissions.CreateAllChapters, RolePermissions.CreateOwnComments, };
            var role = await client.CreateRoleAsync(new(testName($"role_{guid}"), "desc1", permissions: perms)).WillBeDiscarded(container);
            var user1 = await client.CreateUserAsync(new(testName("user1"), $"user1-{guid}@example.com", roles: new[] { role.id, })).WillBeDiscarded(container);
            var user2 = await client.CreateUserAsync(new(testName("user2"), $"user2-{guid}@example.com", roles: new[] { role.id, })).WillBeDiscarded(container);

            var readed = await client.ReadRoleAsync(role.id);
            readed.Should().BeEquivalentTo(role, o => o.Excluding(t => t.users));
            readed.users.Select(u => u.id).Should().BeEquivalentTo(new[] { user1.id, user2.id, });
        }
    }

    [TestMethod()]
    public async Task UpdateRoleAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);

        {// name & desc & permissions
            var now = DateTime.UtcNow;
            var guid = Guid.NewGuid().ToString();
            var perms = new[] { RolePermissions.AccessSystemAPI, RolePermissions.CreateAllChapters, RolePermissions.CreateOwnComments, };
            var created = await client.CreateRoleAsync(new(testName($"role_{guid}"), "desc", permissions: perms)).WillBeDiscarded(container);
            await Task.Delay(2 * 1000);     // for update timestamp
            var newperms = new[] { RolePermissions.ViewOwnPages, RolePermissions.ManageSettings, };
            var updated = await client.UpdateRoleAsync(created.id, new(testName($"upd-role_{guid}"), "upd-desc", permissions: newperms));
            updated.id.Should().Be(created.id);
            updated.display_name.Should().Be(testName($"upd-role_{guid}"));
            updated.description.Should().Be("upd-desc");
            updated.mfa_enforced.Should().Be(created.mfa_enforced);
            updated.permissions.Should().BeEquivalentTo(newperms);
            updated.users.Should().BeNullOrEmpty();
            updated.created_at.Should().Be(created.created_at);
            updated.updated_at.Should().BeAfter(created.updated_at);
        }
        {// 
            var guid = Guid.NewGuid().ToString();
            var created = await client.CreateRoleAsync(new(testName($"role_{guid}"))).WillBeDiscarded(container);
            var user1 = await client.CreateUserAsync(new(testName($"user1_{guid}"), $"user1-{guid}@example.com", roles: new[] { created.id, })).WillBeDiscarded(container);
            var user2 = await client.CreateUserAsync(new(testName($"user2_{guid}"), $"user2-{guid}@example.com", roles: new[] { created.id, })).WillBeDiscarded(container);
            var user3 = await client.CreateUserAsync(new(testName($"user3_{guid}"), $"user3-{guid}@example.com")).WillBeDiscarded(container);
            var user4 = await client.CreateUserAsync(new(testName($"user4_{guid}"), $"user4-{guid}@example.com")).WillBeDiscarded(container);

            var updated = await client.UpdateRoleAsync(created.id, new());
            updated.id.Should().Be(created.id);
            updated.users.Select(u => u.id).Should().BeEquivalentTo(new[] { user1.id, user2.id, });
        }

    }

    [TestMethod()]
    public async Task DeleteRoleAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {
            var role = await client.CreateRoleAsync(new(testName("aaa")));
            (await client.ListRolesAsync(new(filters: [new(nameof(RoleSummary.id), $"{role.id}")]))).data.Should().Contain(d => d.id == role.id);
            await client.DeleteRoleAsync(role.id);
            (await client.ListRolesAsync(new(filters: [new(nameof(RoleSummary.id), $"{role.id}")]))).data.Should().NotContain(d => d.id == role.id);
        }
    }
    #endregion
}
