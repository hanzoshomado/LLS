using UnityEngine;

public class RespawnButtonMenu : MonoBehaviour
{
	public GameObject DeathmatchInfo;

	public GameObject LastSantaInfo;

	private SantaCharacterController _santaCharacter;

	private void Start()
	{
		DeathmatchInfo.SetActive(false);
		LastSantaInfo.SetActive(false);
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
		DeathmatchInfo.SetActive(false);
		LastSantaInfo.SetActive(false);
		if ((_santaCharacter != null && (_santaCharacter.IsDetached() || _santaCharacter.IsAlive())) || !GameModeManager.Instance.IsGameModeInfoLoaded() || GameModeManager.Instance.HasRoundEnded())
		{
			return;
		}
		if (GameModeManager.Instance.IsLastSantaStanding() && GameModeManager.Instance.IsRoundActive())
		{
			LastSantaInfo.SetActive(true);
			return;
		}
		DeathmatchInfo.SetActive(true);
		if (Input.GetKeyDown(KeyCode.F))
		{
			if (BoltNetwork.isClient)
			{
				ClientWantsRespawnEvent clientWantsRespawnEvent = ClientWantsRespawnEvent.Create();
				clientWantsRespawnEvent.Send();
			}
			else if (BoltNetwork.isServer)
			{
				Singleton<GlobalEventManager>.Instance.Dispatch("ServerWantsRespawn");
			}
		}
	}
}
