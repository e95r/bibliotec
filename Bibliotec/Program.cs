using Bibliotec.Database;

namespace Bibliotec;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        DatabaseInitializer.EnsureCreated();
        Application.Run(new MainForm());
    }
}
