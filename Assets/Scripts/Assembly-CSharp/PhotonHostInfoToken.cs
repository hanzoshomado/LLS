using System;
using Bolt;
using ExitGames.Client.Photon;
using UdpKit;

[Obsolete("Use the new IBoltPhotonCloudRoomProperties interface on a custom protocol token class to supply room properties instead")]
public class PhotonHostInfoToken : IProtocolToken
{
	public Hashtable CustomRoomProperties = new Hashtable();

	public void Read(UdpPacket packet)
	{
	}

	public void Write(UdpPacket packet)
	{
	}
}
