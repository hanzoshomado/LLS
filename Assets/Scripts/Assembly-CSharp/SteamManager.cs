using System;
using System.IO;
using System.Text;
using Steamworks;
using UnityEngine;

public class SteamManager : Singleton<SteamManager>
{
	[Serializable]
	private class SavedUsername
	{
		public string Username;
	}

	private const string UsernameFileName = "username.json";

	public uint SteamAppID;

	public string TestSteamUsername;

	private string _savedUsername = string.Empty;

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

	public override void Awake()
	{
		base.Awake();
		// Runs before any Start(), so the menu can read this when it fills the name field.
		_savedUsername = loadSavedUsername();
		if (_savedUsername.Length != 0)
		{
			TestSteamUsername = _savedUsername;
		}
	}

	private static string GetUsernameFilePath()
	{
		return Path.Combine(Application.persistentDataPath, UsernameFileName);
	}

	// Empty when nothing has been saved yet, which is how the menu knows to invent a name.
	public string GetSavedTestUsername()
	{
		return _savedUsername;
	}

	public void SaveTestUsername()
	{
		string text = ((TestSteamUsername == null) ? string.Empty : TestSteamUsername.Trim());
		if (text.Length == 0 || text == _savedUsername)
		{
			return;
		}
		_savedUsername = text;
		try
		{
			SavedUsername savedUsername = new SavedUsername();
			savedUsername.Username = text;
			File.WriteAllText(GetUsernameFilePath(), JsonUtility.ToJson(savedUsername));
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Could not save username to " + GetUsernameFilePath() + ": " + ex.Message);
		}
	}

	private static string loadSavedUsername()
	{
		string usernameFilePath = GetUsernameFilePath();
		try
		{
			if (!File.Exists(usernameFilePath))
			{
				return string.Empty;
			}
			SavedUsername savedUsername = JsonUtility.FromJson<SavedUsername>(File.ReadAllText(usernameFilePath));
			if (savedUsername == null || savedUsername.Username == null)
			{
				return string.Empty;
			}
			return savedUsername.Username.Trim();
		}
		catch (Exception ex)
		{
			// A corrupt file just means we fall back to a fresh name rather than blocking startup.
			Debug.LogWarning("Could not read username from " + usernameFilePath + ": " + ex.Message);
			return string.Empty;
		}
	}

	public string GetSteamUsername()
	{
		if (!Singleton<GameVersionManager>.Instance.IsSteamBuild())
		{
			// An empty name replicates as a blank name tag, which just looks broken.
			string text = ((TestSteamUsername == null) ? string.Empty : TestSteamUsername.Trim());
			return (text.Length == 0) ? "Santa" : text;
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
