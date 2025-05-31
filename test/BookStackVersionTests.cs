namespace BookStackApiClient.Tests;

[TestClass]
public class BookStackVersionTests
{
    [TestMethod]
    public void TryParse_able()
    {
        BookStackVersion.TryParse("1.2.3", out _).Should().BeTrue();
        BookStackVersion.TryParse("v1.2.3", out _).Should().BeTrue();
        BookStackVersion.TryParse("1.2", out _).Should().BeTrue();
        BookStackVersion.TryParse("v1.2", out _).Should().BeTrue();
        BookStackVersion.TryParse(" 1 . 2 . 3 ", out _).Should().BeTrue();
        BookStackVersion.TryParse("1.2.3.4", out _).Should().BeTrue();

        BookStackVersion.TryParse("1.2.3.A", out _).Should().BeTrue();
        BookStackVersion.TryParse("1.2.3-A", out _).Should().BeTrue();
        BookStackVersion.TryParse("1.2.A", out _).Should().BeTrue();
        BookStackVersion.TryParse("1.2-A", out _).Should().BeTrue();

        BookStackVersion.TryParse("abc", out _).Should().BeFalse();
        BookStackVersion.TryParse("1", out _).Should().BeFalse();
    }

    [TestMethod]
    public void TryParse_value()
    {
        void vaildate(string text, int major, int minor, int rev, string ext)
        {
            var ver = BookStackVersion.TryParse(text, out var v) ? v : throw new Exception();
            ver.Major.Should().Be(major);
            ver.Minor.Should().Be(minor);
            ver.Revision.Should().Be(rev);
            ver.Ext.Should().Be(ext);
        }

        vaildate("1.2.3", 1, 2, 3, "");
        vaildate("v1.2.3", 1, 2, 3, "");
        vaildate("1.2", 1, 2, 0, "");
        vaildate("v1.2", 1, 2, 0, "");
        vaildate(" 1 . 2 . 3 ", 1, 2, 3, "");
        vaildate("1.2.3-A", 1, 2, 3, "A");
        vaildate("1.2-A", 1, 2, 0, "A");
    }

    [TestMethod]
    public void Parse_able()
    {
        FluentActions.Invoking(() => BookStackVersion.Parse("1.2.3")).Should().NotThrow();
        FluentActions.Invoking(() => BookStackVersion.Parse("v1.2.3")).Should().NotThrow();
        FluentActions.Invoking(() => BookStackVersion.Parse("1.2")).Should().NotThrow();
        FluentActions.Invoking(() => BookStackVersion.Parse("v1.2")).Should().NotThrow();
        FluentActions.Invoking(() => BookStackVersion.Parse(" 1 . 2 . 3 ")).Should().NotThrow();
        FluentActions.Invoking(() => BookStackVersion.Parse("1.2.3.4")).Should().NotThrow();

        FluentActions.Invoking(() => BookStackVersion.Parse("1.2.3.A")).Should().NotThrow();
        FluentActions.Invoking(() => BookStackVersion.Parse("1.2.3-A")).Should().NotThrow();
        FluentActions.Invoking(() => BookStackVersion.Parse("1.2.A")).Should().NotThrow();
        FluentActions.Invoking(() => BookStackVersion.Parse("1.2-A")).Should().NotThrow();

        FluentActions.Invoking(() => BookStackVersion.Parse("abc")).Should().Throw<Exception>();
        FluentActions.Invoking(() => BookStackVersion.Parse("1")).Should().Throw<Exception>();
    }

    [TestMethod]
    public void Equals()
    {
        new BookStackVersion(1, 2, 3).Equals(new BookStackVersion(1, 2, 3)).Should().BeTrue();
        new BookStackVersion(1, 2, 3, "x").Equals(new BookStackVersion(1, 2, 3, "x")).Should().BeTrue();

        new BookStackVersion(1, 2, 3).Equals(new BookStackVersion(1, 2, 4)).Should().BeFalse();
        new BookStackVersion(1, 2, 3).Equals(new BookStackVersion(1, 3, 3)).Should().BeFalse();
        new BookStackVersion(1, 2, 3).Equals(new BookStackVersion(2, 2, 3)).Should().BeFalse();
        new BookStackVersion(1, 2, 3).Equals(new BookStackVersion(1, 2, 3, "a")).Should().BeFalse();
        new BookStackVersion(1, 2, 3, "a").Equals(new BookStackVersion(1, 2, 3, "b")).Should().BeFalse();
    }

    [TestMethod]
    public void Compare()
    {
        new BookStackVersion(1, 2, 3).CompareTo(new BookStackVersion(1, 2, 3)).Should().Be(0);
        new BookStackVersion(1, 2, 3, "x").CompareTo(new BookStackVersion(1, 2, 3, "x")).Should().Be(0);

        new BookStackVersion(1, 2, 3).CompareTo(new BookStackVersion(1, 2, 2)).Should().BePositive();
        new BookStackVersion(1, 2, 3).CompareTo(new BookStackVersion(1, 2, 4)).Should().BeNegative();

        new BookStackVersion(1, 2, 3).CompareTo(new BookStackVersion(1, 1, 3)).Should().BePositive();
        new BookStackVersion(1, 2, 3).CompareTo(new BookStackVersion(1, 3, 3)).Should().BeNegative();

        new BookStackVersion(1, 2, 3).CompareTo(new BookStackVersion(0, 2, 3)).Should().BePositive();
        new BookStackVersion(1, 2, 3).CompareTo(new BookStackVersion(2, 2, 3)).Should().BeNegative();

        new BookStackVersion(1, 2, 3, "c").CompareTo(new BookStackVersion(1, 2, 3, "b")).Should().BePositive();
        new BookStackVersion(1, 2, 3, "c").CompareTo(new BookStackVersion(1, 2, 3, "d")).Should().BeNegative();
    }


    [TestMethod]
    public void CompareOperator()
    {
        (new BookStackVersion(1, 2, 3, "c") < new BookStackVersion(1, 2, 3, "b")).Should().Be(false);
        (new BookStackVersion(1, 2, 3, "c") < new BookStackVersion(1, 2, 3, "c")).Should().Be(false);
        (new BookStackVersion(1, 2, 3, "c") < new BookStackVersion(1, 2, 3, "d")).Should().Be(true);

        (new BookStackVersion(1, 2, 3, "c") <= new BookStackVersion(1, 2, 3, "b")).Should().Be(false);
        (new BookStackVersion(1, 2, 3, "c") <= new BookStackVersion(1, 2, 3, "c")).Should().Be(true);
        (new BookStackVersion(1, 2, 3, "c") <= new BookStackVersion(1, 2, 3, "d")).Should().Be(true);

        (new BookStackVersion(1, 2, 3, "c") > new BookStackVersion(1, 2, 3, "b")).Should().Be(true);
        (new BookStackVersion(1, 2, 3, "c") > new BookStackVersion(1, 2, 3, "c")).Should().Be(false);
        (new BookStackVersion(1, 2, 3, "c") > new BookStackVersion(1, 2, 3, "d")).Should().Be(false);

        (new BookStackVersion(1, 2, 3, "c") >= new BookStackVersion(1, 2, 3, "b")).Should().Be(true);
        (new BookStackVersion(1, 2, 3, "c") >= new BookStackVersion(1, 2, 3, "c")).Should().Be(true);
        (new BookStackVersion(1, 2, 3, "c") >= new BookStackVersion(1, 2, 3, "d")).Should().Be(false);
    }



}
