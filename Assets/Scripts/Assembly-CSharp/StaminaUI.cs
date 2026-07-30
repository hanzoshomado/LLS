using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
	public Image InnerHealthImage;

	public GameObject OuterContainer;

	private SantaCharacterController _santaCharacter;

	private void Start()
	{
		Singleton<GlobalEventManager>.Instance.AddEventListener<SantaCharacterController>("CharacterWithControlSpawned", onCharacterWithControlSpawned);
	}

	private void OnDestroy()
	{
		Singleton<GlobalEventManager>.Instance.RemoveEventListener<SantaCharacterController>("CharacterWithControlSpawned", onCharacterWithControlSpawned);
	}

	private void onCharacterWithControlSpawned(SantaCharacterController santaCharacter)
	{
		_santaCharacter = santaCharacter;
	}

	private void Update()
	{
		if (_santaCharacter != null && !_santaCharacter.IsDetached())
		{
			OuterContainer.SetActive(true);
			InnerHealthImage.fillAmount = _santaCharacter.GetStamina() / _santaCharacter.StartStamina;
		}
		else
		{
			OuterContainer.SetActive(false);
		}
	}
}
