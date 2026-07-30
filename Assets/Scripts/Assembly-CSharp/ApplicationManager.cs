using System.Collections;
using UnityEngine;

public class ApplicationManager : MonoBehaviour
{
	public static ApplicationManager Instance;

	public bool isPlaying;

	public IEnumerator Check_isPlaying_Coroutine;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else if (Instance != this)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		Check_isPlaying_Coroutine = Check_isPlaying_IEnumerator();
		StartCoroutine(Check_isPlaying_Coroutine);
	}

	private IEnumerator Check_isPlaying_IEnumerator()
	{
		isPlaying = true;
		yield return new WaitForSeconds(0f);
	}
}
