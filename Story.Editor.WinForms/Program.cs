namespace Story.Editor.WinForms;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new EditorForm());
    }
}