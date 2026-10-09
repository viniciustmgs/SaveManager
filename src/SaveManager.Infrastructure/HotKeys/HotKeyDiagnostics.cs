namespace SaveManager.Infrastructure.HotKeys;

// set SAVEMANAGER_HOTKEY_DEBUG=1 to debug
public static class HotKeyDiagnostics
{
    private const string Variable = "SAVEMANAGER_HOTKEY_DEBUG";

    private static readonly bool Trace =
        Environment.GetEnvironmentVariable(Variable) is "1" or "true" or "TRUE";

    public static bool IsEnabled => Trace;

    public static void Write(string message)
    {
        if (Trace)
            Console.Error.WriteLine($"[hotkey] {message}");
    }
}