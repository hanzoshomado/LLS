using Bolt;

[BoltGlobalBehaviour]
public class GameConnectionManager : GlobalEventListener
{
	private bool _isDisconnecting;

	private static GameConnectionManager _instance;

	public static GameConnectionManager Instance
	{
		get
		{
			return _instance;
		}
	}

	private void Awake()
	{
		BoltNetwork.RegisterTokenClass<SantaRoomProtocolToken>();
		_instance = this;
	}

	private void OnApplicationQuit()
	{
		if (!BoltNetwork.isServer)
		{
			return;
		}
		foreach (BoltConnection connection in BoltNetwork.connections)
		{
			connection.Disconnect();
		}
	}

	public override void Disconnected(BoltConnection connection)
	{
		if (BoltNetwork.isClient)
		{
			if (Singleton<UIRoot>.Instance != null)
			{
				Singleton<UIRoot>.Instance.DisconnectedPanel.Show();
			}
		}
		else if (BoltNetwork.isServer)
		{
			SantaCharacterController characterWithConnection = CharacterTracker.Instance.GetCharacterWithConnection(connection);
			if (characterWithConnection != null)
			{
				BoltNetwork.Destroy(characterWithConnection.gameObject);
			}
		}
	}
}
