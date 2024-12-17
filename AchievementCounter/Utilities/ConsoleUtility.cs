using System.Runtime.InteropServices;

namespace AchievementCounter.Utilities;

public class ConsoleUtility {
	private const int SW_HIDE = 0;
	private const int SW_SHOW = 5;
	
	private static readonly IntPtr _handle = GetConsoleWindow();
	
	[DllImport("kernel32.dll")]
	private static extern IntPtr GetConsoleWindow();
	[DllImport("user32.dll")]
	private static extern bool ShowWindow(IntPtr hWnd,int nCmdShow);
	

	public static void Hide() {
		ShowWindow(_handle,SW_HIDE);
	}
	public static void Show() {
		ShowWindow(_handle,SW_SHOW);
	}
}