using System.Media;
using System.Runtime.InteropServices;
using System.Text;

internal class Program
{
	private delegate IntPtr WinEventDelegate(IntPtr hWinEventHook, uint eventType,
		IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

	private static SoundPlayer player = new("effect.wav");

	[DllImport("user32.dll")]
	private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax,
		IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc,
		uint idProcess, uint idThread, uint dwFlags);

	[DllImport("user32.dll")]
	private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

	[DllImport("user32.dll")]
	private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

	[DllImport("user32.dll")]
	private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

	[DllImport("user32.dll")]
	private static extern bool TranslateMessage([In] ref MSG lpMsg);

	[DllImport("user32.dll")]
	private static extern IntPtr DispatchMessage([In] ref MSG lpMsg);

	[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
	private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

	[StructLayout(LayoutKind.Sequential)]
	public struct MSG
	{
		public IntPtr hwnd;
		public uint message;
		public IntPtr wParam;
		public IntPtr lParam;
		public uint time;
		public System.Drawing.Point pt;
	}

	// イベント定数
	private const uint EVENT_OBJECT_CREATE = 0x8000;
	private const uint EVENT_OBJECT_SHOW = 0x8002;

	// フラグ
	private const uint WINEVENT_OUTOFCONTEXT = 0x0000;
	private const uint WINEVENT_SKIPOWNTHREAD = 0x0001;

	private static WinEventDelegate winEventProc = new(WinEventCallback);
	private static IntPtr hookId = IntPtr.Zero;

	private static void Main(string[] args)
	{
		// ウィンドウ作成(EVENT_OBJECT_CREATE)とウィンドウ表示(EVENT_OBJECT_SHOW)の両方をフックする
		hookId = SetWinEventHook(
			EVENT_OBJECT_CREATE,
			EVENT_OBJECT_SHOW,
			IntPtr.Zero,
			winEventProc,
			0, 0,
			WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNTHREAD);

		if (hookId == IntPtr.Zero)
		{
			Console.WriteLine("フックの設定に失敗しました");
			return;
		}

		// メッセージループを実行
		MSG msg = new();
		while (GetMessage(out msg, IntPtr.Zero, 0, 0))
		{
			TranslateMessage(ref msg);
			DispatchMessage(ref msg);
		}
	}

	private static IntPtr WinEventCallback(IntPtr hWinEventHook, uint eventType,
		IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
	{
		// ウィンドウ関連のイベントのみを処理
		if (idObject == 0 || idObject == 1)
		{
			StringBuilder sb = new(256);
			GetWindowText(hwnd, sb, sb.Capacity);
			string windowTitle = sb.ToString();

			StringBuilder classSb = new(256);
			GetClassName(hwnd, classSb, classSb.Capacity);
			string className = classSb.ToString();

			if (className == "OperationStatusWindow")
			{
				player.Play();
			}
		}
		return IntPtr.Zero;
	}
}