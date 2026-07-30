public class GameVersionManager : Singleton<GameVersionManager>
{
	public bool TestNoSteamMode;

	private const bool _isUnityEditor = false;

	private const bool _isSteamBuild = true;

	private const bool _isDevelopmentPreviewBuild = false;

	public bool IsUnityEditor()
	{
		return false;
	}

	public bool IsSteamBuild()
	{
		return !TestNoSteamMode;
	}

	public bool IsDevelopmentPreviewBuild()
	{
		return false;
	}
}
