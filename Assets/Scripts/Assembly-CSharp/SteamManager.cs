using System;
using System.Text;
using Steamworks;
using UnityEngine;

public class SteamManager : Singleton<SteamManager>
{
	public uint SteamAppID;

	public string TestSteamUsername;

	private SteamAPIWarningMessageHook_t _steamAPIWarningMessageHook;

	private bool _hasBeenInitialized;

	public bool Initialized
	{
		get
		{
			return Singleton<SteamManager>.Instance._hasBeenInitialized;
		}
	}

	private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	public string GetSteamUsername()
	{
		if (!Singleton<GameVersionManager>.Instance.IsSteamBuild())
		{
			return TestSteamUsername;
		}
		return SteamFriends.GetPersonaName();
	}

	public bool Initialize()
	{
		if (!Singleton<GameVersionManager>.Instance.IsSteamBuild())
		{
			return true;
		}
		if (!Packsize.Test())
		{
			Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
		}
		if (!DllCheck.Test())
		{
			Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
		}
		try
		{
			if (SteamAPI.RestartAppIfNecessary(new AppId_t(SteamAppID)))
			{
				Application.Quit();
				return false;
			}
		}
		catch (DllNotFoundException ex)
		{
			string text = "[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + ex;
			Application.Quit();
			return false;
		}
		_hasBeenInitialized = SteamAPI.Init();
		if (!_hasBeenInitialized)
		{
			return false;
		}
		AddAPIWarningMessageHook();
		return true;
	}

	private void AddAPIWarningMessageHook()
	{
		if (_steamAPIWarningMessageHook == null)
		{
			_steamAPIWarningMessageHook = SteamAPIDebugTextHook;
			SteamClient.SetWarningMessageHook(_steamAPIWarningMessageHook);
		}
	}

	private void OnDestroy()
	{
		if (_hasBeenInitialized)
		{
			SteamAPI.Shutdown();
		}
	}

	private void Update()
	{
		if (_hasBeenInitialized)
		{
			SteamAPI.RunCallbacks();
		}
	}
}
