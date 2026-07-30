using System;
using System.Collections.Generic;
using System.Reflection;
using BoltInternal;
using UdpKit;
using UnityEngine;

public static class BoltLauncher
{
	private static UdpPlatform UserAssignedPlatform;

	public static void StartSinglePlayer()
	{
		StartSinglePlayer(BoltRuntimeSettings.instance.GetConfigCopy());
	}

	public static void StartSinglePlayer(BoltConfig config)
	{
		SetUdpPlatform(new NullPlatform());
		Initialize(BoltNetworkModes.Host, UdpEndPoint.Any, config);
	}

	public static void StartServer()
	{
		StartServer(UdpEndPoint.Any);
	}

	public static void StartServer(int port)
	{
		if (port >= 0 && port <= 65535)
		{
			StartServer(new UdpEndPoint(UdpIPv4Address.Any, (ushort)port));
			return;
		}
		throw new ArgumentOutOfRangeException(string.Format("'port' must be >= 0 and <= {0}", ushort.MaxValue));
	}

	public static void StartServer(BoltConfig config)
	{
		StartServer(UdpEndPoint.Any, config);
	}

	public static void StartServer(BoltConfig config, string scene)
	{
		StartServer(UdpEndPoint.Any, config, scene);
	}

	public static void StartServer(UdpEndPoint endpoint)
	{
		StartServer(endpoint, BoltRuntimeSettings.instance.GetConfigCopy());
	}

	public static void StartServer(UdpEndPoint endpoint, string scene)
	{
		StartServer(endpoint, BoltRuntimeSettings.instance.GetConfigCopy(), scene);
	}

	public static void StartServer(UdpEndPoint endpoint, BoltConfig config)
	{
		Initialize(BoltNetworkModes.Host, endpoint, config);
	}

	public static void StartServer(UdpEndPoint endpoint, BoltConfig config, string scene)
	{
		Initialize(BoltNetworkModes.Host, endpoint, config, scene);
	}

	public static void StartClient()
	{
		StartClient(UdpEndPoint.Any);
	}

	public static void StartClient(BoltConfig config)
	{
		StartClient(UdpEndPoint.Any, config);
	}

	public static void StartClient(UdpEndPoint endpoint)
	{
		StartClient(endpoint, BoltRuntimeSettings.instance.GetConfigCopy());
	}

	public static void StartClient(UdpEndPoint endpoint, BoltConfig config)
	{
		Initialize(BoltNetworkModes.Client, endpoint, config);
	}

	public static void StartClient(int port)
	{
		if (port >= 0 && port <= 65535)
		{
			StartClient(new UdpEndPoint(UdpIPv4Address.Any, (ushort)port));
			return;
		}
		throw new ArgumentOutOfRangeException(string.Format("'port' must be >= 0 and <= {0}", ushort.MaxValue));
	}

	public static void Shutdown()
	{
		BoltNetworkInternal.__Shutdown();
	}

	private static void Initialize(BoltNetworkModes modes, UdpEndPoint endpoint, BoltConfig config)
	{
		Initialize(modes, endpoint, config, null);
	}

	private static void Initialize(BoltNetworkModes modes, UdpEndPoint endpoint, BoltConfig config, string scene)
	{
		BoltNetworkInternal.DebugDrawer = new UnityDebugDrawer();
		BoltNetworkInternal.UsingUnityPro = true;
		BoltNetworkInternal.GetSceneName = GetSceneName;
		BoltNetworkInternal.GetSceneIndex = GetSceneIndex;
		BoltNetworkInternal.GetGlobalBehaviourTypes = GetGlobalBehaviourTypes;
		BoltNetworkInternal.EnvironmentSetup = BoltNetworkInternal_User.EnvironmentSetup;
		BoltNetworkInternal.EnvironmentReset = BoltNetworkInternal_User.EnvironmentReset;
		BoltNetworkInternal.__Initialize(modes, endpoint, config, CreateUdpPlatform(), scene);
	}

	private static int GetSceneIndex(string name)
	{
		return BoltScenes_Internal.GetSceneIndex(name);
	}

	private static string GetSceneName(int index)
	{
		return BoltScenes_Internal.GetSceneName(index);
	}

	public static List<STuple<BoltGlobalBehaviourAttribute, Type>> GetGlobalBehaviourTypes()
	{
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		List<STuple<BoltGlobalBehaviourAttribute, Type>> list = new List<STuple<BoltGlobalBehaviourAttribute, Type>>();
		try
		{
			Type[] types = executingAssembly.GetTypes();
			foreach (Type type in types)
			{
				if (typeof(MonoBehaviour).IsAssignableFrom(type))
				{
					BoltGlobalBehaviourAttribute[] array = (BoltGlobalBehaviourAttribute[])type.GetCustomAttributes(typeof(BoltGlobalBehaviourAttribute), false);
					if (array.Length == 1)
					{
						list.Add(new STuple<BoltGlobalBehaviourAttribute, Type>(array[0], type));
					}
				}
			}
		}
		catch
		{
		}
		return list;
	}

	public static void SetUdpPlatform(UdpPlatform platform)
	{
		UserAssignedPlatform = platform;
	}

	public static UdpPlatform CreateUdpPlatform()
	{
		if (UserAssignedPlatform != null)
		{
			return UserAssignedPlatform;
		}
		return new DotNetPlatform();
	}
}
