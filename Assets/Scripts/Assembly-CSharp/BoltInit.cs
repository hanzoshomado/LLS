using Bolt;
using UdpKit;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoltInit : GlobalEventListener
{
	private enum State
	{
		SelectMode = 0,
		SelectMap = 1,
		EnterServerIp = 2,
		StartServer = 3,
		StartClient = 4,
		Started = 5
	}

	private State state;

	private string map;

	private string serverAddress = "127.0.0.1";

	private int serverPort = 25000;

	private void Awake()
	{
		serverPort = BoltRuntimeSettings.instance.debugStartPort;
	}

	private void OnGUI()
	{
		Rect position = new Rect(10f, 10f, 140f, 75f);
		Rect screenRect = new Rect(10f, 90f, Screen.width - 20, Screen.height - 100);
		GUI.Box(position, Resources.Load("BoltLogo") as Texture2D);
		GUILayout.BeginArea(screenRect);
		switch (state)
		{
		case State.SelectMode:
			State_SelectMode();
			break;
		case State.SelectMap:
			State_SelectMap();
			break;
		case State.EnterServerIp:
			State_EnterServerIp();
			break;
		case State.StartClient:
			State_StartClient();
			break;
		case State.StartServer:
			State_StartServer();
			break;
		}
		GUILayout.EndArea();
	}

	private void State_EnterServerIp()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("Server IP: ");
		serverAddress = GUILayout.TextField(serverAddress);
		if (GUILayout.Button("Connect"))
		{
			state = State.StartClient;
		}
		GUILayout.EndHorizontal();
	}

	private void State_SelectMode()
	{
		if (ExpandButton("Server"))
		{
			state = State.SelectMap;
		}
		if (ExpandButton("Client"))
		{
			state = State.EnterServerIp;
		}
	}

	private void State_SelectMap()
	{
		foreach (string allScene in BoltScenes.AllScenes)
		{
			if (SceneManager.GetActiveScene().name != allScene && ExpandButton(allScene))
			{
				map = allScene;
				state = State.StartServer;
			}
		}
	}

	private void State_StartServer()
	{
		BoltLauncher.StartServer(new UdpEndPoint(UdpIPv4Address.Any, (ushort)serverPort));
		state = State.Started;
	}

	private void State_StartClient()
	{
		BoltLauncher.StartClient(UdpEndPoint.Any);
		state = State.Started;
	}

	private bool ExpandButton(string text)
	{
		return GUILayout.Button(text, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
	}

	public override void BoltStartDone()
	{
		if (BoltNetwork.isClient)
		{
			BoltNetwork.Connect(new UdpEndPoint(UdpIPv4Address.Parse(serverAddress), (ushort)serverPort));
		}
		else
		{
			BoltNetwork.LoadScene(map);
		}
	}
}
