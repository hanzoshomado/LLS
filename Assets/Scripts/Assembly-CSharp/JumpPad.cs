using Bolt;
using UnityEngine;

public class JumpPad : EntityEventListener<IJumpPadState>
{
	private int _numAnimationsPlayed;

	private Vector3 _fullLaunchVelocity;

	private float _cooldownEndServerTime;

	public void Intialize(float launchVelocity, Vector3 launchDirection)
	{
		_fullLaunchVelocity = launchVelocity * launchDirection;
		base.state.LaunchVelocity = _fullLaunchVelocity;
	}

	// Host only. Zeroing the replicated LaunchVelocity is what puts the pad on cooldown: every
	// peer reads the same zero, so a client can't predict a bounce the host is going to refuse.
	public void ServerBeginCooldown(float seconds)
	{
		if (!BoltNetwork.isServer || seconds <= 0f)
		{
			return;
		}
		if (_fullLaunchVelocity.sqrMagnitude < 0.0001f)
		{
			_fullLaunchVelocity = base.state.LaunchVelocity;
		}
		_cooldownEndServerTime = BoltNetwork.serverTime + seconds;
		base.state.LaunchVelocity = Vector3.zero;
	}

	public override void SimulateOwner()
	{
		if (_cooldownEndServerTime > 0f && BoltNetwork.serverTime >= _cooldownEndServerTime)
		{
			_cooldownEndServerTime = 0f;
			base.state.LaunchVelocity = _fullLaunchVelocity;
		}
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

	// The cooldown zeroes the replicated velocity, so every peer caches the last real value and
	// keeps handing it out - the player who triggered the cooldown still needs it to bounce.
	public Vector3 GetFullLaunchVelocity()
	{
		Vector3 launchVelocity = base.state.LaunchVelocity;
		if (launchVelocity.sqrMagnitude > 0.0001f)
		{
			_fullLaunchVelocity = launchVelocity;
		}
		return _fullLaunchVelocity;
	}

	public bool IsOnCooldown()
	{
		return base.state.LaunchVelocity.sqrMagnitude < 0.0001f;
	}
}
