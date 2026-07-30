using Bolt;
using UnityEngine;

public class GameInfoController : EntityBehaviour<IGameInfoState>
{
	public int SecondsBeforeStartingGame;

	public int SecondsBeforeStartingGameAfterEnded;

	public int MinPlayersToStartGame;

	private float _serverSecondsLeftToGameStart;

	private bool _isCountingDownToGameStart;

	private bool _isDetached;

	public override void Attached()
	{
		_isDetached = false;
		base.state.AddCallback("SecondsToGameStart", OnSecondsToGameStartUpdated);
		base.state.AddCallback("CurrentRoundState", OnCurrentRoundStateChanged);
		base.state.AddCallback("WinnerSteamName", OnWinnerNameChanged);
	}

	public override void Detached()
	{
		_isDetached = true;
	}

	public bool IsDetached()
	{
		return _isDetached;
	}

	private void OnSecondsToGameStartUpdated()
	{
		Singleton<GlobalEventManager>.Instance.Dispatch("SecondsToGameStartedUpdated");
	}

	private void OnCurrentRoundStateChanged()
	{
		if (IsRoundState(RoundState.NotStarted))
		{
			Singleton<GlobalEventManager>.Instance.Dispatch("GameStartedWaitingToStart");
		}
		else if (IsRoundState(RoundState.RoundActive))
		{
			Singleton<GlobalEventManager>.Instance.Dispatch("GameStarted");
		}
		else if (IsRoundState(RoundState.RoundEnded))
		{
			Singleton<GlobalEventManager>.Instance.Dispatch("GameEnded");
		}
	}

	private void OnWinnerNameChanged()
	{
		Singleton<GlobalEventManager>.Instance.Dispatch("RoundWinnerNameChanged");
	}

	public override void SimulateOwner()
	{
		if (!BoltNetwork.isServer)
		{
			return;
		}
		if (Singleton<GameVersionManager>.Instance.IsUnityEditor() && Input.GetKeyDown(KeyCode.R))
		{
			if (_isCountingDownToGameStart)
			{
				_serverSecondsLeftToGameStart = 0f;
			}
			SecondsBeforeStartingGame = 0;
			MinPlayersToStartGame = 0;
		}
		if (!IsRoundState(RoundState.RoundActive))
		{
			if (base.state.GameMode != 1)
			{
				base.state.CurrentRoundState = 1;
			}
			else
			{
				serverUpdateCountDownToGameStarted();
			}
		}
	}

	public bool IsRoundState(RoundState roundState)
	{
		return base.state.CurrentRoundState == (int)roundState;
	}

	private void serverUpdateCountDownToGameStarted()
	{
		if (!_isCountingDownToGameStart && shouldStartCountDownToNextRoundStart())
		{
			_isCountingDownToGameStart = true;
			_serverSecondsLeftToGameStart = ((!IsRoundState(RoundState.RoundEnded)) ? SecondsBeforeStartingGame : SecondsBeforeStartingGameAfterEnded);
		}
		if (!_isCountingDownToGameStart)
		{
			return;
		}
		_serverSecondsLeftToGameStart -= BoltNetwork.frameDeltaTime;
		int num = Mathf.CeilToInt(_serverSecondsLeftToGameStart);
		if (base.state.SecondsToGameStart != num)
		{
			base.state.SecondsToGameStart = num;
		}
		if (!shouldStartCountDownToNextRoundStart())
		{
			_isCountingDownToGameStart = false;
			Singleton<GlobalEventManager>.Instance.Dispatch("CountDownAborted");
		}
		if (num <= 0)
		{
			if (IsRoundState(RoundState.NotStarted))
			{
				base.state.CurrentRoundState = 1;
			}
			else if (IsRoundState(RoundState.RoundEnded))
			{
				base.state.CurrentRoundState = 0;
				GameModeManager.Instance.ResetGameEntitiesToNotStarted();
			}
			_isCountingDownToGameStart = false;
		}
	}

	private bool shouldStartCountDownToNextRoundStart()
	{
		return (IsRoundState(RoundState.NotStarted) && CharacterTracker.Instance.GetNumberOfLivingCharacters() >= MinPlayersToStartGame) || IsRoundState(RoundState.RoundEnded);
	}
}
