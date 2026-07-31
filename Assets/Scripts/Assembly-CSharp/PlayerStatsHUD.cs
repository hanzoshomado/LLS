using System.Collections.Generic;
using System.Text;
using Bolt;
using UnityEngine;
using UnityEngine.UI;

// Builds its own UI at runtime under UIRoot rather than living in the scene, so there is
// nothing to wire up in the editor. Reads PlayerStatsManager, which every peer already has.
[BoltGlobalBehaviour]
public class PlayerStatsHUD : GlobalEventListener
{
	private const int LeaderboardMaxRows = 8;

	private const float EliminationDisplaySeconds = 3f;

	// Static so a rebuild of the UI (scene load) can't swallow a popup already in flight.
	private static string _pendingEliminationName;

	private static float _eliminationHideTime;

	private static string _localDisplayName = string.Empty;

	private RectTransform _eliminationRoot;

	private Text _eliminationLabel;

	private string _shownEliminationName;

	private RectTransform _winCounterRoot;

	private Text _winCounterLabel;

	private RectTransform _leaderboardRoot;

	private Text _leaderboardNames;

	private Text _leaderboardScores;

	private Font _font;

	private int _lastShownWins = -1;

	private void Update()
	{
		if (Singleton<UIRoot>.Instance == null)
		{
			return;
		}
		// Scene loads destroy the UI we parented under UIRoot, so rebuild when it's gone.
		if (_winCounterRoot == null || _leaderboardRoot == null)
		{
			buildUI();
			if (_winCounterRoot == null)
			{
				return;
			}
		}
		updateWinCounter();
		updateLeaderboard();
		updateElimination();
	}

	// Called on the machine of whoever got the kill - the host resolves that, so this is
	// already "you" by the time it runs.
	public static void ShowElimination(string victimName)
	{
		if (string.IsNullOrEmpty(victimName))
		{
			return;
		}
		_pendingEliminationName = victimName;
		_eliminationHideTime = Time.time + EliminationDisplaySeconds;
	}

	private void updateElimination()
	{
		bool flag = _pendingEliminationName != null && Time.time < _eliminationHideTime;
		if (flag && _shownEliminationName != _pendingEliminationName)
		{
			_shownEliminationName = _pendingEliminationName;
			_eliminationLabel.text = "You eliminated (" + _pendingEliminationName + ")";
		}
		if (_eliminationRoot.gameObject.activeSelf != flag)
		{
			_eliminationRoot.gameObject.SetActive(flag);
		}
		if (!flag)
		{
			_shownEliminationName = null;
		}
	}

	private void updateWinCounter()
	{
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
			_winCounterLabel.text = "WINS  " + num;
		}
	}

	private void updateLeaderboard()
	{
		bool flag2 = false;
		if (GameModeManager.Instance != null && GameModeManager.Instance.IsGameModeInfoLoaded())
		{
			flag2 = GameModeManager.Instance.HasRoundEnded();
		}
		// Tab can only ever add the board on top of the between-rounds one; releasing it can
		// never hide the automatic display, and holding it there changes nothing.
		bool flag = flag2 || isLeaderboardKeyHeld();
		if (_leaderboardRoot.gameObject.activeSelf != flag)
		{
			_leaderboardRoot.gameObject.SetActive(flag);
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
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder.Append("PLAYER\n");
		stringBuilder2.Append("WINS     KILLS\n");
		int num = Mathf.Min(leaderboard.Count, LeaderboardMaxRows);
		for (int i = 0; i < num; i++)
		{
			KeyValuePair<string, PlayerStatsManager.PlayerStats> keyValuePair = leaderboard[i];
			stringBuilder.Append(keyValuePair.Value.HasCrown ? "* " : "   ").Append(keyValuePair.Key).Append('\n');
			stringBuilder2.Append(keyValuePair.Value.Wins).Append("          ").Append(keyValuePair.Value.Kills)
				.Append('\n');
		}
		if (leaderboard.Count == 0)
		{
			stringBuilder.Append("(no rounds played yet)");
		}
		_leaderboardNames.text = stringBuilder.ToString();
		_leaderboardScores.text = stringBuilder2.ToString();
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

	private static bool isLeaderboardKeyHeld()
	{
		if (Singleton<FocusManager>.Instance != null && !Singleton<FocusManager>.Instance.HasFocus())
		{
			return false;
		}
		if (Singleton<UIRoot>.Instance != null && Singleton<UIRoot>.Instance.ShouldUnlockMouse())
		{
			return false;
		}
		return Input.GetKey(KeyCode.Tab);
	}

	private void buildUI()
	{
		Transform transform = Singleton<UIRoot>.Instance.transform;
		if (_font == null)
		{
			_font = findFont();
		}

		// Sizes are tuned for the 800x600 reference resolution this canvas scales to, where the
		// game's own labels sit at 10-16 and its headers at 24-42.

		// Top-right win counter.
		_winCounterRoot = createRect("WinCounter", transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
		_winCounterRoot.anchoredPosition = new Vector2(-12f, -10f);
		_winCounterRoot.sizeDelta = new Vector2(140f, 24f);
		_winCounterLabel = createText(_winCounterRoot, 18, TextAnchor.UpperRight);
		_lastShownWins = -1;

		// Elimination popup, bottom centre. The health bar is anchored bottom-centre at y 27.7
		// with a height of 20, so its top edge is ~38; sit clear of that.
		_eliminationRoot = createRect("EliminationPopup", transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
		_eliminationRoot.anchoredPosition = new Vector2(0f, 52f);
		_eliminationRoot.sizeDelta = new Vector2(500f, 30f);
		_eliminationLabel = createText(_eliminationRoot, 24, TextAnchor.LowerCenter);
		_eliminationLabel.color = new Color(0.91f, 0.16f, 0.16f, 1f);
		_eliminationRoot.gameObject.SetActive(false);

		// Between-round leaderboard, centred.
		_leaderboardRoot = createRect("Leaderboard", transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
		_leaderboardRoot.anchoredPosition = new Vector2(0f, -30f);
		_leaderboardRoot.sizeDelta = new Vector2(330f, 190f);
		Image image = _leaderboardRoot.gameObject.AddComponent<Image>();
		image.color = new Color(0f, 0f, 0f, 0.65f);

		RectTransform rectTransform = createRect("Names", _leaderboardRoot, new Vector2(0f, 0f), new Vector2(0.62f, 1f), new Vector2(0f, 1f));
		rectTransform.offsetMin = new Vector2(14f, 10f);
		rectTransform.offsetMax = new Vector2(0f, -10f);
		_leaderboardNames = createText(rectTransform, 14, TextAnchor.UpperLeft);

		RectTransform rectTransform2 = createRect("Scores", _leaderboardRoot, new Vector2(0.62f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f));
		rectTransform2.offsetMin = new Vector2(0f, 10f);
		rectTransform2.offsetMax = new Vector2(-14f, -10f);
		_leaderboardScores = createText(rectTransform2, 14, TextAnchor.UpperLeft);

		_leaderboardRoot.SetAsLastSibling();
		_leaderboardRoot.gameObject.SetActive(false);
	}

	private static RectTransform createRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
	{
		GameObject gameObject = new GameObject(name, typeof(RectTransform));
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		rectTransform.SetParent(parent, false);
		rectTransform.anchorMin = anchorMin;
		rectTransform.anchorMax = anchorMax;
		rectTransform.pivot = pivot;
		rectTransform.anchoredPosition = Vector2.zero;
		return rectTransform;
	}

	private Text createText(RectTransform parent, int fontSize, TextAnchor alignment)
	{
		GameObject gameObject = new GameObject("Label", typeof(RectTransform));
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		rectTransform.SetParent(parent, false);
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.one;
		rectTransform.offsetMin = Vector2.zero;
		rectTransform.offsetMax = Vector2.zero;
		Text text = gameObject.AddComponent<Text>();
		text.font = _font;
		text.fontSize = fontSize;
		text.fontStyle = FontStyle.Bold;
		text.alignment = alignment;
		text.color = Color.white;
		text.horizontalOverflow = HorizontalWrapMode.Overflow;
		text.verticalOverflow = VerticalWrapMode.Overflow;
		text.raycastTarget = false;
		return text;
	}

	// Borrow whatever font the game's own UI already uses so this matches and can't come out blank.
	private static Font findFont()
	{
		Text[] array = Object.FindObjectsOfType<Text>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].font != null)
			{
				return array[i].font;
			}
		}
		Font builtinResource = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		if (builtinResource == null)
		{
			builtinResource = Resources.GetBuiltinResource<Font>("Arial.ttf");
		}
		return builtinResource;
	}
}
