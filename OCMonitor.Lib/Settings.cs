namespace OCMonitor.Lib;

public class Settings : ISettings
{
    public bool Contains(string name)
    {
        return false;
    }

    public void SetValue(string name, string value)
    {
    }

    public string GetValue(string name, string value)
    {
        return value;
    }

    public void Remove(string name)
    {
    }
}