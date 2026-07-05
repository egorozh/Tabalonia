using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;
using Tabalonia.Tests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace Tabalonia.Tests;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApp>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

public class TestApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        Styles.Add(new Themes.Custom.CustomTheme());
    }
}
