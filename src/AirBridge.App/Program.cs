namespace AirBridge.App;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // A second instance would fight the first one over the AirPlay process and ports.
        using Mutex singleInstance = new(initiallyOwned: true, @"Local\AirBridge.SingleInstance", out var isFirstInstance);
        if (!isFirstInstance) return;

        App app = new();
        app.InitializeComponent();
        app.Run();
    }
}
