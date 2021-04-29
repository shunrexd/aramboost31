

// JannaSharp_LobbyCrash.ProcessHelper
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using aramboost31;

internal static class ProcessHelper
{
	public enum Options
	{
		List,
		Kill,
		Suspend,
		Resume
	}

	[Flags]
	public enum ThreadAccess
	{
		TERMINATE = 0x1,
		SUSPEND_RESUME = 0x2,
		GET_CONTEXT = 0x8,
		SET_CONTEXT = 0x10,
		SET_INFORMATION = 0x20,
		QUERY_INFORMATION = 0x40,
		SET_THREAD_TOKEN = 0x80,
		IMPERSONATE = 0x100,
		DIRECT_IMPERSONATION = 0x200
	}

	public class Param
	{
		public int PID { get; set; }

		public string Expression { get; set; }

		public Options Option { get; set; }
	}

	private const int SW_HIDE = 0;

	[DllImport("kernel32.dll")]
	private static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

	[DllImport("kernel32.dll")]
	private static extern uint SuspendThread(IntPtr hThread);

	[DllImport("kernel32.dll")]
	private static extern int ResumeThread(IntPtr hThread);

	[DllImport("user32.dll")]
	private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	public static void HideWindow(int processId)
	{
		IntPtr mainWindowHandle = Process.GetProcessById(processId).MainWindowHandle;
		ShowWindow(mainWindowHandle, 0);
	}

	public static void SuspendProcess(int processId)
	{
		foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
		{
			IntPtr ıntPtr = OpenThread(ThreadAccess.SUSPEND_RESUME, bInheritHandle: false, (uint)thread.Id);
			if (ıntPtr == IntPtr.Zero)
			{
				break;
			}
			SuspendThread(ıntPtr);
		}
	}

	public static void ResumeProcess(int processId)
	{
		foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
		{
			IntPtr ıntPtr = OpenThread(ThreadAccess.SUSPEND_RESUME, bInheritHandle: false, (uint)thread.Id);
			if (ıntPtr == IntPtr.Zero)
			{
				break;
			}
			ResumeThread(ıntPtr);
		}
	}

	public static void KillProcess(int processId)
	{
		Process.GetProcessById(processId).Kill();
	}
}
