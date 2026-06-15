using PaleteControl.Forms;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

namespace PaleteControl;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.Run(new MainForm());
    }
}
