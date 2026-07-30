using UnityEngine;

public class VersionNumberManager : Singleton<VersionNumberManager>
{
	private string _versionString;

	public string GetVersionString()
	{
		if (_versionString == null)
		{
			_versionString = LoadVersionStringFromFile();
		}
		return _versionString;
	}

	public string GetUserFacingVersionDescription()
	{
		string text = "Version";
		if (Singleton<GameVersionManager>.Instance.IsUnityEditor())
		{
			text = ((!Singleton<GameVersionManager>.Instance.IsSteamBuild()) ? "Editor (No Steam) Version" : "Editor (Steam) Version");
		}
		else if (Singleton<GameVersionManager>.Instance.IsDevelopmentPreviewBuild())
		{
			text = "Development Version";
		}
		else if (!Singleton<GameVersionManager>.Instance.IsSteamBuild())
		{
			text = "Built (No Steam) Version";
		}
		return text + ": " + GetVersionString();
	}

	public static string LoadVersionStringFromFile()
	{
		return (Resources.Load("version") as TextAsset).text;
	}

	public bool IsCurrentVersionOlderThan(string otherVersionString)
	{
		if (string.IsNullOrEmpty(otherVersionString))
		{
			return false;
		}
		string[] array = GetVersionString().Split(".".ToCharArray());
		string[] array2 = otherVersionString.Split(".".ToCharArray());
		if (array.Length <= 1 || array2.Length <= 1)
		{
			return false;
		}
		for (int i = 0; i < array.Length && i < array2.Length; i++)
		{
			int result;
			int result2;
			if (int.TryParse(array[i], out result) && int.TryParse(array2[i], out result2))
			{
				if (result > result2)
				{
					return false;
				}
				if (result < result2)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsCurrentVersionNewerThan(string otherVersionString)
	{
		if (string.IsNullOrEmpty(otherVersionString))
		{
			return false;
		}
		string[] array = GetVersionString().Split(".".ToCharArray());
		string[] array2 = otherVersionString.Split(".".ToCharArray());
		if (array.Length <= 1 || array2.Length <= 1)
		{
			return false;
		}
		for (int i = 0; i < array.Length && i < array2.Length; i++)
		{
			int result;
			int result2;
			if (int.TryParse(array[i], out result) && int.TryParse(array2[i], out result2))
			{
				if (result > result2)
				{
					return true;
				}
				if (result < result2)
				{
					return false;
				}
			}
		}
		return false;
	}
}
