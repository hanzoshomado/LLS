using System;
using System.Collections.Generic;
using Bolt;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using UdpKit;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : GlobalEventListener
{
	public GameObject MainScreen;

	public GameObject HostScreen;

	public GameObject JoinScreen;

	public GameObject JoinFailScreen;

	public GameObject SteamFailureScreen;

	public GameObject SteamTestModeConfig;

	public GameObject NoServersAvailableScreen;

	public GameObject ConnectingScreen;

	public string HigherVersionAvailableText;

	public Text JoinFailHeader;

	public Text SteamUsernameGreeting;

	public Text HostErrorMessage;

	public InputField HostNameInput;

	public Text HostNameLabel;

	public Toggle HostStartsInSpectatorMode;

	public RectTransform JoinGameButtonContainer;

	public JoinGameClientButton JoinGameButtonPrefab;

	public string SteamUsernameGreetingPrefix;

	public string SteamUsernameGreetingSuffix;

	public Button CreateGameButton;

	public InputField TestNoSteamUsername;

	private int _numGreaterVersionServers;

	private float _lastClientStartTime;

	private void Start()
	{
		BoltLauncher.SetUdpPlatform(new PhotonPlatform());
		MainScreen.SetActive(true);
		HostScreen.SetActive(false);
		JoinScreen.SetActive(false);
		JoinFailScreen.SetActive(false);
		ConnectingScreen.SetActive(false);
		JoinGameButtonContainer.gameObject.SetActive(true);
		SteamFailureScreen.SetActive(false);
		NoServersAvailableScreen.SetActive(false);
		SteamTestModeConfig.SetActive(!Singleton<GameVersionManager>.Instance.IsSteamBuild());
		string savedTestUsername = ((Singleton<SteamManager>.Instance != null) ? Singleton<SteamManager>.Instance.GetSavedTestUsername() : string.Empty);
		TestNoSteamUsername.text = ((savedTestUsername.Length != 0) ? savedTestUsername : ("TestPlayer-" + Guid.NewGuid().ToString().Substring(0, 5)));
		TestNoSteamUsername.onEndEdit.AddListener(onTestUsernameEndEdit);
		if (Singleton<GameVersionManager>.Instance.IsSteamBuild())
		{
			if (Singleton<SteamManager>.Instance.Initialize())
			{
				UpdateSteamUsername();
			}
			else
			{
				SteamFailureScreen.SetActive(true);
				MainScreen.SetActive(false);
			}
		}
		Singleton<GlobalEventManager>.Instance.AddEventListener<OperationResponse>("LoadBalancerOpResponseReceived", onLoadBalancerOpResponse);
		Singleton<GlobalEventManager>.Instance.Dispatch("MainMenuLoaded");
	}

	private void OnDestroy()
	{
		Singleton<GlobalEventManager>.Instance.RemoveEventListener<OperationResponse>("LoadBalancerOpResponseReceived", onLoadBalancerOpResponse);
		// Catches the case where the player types a name and starts a game without ever
		// deselecting the field, which would otherwise never fire onEndEdit.
		if (Singleton<SteamManager>.Instance != null)
		{
			Singleton<SteamManager>.Instance.SaveTestUsername();
		}
	}

	private void onTestUsernameEndEdit(string value)
	{
		Singleton<SteamManager>.Instance.TestSteamUsername = value;
		Singleton<SteamManager>.Instance.SaveTestUsername();
	}

	private void UpdateSteamUsername()
	{
		SteamUsernameGreeting.text = SteamUsernameGreetingPrefix + Singleton<SteamManager>.Instance.GetSteamUsername() + SteamUsernameGreetingSuffix;
	}

	public void OnBackButtonClicked()
	{
		if (!BoltNetwork.isRunning || !isDisconnecting())
		{
		}
		MainScreen.SetActive(true);
		HostScreen.SetActive(false);
		JoinFailScreen.SetActive(false);
		JoinScreen.SetActive(false);
	}

	public override void BoltStartDone()
	{
	}

	private bool isDisconnecting()
	{
		return PhotonPoller.Instance != null && PhotonPoller.Instance.LoadBalancerClient != null && PhotonPoller.Instance.LoadBalancerClient.State == ClientState.Disconnecting;
	}

	public void OnExitButtonClicked()
	{
		Application.Quit();
	}

	public void OnHostButtonClicked()
	{
		if (BoltNetwork.isRunning && !isDisconnecting() && PhotonPoller.Instance != null)
		{
			BoltLauncher.Shutdown();
		}
		MainScreen.SetActive(false);
		HostScreen.SetActive(true);
		JoinScreen.SetActive(false);
		JoinFailScreen.SetActive(false);
		HostErrorMessage.gameObject.SetActive(false);
		AssignRandomHostName();
	}

	public void AssignRandomHostName()
	{
		HostNameLabel.text = Singleton<HostNameGenerator>.Instance.GetRandomHostName();
	}

	public void OnJoinButtonClicked()
	{
		MainScreen.SetActive(false);
		HostScreen.SetActive(false);
		JoinScreen.SetActive(true);
		JoinFailScreen.SetActive(false);
		TransformUtils.DestroyAllChildren(JoinGameButtonContainer);
		refreshJoinSessionList();
	}

	public void OnCreateHostedGameClicked()
	{
		if (!BoltNetwork.isRunning && !isDisconnecting())
		{
			HostErrorMessage.gameObject.SetActive(false);
			Singleton<GameSettingsManger>.Instance.SetServerStartsInSpectatorMode(HostStartsInSpectatorMode.isOn);
			GameModeManager.Instance.ServerSetGameMode(GameModeType.LastSantaStanding);
			BoltLauncher.StartServer();
		}
	}

	private void Update()
	{
		if (HostScreen.activeSelf)
		{
			CreateGameButton.interactable = !BoltNetwork.isRunning && !isDisconnecting() && PhotonPoller.Instance == null;
		}
		if (BoltNetwork.isRunning && BoltNetwork.isServer && PhotonPoller.Instance != null && PhotonPoller.Instance.LoadBalancerClient != null && PhotonPoller.Instance.LoadBalancerClient.State == ClientState.JoinedLobby)
		{
			SantaRoomProtocolToken santaRoomProtocolToken = new SantaRoomProtocolToken();
			santaRoomProtocolToken.VersionIdentifier = Singleton<VersionNumberManager>.Instance.GetVersionString();
			BoltNetwork.SetHostInfo(HostNameLabel.text, santaRoomProtocolToken);
			GameModeManager.Instance.SetGameName(HostNameLabel.text);
			BoltNetwork.LoadScene("Gameplay");
			base.enabled = false;
		}
		if (JoinScreen.activeSelf && !BoltNetwork.isRunning && !BoltNetwork.isConnected && !isDisconnecting() && Time.time > _lastClientStartTime + 2f)
		{
			BoltLauncher.StartClient();
			_lastClientStartTime = Time.time;
		}
		if (NoServersInList())
		{
			refreshJoinSessionList();
		}
		if (HigherServersAvailable())
		{
			SteamUsernameGreeting.text = HigherVersionAvailableText;
		}
		if (!Singleton<GameVersionManager>.Instance.IsSteamBuild())
		{
			Singleton<SteamManager>.Instance.TestSteamUsername = TestNoSteamUsername.text;
			if (!HigherServersAvailable())
			{
				UpdateSteamUsername();
			}
		}
	}

	public override void ConnectFailed(UdpEndPoint endpoint, IProtocolToken token)
	{
		MonoBehaviour.print("Connection failed!");
	}

	public override void ConnectRefused(UdpEndPoint endpoint, IProtocolToken token)
	{
		MonoBehaviour.print("Connection refused!");
	}

	private void onLoadBalancerOpResponse(OperationResponse response)
	{
		if (response.OperationCode == 226 && !string.IsNullOrEmpty(response.DebugMessage))
		{
			JoinScreen.SetActive(false);
			JoinFailScreen.SetActive(true);
			ConnectingScreen.SetActive(false);
			JoinGameButtonContainer.gameObject.SetActive(true);
			JoinFailHeader.text = response.DebugMessage;
		}
	}

	private bool HigherServersAvailable()
	{
		return _numGreaterVersionServers > 0;
	}

	private bool NoServersInList()
	{
		return JoinGameButtonContainer.transform.childCount == 0;
	}

	public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
	{
		refreshJoinSessionList();
		NoServersAvailableScreen.SetActive(NoServersInList());
	}

	private void refreshJoinSessionList()
	{
		if (!BoltNetwork.isRunning || !BoltNetwork.isClient)
		{
			return;
		}
		_numGreaterVersionServers = 0;
		TransformUtils.DestroyAllChildren(JoinGameButtonContainer);
		foreach (KeyValuePair<Guid, UdpSession> session in BoltNetwork.SessionList)
		{
			if (session.Value.Source != UdpSessionSource.Zeus)
			{
				IProtocolToken protocolToken;
				try
				{
					protocolToken = session.Value.GetProtocolToken();
				}
				catch (ArgumentException)
				{
					Debug.Log("Token parse failed for: " + session.Value.HostName);
					protocolToken = null;
				}
				SantaRoomProtocolToken santaRoomProtocolToken = protocolToken as SantaRoomProtocolToken;
				bool flag = santaRoomProtocolToken == null;
				bool flag2 = !flag && Singleton<VersionNumberManager>.Instance.IsCurrentVersionOlderThan(santaRoomProtocolToken.VersionIdentifier);
				bool flag3 = !flag && Singleton<VersionNumberManager>.Instance.IsCurrentVersionNewerThan(santaRoomProtocolToken.VersionIdentifier);
				if (!flag && !flag3 && !flag2)
				{
					JoinGameClientButton joinGameClientButton = UnityEngine.Object.Instantiate(JoinGameButtonPrefab, JoinGameButtonContainer);
					joinGameClientButton.transform.localScale = Vector3.one;
					joinGameClientButton.Initialize(session.Value, OnJoinStart);
				}
				else if (flag2)
				{
					_numGreaterVersionServers++;
				}
			}
		}
	}

	private void OnJoinStart(string gameName)
	{
		GameModeManager.Instance.SetGameName(gameName);
		JoinGameButtonContainer.gameObject.SetActive(false);
		ConnectingScreen.SetActive(true);
	}
}
