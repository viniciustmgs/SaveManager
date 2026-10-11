using SaveManager.Infrastructure.Updates;
using Xunit;

namespace SaveManager.Infrastructure.Tests
{
    public class AppVersionTests
    {
        [Theory]
        [InlineData("1.2.3", 1, 2, 3)]
        [InlineData("v1.2.3", 1, 2, 3)]
        [InlineData("V1.2.3", 1, 2, 3)]
        [InlineData("  v1.2.3  ", 1, 2, 3)]
        [InlineData("1.0", 1, 0, 0)]
        [InlineData("2", 2, 0, 0)]
        [InlineData("v1.0.0+abc123", 1, 0, 0)]
        [InlineData("1.0.0-beta", 1, 0, 0)]
        public void ParsesTheUsualShapes(string value, int major, int minor, int patch)
        {
            Assert.True(AppVersion.TryParse(value, out var version));

            Assert.Equal(new AppVersion(major, minor, patch), version);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("nightly")]
        [InlineData("1.2.3.4")]
        [InlineData("-1.0.0")]
        [InlineData("v")]
        public void RejectsWhatItCannotRead(string? value)
        {
            Assert.False(AppVersion.TryParse(value, out _));
        }

        [Theory]
        [InlineData("v1.0.0", 1, 0, 0)]
        [InlineData("v2.1.0", 2, 1, 0)]
        [InlineData("v1.0.1+build", 1, 0, 1)]
        public void AcceptsReleaseTags(string tag, int major, int minor, int patch)
        {
            Assert.True(AppVersion.TryParseReleaseTag(tag, out var version));

            Assert.Equal(new AppVersion(major, minor, patch), version);
        }

        [Theory]
        [InlineData("v1.1.0-beta")]
        [InlineData("v1.1.0-rc1")]
        [InlineData("v2.0.0-alpha.1")]
        [InlineData("garbage")]
        public void RefusesPrereleases(string tag)
        {
            Assert.False(AppVersion.TryParseReleaseTag(tag, out _));
        }

        [Theory]
        [InlineData("1.0.1", "1.0.0", 1)]
        [InlineData("1.1.0", "1.0.9", 1)]
        [InlineData("2.0.0", "1.9.9", 1)]
        [InlineData("1.0.0", "1.0.0", 0)]
        [InlineData("1.0.0", "1.0.1", -1)]
        [InlineData("1.0.0", "1.1.0", -1)]
        [InlineData("1.0.0", "0.9.9", 1)]
        public void OrdersVersions(string left, string right, int expected)
        {
            Assert.True(AppVersion.TryParse(left, out var a));
            Assert.True(AppVersion.TryParse(right, out var b));

            Assert.Equal(expected, Math.Sign(a.CompareTo(b)));
        }
    }
}