using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplaySceneManager : Singleton<GameplaySceneManager>
{
	private bool _isDisconnecting;

	public void DisconnectAndExitToMainMenu()
	{
		if (!_isDisconnecting)
		{
			if (BoltNetwork.isConnected && PhotonPoller.Instance != null)
			{
				_isDisconnecting = true;
				BoltLauncher.Shutdown();
				StartCoroutine(waitForDisconnectThenSwitchScenes());
			}
			else
			{
				SceneManager.LoadScene("MainMenu");
			}
		}
	}

	private IEnumerator waitForDisconnectThenSwitchScenes()
	{
		while (BoltNetwork.isConnected || PhotonPoller.Instance != null)
		{
			yield return new WaitForSecondsRealtime(0.1f);
		}
		_isDisconnecting = false;
		SceneManager.LoadScene("MainMenu");
	}
}
