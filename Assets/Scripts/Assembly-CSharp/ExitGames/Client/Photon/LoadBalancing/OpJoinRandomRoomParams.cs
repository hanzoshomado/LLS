namespace ExitGames.Client.Photon.LoadBalancing
{
	public class OpJoinRandomRoomParams
	{
		public Hashtable ExpectedCustomRoomProperties;

		public byte ExpectedMaxPlayers;

		public MatchmakingMode MatchingType;

		public TypedLobby TypedLobby;

		public string SqlLobbyFilter;

		public string[] ExpectedUsers;
	}
}
