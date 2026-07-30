using UnityEngine;
using UnityEngine.UI;

public class CrossbowUI : MonoBehaviour
{
	public GameObject CrossHairs;

	public Text AmmoCounterLabel;

	public GameObject AmmoCounterRoot;

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
		if (_santaCharacter == null || _santaCharacter.IsDetached())
		{
			CrossHairs.SetActive(false);
			AmmoCounterRoot.SetActive(false);
			return;
		}
		CrossHairs.SetActive(_santaCharacter.IsAimingCrossBow());
		if (_santaCharacter.HasCrossbow())
		{
			AmmoCounterRoot.SetActive(true);
			AmmoCounterLabel.text = _santaCharacter.state.CurrentWeaponAmmo.ToString();
		}
		else
		{
			AmmoCounterRoot.SetActive(false);
		}
	}
}
