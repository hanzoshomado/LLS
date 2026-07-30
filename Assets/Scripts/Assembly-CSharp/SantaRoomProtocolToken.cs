using Bolt;
using UdpKit;

public class SantaRoomProtocolToken : IProtocolToken
{
	public string VersionIdentifier;

	public void Read(UdpPacket packet)
	{
		VersionIdentifier = packet.ReadString();
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteString(VersionIdentifier);
	}
}
