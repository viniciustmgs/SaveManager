using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(SaveManager.UI.Headless.Tests.TestAppBuilder))]

namespace SaveManager.UI.Headless.Tests
{
    public static class TestAppBuilder
    {
        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<TestApplication>()
                .UseHeadless(new AvaloniaHeadlessPlatformOptions());
    }

    public sealed class TestApplication : Avalonia.Application
    {
        public override void Initialize()
        {
            Styles.Add(new Avalonia.Themes.Fluent.FluentTheme());
        }
    }

    public static class Ui
    {
        private static readonly Lazy<HeadlessUnitTestSession> Session =
            new(() => HeadlessUnitTestSession.StartNew(typeof(TestAppBuilder)));

        public static T Get<T>(Func<T> body) =>
            Session.Value.Dispatch(async () => body(), CancellationToken.None)
                .GetAwaiter().GetResult();

        public static void Run(Func<Task> body) =>
            Session.Value.Dispatch(body, CancellationToken.None).GetAwaiter().GetResult();

        public static T RunAsync<T>(Func<Task<T>> body) =>
            Session.Value.Dispatch(body, CancellationToken.None).GetAwaiter().GetResult();
    }
}