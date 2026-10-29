namespace AirBridge.Presenters.Contracts;

public interface ILogsView
{
    event EventHandler OnClosed;
    
    void Show(IReadOnlyList<string> initialLines);
    void AppendLine(string line);
    void Close();
}