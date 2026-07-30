using Bolt;
using UnityEngine;

public class SantaCharacterRagdoll : EntityBehaviour<ISantaRagdollState>
{
	public Transform BodyTransform;

	public Transform HandR;

	public Transform HandL;

	public Transform FootL;

	public Transform FootR;

	public Transform Head;

	public override void Attached()
	{
		base.state.SetTransforms(base.state.BodyTransform, BodyTransform);
		if (!base.entity.isOwner)
		{
			BodyTransform.GetComponent<Rigidbody>().isKinematic = true;
		}
	}

	public void MatchSanta(SantaCharacterRagdollSource santaCharacterRagdollSource)
	{
		BodyTransform.position = santaCharacterRagdollSource.BodyTransform.position + new Vector3(0f, 2f, 0f);
		BodyTransform.rotation = santaCharacterRagdollSource.BodyTransform.rotation;
		HandR.position = santaCharacterRagdollSource.HandR.position;
		HandR.rotation = santaCharacterRagdollSource.HandR.rotation;
		HandL.position = santaCharacterRagdollSource.HandL.position;
		HandL.rotation = santaCharacterRagdollSource.HandL.rotation;
		FootL.position = santaCharacterRagdollSource.FootL.position;
		FootL.rotation = santaCharacterRagdollSource.FootL.rotation;
		FootR.position = santaCharacterRagdollSource.FootR.position;
		FootR.rotation = santaCharacterRagdollSource.FootR.rotation;
		Head.position = santaCharacterRagdollSource.Head.position;
		Head.rotation = santaCharacterRagdollSource.Head.rotation;
	}
}
