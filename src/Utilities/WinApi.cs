using System.Runtime.InteropServices;

namespace AchievementCounter.Utilities;

public class WinApi {
    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetConsoleWindow();
    
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd,int nCmdShow);
    

    public static void HideWindow(IntPtr handle) {
        ShowWindow(handle,SW_HIDE);
    }
    
    public static void ShowWindow(IntPtr handle) {
        ShowWindow(handle, SW_SHOW);
    }
}