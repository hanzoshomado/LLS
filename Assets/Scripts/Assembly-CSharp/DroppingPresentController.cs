using System.Collections;
using Bolt;
using UnityEngine;

public class DroppingPresentController : EntityBehaviour<IDroppingPresentState>
{
	public ParticleSystem ParachuteDestroyedVFX;

	public Vector3 StartPositionOffset;

	public float FallSpeed;

	public MinMaxRange RandomWindSpeedAdditions;

	public MinMaxRange TimeBetweenWindBursts;

	public float WindBurstDuration;

	private float _startDistanceMagnitude;

	private float _currentWindSpeedAdditionX;

	private float _currentWindSpeedAdditionZ;

	private float _timeToApplyNextWind;

	private float _windStartTime;

	public Transform FlarePrefab;

	private Transform _flare;

	public override void Attached()
	{
		base.state.SetTransforms(base.state.Transform, base.transform);
		base.state.AddCallback("HasLanded", onHasLandedChanged);
		base.state.AddCallback("TargetPosition", onTargetPositionSet);
		if (base.state.HasLanded)
		{
			GetComponent<Animator>().Play("Open", 0, 0f);
		}
	}

	public void Initialize(Vector3 targetWorldPosition, Vector3 eulerAngles)
	{
		base.state.TargetPosition = targetWorldPosition;
		base.transform.position = targetWorldPosition + new Vector3(Random.Range(0f - StartPositionOffset.x, StartPositionOffset.x), StartPositionOffset.y, Random.Range(0f - StartPositionOffset.z, StartPositionOffset.z));
		base.transform.localEulerAngles = eulerAngles;
		_startDistanceMagnitude = StartPositionOffset.magnitude;
		_currentWindSpeedAdditionX = 0f;
		_currentWindSpeedAdditionZ = 0f;
		_timeToApplyNextWind = BoltNetwork.serverTime + TimeBetweenWindBursts.GetRandomValue() * 2f;
		_windStartTime = -1f;
	}

	private void OnDestroy()
	{
		destroyFlare();
	}

	private void onTargetPositionSet()
	{
		if (_flare == null && !base.state.HasLanded)
		{
			_flare = Object.Instantiate(FlarePrefab);
			_flare.position = base.state.TargetPosition;
		}
	}

	private void destroyFlare()
	{
		if (_flare != null)
		{
			Object.Destroy(_flare.gameObject);
		}
	}

	public override void SimulateOwner()
	{
		if (!base.state.HasLanded)
		{
			Vector3 vector = base.state.TargetPosition - base.transform.position;
			float magnitude = vector.magnitude;
			Vector3 vector2 = vector.normalized * FallSpeed * BoltNetwork.frameDeltaTime;
			if (_windStartTime > 0f && BoltNetwork.serverTime < _windStartTime + WindBurstDuration)
			{
				float num = 1f - (_windStartTime + WindBurstDuration - BoltNetwork.serverTime) / WindBurstDuration;
				float num2 = magnitude / _startDistanceMagnitude;
				num *= num2;
				vector2.x += _currentWindSpeedAdditionX * BoltNetwork.frameDeltaTime * num;
				vector2.z += _currentWindSpeedAdditionZ * BoltNetwork.frameDeltaTime * num;
			}
			if (magnitude < vector2.magnitude)
			{
				vector2 = magnitude * vector2.normalized;
				base.state.HasLanded = true;
			}
			base.transform.position += vector2;
			if (BoltNetwork.serverTime > _timeToApplyNextWind)
			{
				_currentWindSpeedAdditionX = RandomWindSpeedAdditions.GetRandomValue();
				_currentWindSpeedAdditionZ = RandomWindSpeedAdditions.GetRandomValue();
				_timeToApplyNextWind = BoltNetwork.serverTime + TimeBetweenWindBursts.GetRandomValue();
				_windStartTime = BoltNetwork.serverTime;
			}
		}
	}

	private void onHasLandedChanged()
	{
		ParachuteDestroyedVFX.Play();
		Singleton<AudioManager>.Instance.PlayClipAtTransform(Singleton<AudioLibrary>.Instance.GiftParachuteExplode, base.transform, 0f, false, 0.45f);
		Singleton<AudioManager>.Instance.PlayClipAtTransform(Singleton<AudioLibrary>.Instance.GiftParachuteExplodeSecondary, base.transform, 0f, false, 0.45f);
		GetComponent<Animator>().Play("Open", 0, 0f);
		destroyFlare();
		if (BoltNetwork.isServer)
		{
			StartCoroutine(waitThenSpawnWeapon());
		}
	}

	private IEnumerator waitThenSpawnWeapon()
	{
		yield return new WaitForSeconds(1f);
		WeaponDropManager.Instance.SpawnRandomPickup(base.state.TargetPosition);
	}

	public void PlayBoxOpenSound()
	{
		Singleton<AudioManager>.Instance.PlayClipAtTransform(Singleton<AudioLibrary>.Instance.GiftBoxOpen, base.transform, 0f, false, 0.45f);
	}

	public void PlayRevealSound()
	{
		Singleton<AudioManager>.Instance.PlayClipAtTransform(Singleton<AudioLibrary>.Instance.GiftBoxPostReveal, base.transform, 0f, false, 0.25f);
	}
}
