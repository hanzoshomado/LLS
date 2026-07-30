using UnityEngine;

public class DisconnectedPanel : MonoBehaviour
{
	public void Show()
	{
		base.gameObject.SetActive(true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(false);
	}

	public void OnReturnToMainMenuClicked()
	{
		Singleton<GameplaySceneManager>.Instance.DisconnectAndExitToMainMenu();
	}
}
