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
		bool flag = false;
		if (GameModeManager.Instance != null && GameModeManager.Instance.IsGameModeInfoLoaded())
		{
			flag = GameModeManager.Instance.HasRoundEnded();
		}
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

	private static string getLocalUsername()
	{
		if (Singleton<SteamManager>.Instance != null)
		{
			return Singleton<SteamManager>.Instance.GetSteamUsername();
		}
		return string.Empty;
	}

	private void buildUI()
	{
		Transform transform = Singleton<UIRoot>.Instance.transform;
		if (_font == null)
		{
			_font = findFont();
		}

		// Top-right win counter.
		_winCounterRoot = createRect("WinCounter", transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
		_winCounterRoot.anchoredPosition = new Vector2(-18f, -14f);
		_winCounterRoot.sizeDelta = new Vector2(220f, 40f);
		_winCounterLabel = createText(_winCounterRoot, 26, TextAnchor.UpperRight);
		_lastShownWins = -1;

		// Between-round leaderboard, centred.
		_leaderboardRoot = createRect("Leaderboard", transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
		_leaderboardRoot.anchoredPosition = new Vector2(0f, -40f);
		_leaderboardRoot.sizeDelta = new Vector2(460f, 300f);
		Image image = _leaderboardRoot.gameObject.AddComponent<Image>();
		image.color = new Color(0f, 0f, 0f, 0.65f);

		RectTransform rectTransform = createRect("Names", _leaderboardRoot, new Vector2(0f, 0f), new Vector2(0.62f, 1f), new Vector2(0f, 1f));
		rectTransform.offsetMin = new Vector2(22f, 16f);
		rectTransform.offsetMax = new Vector2(0f, -16f);
		_leaderboardNames = createText(rectTransform, 22, TextAnchor.UpperLeft);

		RectTransform rectTransform2 = createRect("Scores", _leaderboardRoot, new Vector2(0.62f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f));
		rectTransform2.offsetMin = new Vector2(0f, 16f);
		rectTransform2.offsetMax = new Vector2(-22f, -16f);
		_leaderboardScores = createText(rectTransform2, 22, TextAnchor.UpperLeft);

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
