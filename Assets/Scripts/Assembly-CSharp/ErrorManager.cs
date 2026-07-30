using System;
using System.Collections;
using UnityEngine;

public class ErrorManager : Singleton<ErrorManager>
{
	public ErrorCanvas ErrorCanvasPrefab;

	private bool _hasCrashed;

	public override void Awake()
	{
		base.Awake();
		Application.logMessageReceived += HandleLog;
	}

	private void OnDisable()
	{
		Application.logMessageReceived -= HandleLog;
	}

	public bool HasCrashed()
	{
		return _hasCrashed;
	}

	public void LogExceptionWithoutPausingGame(Exception exception, string errorData = null)
	{
		sendExceptionDetailsToLoggly(LogType.Exception, exception.Message, exception.StackTrace, errorData);
	}

	private void HandleLog(string logString, string stackTrace, LogType type)
	{
		if (!_hasCrashed && (type == LogType.Error || type == LogType.Exception) && !logString.StartsWith("TwitchChatter"))
		{
			_hasCrashed = true;
			ErrorCanvas errorCanvas = UnityEngine.Object.Instantiate(ErrorCanvasPrefab);
			errorCanvas.StackTraceText.text = logString + " " + stackTrace;
			Time.timeScale = 0f;
			sendExceptionDetailsToLoggly(type, logString, stackTrace);
		}
	}

	private void sendExceptionDetailsToLoggly(LogType type, string logString, string stackTrace, string errorData = null)
	{
		string value = type.ToString();
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("LEVEL", value);
		wWWForm.AddField("Message", logString);
		wWWForm.AddField("Stack_Trace", stackTrace);
		wWWForm.AddField("Device_Model", SystemInfo.deviceModel);
		if (!string.IsNullOrEmpty(errorData))
		{
			wWWForm.AddField("ErrorData", errorData);
		}
		wWWForm.AddField("IsEditor", "false");
		StartCoroutine(SendDataToLoggly(wWWForm));
	}

	public IEnumerator SendDataToLoggly(WWWForm form)
	{
		yield return new WWW("http://logs-01.loggly.com/inputs/72ad6667-99a1-4af5-8c27-2d51300cc2b0/tag/SantaGame", form);
	}
}
