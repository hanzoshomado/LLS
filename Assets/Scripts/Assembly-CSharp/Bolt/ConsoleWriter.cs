using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Bolt
{
	public static class ConsoleWriter
	{
		private static class PInvoke
		{
			public const int STD_OUTPUT_HANDLE = -11;

			[DllImport("kernel32.dll", SetLastError = true)]
			public static extern bool AttachConsole(uint dwProcessId);

			[DllImport("kernel32.dll", SetLastError = true)]
			public static extern bool AllocConsole();

			[DllImport("kernel32.dll", SetLastError = true)]
			public static extern bool FreeConsole();

			[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
			public static extern IntPtr GetStdHandle(int nStdHandle);

			[DllImport("kernel32.dll")]
			public static extern bool SetConsoleTitle(string lpConsoleTitle);
		}

		private static TextWriter realOut;

		public static void Open()
		{
			if (realOut == null)
			{
				realOut = Console.Out;
			}
			if (!PInvoke.AttachConsole(uint.MaxValue))
			{
				PInvoke.AllocConsole();
			}
			try
			{
				IntPtr stdHandle = PInvoke.GetStdHandle(-11);
				FileStream stream = new FileStream(stdHandle, FileAccess.Write);
				StreamWriter streamWriter = new StreamWriter(stream, Encoding.ASCII);
				streamWriter.AutoFlush = true;
				Console.SetOut(streamWriter);
			}
			catch (Exception message)
			{
				Debug.Log(message);
			}
		}

		public static void Close()
		{
			PInvoke.FreeConsole();
			Console.SetOut(realOut);
			realOut = null;
		}
	}
}
