using System;
using System.Runtime.InteropServices;

namespace OCMonitor.App;

public static class Utils
{
    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_HIDE = 0;
    const int SW_SHOW = 5;
    
    public static void HideCurrentConsoleWindow()
    {
        ShowWindow(GetConsoleWindow(), SW_HIDE);
    }
    
    public static void ShowCurrentConsoleWindow()
    {
        ShowWindow(GetConsoleWindow(), SW_SHOW);
    }
}