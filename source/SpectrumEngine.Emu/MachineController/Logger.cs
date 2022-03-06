namespace SpectrumEngine.Emu;

public static class Logger
{
    private readonly static List<string> _messages = new();

    public static void Log(string msg) => _messages.Add(msg);

    public static void Flush() => File.WriteAllText("C:/Temp/Log.txt", string.Join(Environment.NewLine, _messages));
}
