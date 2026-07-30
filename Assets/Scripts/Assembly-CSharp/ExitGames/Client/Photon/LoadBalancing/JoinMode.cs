namespace ExitGames.Client.Photon.LoadBalancing
{
	public enum JoinMode : byte
	{
		Default = 0,
		CreateIfNotExists = 1,
		JoinOrRejoin = 2,
		RejoinOnly = 3
	}
}
