using System;
using System.Collections.Generic;
using Bolt;
using UdpKit;
using UnityEngine;

public class BoltPhotonInit : GlobalEventListener
{
	public class RoomProtocolToken : IProtocolToken
	{
		public string ArbitraryData;

		public void Read(UdpPacket packet)
		{
			ArbitraryData = packet.ReadString();
		}

		public void Write(UdpPacket packet)
		{
			packet.WriteString(ArbitraryData);
		}
	}

	private enum State
	{
		SelectMode = 0,
		ModeServer = 1,
		ModeClient = 2
	}

	private State _state;

	private void Awake()
	{
		BoltLauncher.SetUdpPlatform(new PhotonPlatform());
	}

	public override void BoltStartDone()
	{
	}

	private void OnGUI()
	{
		switch (_state)
		{
		case State.SelectMode:
			if (GUILayout.Button("Start Client"))
			{
				BoltLauncher.StartClient();
				_state = State.ModeClient;
			}
			if (GUILayout.Button("Start Server"))
			{
				BoltLauncher.StartServer();
				_state = State.ModeServer;
			}
			break;
		case State.ModeServer:
			if (BoltNetwork.isRunning && BoltNetwork.isServer && GUILayout.Button("Publish HostInfo And Load Map"))
			{
				BoltNetwork.SetHostInfo("Erik has Christmas Game!!!!", null);
				BoltNetwork.LoadScene("Gameplay");
			}
			break;
		case State.ModeClient:
			if (!BoltNetwork.isRunning || !BoltNetwork.isClient)
			{
				break;
			}
			GUILayout.Label("Session List");
			{
				foreach (KeyValuePair<Guid, UdpSession> session in BoltNetwork.SessionList)
				{
					RoomProtocolToken roomProtocolToken = null;
					if (GUILayout.Button(string.Concat(session.Value.Source, " / ", session.Value.HostName, " (", session.Value.Id, ")", (roomProtocolToken == null) ? string.Empty : roomProtocolToken.ArbitraryData)))
					{
						BoltNetwork.Connect(session.Value);
					}
				}
				break;
			}
		}
	}
}
