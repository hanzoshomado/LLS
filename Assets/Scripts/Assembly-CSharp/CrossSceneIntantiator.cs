using System.Collections.Generic;
using UnityEngine;

public class CrossSceneIntantiator : MonoBehaviour
{
	public string UniqueName;

	public GameObject PrefabToInstantiate;

	private static Dictionary<string, GameObject> crossSceneObjects = new Dictionary<string, GameObject>();

	private void Awake()
	{
		if (!crossSceneObjects.ContainsKey(UniqueName) || crossSceneObjects[UniqueName] == null)
		{
			GameObject gameObject = Object.Instantiate(PrefabToInstantiate);
			crossSceneObjects[UniqueName] = gameObject;
			Object.DontDestroyOnLoad(gameObject);
		}
		Object.Destroy(base.gameObject);
	}
}
