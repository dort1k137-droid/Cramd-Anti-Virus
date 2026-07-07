namespace ProgramLauncher;

static class Program
{
    /// <summary>
    ///  Cramd Anti-Virus — main entry point.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }    
}