using Bolt;
using UnityEngine;

public class CrossbowProjectile : EntityBehaviour<IProjectileState>
{
	public int Damage;

	public float FlySpeed;

	public float DistanceFromCenterToDestroySelf;

	public float RangeToMakeFlyBySound = 8f;

	private int _attackID;

	private SantaCharacterController _owner;

	private int _serverFrameFiredFromClient;

	private int _actualServerFrameCreated;

	private bool _hasPlayedSoundForViewer;

	private Transform _flyByListenerPlayer;

	public void Start()
	{
		Singleton<AudioManager>.Instance.PlayClipAtPosition(Singleton<AudioLibrary>.Instance.CrossbowFire, base.transform.position);
		_hasPlayedSoundForViewer = false;
		SantaCharacterController santaWithControl = CharacterTracker.Instance.GetSantaWithControl();
		if (santaWithControl != null)
		{
			float num = Quaternion.Angle(santaWithControl.TiltRoot.transform.rotation, base.transform.rotation);
			if (!((double)num < 0.1))
			{
				_flyByListenerPlayer = santaWithControl.transform;
			}
		}
	}

	public void Intialize(SantaCharacterController owner, int attackID, Vector3 spawnPosition, Vector3 startEulerAngles, int commandServerFrame)
	{
		base.transform.position = spawnPosition;
		base.transform.eulerAngles = startEulerAngles;
		_owner = owner;
		_attackID = attackID;
		_serverFrameFiredFromClient = commandServerFrame;
		_actualServerFrameCreated = BoltNetwork.serverFrame;
	}

	private void Update()
	{
		if (!_hasPlayedSoundForViewer && _flyByListenerPlayer != null && (_flyByListenerPlayer.position - base.transform.position).magnitude < RangeToMakeFlyBySound)
		{
			_hasPlayedSoundForViewer = true;
			Singleton<AudioManager>.Instance.PlayClipAtTransform(Singleton<AudioLibrary>.Instance.CrossbowFlyBy, base.transform);
		}
	}

	public override void Attached()
	{
		base.state.SetTransforms(base.state.Transform, base.transform);
	}

	public override void SimulateOwner()
	{
		Vector3 position = base.transform.position;
		Vector3 vector = position + base.transform.forward * FlySpeed * BoltNetwork.frameDeltaTime;
		Vector3 vector2 = vector - position;
		float magnitude = vector2.magnitude;
		Ray ray = new Ray(position, base.transform.forward);
		int num = BoltNetwork.serverFrame - _actualServerFrameCreated;
		int frame = _serverFrameFiredFromClient + num;
		BoltPhysicsHits boltPhysicsHits = BoltNetwork.RaycastAll(ray, frame);
		SantaCharacterController santaCharacterController = null;
		BoltPhysicsHit hit = default(BoltPhysicsHit);
		if (boltPhysicsHits.count > 0)
		{
			for (int i = 0; i < boltPhysicsHits.count; i++)
			{
				hit = boltPhysicsHits.GetHit(i);
				if (hit.distance <= magnitude && hit.hitbox.hitboxType != BoltHitboxType.Proximity)
				{
					SantaCharacterController component = hit.body.GetComponent<SantaCharacterController>();
					if (component != null && component != _owner)
					{
						santaCharacterController = component;
						break;
					}
				}
			}
		}
		int layerMask = 1 << LayerMask.NameToLayer("Environment");
		bool flag = false;
		RaycastHit hitInfo;
		if (Physics.Raycast(ray, out hitInfo, magnitude, layerMask))
		{
			flag = true;
		}
		if (flag && (santaCharacterController == null || hitInfo.distance < hit.distance))
		{
			CrossbowEnvironmentHitEvent crossbowEnvironmentHitEvent = CrossbowEnvironmentHitEvent.Create();
			crossbowEnvironmentHitEvent.Position = hitInfo.point;
			crossbowEnvironmentHitEvent.EulerAngles = base.transform.localEulerAngles;
			crossbowEnvironmentHitEvent.Send();
			BoltNetwork.Destroy(base.gameObject);
		}
		else if (santaCharacterController != null)
		{
			Vector3 worldImpactPosition = position + vector2.normalized * hit.distance;
			if (hit.hitbox.hitboxType == BoltHitboxType.Body)
			{
				santaCharacterController.TryTakeDamageFromAttack(_owner, Damage, base.transform.forward, _attackID, WeaponType.Crossbow, worldImpactPosition);
			}
			else
			{
				santaCharacterController.TryTakeReindeerDamageFromAttack(_owner, Damage, base.transform.forward, _attackID, WeaponType.Crossbow, worldImpactPosition);
			}
			BoltNetwork.Destroy(base.gameObject);
		}
		else
		{
			base.transform.position += base.transform.forward * FlySpeed * BoltNetwork.frameDeltaTime;
			if (base.transform.position.magnitude > DistanceFromCenterToDestroySelf)
			{
				BoltNetwork.Destroy(base.gameObject);
			}
		}
	}
}
