using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

internal class BoltDebugStartSettings
{
	private static class HWND
	{
		public static IntPtr NoTopMost = new IntPtr(-2);

		public static IntPtr TopMost = new IntPtr(-1);

		public static IntPtr Top = new IntPtr(0);

		public static IntPtr Bottom = new IntPtr(1);
	}

	private static class SWP
	{
		public static readonly int NOSIZE = 1;

		public static readonly int NOMOVE = 2;

		public static readonly int NOZORDER = 4;

		public static readonly int NOREDRAW = 8;

		public static readonly int NOACTIVATE = 16;

		public static readonly int DRAWFRAME = 32;

		public static readonly int FRAMECHANGED = 32;

		public static readonly int SHOWWINDOW = 64;

		public static readonly int HIDEWINDOW = 128;

		public static readonly int NOCOPYBITS = 256;

		public static readonly int NOOWNERZORDER = 512;

		public static readonly int NOREPOSITION = 512;

		public static readonly int NOSENDCHANGING = 1024;

		public static readonly int DEFERERASE = 8192;

		public static readonly int ASYNCWINDOWPOS = 16384;
	}

	private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

	private static readonly object handle = new object();

	private static HandleRef unityHandle = default(HandleRef);

	public static bool startServer
	{
		get
		{
			return Environment.GetCommandLineArgs().Contains("--bolt-debugstart-server");
		}
	}

	public static bool startClient
	{
		get
		{
			return Environment.GetCommandLineArgs().Contains("--bolt-debugstart-client");
		}
	}

	public static int windowIndex
	{
		get
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			foreach (string text in commandLineArgs)
			{
				if (text.StartsWith("--bolt-window-index-"))
				{
					return int.Parse(text.Replace("--bolt-window-index-", string.Empty));
				}
			}
			return 0;
		}
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int GetWindowThreadProcessId(HandleRef handle, out int processId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr extraData);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int GetSystemMetrics(int index);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

	private static bool Window(IntPtr hWnd, IntPtr lParam)
	{
		int processId = -1;
		int id = Process.GetCurrentProcess().Id;
		GetWindowThreadProcessId(new HandleRef(handle, hWnd), out processId);
		if (processId == id)
		{
			unityHandle = new HandleRef(handle, hWnd);
			return false;
		}
		return true;
	}

	public static void PositionWindow()
	{
		if (!startClient && !startServer)
		{
			return;
		}
		EnumWindows(Window, IntPtr.Zero);
		if (unityHandle.Wrapper == null)
		{
			return;
		}
		int width = Screen.width;
		int height = Screen.height;
		int x = 0;
		int y = 0;
		int systemMetrics = GetSystemMetrics(0);
		int systemMetrics2 = GetSystemMetrics(1);
		if (startServer)
		{
			x = systemMetrics / 2 - width / 2;
			y = systemMetrics2 / 2 - height / 2;
		}
		else
		{
			switch (windowIndex % 4)
			{
			case 1:
				x = systemMetrics - width;
				break;
			case 2:
				y = systemMetrics2 - height;
				break;
			case 3:
				x = systemMetrics - width;
				y = systemMetrics2 - height;
				break;
			}
		}
		SetWindowPos(unityHandle.Handle, HWND.Top, x, y, width, height, SWP.NOSIZE);
	}
}
