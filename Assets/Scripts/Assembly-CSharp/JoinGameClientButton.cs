using System;
using UdpKit;
using UnityEngine;
using UnityEngine.UI;

public class JoinGameClientButton : MonoBehaviour
{
	public Text ButtonLabel;

	private UdpSession _session;

	private Action<string> _onJoining;

	public void Initialize(UdpSession session, Action<string> onJoining)
	{
		_session = session;
		_onJoining = onJoining;
		ButtonLabel.text = string.Concat(session.HostName.ToString(), " (", session.Source, ") (", session.ConnectionsCurrent, "/", session.ConnectionsMax, ")");
	}

	public void OnButtonClicked()
	{
		_onJoining(_session.HostName);
		BoltNetwork.Connect(_session);
	}
}
