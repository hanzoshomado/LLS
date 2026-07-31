using UnityEngine;
using UnityEngine.UI;

public class WeaponAmmoUI : MonoBehaviour
{
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
        AmmoCounterRoot.SetActive(false);
        return;
    }

    WeaponType equipped = (WeaponType)_santaCharacter.GetEquippedWeaponTypeAsInt();
    bool hasAmmoBasedWeapon = equipped == WeaponType.Crossbow 
        || equipped == WeaponType.Pistol 
        || equipped == WeaponType.LightningGun
        || equipped == WeaponType.SnowballLauncher;

    if (hasAmmoBasedWeapon)
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