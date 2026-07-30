using UnityEngine;

public class MeleeImpactTrigger : MonoBehaviour
{
	public SantaCharacterController Owner;

	private void OnTriggerEnter(Collider otherCollider)
	{
		if (BoltNetwork.isServer)
		{
			SantaCharacterController component = otherCollider.GetComponent<SantaCharacterController>();
			if (component != null && component != Owner)
			{
				Owner.OnLandedMeleeStrike(component, false);
			}
			ReindeerPlayerAnimator component2 = otherCollider.GetComponent<ReindeerPlayerAnimator>();
			if (component2 != null && component2.Owner != Owner)
			{
				Owner.OnLandedMeleeStrike(component2.Owner, true);
			}
		}
	}
}
