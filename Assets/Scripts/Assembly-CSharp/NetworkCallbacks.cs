using System.Collections.Generic;
using Bolt;
using UnityEngine;

[BoltGlobalBehaviour]
public class NetworkCallbacks : GlobalEventListener
{
	private List<string> _logMessages;

	private void Awake()
	{
		_logMessages = new List<string>();
	}

	public override void OnEvent(LogEvent evnt)
	{
		if (WeaponDebugger.IsRequest(evnt.message))
		{
			return;
		}
		_logMessages.Insert(0, evnt.message);
	}

	private void OnGUI()
	{
		if (_logMessages.Count != 0)
		{
			int num = Mathf.Min(5, _logMessages.Count);
			GUILayout.BeginArea(new Rect(Screen.width / 2 - 200, Screen.height - 100, 400f, 100f), GUI.skin.box);
			for (int i = 0; i < num; i++)
			{
				GUILayout.Label(_logMessages[i]);
			}
			GUILayout.EndArea();
		}
	}
}
