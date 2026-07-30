namespace ExitGames.Client.Photon.LoadBalancing
{
	public enum ClientState
	{
		Uninitialized = 0,
		Queued = 1,
		Authenticated = 2,
		JoinedLobby = 3,
		DisconnectingFromMasterserver = 4,
		ConnectingToGameserver = 5,
		ConnectedToGameserver = 6,
		Joining = 7,
		Joined = 8,
		Leaving = 9,
		DisconnectingFromGameserver = 10,
		ConnectingToMasterserver = 11,
		QueuedComingFromGameserver = 12,
		Disconnecting = 13,
		Disconnected = 14,
		ConnectedToMaster = 15,
		ConnectingToNameServer = 16,
		ConnectedToNameServer = 17,
		DisconnectingFromNameServer = 18,
		Authenticating = 19
	}
}
