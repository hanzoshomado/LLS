using System;
using System.Collections.Generic;
using Bolt;
using UnityEngine;
using System.Reflection;

public class WeaponDropManager : GlobalEventListener
{
	public float WaitBetweenDrops;

	public WeaponType[] WeaponDropOptions;

	private List<WeaponDropPoint> _dropPoints;

	private List<PickupItemController> _itemPickups;

	private float _timeForNextDrop;

	private WeaponType _lastSpawnedWeapon;

	private static WeaponDropManager _instance;

	public static WeaponDropManager Instance
	{
		get
		{
			return _instance;
		}
	}

	private void Start()
	{
		_timeForNextDrop = -1f;
		_dropPoints = new List<WeaponDropPoint>(UnityEngine.Object.FindObjectsOfType<WeaponDropPoint>());
		Singleton<GlobalEventManager>.Instance.AddEventListener("GameStarted", onGameStarted);
		Singleton<GlobalEventManager>.Instance.AddEventListener("GameEnded", onGameEnded);
	}

	private void OnDestroy()
	{
		Singleton<GlobalEventManager>.Instance.RemoveEventListener("GameStarted", onGameStarted);
		Singleton<GlobalEventManager>.Instance.RemoveEventListener("GameEnded", onGameEnded);
	}

	public override void EntityAttached(BoltEntity entity)
	{
		if (entity.StateIs<IPickupItemState>())
		{
			_itemPickups.Add(entity.GetComponent<PickupItemController>());
		}
	}

	public override void EntityDetached(BoltEntity entity)
	{
		if (entity.StateIs<IPickupItemState>())
		{
			_itemPickups.Remove(entity.GetComponent<PickupItemController>());
		}
	}

	private void onGameStarted()
	{
		if (BoltNetwork.isServer)
		{
			for (int i = 0; i < _dropPoints.Count; i++)
			{
				_dropPoints[i].HasDroppedHere = false;
			}
			DropWeaponInCenterOfArena();
			DropWeaponAtRandomDropPoint();
			_timeForNextDrop = Time.time + WaitBetweenDrops;
		}
	}

	private void onGameEnded()
	{
		if (BoltNetwork.isServer)
		{
			_timeForNextDrop = -1f;
		}
	}

	private void Update()
	{
		if (BoltNetwork.isServer && _timeForNextDrop > 0f && Time.time > _timeForNextDrop)
		{
			DropWeaponAtRandomDropPoint();
			_timeForNextDrop = Time.time + WaitBetweenDrops;
		}
	}

	public void DropWeaponInCenterOfArena()
	{
		dropPresentAtDropPoint(GetCenterDropPoint());
	}

	public void DropWeaponAtRandomDropPoint()
	{
		List<WeaponDropPoint> list = new List<WeaponDropPoint>();
		for (int i = 0; i < _dropPoints.Count; i++)
		{
			if (!_dropPoints[i].HasDroppedHere)
			{
				list.Add(_dropPoints[i]);
			}
		}
		if (list.Count > 0)
		{
			dropPresentAtDropPoint(list[UnityEngine.Random.Range(0, list.Count)]);
		}
	}

	private void dropPresentAtDropPoint(WeaponDropPoint dropPoint)
	{
		BoltEntity boltEntity = BoltNetwork.Instantiate(BoltPrefabs.Present);
		boltEntity.GetComponent<DroppingPresentController>().Initialize(dropPoint.transform.position, dropPoint.transform.localEulerAngles);
		dropPoint.HasDroppedHere = true;
	}

	private WeaponDropPoint GetCenterDropPoint()
	{
		for (int i = 0; i < _dropPoints.Count; i++)
		{
			if (_dropPoints[i].IsStartAreaDropPoint)
			{
				return _dropPoints[i];
			}
		}
		return null;
	}

	public void SpawnRandomPickup(Vector3 position)
	{
		List<WeaponType> list = new List<WeaponType>(WeaponDropOptions);
		if (_lastSpawnedWeapon != WeaponType.None)
		{
			list.Remove(_lastSpawnedWeapon);
		}
		_lastSpawnedWeapon = list[UnityEngine.Random.Range(0, list.Count)];
		SpawnPickup(position, _lastSpawnedWeapon);
	}

	private static PrefabId MakePrefabId(int id)
	{
		ConstructorInfo ctor = typeof(PrefabId).GetConstructor(
			BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
			null,
			new System.Type[] { typeof(int) },
			null
		);
		return (PrefabId)ctor.Invoke(new object[] { id });
	}

	public void SpawnPickup(Vector3 position, WeaponType weaponType, int ammo = -1)
	{
		PrefabId prefabId;
		switch (weaponType)
		{
		case WeaponType.Sword:
			prefabId = BoltPrefabs.SwordPickup;
			break;
		case WeaponType.Crossbow:
			prefabId = BoltPrefabs.CrossbowPickup;
			break;
		case WeaponType.Reindeer:
			prefabId = BoltPrefabs.ReindeerPickup;
			break;
		case WeaponType.Pistol:
			prefabId = SantaCharacterController.MakePrefabId(13);
			break;
		case WeaponType.LightningGun:
			prefabId = SantaCharacterController.MakePrefabId(14);
			break;
		case WeaponType.SnowballLauncher:
			prefabId = SantaCharacterController.MakePrefabId(16);
			break;
		default:
			throw new NotImplementedException("No prefab assigned to weapon: " + weaponType);
		}
		BoltEntity boltEntity = BoltNetwork.Instantiate(prefabId);
		boltEntity.transform.position = position;
		if (ammo > 0)
		{
			boltEntity.GetComponent<PickupItemController>().SetAmmo(ammo);
		}
	}

	public PickupItemController GetPickupInRangeOf(Vector3 position)
	{
		for (int i = 0; i < _itemPickups.Count; i++)
		{
			if (_itemPickups[i].IsInRangeOf(position))
			{
				return _itemPickups[i];
			}
		}
		return null;
	}

	private void Awake()
	{
		_instance = this;
		_itemPickups = new List<PickupItemController>();
	}
}
