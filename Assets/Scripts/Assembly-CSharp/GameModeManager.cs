using System.Collections.Generic;
using Bolt;

public class GameModeManager : GlobalEventListener
{
	private GameModeType _requestedGameMode;

	private string _currentGameName;

	private GameInfoController _gameInfoController;

	private static GameModeManager _instance;

	public static GameModeManager Instance
	{
		get
		{
			return _instance;
		}
	}

	public void ServerSetGameMode(GameModeType value)
	{
		_requestedGameMode = value;
	}

	public bool IsGameModeInfoLoaded()
	{
		return _gameInfoController != null && !_gameInfoController.IsDetached();
	}

	public bool IsLastSantaStanding()
	{
		return _gameInfoController.state.GameMode == 1;
	}

	public int GetVisualSecondsToGameStart()
	{
		int num = _gameInfoController.state.SecondsToGameStart;
		if (HasRoundEnded())
		{
			num += _gameInfoController.SecondsBeforeStartingGame;
		}
		return num;
	}

	public int GetStartGameMaxCountdown()
	{
		return _gameInfoController.SecondsBeforeStartingGame;
	}

	public bool IsWaitingForRoundToStart()
	{
		return _gameInfoController.IsRoundState(RoundState.NotStarted);
	}

	public bool IsRoundActive()
	{
		return _gameInfoController.IsRoundState(RoundState.RoundActive) || !IsLastSantaStanding();
	}

	public bool HasRoundEnded()
	{
		return _gameInfoController.IsRoundState(RoundState.RoundEnded);
	}

	public string GetRoundWinnerName()
	{
		return _gameInfoController.state.WinnerSteamName;
	}

	public override void SceneLoadLocalDone(string map)
	{
		if (BoltNetwork.isServer)
		{
			BoltEntity boltEntity = BoltNetwork.Instantiate(BoltPrefabs.GameInfo);
			_gameInfoController = boltEntity.GetComponent<GameInfoController>();
			_gameInfoController.state.GameMode = (int)_requestedGameMode;
			Singleton<GlobalEventManager>.Instance.Dispatch("GameInfoLoaded");
		}
	}

	public override void EntityAttached(BoltEntity entity)
	{
		if (entity.StateIs<IGameInfoState>())
		{
			_gameInfoController = entity.GetComponent<GameInfoController>();
			if (BoltNetwork.isClient)
			{
				Singleton<GlobalEventManager>.Instance.Dispatch("GameInfoLoaded");
			}
		}
	}

	public override void EntityDetached(BoltEntity entity)
	{
		if (BoltNetwork.isServer && entity.StateIs<ISantaState>() && IsRoundActive() && CharacterTracker.Instance.GetNumberOfLivingCharacters() <= 1)
		{
			_gameInfoController.state.CurrentRoundState = 2;
			List<SantaCharacterController> allLivingCharacters = CharacterTracker.Instance.GetAllLivingCharacters();
			if (allLivingCharacters.Count == 1)
			{
				_gameInfoController.state.WinnerSteamName = allLivingCharacters[0].state.SteamUsername;
				allLivingCharacters[0].PlayWinAnimation();
				PlayerStatsManager.ReportWin(allLivingCharacters[0]);
			}
			else
			{
				_gameInfoController.state.WinnerSteamName = "Nobody";
			}
		}
	}

	public void ResetGameEntitiesToNotStarted()
	{
		foreach (BoltEntity entity in BoltNetwork.entities)
		{
			if (entity.StateIs<ISantaState>() || entity.StateIs<ISpectatorState>() || entity.StateIs<ISantaRagdollState>() || entity.StateIs<IDroppingPresentState>() || entity.StateIs<IPickupItemState>())
			{
				BoltNetwork.Destroy(entity.gameObject);
			}
		}
		foreach (BoltConnection connection in BoltNetwork.connections)
		{
			ServerCharacterManager.Instance.SpawnCharacter(connection);
		}
		if (Singleton<GameSettingsManger>.Instance.ShouldServerStartInSpectatorMode())
		{
			ServerCharacterManager.Instance.SpawnSpectator(null);
		}
		else
		{
			ServerCharacterManager.Instance.SpawnCharacter(null);
		}
	}

	public void SetGameName(string gameName)
	{
		_currentGameName = gameName;
	}

	public string GetGameName()
	{
		return _currentGameName;
	}

	private void Awake()
	{
		_instance = this;
	}
}
