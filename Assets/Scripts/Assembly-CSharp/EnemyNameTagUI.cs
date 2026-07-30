using UnityEngine;
using UnityEngine.UI;

public class EnemyNameTagUI : MonoBehaviour
{
	public Image InnerHealthImage;

	public Text SteamUsername;

	public GameObject SteamUsernameBG;

	private SantaCharacterController _santaCharacter;

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
		if (!PlayerIsNullOrDead())
		{
			SteamUsernameBG.SetActive(!string.IsNullOrEmpty(_santaCharacter.state.SteamUsername));
			SteamUsername.text = _santaCharacter.state.SteamUsername;
		}
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
		}
	}

	private bool PlayerIsNullOrDead()
	{
		return _santaCharacter == null || _santaCharacter.IsDetached() || !_santaCharacter.IsAlive();
	}
}
