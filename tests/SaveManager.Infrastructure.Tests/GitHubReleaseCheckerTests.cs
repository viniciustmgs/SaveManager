using SaveManager.Domain.Enums;
using SaveManager.Infrastructure.Updates;
using System.Net;
using Xunit;

namespace SaveManager.Infrastructure.Tests
{
    public class GitHubReleaseCheckerTests
    {
        private const string Endpoint = "https://example.test/releases/latest";

        [Fact]
        public async Task NewerReleaseIsReported()
        {
            var checker = Build(HttpStatusCode.OK, """{"tag_name":"v1.1.0"}""");

            var result = await checker.CheckAsync("v1.0.0", CancellationToken.None);

            Assert.Equal(UpdateCheckStatus.UpdateAvailable, result.Status);
            Assert.Equal("v1.1.0", result.LatestVersion);
        }

        [Fact]
        public async Task SameVersionIsUpToDate()
        {
            var checker = Build(HttpStatusCode.OK, """{"tag_name":"v1.0.0"}""");

            var result = await checker.CheckAsync("v1.0.0", CancellationToken.None);

            Assert.Equal(UpdateCheckStatus.UpToDate, result.Status);
        }

        [Fact]
        public async Task OlderPublishedVersionIsStillUpToDate()
        {
            var checker = Build(HttpStatusCode.OK, """{"tag_name":"v0.9.0"}""");

            var result = await checker.CheckAsync("v1.0.0", CancellationToken.None);

            Assert.Equal(UpdateCheckStatus.UpToDate, result.Status);
        }

        [Fact]
        public async Task NoReleasesHasItsOwnOutcome()
        {
            var checker = Build(HttpStatusCode.NotFound, "");

            var result = await checker.CheckAsync("v1.0.0", CancellationToken.None);

            Assert.Equal(UpdateCheckStatus.NoReleases, result.Status);
        }

        [Theory]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task BadStatusIsAFailure(HttpStatusCode status)
        {
            var checker = Build(status, "");

            var result = await checker.CheckAsync("v1.0.0", CancellationToken.None);

            Assert.Equal(UpdateCheckStatus.Failed, result.Status);
        }

        [Theory]
        [InlineData("not json at all")]
        [InlineData("{}")]
        [InlineData("""{"tag_name":""}""")]
        [InlineData("""{"tag_name":"nightly"}""")]
        public async Task UnreadablePayloadIsAFailureRatherThanACrash(string body)
        {
            var checker = Build(HttpStatusCode.OK, body);

            var result = await checker.CheckAsync("v1.0.0", CancellationToken.None);

            Assert.Equal(UpdateCheckStatus.Failed, result.Status);
        }

        [Fact]
        public async Task UnreadableLocalVersionIsAFailure()
        {
            var checker = Build(HttpStatusCode.OK, """{"tag_name":"v1.1.0"}""");

            var result = await checker.CheckAsync("not-a-version", CancellationToken.None);

            Assert.Equal(UpdateCheckStatus.Failed, result.Status);
        }

        [Fact]
        public async Task NetworkFailureIsAFailureRatherThanAnException()
        {
            var checker = new GitHubReleaseChecker(
                new HttpClient(new ThrowingHandler()), Endpoint);

            var result = await checker.CheckAsync("v1.0.0", CancellationToken.None);

            Assert.Equal(UpdateCheckStatus.Failed, result.Status);
        }

        [Fact]
        public async Task CancellationPropagatesSoTheDialogCanClose()
        {
            var checker = Build(HttpStatusCode.OK, """{"tag_name":"v1.1.0"}""");

            using var cts = new CancellationTokenSource();

            await cts.CancelAsync();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => checker.CheckAsync("v1.0.0", cts.Token));
        }

        private static GitHubReleaseChecker Build(HttpStatusCode status, string body) =>
            new(new HttpClient(new StubHandler(status, body)), Endpoint);

        private sealed class StubHandler(HttpStatusCode status, string body) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                return Task.FromResult(new HttpResponseMessage(status)
                {
                    Content = new StringContent(body)
                });
            }
        }

        private sealed class ThrowingHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken) =>
                throw new HttpRequestException("no network");
        }
    }
}