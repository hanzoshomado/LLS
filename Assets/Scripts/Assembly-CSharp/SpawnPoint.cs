using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
	private void Awake()
	{
		GetComponent<Renderer>().enabled = false;
	}
}
