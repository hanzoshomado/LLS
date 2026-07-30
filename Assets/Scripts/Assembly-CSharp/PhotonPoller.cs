using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using UdpKit;
using UnityEngine;

public class PhotonPoller : GlobalEventListener
{
	public enum ConnectState
	{
		Idle = 0,
		JoinRoomPending = 1,
		DirectPending = 2,
		DirectFailed = 3,
		DirectSuccess = 4,
		RelayPending = 5,
		RelayFailed = 6,
		RelaySuccess = 7
	}

	private class PhotonSession : UdpSession
	{
		internal Guid _id;

		internal Guid _socketPeerId;

		internal int _playerCount;

		internal int _playerLimit;

		internal string _roomName;

		internal byte[] _hostData;

		public override int ConnectionsCurrent
		{
			get
			{
				return _playerCount;
			}
		}

		public override int ConnectionsMax
		{
			get
			{
				return _playerLimit;
			}
		}

		public override bool HasLan
		{
			get
			{
				return false;
			}
		}

		public override bool HasWan
		{
			get
			{
				return true;
			}
		}

		public override string HostName
		{
			get
			{
				return _roomName;
			}
		}

		public override Guid Id
		{
			get
			{
				return _id;
			}
		}

		public override bool IsDedicatedServer
		{
			get
			{
				return false;
			}
		}

		public override UdpEndPoint LanEndPoint
		{
			get
			{
				return default(UdpEndPoint);
			}
		}

		public override UdpSessionSource Source
		{
			get
			{
				return UdpSessionSource.Photon;
			}
		}

		public override UdpEndPoint WanEndPoint
		{
			get
			{
				return default(UdpEndPoint);
			}
		}

		public override byte[] HostData
		{
			get
			{
				return _hostData;
			}
		}

		public override object HostObject { get; set; }

		public override UdpSession Clone()
		{
			return (UdpSession)MemberwiseClone();
		}
	}

	private class PhotonLoadBalancingClient : LoadBalancingClient
	{
		public override void DebugReturn(DebugLevel level, string message)
		{
			switch (level)
			{
			case DebugLevel.ERROR:
				Debug.LogError(message);
				break;
			case DebugLevel.WARNING:
				Debug.LogWarning(message);
				break;
			case DebugLevel.INFO:
				Debug.Log(message);
				break;
			case DebugLevel.ALL:
				Debug.Log(message);
				break;
			}
		}
	}

	private class PhotonPacket
	{
		public byte[] Data;

		public int Remote;

		public PhotonPacket()
		{
		}

		public PhotonPacket(int size)
		{
			Data = new byte[size];
		}
	}

	private class SynchronizedQueue<T>
	{
		private Queue<T> queue = new Queue<T>();

		public int Count
		{
			get
			{
				lock (queue)
				{
					return queue.Count;
				}
			}
		}

		public void Clear()
		{
			lock (queue)
			{
				queue.Clear();
			}
		}

		public void Enqueue(T item)
		{
			lock (queue)
			{
				queue.Enqueue(item);
			}
		}

		public bool TryDequeue(out T item)
		{
			lock (queue)
			{
				if (queue.Count > 0)
				{
					item = queue.Dequeue();
					return true;
				}
				item = default(T);
				return false;
			}
		}
	}

	private struct Timer
	{
		private float _expire;

		public bool Expired
		{
			get
			{
				return Time.realtimeSinceStartup >= _expire;
			}
		}

		public bool Waiting
		{
			get
			{
				return Time.realtimeSinceStartup < _expire;
			}
		}

		public Timer(float wait)
		{
			_expire = Time.realtimeSinceStartup + wait;
		}
	}

	private static PhotonPoller _instance;

	private const byte DATA_EVENT_CODE = 1;

	private const float ROOM_UPDATE_RATE = 5f;

	private const float ROOM_CREATE_TIMEOUT = 2f;

	private const float ROOM_JOIN_TIMEOUT = 10f;

	private Timer _roomUpdateTimer;

	private ClientState _state;

	public ConnectState _connectState;

	private Coroutine _currentConnectRoutine;

	private PhotonPlatformConfig _config;

	private PhotonLoadBalancingClient _lbClient;

	private SynchronizedQueue<PhotonPacket> _packetPool = new SynchronizedQueue<PhotonPacket>();

	private SynchronizedQueue<PhotonPacket> _packetSend = new SynchronizedQueue<PhotonPacket>();

	private SynchronizedQueue<PhotonPacket> _packetRecv = new SynchronizedQueue<PhotonPacket>();

	public static PhotonPoller Instance
	{
		get
		{
			return _instance;
		}
	}

	public LoadBalancingClient LoadBalancerClient
	{
		get
		{
			return _lbClient;
		}
	}

	public int HostPlayerId
	{
		get
		{
			if (_lbClient == null)
			{
				return -1;
			}
			return _lbClient.CurrentRoom.MasterClientId;
		}
	}

	public static void RegisterTokenClass()
	{
		BoltNetwork.RegisterTokenClass<PhotonHostInfoToken>();
	}

	public static void CreatePoller(PhotonPlatformConfig config)
	{
		if ((bool)_instance)
		{
			return;
		}
		PhotonPoller[] array = UnityEngine.Object.FindObjectsOfType<PhotonPoller>();
		if (array.Length == 0)
		{
			_instance = new GameObject(typeof(PhotonPoller).Name).AddComponent<PhotonPoller>();
		}
		if (array.Length == 1)
		{
			_instance = array[0];
		}
		if (array.Length >= 2)
		{
			_instance = array[0];
			for (int i = 1; i < array.Length; i++)
			{
				UnityEngine.Object.Destroy(array[i].gameObject);
			}
		}
		_instance._config = config;
		UnityEngine.Object.DontDestroyOnLoad(_instance);
	}

	private void Disconnect()
	{
		if (_lbClient != null)
		{
			_lbClient.Disconnect();
			_lbClient = null;
		}
	}

	private void OnDestroy()
	{
		Disconnect();
	}

	protected new void OnDisable()
	{
		base.OnDisable();
		Disconnect();
	}

	private void Start()
	{
		Disconnect();
		_lbClient = new PhotonLoadBalancingClient();
		_lbClient.OnEventAction += OnEventAction;
		_lbClient.OnOpResponseAction += OnOpResponseAction;
		_lbClient.OnStateChangeAction += OnStateChangeAction;
		_lbClient.AutoJoinLobby = true;
		_lbClient.AppId = _config.AppId;
		if (_config.UseOnPremise)
		{
			_lbClient.Connect(_config.OnPremiseServerIpAddress, _config.AppId, "1.0", string.Empty, null);
		}
		else
		{
			_lbClient.ConnectToRegionMaster(_config.RegionMaster);
		}
	}

	private void Update()
	{
		if (_lbClient != null)
		{
			if (_lbClient.State == ClientState.Joined && _lbClient.State != _state)
			{
				_packetSend.Clear();
				_packetRecv.Clear();
			}
			if (_lbClient.State == ClientState.JoinedLobby && _roomUpdateTimer.Expired)
			{
				BoltNetwork.UpdateSessionList(FetchSessionListFromPhoton());
				_roomUpdateTimer = new Timer(5f);
			}
			PollIn();
			PollOut();
			_state = _lbClient.State;
		}
	}

	private Map<Guid, UdpSession> FetchSessionListFromPhoton()
	{
		Map<Guid, UdpSession> map = new Map<Guid, UdpSession>();
		foreach (KeyValuePair<string, RoomInfo> roomInfo in LoadBalancerClient.RoomInfoList)
		{
			if (!roomInfo.Value.IsOpen)
			{
				continue;
			}
			try
			{
				PhotonSession photonSession = new PhotonSession();
				photonSession._roomName = roomInfo.Key;
				photonSession._id = new Guid((roomInfo.Value.CustomProperties["UdpSessionId"] as string) ?? string.Empty);
				photonSession._hostData = roomInfo.Value.CustomProperties["UserToken"] as byte[];
				if (_config.UsePunchThrough)
				{
					try
					{
						photonSession._socketPeerId = new Guid((roomInfo.Value.CustomProperties["SocketPeerId"] as string) ?? string.Empty);
					}
					catch
					{
					}
				}
				photonSession._playerCount = roomInfo.Value.PlayerCount;
				photonSession._playerLimit = roomInfo.Value.MaxPlayers;
				map = map.Add(photonSession.Id, photonSession);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
		return map;
	}

	public override void ConnectFailed(UdpEndPoint endpoint, IProtocolToken token)
	{
		if (_connectState == ConnectState.DirectPending)
		{
			ChangeState(ConnectState.DirectFailed);
		}
		if (_connectState == ConnectState.RelayPending)
		{
			ChangeState(ConnectState.RelayFailed);
		}
	}

	public override void Connected(BoltConnection connection)
	{
		if (_connectState == ConnectState.DirectPending)
		{
			ChangeState(ConnectState.DirectSuccess);
		}
		if (_connectState == ConnectState.RelayPending)
		{
			ChangeState(ConnectState.RelaySuccess);
		}
	}

	public override void BoltShutdownBegin(AddCallback registerDoneCallback)
	{
		base.BoltShutdownBegin(registerDoneCallback);
		UnityEngine.Object.Destroy(Instance.gameObject);
	}

	private void OnStateChangeAction(ClientState obj)
	{
	}

	private void OnOpResponseAction(OperationResponse obj)
	{
		Singleton<GlobalEventManager>.Instance.Dispatch("LoadBalancerOpResponseReceived", obj);
	}

	private void OnEventAction(EventData obj)
	{
		switch (obj.Code)
		{
		case 254:
			if (BoltNetwork.server != null && (int)obj.Parameters[254] == 1)
			{
				BoltNetwork.server.Disconnect();
			}
			break;
		case 1:
		{
			int remote = (int)obj.Parameters[254];
			byte[] data = (byte[])obj.Parameters[245];
			_packetRecv.Enqueue(new PhotonPacket
			{
				Data = data,
				Remote = remote
			});
			break;
		}
		}
	}

	private void PollIn()
	{
		while (_lbClient.loadBalancingPeer.DispatchIncomingCommands())
		{
		}
	}

	private void PollOut()
	{
		PhotonPacket item;
		while (_packetSend.TryDequeue(out item))
		{
			_lbClient.loadBalancingPeer.OpRaiseEvent(1, item.Data, false, new RaiseEventOptions
			{
				CachingOption = EventCaching.DoNotCache,
				SequenceChannel = 0,
				TargetActors = new int[1] { item.Remote }
			});
		}
		_lbClient.loadBalancingPeer.SendOutgoingCommands();
	}

	private byte[] CloneArray(byte[] array, int size)
	{
		byte[] array2 = new byte[size];
		Buffer.BlockCopy(array, 0, array2, 0, size);
		return array2;
	}

	public static void UpdateHostInfo(object protocolToken)
	{
		Instance.StartCoroutine(Instance.UpdateHostInfoRoutine(protocolToken));
	}

	private IEnumerator UpdateHostInfoRoutine(object protocolToken)
	{
		Timer t = new Timer(2f);
		while (_lbClient.State != ClientState.JoinedLobby && t.Waiting)
		{
			yield return null;
		}
		if (_lbClient.State == ClientState.Joined)
		{
			ExitGames.Client.Photon.Hashtable customRoomProperties = null;
			PhotonHostInfoToken hostInfoToken = protocolToken as PhotonHostInfoToken;
			if (hostInfoToken != null)
			{
				customRoomProperties = hostInfoToken.CustomRoomProperties;
			}
			IBoltPhotonCloudRoomProperties boltPhotonCloudRoomProperties = protocolToken as IBoltPhotonCloudRoomProperties;
			if (boltPhotonCloudRoomProperties != null)
			{
				customRoomProperties = boltPhotonCloudRoomProperties.CustomRoomProperties;
			}
			if (customRoomProperties == null)
			{
				customRoomProperties = new ExitGames.Client.Photon.Hashtable();
			}
			if (protocolToken != null && !(protocolToken is PhotonHostInfoToken))
			{
				customRoomProperties["UserToken"] = ((IProtocolToken)protocolToken).ToByteArray();
			}
			_lbClient.OpSetCustomPropertiesOfRoom(customRoomProperties);
			BoltConsole.Write("Updating room properties");
		}
	}

	public static void SetHostInfo(string servername, bool dedicated, object protocolToken)
	{
		Instance.StartCoroutine(Instance.SetHostInfoRoutine(servername, dedicated, protocolToken));
	}

	private IEnumerator SetHostInfoRoutine(string servername, bool dedicated, object protocolToken)
	{
		Timer t = new Timer(2f);
		while (_lbClient.State != ClientState.JoinedLobby && t.Waiting)
		{
			yield return null;
		}
		if (_lbClient.State == ClientState.JoinedLobby)
		{
			int maxPlayers = ((!dedicated) ? (BoltNetwork.maxConnections + 1) : BoltNetwork.maxConnections);
			ExitGames.Client.Photon.Hashtable customRoomProperties = null;
			PhotonHostInfoToken hostInfoToken = protocolToken as PhotonHostInfoToken;
			if (hostInfoToken != null)
			{
				customRoomProperties = hostInfoToken.CustomRoomProperties;
			}
			IBoltPhotonCloudRoomProperties boltPhotonCloudRoomProperties = protocolToken as IBoltPhotonCloudRoomProperties;
			if (boltPhotonCloudRoomProperties != null)
			{
				customRoomProperties = boltPhotonCloudRoomProperties.CustomRoomProperties;
			}
			if (customRoomProperties == null)
			{
				customRoomProperties = new ExitGames.Client.Photon.Hashtable();
			}
			if (protocolToken != null && !(protocolToken is PhotonHostInfoToken))
			{
				customRoomProperties["UserToken"] = ((IProtocolToken)protocolToken).ToByteArray();
			}
			customRoomProperties["UdpSessionId"] = Guid.NewGuid().ToString();
			if (_config.UsePunchThrough)
			{
				customRoomProperties["SocketPeerId"] = BoltNetwork.UdpSocket.SocketPeerId.ToString();
			}
			_lbClient.OpCreateRoom(servername, new RoomOptions
			{
				CustomRoomProperties = customRoomProperties,
				CustomRoomPropertiesForLobby = new string[3] { "UdpSessionId", "SocketPeerId", "UserToken" },
				MaxPlayers = (byte)maxPlayers
			}, null);
		}
	}

	public static bool JoinSession(UdpSession session, object token)
	{
		if (session.Source == UdpSessionSource.Photon)
		{
			if (Instance._connectState != ConnectState.Idle)
			{
				return true;
			}
			if (Instance._lbClient.State != ClientState.JoinedLobby)
			{
				return true;
			}
			Instance._currentConnectRoutine = Instance.StartCoroutine(Instance.JoinSessionRoutine(session, token));
			return true;
		}
		return false;
	}

	private IEnumerator JoinSessionRoutine(UdpSession session, object token)
	{
		ChangeState(ConnectState.JoinRoomPending);
		LoadBalancerClient.OpJoinRoom(session.HostName);
		Timer timer = new Timer(10f);
		while (_lbClient.State != ClientState.Joined && timer.Waiting)
		{
			yield return null;
		}
		if (_lbClient.State != ClientState.Joined)
		{
			_currentConnectRoutine = null;
			ChangeState(ConnectState.Idle);
			yield break;
		}
		Zeus.RequestSessionList();
		yield return new WaitForSeconds(5f);
		Debug.Log("usePunch: " + _config.UsePunchThrough);
		if (_config.UsePunchThrough)
		{
			PhotonSession s = (PhotonSession)session;
			if (s._socketPeerId != Guid.Empty)
			{
				Debug.Log("Sessions: looking for: " + s._socketPeerId.ToString());
				foreach (KeyValuePair<Guid, UdpSession> session2 in BoltNetwork.SessionList)
				{
					KeyValuePair<Guid, UdpSession> keyValuePair = session2;
				}
				UdpSession zeusSession;
				bool tryFind = BoltNetwork.SessionList.TryFind(s._socketPeerId, out zeusSession);
				Debug.Log("tryFind: " + tryFind);
				if (tryFind && zeusSession.Source == UdpSessionSource.Zeus)
				{
					ChangeState(ConnectState.DirectPending);
					Debug.Log("PhotonPoller::PreConnectDirect");
					BoltNetwork.Connect(zeusSession, token as IProtocolToken);
					Debug.Log("PhotonPoller::PosConnectDirect");
					while (_connectState == ConnectState.DirectPending)
					{
						yield return null;
					}
					if (_connectState == ConnectState.DirectSuccess)
					{
						ChangeState(ConnectState.Idle);
						yield break;
					}
				}
			}
		}
		_currentConnectRoutine = null;
		if (_lbClient.State != ClientState.Joined)
		{
			ChangeState(ConnectState.JoinRoomPending);
			LoadBalancerClient.OpJoinRoom(session.HostName);
			timer = new Timer(10f);
			while (_lbClient.State != ClientState.Joined && timer.Waiting)
			{
				yield return null;
			}
		}
		if (_lbClient.State != ClientState.Joined)
		{
			_currentConnectRoutine = null;
			Debug.Log("Failed to join room");
			ChangeState(ConnectState.Idle);
			yield break;
		}
		ChangeState(ConnectState.RelayPending);
		Debug.Log("PhotonPoller::PreConnectRelay");
		Debug.Log("PhotonPoller::LBClientState: " + _lbClient.State);
		BoltNetwork.Connect(new UdpEndPoint(new UdpIPv4Address((uint)HostPlayerId), 0), token as IProtocolToken);
		Debug.Log("PhotonPoller::PostConnectRelay");
		while (_connectState == ConnectState.RelayPending)
		{
			yield return null;
		}
		if (_connectState == ConnectState.RelayFailed)
		{
			Debug.Log("-- THIS SHOULDN'T HAPPEN -- Connecting to photon room '{0}' failed " + session.HostName);
		}
		ChangeState(ConnectState.Idle);
	}

	private PhotonPacket AllocPacket(int size)
	{
		PhotonPacket item;
		if (_packetPool.TryDequeue(out item))
		{
			Array.Resize(ref item.Data, size);
			return item;
		}
		return new PhotonPacket(size);
	}

	private void ChangeState(ConnectState state)
	{
		Debug.Log(string.Format("Changing Connect State: {0} => {1}", _connectState, state));
		_connectState = state;
	}

	private void FreePacket(PhotonPacket packet)
	{
		_packetPool.Enqueue(packet);
	}

	public static int RecvFrom(byte[] buffer, int bufferSize, ref UdpEndPoint endpoint)
	{
		PhotonPacket item;
		if (Instance._packetRecv.TryDequeue(out item))
		{
			Buffer.BlockCopy(item.Data, 0, buffer, 0, item.Data.Length);
			endpoint = new UdpEndPoint(new UdpIPv4Address((uint)item.Remote), 0);
			return item.Data.Length;
		}
		return -1;
	}

	public static bool RecvPoll()
	{
		return Instance._packetRecv.Count > 0;
	}

	public static int SendTo(byte[] buffer, int bytesToSend, UdpEndPoint endpoint)
	{
		PhotonPacket photonPacket = Instance.AllocPacket(bytesToSend);
		UdpIPv4Address address = endpoint.Address;
		photonPacket.Remote = (int)address.Packed;
		Buffer.BlockCopy(buffer, 0, photonPacket.Data, 0, bytesToSend);
		Instance._packetSend.Enqueue(photonPacket);
		return bytesToSend;
	}
}
