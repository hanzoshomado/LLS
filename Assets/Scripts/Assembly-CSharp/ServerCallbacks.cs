using Bolt;

[BoltGlobalBehaviour(BoltNetworkModes.Host)]
public class ServerCallbacks : GlobalEventListener
{
	public override void Connected(BoltConnection connection)
	{
		LogEvent logEvent = LogEvent.Create();
		logEvent.message = string.Concat(connection.RemoteEndPoint, " has connected!");
	}

	public override void Disconnected(BoltConnection connection)
	{
		LogEvent logEvent = LogEvent.Create();
		logEvent.message = string.Concat(connection.RemoteEndPoint, " has disconnected!");
	}
}
