using UnityEngine;

public class FollowWorldTransform : FollowWorldPosition
{
	public Transform TargetTransform;

	protected override void Update()
	{
		if (TargetTransform != null)
		{
			_targetPosition = TargetTransform.position;
			base.Update();
		}
	}
}
