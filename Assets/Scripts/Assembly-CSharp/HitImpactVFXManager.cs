using Bolt;
using UnityEngine;

public class HitImpactVFXManager : GlobalEventListener
{
	public float HitVFXDestroyCountDown;

	public PooledPrefab PunchHitVFXPool;

	public PooledPrefab SwordHitVFXPool;

	public PooledPrefab CrossbowBoltEnvironmentHitVFXPool;

	public override void OnEvent(HitSoundAndVFXEvent hitEvent)
	{
		PooledPrefab pooledPrefab;
		if (hitEvent.WeaponUsed == 1)
		{
			pooledPrefab = SwordHitVFXPool;
			Singleton<AudioManager>.Instance.PlayClipAtTransform(Singleton<AudioLibrary>.Instance.SwordImpact, base.transform);
		}
		else
		{
			pooledPrefab = PunchHitVFXPool;
			Singleton<AudioManager>.Instance.PlayClipAtTransform(Singleton<AudioLibrary>.Instance.PunchImpact, base.transform);
		}
		Transform transform = pooledPrefab.InstantiateNewObject();
		transform.position = hitEvent.Position;
		transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, hitEvent.ImpactRotationY, transform.localEulerAngles.z);
		transform.GetComponent<DestroyPooledAfterWait>().StartCountdown(HitVFXDestroyCountDown);
		transform.GetComponent<ParticleSystem>().Play();
	}

	public override void OnEvent(CrossbowEnvironmentHitEvent hitEvent)
	{
		PooledPrefab crossbowBoltEnvironmentHitVFXPool = CrossbowBoltEnvironmentHitVFXPool;
		Transform transform = crossbowBoltEnvironmentHitVFXPool.InstantiateNewObject();
		transform.position = hitEvent.Position;
		transform.localEulerAngles = hitEvent.EulerAngles;
		transform.GetComponent<DestroyPooledAfterWait>().StartCountdown(HitVFXDestroyCountDown);
		transform.GetComponent<ParticleSystemReference>().ParticleSystem.Play();
		Singleton<AudioManager>.Instance.PlayClipAtPosition(Singleton<AudioLibrary>.Instance.CrossbowImpactEnv, hitEvent.Position);
	}
}
