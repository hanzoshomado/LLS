using UnityEngine;

public class ChooseLocation : MonoBehaviour
{
	public Transform[] EndGame_potentialTransformPositions;

	private void Start()
	{
		int num = Random.Range(0, EndGame_potentialTransformPositions.Length - 1);
		Vector3 position = base.transform.position;
		if (EndGame_potentialTransformPositions.Length > 0)
		{
			position.x = EndGame_potentialTransformPositions[num].position.x;
			position.y = EndGame_potentialTransformPositions[num].position.z;
		}
		base.transform.position = position;
	}
}
