using Bolt;
using UnityEngine;

public class JumpPad : EntityEventListener<IJumpPadState>
{
	private int _numAnimationsPlayed;

	public void Intialize(float launchVelocity, Vector3 launchDirection)
	{
		base.state.LaunchVelocity = launchVelocity * launchDirection;
	}

	public override void Attached()
	{
		base.state.SetTransforms(base.state.Transform, base.transform);
		base.state.AddCallback("JumpAnimationsPlayed", PlayAnimationOnIncrease);
	}

	private void PlayAnimationOnIncrease()
	{
		if (base.state.JumpAnimationsPlayed > _numAnimationsPlayed)
		{
			PlayJumpEffect();
		}
	}

	private void PlayJumpEffect()
	{
		_numAnimationsPlayed++;
		GetComponent<Animator>().Play("Jump", 0, 0f);
		Singleton<AudioManager>.Instance.PlayClipAtTransform(Singleton<AudioLibrary>.Instance.JumpPadCompress, base.transform);
		Singleton<AudioManager>.Instance.PlayClipAtTransform(Singleton<AudioLibrary>.Instance.JumpPadBoing, base.transform);
	}

	public void PlayAndDispatchJumpEffect(bool isController)
	{
		if (isController)
		{
			PlayJumpEffect();
		}
		if (BoltNetwork.isServer)
		{
			base.state.JumpAnimationsPlayed++;
		}
	}

	public Vector3 GetLaunchVelocity()
	{
		return base.state.LaunchVelocity;
	}
}
