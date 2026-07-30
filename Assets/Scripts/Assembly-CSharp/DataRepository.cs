using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class DataRepository : Singleton<DataRepository>
{
	private static JsonSerializerSettings _settings;

	private List<string> _loadedDataCorruptWithNoRecovery = new List<string>();

	public bool TryLoad<T>(string filepath, out T loadedData, bool useResourcesFolder = false)
	{
		string fullPath = GetFullPath(filepath, useResourcesFolder);
		if (File.Exists(fullPath))
		{
			string text = string.Empty;
			try
			{
				text = File.ReadAllText(fullPath);
				loadedData = JsonConvert.DeserializeObject<T>(text, GetSettings());
				return true;
			}
			catch (Exception exception)
			{
				Singleton<ErrorManager>.Instance.LogExceptionWithoutPausingGame(exception, text);
				string path = fullPath + ".bak";
				if (File.Exists(path))
				{
					string text2 = string.Empty;
					try
					{
						text2 = File.ReadAllText(path);
						loadedData = JsonConvert.DeserializeObject<T>(text2, GetSettings());
						return true;
					}
					catch (Exception exception2)
					{
						Singleton<ErrorManager>.Instance.LogExceptionWithoutPausingGame(exception2, text2);
					}
				}
				_loadedDataCorruptWithNoRecovery.Add(filepath);
			}
		}
		loadedData = default(T);
		return false;
	}

	public List<string> GetLoadedDataCorruptWithNoRecovery()
	{
		return _loadedDataCorruptWithNoRecovery;
	}

	public void Save(object serializedObject, string fileName, bool useResourcesFolder = false)
	{
		string fullPath = GetFullPath(fileName, useResourcesFolder);
		EnsureDirectoryForFileCreated(fullPath);
		if (File.Exists(fullPath) && !useResourcesFolder)
		{
			string contents = File.ReadAllText(fullPath);
			File.WriteAllText(fullPath + ".bak", contents);
		}
		string contents2 = JsonConvert.SerializeObject(serializedObject, Formatting.None, GetSettings());
		File.WriteAllText(fullPath, contents2);
	}

	private static void EnsureDirectoryForFileCreated(string fullPath)
	{
		string directoryName = Path.GetDirectoryName(fullPath);
		if (directoryName != null)
		{
			Directory.CreateDirectory(directoryName);
		}
	}

	public string GetRootDataPath(bool isUsingResourcesFolder)
	{
		if (isUsingResourcesFolder)
		{
			return Application.dataPath + "/Resources/Data";
		}
		return Application.persistentDataPath;
	}

	public string GetFullPath(string fileName, bool useResourcesFolder = false)
	{
		return GetRootDataPath(useResourcesFolder) + "/" + fileName + ".json";
	}

	public JsonSerializerSettings GetSettings()
	{
		if (_settings == null)
		{
			_settings = CreateSettings();
		}
		return _settings;
	}

	public static JsonSerializerSettings CreateSettings()
	{
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.TypeNameHandling = TypeNameHandling.All;
		jsonSerializerSettings.Converters = new List<JsonConverter>
		{
			new ColorConverter()
		};
		return jsonSerializerSettings;
	}

	public bool FileExists(string fileName)
	{
		return File.Exists(GetFullPath(fileName));
	}
}
