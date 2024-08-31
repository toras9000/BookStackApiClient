namespace BookStackApiClient.Tests;

[TestClass()]
public class BookStackClientUsersTests : BookStackClientTestsBase
{
    #region users
    [TestMethod()]
    public async Task ListUsersAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        var guid = Guid.NewGuid().ToString();
        var user1 = await client.CreateUserAsync(new(testName("user1"), $"user1_{guid}@example.com")).WillBeDiscarded(container);
        var user2 = await client.CreateUserAsync(new(testName("user2"), $"user2_{guid}@example.com")).WillBeDiscarded(container);
        var user3 = await client.CreateUserAsync(new(testName("user3"), $"user3_{guid}@example.com")).WillBeDiscarded(container);

        var users = await client.ListUsersAsync();
        foreach (var user in container.Users)
        {
            var actual = users.data.Should().Contain(i => i.id == user.id).Subject;
            var expect = user;
            actual.Should().BeEquivalentTo(expect, o => o.ExcludingMissingMembers());
        }
    }

    [TestMethod()]
    public async Task ListUsersAsync_options()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);

        var prefix1 = testName($"user_{Guid.NewGuid()}_");
        for (var i = 0; i < 10; i++)
        {
            await client.CreateUserAsync(new($"{prefix1}_U{i:D3}", $"{prefix1}_U{i:D3}@example.com")).WillBeDiscarded(container);
        }
        var prefix2 = testName($"user_{Guid.NewGuid()}_");
        for (var i = 0; i < 10; i++)
        {
            await client.CreateUserAsync(new($"{prefix2}_U{i:D3}", $"{prefix2}_U{i:D3}@example.com")).WillBeDiscarded(container);
        }

        {// range
            var users1 = await client.ListUsersAsync(new(offset: 0, count: 5));
            users1.data.Should().HaveCount(5);
            var users2 = await client.ListUsersAsync(new(offset: 5, count: 5));
            users2.data.Should().HaveCount(5);

            users1.data.Select(d => d.id).Should().NotIntersectWith(users2.data.Select(d => d.id));
        }
        {// filter
            var users = await client.ListUsersAsync(new(filters: [new($"{nameof(UserSummary.name)}:like", $"{prefix1}%")]));
            users.data.Should().AllSatisfy(d => d.name.StartsWith(prefix1));
        }
        {// filter & sort (asc)
            var offset = 0;
            var count = 4;
            var users = await client.ListUsersAsync(new(offset, count, sorts: [nameof(UserSummary.name),], filters: [new($"name:like", $"{prefix1}%")]));
            var expects = container.Users.Where(c => c.name.StartsWith(prefix1)).Select(c => c.id).Skip(offset).Take(count);
            users.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (desc)
            var offset = 0;
            var count = 4;
            var users = await client.ListUsersAsync(new(offset, count, sorts: [$"-{nameof(UserSummary.name)}",], filters: [new($"name:like", $"{prefix1}%")]));
            var expects = container.Users.AsEnumerable().Reverse().Where(c => c.name.StartsWith(prefix1)).Select(c => c.id).Skip(offset).Take(count);
            users.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (asc) & range
            var offset = 2;
            var count = 5;
            var users = await client.ListUsersAsync(new(offset, count, sorts: [nameof(UserSummary.name),], filters: [new($"name:like", $"{prefix2}%")]));
            var expects = container.Users.Where(c => c.name.StartsWith(prefix2)).Select(c => c.id).Skip(offset).Take(count);
            users.data.Select(d => d.id).Should().Equal(expects);
        }
        {// filter & sort (desc) & range
            var offset = 3;
            var count = 4;
            var users = await client.ListUsersAsync(new(offset, count, sorts: [$"-{nameof(UserSummary.name)}",], filters: [new($"name:like", $"{prefix2}%")]));
            var expects = container.Users.AsEnumerable().Reverse().Where(c => c.name.StartsWith(prefix2)).Select(c => c.id).Skip(offset).Take(count);
            users.data.Select(d => d.id).Should().Equal(expects);
        }
    }

    [TestMethod()]
    public async Task CreateUserAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {// name & mail
            var now = DateTime.UtcNow;
            var guid = Guid.NewGuid().ToString();
            var mail = $"aaa_{guid}@example.com";
            var user = await client.CreateUserAsync(new(testName("aaa"), mail)).WillBeDiscarded(container);
            user.name.Should().Be(testName("aaa"));
            user.slug.Should().NotBeNullOrEmpty();
            user.email.Should().Be(mail);
            user.external_auth_id.Should().BeEmpty();
            user.roles.Should().BeNullOrEmpty();
            user.profile_url.Should().NotBeNullOrEmpty();
            user.edit_url.Should().NotBeNullOrEmpty();
            user.avatar_url.Should().NotBeNullOrEmpty();
            user.created_at.Should().BeCloseTo(now, 10.Seconds());
            user.updated_at.Should().BeCloseTo(now, 10.Seconds());
        }
        {// language
            var now = DateTime.UtcNow;
            var guid = Guid.NewGuid().ToString();
            var mail = $"bbb_{guid}@example.com";
            var user = await client.CreateUserAsync(new(testName("bbb"), mail, language: "ja", password: "bbbb1234")).WillBeDiscarded(container);
            user.name.Should().Be(testName("bbb"));
            user.slug.Should().NotBeNullOrEmpty();
            user.email.Should().Be(mail);
            user.external_auth_id.Should().BeEmpty();
            user.roles.Should().BeNullOrEmpty();
            user.profile_url.Should().NotBeNullOrEmpty();
            user.edit_url.Should().NotBeNullOrEmpty();
            user.avatar_url.Should().NotBeNullOrEmpty();
            user.created_at.Should().BeCloseTo(now, 10.Seconds());
            user.updated_at.Should().BeCloseTo(now, 10.Seconds());
        }
    }

    [TestMethod()]
    public async Task ReadUserAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {
            var guid = Guid.NewGuid().ToString();
            var mail = $"xxxx_{guid}@example.com";
            var created = await client.CreateUserAsync(new(testName("xxxx"), mail, language: "ja", password: "xxxx1234")).WillBeDiscarded(container);

            var readed = await client.ReadUserAsync(created.id);
            readed.Should().BeEquivalentTo(created);
        }
    }

    [TestMethod()]
    public async Task UpdateUserAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);

        {// name
            var now = DateTime.UtcNow;
            var guid = Guid.NewGuid().ToString();
            var created = await client.CreateUserAsync(new(testName("user1"), $"user1_{guid}@example.com")).WillBeDiscarded(container);
            await Task.Delay(2 * 1000);     // for update timestamp
            var updated = await client.UpdateUserAsync(created.id, new(testName("upd-user1")));
            updated.id.Should().Be(created.id);
            updated.name.Should().Be(testName("upd-user1"));
            updated.slug.Should().NotBe(created.slug);  // 変わる
            updated.email.Should().Be(created.email);
            updated.external_auth_id.Should().Be(created.external_auth_id);
            updated.profile_url.Should().NotBe(created.profile_url);    // 変わる
            updated.edit_url.Should().Be(created.edit_url);
            updated.avatar_url.Should().Be(created.avatar_url);
            updated.created_at.Should().Be(created.created_at);
            updated.updated_at.Should().BeAfter(created.updated_at);
        }
        {// mail
            var now = DateTime.UtcNow;
            var guid = Guid.NewGuid().ToString();
            var created = await client.CreateUserAsync(new(testName("user2"), $"user2_{guid}@example.com")).WillBeDiscarded(container);
            var updated = await client.UpdateUserAsync(created.id, new(email: $"chg-user2_{guid}@example.com"));
            updated.id.Should().Be(created.id);
            updated.name.Should().Be(testName("user2"));
            updated.slug.Should().Be(created.slug);
            updated.email.Should().Be($"chg-user2_{guid}@example.com");
            updated.external_auth_id.Should().Be(created.external_auth_id);
            updated.profile_url.Should().Be(created.profile_url);
            updated.edit_url.Should().Be(created.edit_url);
            updated.avatar_url.Should().Be(created.avatar_url);
            updated.created_at.Should().Be(created.created_at);
            updated.updated_at.Should().BeOnOrAfter(created.updated_at);
        }

    }

    [TestMethod()]
    public async Task DeleteUserAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        await using var container = new TestResourceContainer(client);
        {
            var guid = Guid.NewGuid().ToString();
            var name = $"xxxx_{guid}_name";
            var mail = $"xxxx_{guid}@example.com";
            var user = await client.CreateUserAsync(new(name, mail, language: "ja", password: "xxxx1234"));
            (await client.ListUsersAsync(new(filters: [new("name", name)]))).data.Should().Contain(d => d.id == user.id);
            await client.DeleteUserAsync(user.id);
            (await client.ListUsersAsync(new(filters: [new("name", name)]))).data.Should().NotContain(d => d.id == user.id);
        }
    }
    #endregion
}
