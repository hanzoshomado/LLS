namespace ExitGames.Client.Photon.LoadBalancing
{
	public class EnterRoomParams
	{
		public string RoomName;

		public RoomOptions RoomOptions;

		public TypedLobby Lobby;

		public Hashtable PlayerProperties;

		public bool OnGameServer = true;

		public bool CreateIfNotExists;

		public bool RejoinOnly;

		public string[] ExpectedUsers;
	}
}
