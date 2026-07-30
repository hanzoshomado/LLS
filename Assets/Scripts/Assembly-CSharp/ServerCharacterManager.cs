using System.Collections.Generic;
using Bolt;
using UnityEngine;

public class ServerCharacterManager : GlobalEventListener
{
	private int _spawnPointIndex;

	private List<SpawnPoint> _spawnPoints;

	private static ServerCharacterManager _instance;

	public static ServerCharacterManager Instance
	{
		get
		{
			return _instance;
		}
	}

	private void Start()
	{
		Singleton<GlobalEventManager>.Instance.AddEventListener("ServerWantsRespawn", onServerWantsRespawn);
		Singleton<GlobalEventManager>.Instance.AddEventListener<SantaCharacterController>("PlayerKilledServer", OnPlayerKilled);
	}

	private void OnDestroy()
	{
		Singleton<GlobalEventManager>.Instance.RemoveEventListener("ServerWantsRespawn", onServerWantsRespawn);
		Singleton<GlobalEventManager>.Instance.RemoveEventListener<SantaCharacterController>("PlayerKilledServer", OnPlayerKilled);
	}

	public override void OnEvent(ClientWantsRespawnEvent respawnEvent)
	{
		if (BoltNetwork.isServer)
		{
			respawnCharacterIfNotAlreadyAlive(respawnEvent.RaisedBy);
		}
	}

	private void onServerWantsRespawn()
	{
		if (BoltNetwork.isServer)
		{
			respawnCharacterIfNotAlreadyAlive(null);
		}
	}

	public override void BoltShutdownBegin(AddCallback registerDoneCallback)
	{
		_spawnPoints = null;
	}

	private void respawnCharacterIfNotAlreadyAlive(BoltConnection connection)
	{
		SantaCharacterController aliveCharacterWithConnection = CharacterTracker.Instance.GetAliveCharacterWithConnection(connection);
		if (aliveCharacterWithConnection == null)
		{
			SantaCharacterController deadCharacterWithConnection = CharacterTracker.Instance.GetDeadCharacterWithConnection(connection);
			if (deadCharacterWithConnection != null)
			{
				BoltNetwork.Destroy(deadCharacterWithConnection.gameObject);
			}
			SpawnCharacter(connection);
		}
	}

	public override void Connected(BoltConnection connection)
	{
		if (BoltNetwork.isServer)
		{
			if (GameModeManager.Instance.IsLastSantaStanding() && !GameModeManager.Instance.IsWaitingForRoundToStart())
			{
				SpawnSpectator(connection);
			}
			else
			{
				SpawnCharacter(connection);
			}
		}
	}

	public override void SceneLoadLocalDone(string map)
	{
		if (BoltNetwork.isServer)
		{
			if (Singleton<GameSettingsManger>.Instance.ShouldServerStartInSpectatorMode())
			{
				SpawnSpectator(null);
			}
			else
			{
				SpawnCharacter(null);
			}
		}
	}

	public void SpawnCharacter(BoltConnection connection)
	{
		SpectatorController spectatorControlledBy = getSpectatorControlledBy(connection);
		if (spectatorControlledBy != null)
		{
			BoltNetwork.Destroy(spectatorControlledBy.gameObject);
		}
		BoltEntity boltEntity = spawnSantaEntity();
		if (connection != null)
		{
			boltEntity.AssignControl(connection);
		}
		else
		{
			boltEntity.TakeControl();
		}
	}

	private SpectatorController getSpectatorControlledBy(BoltConnection connection)
	{
		foreach (BoltEntity entity in BoltNetwork.entities)
		{
			if (entity.StateIs<ISpectatorState>() && entity.controller == connection)
			{
				return entity.GetComponent<SpectatorController>();
			}
		}
		return null;
	}

	public SpectatorController SpawnSpectator(BoltConnection connection)
	{
		Vector3 position = new Vector3(Random.Range(-8, 8), 5f, Random.Range(-8, 8));
		float y = Mathf.Atan2(0f - position.x, 0f - position.z) * 57.29578f;
		BoltEntity boltEntity = BoltNetwork.Instantiate(BoltPrefabs.SpectatorCamera, position, Quaternion.Euler(0f, y, 0f));
		if (connection != null)
		{
			boltEntity.AssignControl(connection);
		}
		else
		{
			boltEntity.TakeControl();
		}
		return boltEntity.GetComponent<SpectatorController>();
	}

	private BoltEntity spawnSantaEntity()
	{
		if (_spawnPoints == null)
		{
			_spawnPoints = new List<SpawnPoint>(Object.FindObjectsOfType<SpawnPoint>());
			ShuffleUtils.ShuffleList(_spawnPoints);
			_spawnPointIndex = 0;
		}
		SpawnPoint spawnPoint = _spawnPoints[_spawnPointIndex];
		_spawnPointIndex++;
		if (_spawnPointIndex >= _spawnPoints.Count)
		{
			_spawnPointIndex = 0;
		}
		Vector3 position = spawnPoint.transform.position;
		float y = Mathf.Atan2(0f - position.x, 0f - position.z) * 57.29578f;
		return BoltNetwork.Instantiate(BoltPrefabs.SantaCharacter, position, Quaternion.Euler(0f, y, 0f));
	}

	private void OnPlayerKilled(SantaCharacterController santaController)
	{
		if (BoltNetwork.isServer)
		{
			SpectatorController spectatorController = null;
			spectatorController = ((!santaController.HasBeenUnderLocalControl()) ? SpawnSpectator(santaController.entity.controller) : SpawnSpectator(null));
			spectatorController.transform.position = santaController.PlayerCamera.transform.position;
			spectatorController.transform.eulerAngles = santaController.PlayerCamera.transform.eulerAngles;
		}
	}

	private void Awake()
	{
		_instance = this;
	}
}
