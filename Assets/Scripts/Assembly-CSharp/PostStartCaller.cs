using UnityEngine;

public class PostStartCaller : MonoBehaviour
{
	private void Update()
	{
		base.gameObject.SendMessage("OnPostStart", SendMessageOptions.DontRequireReceiver);
		Object.Destroy(this);
	}
}
