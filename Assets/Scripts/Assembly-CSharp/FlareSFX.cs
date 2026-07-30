using UnityEngine;

public class FlareSFX : MonoBehaviour
{
	private WorldAudioSource _idleClip;

	private void Start()
	{
		Singleton<AudioManager>.Instance.PlayClipAtPosition(Singleton<AudioLibrary>.Instance.FlareStart, base.transform.position, 0f, false, 1.25f);
		_idleClip = Singleton<AudioManager>.Instance.PlayClipAtPosition(Singleton<AudioLibrary>.Instance.FlareIdle, base.transform.position, 0f, false, 0.45f);
	}

	private void OnDestroy()
	{
		if (_idleClip != null)
		{
			_idleClip.StopPlaying();
		}
	}
}
