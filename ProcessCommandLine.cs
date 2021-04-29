

// JannaSharp_LobbyCrash.ProcessCommandLine
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using aramboost31;

internal static class ProcessCommandLine
{
	private static class Win32Native
	{
		[Flags]
		public enum OpenProcessDesiredAccessFlags : uint
		{
			PROCESS_VM_READ = 0x10u,
			PROCESS_QUERY_INFORMATION = 0x400u
		}

		public struct ProcessBasicInformation
		{
			public IntPtr Reserved1;

			public IntPtr PebBaseAddress;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
			public IntPtr[] Reserved2;

			public IntPtr UniqueProcessId;

			public IntPtr Reserved3;
		}

		public struct UnicodeString
		{
			public ushort Length;

			public ushort MaximumLength;

			public IntPtr Buffer;
		}

		public struct PEB
		{
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
			public IntPtr[] Reserved;

			public IntPtr ProcessParameters;
		}

		public struct RtlUserProcessParameters
		{
			public uint MaximumLength;

			public uint Length;

			public uint Flags;

			public uint DebugFlags;

			public IntPtr ConsoleHandle;

			public uint ConsoleFlags;

			public IntPtr StandardInput;

			public IntPtr StandardOutput;

			public IntPtr StandardError;

			public UnicodeString CurrentDirectory;

			public IntPtr CurrentDirectoryHandle;

			public UnicodeString DllPath;

			public UnicodeString ImagePathName;

			public UnicodeString CommandLine;
		}

		public const uint PROCESS_BASIC_INFORMATION = 0u;

		[DllImport("ntdll.dll")]
		public static extern uint NtQueryInformationProcess(IntPtr ProcessHandle, uint ProcessInformationClass, IntPtr ProcessInformation, uint ProcessInformationLength, out uint ReturnLength);

		[DllImport("kernel32.dll")]
		public static extern IntPtr OpenProcess(OpenProcessDesiredAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwProcessId);

		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, uint nSize, out uint lpNumberOfBytesRead);

		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(IntPtr hObject);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "CommandLineToArgvW", SetLastError = true)]
		public static extern IntPtr CommandLineToArgv(string lpCmdLine, out int pNumArgs);
	}

	public enum Parameter
	{
		CommandLine,
		WorkingDirectory
	}

	private static bool ReadStructFromProcessMemory<TStruct>(IntPtr hProcess, IntPtr lpBaseAddress, out TStruct val)
	{
		val = default(TStruct);
		int num = Marshal.SizeOf<TStruct>();
		IntPtr ıntPtr = Marshal.AllocHGlobal(num);
		try
		{
			if (Win32Native.ReadProcessMemory(hProcess, lpBaseAddress, ıntPtr, (uint)num, out var lpNumberOfBytesRead) && lpNumberOfBytesRead == num)
			{
				val = Marshal.PtrToStructure<TStruct>(ıntPtr);
				return true;
			}
		}
		finally
		{
			Marshal.FreeHGlobal(ıntPtr);
		}
		return false;
	}

	public static string ErrorToString(int error)
	{
		return (new string[7] { "Success", "Failed to open process for reading", "Failed to query process information", "PEB address was null", "Failed to read PEB information", "Failed to read process parameters", "Failed to read parameter from process" })[Math.Abs(error)];
	}

	public static int Retrieve(Process process, out string parameterValue, Parameter parameter = Parameter.CommandLine)
	{
		int rc = 0;
		parameterValue = null;
		IntPtr hProcess = Win32Native.OpenProcess(Win32Native.OpenProcessDesiredAccessFlags.PROCESS_VM_READ | Win32Native.OpenProcessDesiredAccessFlags.PROCESS_QUERY_INFORMATION, bInheritHandle: false, (uint)process.Id);
		if (hProcess != IntPtr.Zero)
		{
			try
			{
				int num = Marshal.SizeOf<Win32Native.ProcessBasicInformation>();
				IntPtr ıntPtr = Marshal.AllocHGlobal(num);
				try
				{
					if (Win32Native.NtQueryInformationProcess(hProcess, 0u, ıntPtr, (uint)num, out var len) == 0)
					{
						Win32Native.ProcessBasicInformation processBasicInformation = Marshal.PtrToStructure<Win32Native.ProcessBasicInformation>(ıntPtr);
						if (processBasicInformation.PebBaseAddress != IntPtr.Zero)
						{
							if (ReadStructFromProcessMemory<Win32Native.PEB>(hProcess, processBasicInformation.PebBaseAddress, out var val))
							{
								if (ReadStructFromProcessMemory<Win32Native.RtlUserProcessParameters>(hProcess, val.ProcessParameters, out var val2))
								{
									switch (parameter)
									{
										case Parameter.CommandLine:
											parameterValue = ReadUnicodeString(val2.CommandLine);
											break;
										case Parameter.WorkingDirectory:
											parameterValue = ReadUnicodeString(val2.CurrentDirectory);
											break;
									}
								}
								else
								{
									rc = -5;
								}
							}
							else
							{
								rc = -4;
							}
						}
						else
						{
							rc = -3;
						}
					}
					else
					{
						rc = -2;
					}
					string ReadUnicodeString(Win32Native.UnicodeString unicodeString)
					{
						ushort maximumLength = unicodeString.MaximumLength;
						IntPtr ıntPtr2 = Marshal.AllocHGlobal(maximumLength);
						try
						{
							if (Win32Native.ReadProcessMemory(hProcess, unicodeString.Buffer, ıntPtr2, maximumLength, out len))
							{
								rc = 0;
								return Marshal.PtrToStringUni(ıntPtr2);
							}
							rc = -6;
						}
						finally
						{
							Marshal.FreeHGlobal(ıntPtr2);
						}
						return null;
					}
				}
				finally
				{
					Marshal.FreeHGlobal(ıntPtr);
				}
			}
			finally
			{
				Win32Native.CloseHandle(hProcess);
			}
		}
		else
		{
			rc = -1;
		}
		return rc;
	}

	public static IReadOnlyList<string> CommandLineToArgs(string commandLine)
	{
		if (string.IsNullOrEmpty(commandLine))
		{
			return Array.Empty<string>();
		}
		int pNumArgs;
		IntPtr ıntPtr = Win32Native.CommandLineToArgv(commandLine, out pNumArgs);
		if (ıntPtr == IntPtr.Zero)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		try
		{
			string[] array = new string[pNumArgs];
			for (int i = 0; i < array.Length; i++)
			{
				IntPtr ptr = Marshal.ReadIntPtr(ıntPtr, i * IntPtr.Size);
				array[i] = Marshal.PtrToStringUni(ptr);
			}
			return array.ToList().AsReadOnly();
		}
		finally
		{
			Marshal.FreeHGlobal(ıntPtr);
		}
	}
}
