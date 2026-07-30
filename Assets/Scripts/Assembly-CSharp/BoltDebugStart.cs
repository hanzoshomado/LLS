using BoltInternal;
using UdpKit;
using UnityEngine;

public class BoltDebugStart : GlobalEventListenerBase
{
	private UdpEndPoint _serverEndPoint;

	private UdpEndPoint _clientEndPoint;

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Start()
	{
		_serverEndPoint = new UdpEndPoint(UdpIPv4Address.Localhost, (ushort)BoltRuntimeSettings.instance.debugStartPort);
		_clientEndPoint = new UdpEndPoint(UdpIPv4Address.Localhost, 0);
		BoltConfig configCopy = BoltRuntimeSettings.instance.GetConfigCopy();
		configCopy.connectionTimeout = 60000000;
		configCopy.connectionRequestTimeout = 500;
		configCopy.connectionRequestAttempts = 1000;
		if (!string.IsNullOrEmpty(BoltRuntimeSettings.instance.debugStartMapName))
		{
			if (BoltDebugStartSettings.startServer)
			{
				BoltLauncher.StartServer(_serverEndPoint, configCopy);
			}
			else if (BoltDebugStartSettings.startClient)
			{
				BoltLauncher.StartClient(_clientEndPoint, configCopy);
			}
			BoltDebugStartSettings.PositionWindow();
		}
		if (!BoltNetwork.isClient && BoltNetwork.isServer)
		{
		}
	}

	public override void BoltStartDone()
	{
		if (BoltNetwork.isServer)
		{
			BoltNetwork.LoadScene(BoltRuntimeSettings.instance.debugStartMapName);
		}
		else
		{
			BoltNetwork.Connect(_serverEndPoint);
		}
	}

	public override void SceneLoadLocalDone(string arg)
	{
		Object.Destroy(base.gameObject);
	}
}
