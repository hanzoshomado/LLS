using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// One-time setup for the stats HUD. PlayerStatsHUD used to build its widgets in code every time
// a session started, which left nothing in the scene to select or restyle. This creates the same
// layout as real GameObjects under UIRoot and wires the references, after which the whole thing
// is ordinary UI - move it, recolour it, swap the fonts, replace the panel with a sprite.
//
// Run it once per gameplay scene (Gameplay and SinglePlayer Gameplay both have their own UIRoot).
public static class PlayerStatsHUDBuilder
{
	private const string MenuPath = "Tools/Build Player Stats HUD";

	[MenuItem(MenuPath)]
	private static void Build()
	{
		UIRoot uiRoot = Object.FindObjectOfType<UIRoot>();
		if (uiRoot == null)
		{
			EditorUtility.DisplayDialog("Player Stats HUD",
				"No UIRoot in the open scene.\n\nOpen Gameplay or SinglePlayer Gameplay and run this again.", "OK");
			return;
		}

		PlayerStatsHUD existing = Object.FindObjectOfType<PlayerStatsHUD>();
		if (existing != null)
		{
			Selection.activeGameObject = existing.gameObject;
			EditorUtility.DisplayDialog("Player Stats HUD",
				"This scene already has a PlayerStatsHUD - it's selected in the hierarchy now.\n\n" +
				"Delete it first if you want to start over from the default layout.", "OK");
			return;
		}

		Font font = findFont();

		// Sizes match what the old runtime builder used, and are in the 800x600 space this
		// canvas scales to - the game's own labels sit at 10-16 and its headers at 24-42.
		GameObject hudObject = new GameObject("PlayerStatsHUD", typeof(RectTransform));
		RectTransform hudRect = (RectTransform)hudObject.transform;
		hudRect.SetParent(uiRoot.transform, false);
		hudRect.anchorMin = Vector2.zero;
		hudRect.anchorMax = Vector2.one;
		hudRect.offsetMin = Vector2.zero;
		hudRect.offsetMax = Vector2.zero;
		PlayerStatsHUD hud = hudObject.AddComponent<PlayerStatsHUD>();

		// Top-right win counter.
		RectTransform winCounter = createRect("WinCounter", hudRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
		winCounter.anchoredPosition = new Vector2(-12f, -10f);
		winCounter.sizeDelta = new Vector2(140f, 24f);
		hud.WinCounterLabel = createText(winCounter, font, 18, TextAnchor.UpperRight, Color.white);

		// Elimination popup, bottom centre. The health bar is anchored bottom-centre at y 27.7
		// with a height of 20, so its top edge is ~38; sit clear of that.
		RectTransform elimination = createRect("EliminationPopup", hudRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
		elimination.anchoredPosition = new Vector2(0f, 52f);
		elimination.sizeDelta = new Vector2(500f, 30f);
		hud.EliminationRoot = elimination.gameObject;
		hud.EliminationLabel = createText(elimination, font, 24, TextAnchor.LowerCenter, new Color(0.91f, 0.16f, 0.16f, 1f));
		elimination.gameObject.SetActive(false);

		// Between-round leaderboard, centred.
		RectTransform leaderboard = createRect("Leaderboard", hudRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
		leaderboard.anchoredPosition = new Vector2(0f, -30f);
		leaderboard.sizeDelta = new Vector2(330f, 190f);
		Image background = leaderboard.gameObject.AddComponent<Image>();
		background.color = new Color(0f, 0f, 0f, 0.65f);
		hud.LeaderboardRoot = leaderboard.gameObject;

		RectTransform names = createRect("Names", leaderboard, new Vector2(0f, 0f), new Vector2(0.62f, 1f), new Vector2(0f, 1f));
		names.offsetMin = new Vector2(14f, 10f);
		names.offsetMax = new Vector2(0f, -10f);
		hud.LeaderboardNames = createText(names, font, 14, TextAnchor.UpperLeft, Color.white);

		RectTransform scores = createRect("Scores", leaderboard, new Vector2(0.62f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f));
		scores.offsetMin = new Vector2(0f, 10f);
		scores.offsetMax = new Vector2(-14f, -10f);
		hud.LeaderboardScores = createText(scores, font, 14, TextAnchor.UpperLeft, Color.white);

		leaderboard.gameObject.SetActive(false);

		// The old builder put the board last under UIRoot so it drew over the rest of the HUD.
		hudRect.SetAsLastSibling();

		Undo.RegisterCreatedObjectUndo(hudObject, "Build Player Stats HUD");
		EditorSceneManager.MarkSceneDirty(uiRoot.gameObject.scene);
		Selection.activeGameObject = hudObject;
		Debug.Log("Built PlayerStatsHUD under " + uiRoot.name + ". Save the scene to keep it.", hudObject);
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

	private static Text createText(RectTransform parent, Font font, int fontSize, TextAnchor alignment, Color color)
	{
		GameObject gameObject = new GameObject("Label", typeof(RectTransform));
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		rectTransform.SetParent(parent, false);
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.one;
		rectTransform.offsetMin = Vector2.zero;
		rectTransform.offsetMax = Vector2.zero;
		Text text = gameObject.AddComponent<Text>();
		text.font = font;
		text.fontSize = fontSize;
		text.fontStyle = FontStyle.Bold;
		text.alignment = alignment;
		text.color = color;
		text.horizontalOverflow = HorizontalWrapMode.Overflow;
		text.verticalOverflow = VerticalWrapMode.Overflow;
		text.raycastTarget = false;
		return text;
	}

	// Borrow whatever font the game's own UI already uses so this matches out of the box.
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
