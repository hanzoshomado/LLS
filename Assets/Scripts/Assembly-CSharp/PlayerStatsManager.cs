using System.Collections.Generic;
using System.Globalization;
using Bolt;
using UnityEngine;

// Session scoreboard. The host is authoritative: it counts kills and wins, decides who holds
// the crown, and broadcasts each player's row to everyone. Clients only ever apply what they
// are told, so every screen shows the same numbers.
//
// Wins can't live on ISantaState - the Bolt state schema is compiled into bolt.user.dll and
// no new properties can be added - so rows travel as LogEvents keyed by username.
[BoltGlobalBehaviour]
public class PlayerStatsManager : GlobalEventListener
{
	public class PlayerStats
	{
		public int Wins;
		public int Kills;
		public bool HasCrown;
		// When this player most recently reached their current win count. Used to break
		// crown ties in favour of whoever got there first.
		public float ReachedWinsAtTime;
	}

	public const string StatsPrefix = "#stats ";

	public const string EliminationPrefix = "#elim ";

	// Unlike the elimination popup this one goes to everybody, so the whole lobby sees who killed
	// who. Fields are pipe separated because usernames can contain spaces.
	public const string KillFeedPrefix = "#feed ";

	private class PlayerIdentity
	{
		public BoltConnection Connection;

		// The name the player actually typed, used to match them back to this slot if they
		// disconnect and come back.
		public string RawName;

		public string DisplayName;

		public bool IsConnected;
	}

	private static PlayerStatsManager _instance;

	private readonly Dictionary<string, PlayerStats> _statsByUsername = new Dictionary<string, PlayerStats>();

	// Host only. Rows are keyed by display name, so those names have to be unique.
	private readonly List<PlayerIdentity> _identities = new List<PlayerIdentity>();

	private string _crownHolder = string.Empty;

	public static PlayerStatsManager Instance
	{
		get
		{
			return _instance;
		}
	}

	private void Awake()
	{
		_instance = this;
	}

	private void OnDestroy()
	{
		if (_instance == this)
		{
			_instance = null;
		}
	}

	public static bool IsStatsMessage(string message)
	{
		return message != null && message.StartsWith(StatsPrefix, System.StringComparison.Ordinal);
	}

	public static bool IsEliminationMessage(string message)
	{
		return message != null && message.StartsWith(EliminationPrefix, System.StringComparison.Ordinal);
	}

	// Anything this class routes over LogEvent, so the on-screen log box can skip it.
	public static bool IsKillFeedMessage(string message)
	{
		return message != null && message.StartsWith(KillFeedPrefix, System.StringComparison.Ordinal);
	}

	public static bool IsInternalMessage(string message)
	{
		return IsStatsMessage(message) || IsEliminationMessage(message) || IsKillFeedMessage(message);
	}

	// Host only. Nothing stops two players typing the same name, but stats rows and name tags
	// have to tell them apart, so the second one onwards gets a " #n" suffix. The mapping is
	// per connection and holds for the session - if it were recomputed each round the suffixes
	// could swap between players and take their stats with them.
	public static string ResolveDisplayName(BoltConnection connection, string rawName)
	{
		if (_instance == null)
		{
			return rawName;
		}
		return _instance.resolveDisplayName(connection, rawName);
	}

	private string resolveDisplayName(BoltConnection connection, string rawName)
	{
		if (string.IsNullOrEmpty(rawName))
		{
			rawName = "Santa";
		}
		for (int i = 0; i < _identities.Count; i++)
		{
			// Null connection is the host's own character, so only trust the match on a slot
			// that is actually occupied - a vacated slot also holds a null connection.
			if (_identities[i].IsConnected && _identities[i].Connection == connection)
			{
				return _identities[i].DisplayName;
			}
		}
		// Someone rejoining under the same name takes their old row back rather than being
		// handed a suffix and a second, empty row beside it.
		for (int j = 0; j < _identities.Count; j++)
		{
			if (!_identities[j].IsConnected && _identities[j].RawName == rawName)
			{
				_identities[j].Connection = connection;
				_identities[j].IsConnected = true;
				return _identities[j].DisplayName;
			}
		}
		string text = rawName;
		int num = 2;
		while (isDisplayNameTaken(text))
		{
			text = rawName + " #" + num.ToString(CultureInfo.InvariantCulture);
			num++;
		}
		PlayerIdentity playerIdentity = new PlayerIdentity();
		playerIdentity.Connection = connection;
		playerIdentity.RawName = rawName;
		playerIdentity.DisplayName = text;
		playerIdentity.IsConnected = true;
		_identities.Add(playerIdentity);
		return text;
	}

	// Keep the row on the board, but free the slot so the same player reclaims it on return.
	public override void Disconnected(BoltConnection connection)
	{
		if (!BoltNetwork.isServer)
		{
			return;
		}
		for (int i = 0; i < _identities.Count; i++)
		{
			if (_identities[i].IsConnected && _identities[i].Connection == connection)
			{
				_identities[i].IsConnected = false;
				_identities[i].Connection = null;
				return;
			}
		}
	}

	private bool isDisplayNameTaken(string displayName)
	{
		for (int i = 0; i < _identities.Count; i++)
		{
			if (_identities[i].DisplayName == displayName)
			{
				return true;
			}
		}
		return false;
	}

	// Null when the player has no record yet, which the UI reads as zero wins and no crown.
	public PlayerStats GetStatsFor(string username)
	{
		PlayerStats playerStats;
		if (username != null && _statsByUsername.TryGetValue(username, out playerStats))
		{
			return playerStats;
		}
		return null;
	}

	// Leaderboard order: most wins, then most kills, then name so it never jitters between frames.
	public List<KeyValuePair<string, PlayerStats>> GetLeaderboard()
	{
		List<KeyValuePair<string, PlayerStats>> list = new List<KeyValuePair<string, PlayerStats>>(_statsByUsername);
		list.Sort(delegate(KeyValuePair<string, PlayerStats> a, KeyValuePair<string, PlayerStats> b)
		{
			if (a.Value.Wins != b.Value.Wins)
			{
				return b.Value.Wins.CompareTo(a.Value.Wins);
			}
			if (a.Value.Kills != b.Value.Kills)
			{
				return b.Value.Kills.CompareTo(a.Value.Kills);
			}
			return string.Compare(a.Key, b.Key, System.StringComparison.Ordinal);
		});
		return list;
	}

	public static void ReportKill(SantaCharacterController killer, SantaCharacterController victim, WeaponType weaponUsed)
	{
		if (!BoltNetwork.isServer || _instance == null || killer == null)
		{
			return;
		}
		_instance.serverAddKill(getUsernameOf(killer));
		_instance.serverNotifyElimination(killer, getUsernameOf(victim));
		_instance.serverBroadcastKillFeed(killer, victim, weaponUsed);
	}

	// Distance is measured here rather than sent from the shooter so it can't be spoofed, and
	// rounded to a whole unit because nobody reads decimals in a kill feed.
	private void serverBroadcastKillFeed(SantaCharacterController killer, SantaCharacterController victim, WeaponType weaponUsed)
	{
		string killerName = getUsernameOf(killer);
		string victimName = getUsernameOf(victim);
		if (killerName.Length == 0 || victimName.Length == 0 || victim == null)
		{
			return;
		}
		int distance = Mathf.RoundToInt(Vector3.Distance(killer.transform.position, victim.transform.position));
		int verbSeed = ((killer.entity != null && killer.entity.isAttached) ? killer.state.ExecutingAttackID : 0);
		LogEvent logEvent = LogEvent.Create(GlobalTargets.Everyone);
		logEvent.message = string.Concat(new string[]
		{
			KillFeedPrefix,
			killerName,
			"|",
			victimName,
			"|",
			((int)weaponUsed).ToString(CultureInfo.InvariantCulture),
			"|",
			distance.ToString(CultureInfo.InvariantCulture),
			"|",
			verbSeed.ToString(CultureInfo.InvariantCulture)
		});
		logEvent.Send();
	}

	// The popup belongs only to whoever landed the kill, so it goes to that one connection
	// rather than being broadcast with a name for everyone else to filter on.
	private void serverNotifyElimination(SantaCharacterController killer, string victimName)
	{
		if (victimName.Length == 0 || killer.entity == null || !killer.entity.isAttached)
		{
			return;
		}
		BoltConnection controller = killer.entity.controller;
		if (controller == null)
		{
			// No controlling connection means the host owns this character, so it's local.
			PlayerStatsHUD.ShowElimination(victimName);
			return;
		}
		LogEvent logEvent = LogEvent.Create(controller);
		logEvent.message = EliminationPrefix + victimName;
		logEvent.Send();
	}

	public static void ReportWin(SantaCharacterController winner)
	{
		if (!BoltNetwork.isServer || _instance == null || winner == null)
		{
			return;
		}
		_instance.serverAddWin(getUsernameOf(winner));
	}

	private static string getUsernameOf(SantaCharacterController character)
	{
		if (character == null || character.entity == null || !character.entity.isAttached)
		{
			return string.Empty;
		}
		string steamUsername = character.state.SteamUsername;
		return (steamUsername == null) ? string.Empty : steamUsername;
	}

	private PlayerStats getOrCreate(string username)
	{
		PlayerStats playerStats;
		if (!_statsByUsername.TryGetValue(username, out playerStats))
		{
			playerStats = new PlayerStats();
			_statsByUsername[username] = playerStats;
		}
		return playerStats;
	}

	private void serverAddKill(string username)
	{
		if (username.Length == 0)
		{
			return;
		}
		getOrCreate(username).Kills++;
		serverBroadcastRow(username);
	}

	private void serverAddWin(string username)
	{
		if (username.Length == 0)
		{
			return;
		}
		PlayerStats orCreate = getOrCreate(username);
		orCreate.Wins++;
		orCreate.ReachedWinsAtTime = Time.time;
		serverRecalculateCrown();
		serverBroadcastAllRows();
	}

	// Most wins takes the crown. The current holder keeps it whenever they are still tied for
	// the lead, which is what "whoever has had it longest" means in practice; if they lose the
	// lead outright, the tie goes to whoever reached that win count earliest.
	private void serverRecalculateCrown()
	{
		int num = 0;
		foreach (KeyValuePair<string, PlayerStats> item in _statsByUsername)
		{
			if (item.Value.Wins > num)
			{
				num = item.Value.Wins;
			}
		}
		if (num == 0)
		{
			setCrownHolder(string.Empty);
			return;
		}
		PlayerStats statsFor = GetStatsFor(_crownHolder);
		if (statsFor != null && statsFor.Wins == num)
		{
			return;
		}
		string text = string.Empty;
		float num2 = 0f;
		foreach (KeyValuePair<string, PlayerStats> item2 in _statsByUsername)
		{
			if (item2.Value.Wins == num && (text.Length == 0 || item2.Value.ReachedWinsAtTime < num2))
			{
				text = item2.Key;
				num2 = item2.Value.ReachedWinsAtTime;
			}
		}
		setCrownHolder(text);
	}

	private void setCrownHolder(string username)
	{
		if (_crownHolder == username)
		{
			return;
		}
		PlayerStats statsFor = GetStatsFor(_crownHolder);
		if (statsFor != null)
		{
			statsFor.HasCrown = false;
		}
		_crownHolder = username;
		PlayerStats statsFor2 = GetStatsFor(username);
		if (statsFor2 != null)
		{
			statsFor2.HasCrown = true;
		}
	}

	private void serverBroadcastAllRows()
	{
		foreach (KeyValuePair<string, PlayerStats> item in _statsByUsername)
		{
			serverSendRow(item.Key, item.Value);
		}
	}

	private void serverBroadcastRow(string username)
	{
		PlayerStats statsFor = GetStatsFor(username);
		if (statsFor != null)
		{
			serverSendRow(username, statsFor);
		}
	}

	// Username goes last so it may contain spaces.
	private void serverSendRow(string username, PlayerStats stats)
	{
		LogEvent logEvent = LogEvent.Create(GlobalTargets.Everyone);
		logEvent.message = string.Concat(
			StatsPrefix,
			stats.Wins.ToString(CultureInfo.InvariantCulture), " ",
			stats.Kills.ToString(CultureInfo.InvariantCulture), " ",
			(stats.HasCrown ? "1" : "0"), " ",
			username);
		logEvent.Send();
	}

	// Late joiners have missed every row sent so far, so replay the whole table at them.
	public override void Connected(BoltConnection connection)
	{
		if (BoltNetwork.isServer)
		{
			serverBroadcastAllRows();
		}
	}

	public override void OnEvent(LogEvent evnt)
	{
		if (IsEliminationMessage(evnt.message))
		{
			PlayerStatsHUD.ShowElimination(evnt.message.Substring(EliminationPrefix.Length));
			return;
		}
		if (IsKillFeedMessage(evnt.message))
		{
			string[] feed = evnt.message.Substring(KillFeedPrefix.Length).Split(new char[] { '|' }, 5);
			int feedWeapon;
			int feedDistance;
			int feedSeed;
			if (feed.Length == 5
				&& int.TryParse(feed[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out feedWeapon)
				&& int.TryParse(feed[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out feedDistance)
				&& int.TryParse(feed[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out feedSeed))
			{
				PlayerStatsHUD.ShowKill(feed[0], feed[1], (WeaponType)feedWeapon, feedDistance, feedSeed);
			}
			return;
		}
		if (!IsStatsMessage(evnt.message))
		{
			return;
		}
		string text = evnt.message.Substring(StatsPrefix.Length);
		string[] array = text.Split(new char[] { ' ' }, 4);
		int result;
		int result2;
		int result3;
		if (array.Length != 4
			|| !int.TryParse(array[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out result)
			|| !int.TryParse(array[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out result2)
			|| !int.TryParse(array[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out result3))
		{
			return;
		}
		string text2 = array[3];
		if (text2.Length == 0)
		{
			return;
		}
		PlayerStats orCreate = getOrCreate(text2);
		orCreate.Wins = result;
		orCreate.Kills = result2;
		orCreate.HasCrown = result3 != 0;
		if (orCreate.HasCrown)
		{
			_crownHolder = text2;
		}
	}
}
