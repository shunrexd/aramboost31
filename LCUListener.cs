

// JannaSharp_LobbyCrash.LCUListener
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using aramboost31;

internal class LCUListener
{
	private Thread listeningThread;

	private bool listening = false;

	private ConcurrentDictionary<int, LCUClient> gatheredLCUs = new ConcurrentDictionary<int, LCUClient>();

	private ConcurrentDictionary<int, RiotClient> gatheredRiots = new ConcurrentDictionary<int, RiotClient>();

	public void StartListening()
	{
		listening = true;
		listeningThread = new Thread(ListenForAnyClients);
		listeningThread.Start();
	}

	public void StopListening()
	{
		listening = false;
		listeningThread.Join();
	}

	public void WaitForAnyClient()
	{
		while (GetGatheredLCUs().Count <= 0)
		{
			Thread.Sleep(100);
		}
	}

	public void WaitForAnyRiot()
	{
		while (GetGatheredRiots().Count <= 0)
		{
			Thread.Sleep(100);
		}
	}

	public List<LCUClient> GetGatheredLCUs()
	{
		foreach (KeyValuePair<int, LCUClient> gatheredLCU in gatheredLCUs)
		{
			int key = gatheredLCU.Key;
			LCUClient value = gatheredLCU.Value;
			if (!value.IsAlive())
			{
				gatheredLCUs.TryRemove(key, out var _);
			}
		}
		return gatheredLCUs.Values.ToList();
	}

	public List<RiotClient> GetGatheredRiots()
	{
		foreach (KeyValuePair<int, RiotClient> gatheredRiot in gatheredRiots)
		{
			int key = gatheredRiot.Key;
			RiotClient value = gatheredRiot.Value;
			if (!value.IsAlive())
			{
				gatheredRiots.TryRemove(key, out var _);
			}
		}
		return gatheredRiots.Values.ToList();
	}

	private void ListenForAnyClients()
	{
		while (listening)
		{
			Process[] processesByName = Process.GetProcessesByName("LeagueClientUx");
			foreach (Process process in processesByName)
			{
				int ıd = process.Id;
				if (!IsAlreadyFound(ıd))
				{
					ProcessCommandLine.Retrieve(process, out var parameterValue);
					string value = Regex.Match(parameterValue, "(\"--remoting-auth-token=)([^\"]*)(\")").Groups[2].Value;
					string appPort = int.Parse(Regex.Match(parameterValue, "(\"--app-port=)([^\"]*)(\")").Groups[2].Value).ToString();
					LCUClient value2 = new LCUClient(appPort, value, process.Id);
					gatheredLCUs.TryAdd(process.Id, value2);
				}
			}
			Process[] processesByName2 = Process.GetProcessesByName("RiotClientUx");
			foreach (Process process2 in processesByName2)
			{
				int ıd2 = process2.Id;
				if (!IsAlreadyFound(ıd2))
				{
					ProcessCommandLine.Retrieve(process2, out var parameterValue2);
					string value3 = Regex.Match(parameterValue2, "(--remoting-auth-token=)([^\\s]+)").Groups[2].Value;
					string appPort2 = int.Parse(Regex.Match(parameterValue2, "(--app-port=)([0-9]+)").Groups[2].Value).ToString();
					RiotClient value4 = new RiotClient(appPort2, value3, process2.Id);
					gatheredRiots.TryAdd(process2.Id, value4);
				}
			}
			Thread.Sleep(100);
		}
	}

	private bool IsAlreadyFound(int pid)
	{
		foreach (KeyValuePair<int, LCUClient> gatheredLCU in gatheredLCUs)
		{
			if (gatheredLCU.Key == pid)
			{
				return true;
			}
		}
		foreach (KeyValuePair<int, RiotClient> gatheredRiot in gatheredRiots)
		{
			if (gatheredRiot.Key == pid)
			{
				return true;
			}
		}
		return false;
	}
}
