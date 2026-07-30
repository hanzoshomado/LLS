using UnityEngine;

public class WeaponDropPoint : MonoBehaviour
{
	public bool IsStartAreaDropPoint;

	public bool HasDroppedHere { get; set; }

	private void Awake()
	{
		GetComponent<Renderer>().enabled = false;
	}
}
