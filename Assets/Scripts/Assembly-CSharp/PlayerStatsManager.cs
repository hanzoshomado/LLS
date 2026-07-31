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

	private static PlayerStatsManager _instance;

	private readonly Dictionary<string, PlayerStats> _statsByUsername = new Dictionary<string, PlayerStats>();

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

	public static void ReportKill(SantaCharacterController killer)
	{
		if (!BoltNetwork.isServer || _instance == null || killer == null)
		{
			return;
		}
		_instance.serverAddKill(getUsernameOf(killer));
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
