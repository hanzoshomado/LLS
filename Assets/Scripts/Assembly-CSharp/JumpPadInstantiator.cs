using UnityEngine;

public class JumpPadInstantiator : MonoBehaviour
{
	public float VelocityMagnitude;

	public void Start()
	{
		if (BoltNetwork.isServer)
		{
			BoltEntity boltEntity = BoltNetwork.Instantiate(BoltPrefabs.JumpPad, base.transform.position, base.transform.rotation);
			boltEntity.GetComponent<JumpPad>().Intialize(VelocityMagnitude, GetLaunchDirection());
		}
		Object.Destroy(base.gameObject);
	}

	private Vector3 GetLaunchDirection()
	{
		return base.transform.up;
	}
}
