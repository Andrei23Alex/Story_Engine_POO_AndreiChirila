namespace Story.Player.WinForms;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        // Îi spunem aplicației să deschidă direct PlayerForm-ul nostru vizual
        Application.Run(new PlayerForm());
    }
}