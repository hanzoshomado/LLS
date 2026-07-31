using UnityEngine;
using UnityEngine.UI;

public class EnemyNameTagUI : MonoBehaviour
{
	public Image InnerHealthImage;

	public Text SteamUsername;

	public GameObject SteamUsernameBG;

	// Optional. Assign a child Image holding the crown sprite; it is shown only for the
	// player currently leading on wins. Left unassigned, the tag just has no crown.
	// Expected to be parented to the SteamUsername text with a right-edge pivot.
	public Image CrownIcon;

	// Gap between the crown and the first letter of the name, in SteamUsername local units.
	public float CrownNamePadding = 3f;

	private SantaCharacterController _santaCharacter;

	private string _lastRenderedUsername;

	private int _lastRenderedWins = -1;

	private bool _lastRenderedCrown;

	public void SetCharacterAndFollow(SantaCharacterController santaCharacter)
	{
		_santaCharacter = santaCharacter;
		GetComponent<FollowWorldTransform>().TargetTransform = _santaCharacter.NameTagAnchorPoint;
		UpdateUsername();
		_santaCharacter.state.AddCallback("SteamUsername", OnSteamUsernameChanged);
	}

	private void OnSteamUsernameChanged()
	{
		UpdateUsername();
	}

	private void UpdateUsername()
	{
		if (PlayerIsNullOrDead())
		{
			return;
		}
		string text = _santaCharacter.state.SteamUsername;
		SteamUsernameBG.SetActive(!string.IsNullOrEmpty(text));

		int num = 0;
		bool flag = false;
		if (PlayerStatsManager.Instance != null)
		{
			PlayerStatsManager.PlayerStats statsFor = PlayerStatsManager.Instance.GetStatsFor(text);
			if (statsFor != null)
			{
				num = statsFor.Wins;
				flag = statsFor.HasCrown;
			}
		}

		// The scoreboard arrives on its own schedule, so only rebuild when something moved.
		if (_lastRenderedUsername == text && _lastRenderedWins == num && _lastRenderedCrown == flag)
		{
			return;
		}
		_lastRenderedUsername = text;
		_lastRenderedWins = num;
		_lastRenderedCrown = flag;

		SteamUsername.text = text + "(" + num + ")";
		if (CrownIcon == null)
		{
			return;
		}
		CrownIcon.gameObject.SetActive(flag);
		if (!flag)
		{
			return;
		}

		// The name is centred inside a rect far wider than the text, so a fixed offset would
		// drift away from short names. Chase the left edge of the glyphs instead.
		float preferredWidth = SteamUsername.preferredWidth;
		if (preferredWidth <= 0f)
		{
			// Text hasn't been generated yet, so retry next Update rather than park the crown
			// on top of the name.
			_lastRenderedUsername = null;
			return;
		}
		CrownIcon.rectTransform.anchoredPosition = new Vector2(0f - (preferredWidth * 0.5f + CrownNamePadding), 0f);
	}

	private void Update()
	{
		if (PlayerIsNullOrDead())
		{
			_santaCharacter = null;
			Object.Destroy(base.gameObject);
		}
		else
		{
			InnerHealthImage.fillAmount = (float)_santaCharacter.state.HitPoints / (float)_santaCharacter.StartHitpoints;
			UpdateUsername();
		}
	}

	private bool PlayerIsNullOrDead()
	{
		return _santaCharacter == null || _santaCharacter.IsDetached() || !_santaCharacter.IsAlive();
	}
}
