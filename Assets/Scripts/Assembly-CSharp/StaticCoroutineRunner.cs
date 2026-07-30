using System.Collections;
using UnityEngine;

public class StaticCoroutineRunner : MonoBehaviour
{
	private static StaticCoroutineRunner _coroutineRunner;

	private static int _numCoroutinesRunning;

	public static void StartStaticCoroutine(IEnumerator coroutine)
	{
		_numCoroutinesRunning++;
		if (_coroutineRunner == null)
		{
			_coroutineRunner = new GameObject("CoroutineRunner").AddComponent<StaticCoroutineRunner>();
		}
		_coroutineRunner.StartCoroutine(coroutine);
	}

	public static void CleanUpRunnerIfDone()
	{
		_numCoroutinesRunning--;
		if (_numCoroutinesRunning == 0 && _coroutineRunner != null)
		{
			Object.DestroyImmediate(_coroutineRunner.gameObject);
		}
	}
}
