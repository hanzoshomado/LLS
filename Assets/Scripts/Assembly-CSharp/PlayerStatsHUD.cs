using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

// Display only. Every number here comes from PlayerStatsManager, which is the networked
// authority - the host counts kills and wins, decides the crown and broadcasts each row, so this
// just draws what the session has already agreed on.
//
// The widgets are real scene objects assigned below rather than built in code, so the layout can
// be moved and restyled in the editor. Tools > Build Player Stats HUD creates a wired-up default
// set under UIRoot if a scene doesn't have one yet.
public class PlayerStatsHUD : MonoBehaviour
{
	[Header("Win Counter")]
	public Text WinCounterLabel;
	public string WinCounterFormat = "WINS  {0}";

	[Header("Elimination Popup")]
	public GameObject EliminationRoot;
	public Text EliminationLabel;
	public string EliminationFormat = "You eliminated ({0})";
	public float EliminationDisplaySeconds = 3f;

	[Header("Leaderboard")]
	public GameObject LeaderboardRoot;
	public Text LeaderboardNames;
	public Text LeaderboardScores;
	public KeyCode LeaderboardKey = KeyCode.Tab;
	public bool ShowLeaderboardBetweenRounds = true;
	public int LeaderboardMaxRows = 8;

	[Header("Leaderboard Text")]
	public string NamesHeader = "PLAYER";
	public string ScoresHeader = "WINS     KILLS";
	// Both prefixes want the same width or the names below a crown holder step sideways.
	public string CrownPrefix = "* ";
	public string NoCrownPrefix = "   ";
	public string ScoreSeparator = "          ";
	public string EmptyLeaderboardText = "(no rounds played yet)";

	// Static so a scene load can't swallow a popup already in flight.
	private static string _pendingEliminationName;

	private static float _eliminationShownTime;

	private static string _localDisplayName = string.Empty;

	private string _shownEliminationName;

	private int _lastShownWins = -1;

	private readonly StringBuilder _namesBuilder = new StringBuilder();

	private readonly StringBuilder _scoresBuilder = new StringBuilder();

	private void Update()
	{
		updateWinCounter();
		updateLeaderboard();
		updateElimination();
	}

	// Called on the machine of whoever got the kill - the host resolves that, so this is
	// already "you" by the time it runs. Static because PlayerStatsManager is a Bolt global
	// behaviour and has no way to reach into the scene.
	public static void ShowElimination(string victimName)
	{
		if (string.IsNullOrEmpty(victimName))
		{
			return;
		}
		_pendingEliminationName = victimName;
		_eliminationShownTime = Time.time;
	}

	private void updateElimination()
	{
		if (EliminationRoot == null)
		{
			return;
		}
		bool flag = _pendingEliminationName != null && Time.time < _eliminationShownTime + EliminationDisplaySeconds;
		if (flag && _shownEliminationName != _pendingEliminationName)
		{
			_shownEliminationName = _pendingEliminationName;
			if (EliminationLabel != null)
			{
				EliminationLabel.text = string.Format(EliminationFormat, _pendingEliminationName);
			}
		}
		if (EliminationRoot.activeSelf != flag)
		{
			EliminationRoot.SetActive(flag);
		}
		if (!flag)
		{
			_shownEliminationName = null;
		}
	}

	private void updateWinCounter()
	{
		if (WinCounterLabel == null)
		{
			return;
		}
		int num = 0;
		PlayerStatsManager instance = PlayerStatsManager.Instance;
		if (instance != null)
		{
			PlayerStatsManager.PlayerStats statsFor = instance.GetStatsFor(getLocalUsername());
			if (statsFor != null)
			{
				num = statsFor.Wins;
			}
		}
		if (num != _lastShownWins)
		{
			_lastShownWins = num;
			WinCounterLabel.text = string.Format(WinCounterFormat, num);
		}
	}

	private void updateLeaderboard()
	{
		if (LeaderboardRoot == null)
		{
			return;
		}
		bool flag2 = false;
		if (ShowLeaderboardBetweenRounds && GameModeManager.Instance != null && GameModeManager.Instance.IsGameModeInfoLoaded())
		{
			flag2 = GameModeManager.Instance.HasRoundEnded();
		}
		// The key can only ever add the board on top of the between-rounds one; releasing it can
		// never hide the automatic display, and holding it there changes nothing.
		bool flag = flag2 || isLeaderboardKeyHeld();
		if (LeaderboardRoot.activeSelf != flag)
		{
			LeaderboardRoot.SetActive(flag);
		}
		if (!flag)
		{
			return;
		}

		PlayerStatsManager instance = PlayerStatsManager.Instance;
		if (instance == null)
		{
			return;
		}
		List<KeyValuePair<string, PlayerStatsManager.PlayerStats>> leaderboard = instance.GetLeaderboard();
		_namesBuilder.Length = 0;
		_scoresBuilder.Length = 0;
		_namesBuilder.Append(NamesHeader).Append('\n');
		_scoresBuilder.Append(ScoresHeader).Append('\n');
		int num = Mathf.Min(leaderboard.Count, LeaderboardMaxRows);
		for (int i = 0; i < num; i++)
		{
			KeyValuePair<string, PlayerStatsManager.PlayerStats> keyValuePair = leaderboard[i];
			_namesBuilder.Append(keyValuePair.Value.HasCrown ? CrownPrefix : NoCrownPrefix).Append(keyValuePair.Key).Append('\n');
			_scoresBuilder.Append(keyValuePair.Value.Wins).Append(ScoreSeparator).Append(keyValuePair.Value.Kills)
				.Append('\n');
		}
		if (leaderboard.Count == 0)
		{
			_namesBuilder.Append(EmptyLeaderboardText);
		}
		if (LeaderboardNames != null)
		{
			LeaderboardNames.text = _namesBuilder.ToString();
		}
		if (LeaderboardScores != null)
		{
			LeaderboardScores.text = _scoresBuilder.ToString();
		}
	}

	// The host may have suffixed our typed name to keep it unique, and stats are keyed by that
	// resolved name - so read it off our own character rather than trusting what we typed.
	// Cached because the character is gone while we're dead or between rounds.
	private static string getLocalUsername()
	{
		if (CharacterTracker.Instance != null)
		{
			SantaCharacterController santaWithControl = CharacterTracker.Instance.GetSantaWithControl();
			if (santaWithControl != null && santaWithControl.entity != null && santaWithControl.entity.isAttached)
			{
				string steamUsername = santaWithControl.state.SteamUsername;
				if (!string.IsNullOrEmpty(steamUsername))
				{
					_localDisplayName = steamUsername;
				}
			}
		}
		if (_localDisplayName.Length == 0 && Singleton<SteamManager>.Instance != null)
		{
			return Singleton<SteamManager>.Instance.GetSteamUsername();
		}
		return _localDisplayName;
	}

	private bool isLeaderboardKeyHeld()
	{
		if (Singleton<FocusManager>.Instance != null && !Singleton<FocusManager>.Instance.HasFocus())
		{
			return false;
		}
		if (Singleton<UIRoot>.Instance != null && Singleton<UIRoot>.Instance.ShouldUnlockMouse())
		{
			return false;
		}
		return Input.GetKey(LeaderboardKey);
	}
}
