using System;
using UnityEngine;

public class HostNameGenerator : Singleton<HostNameGenerator>
{
	private string[] _nouns;

	private string[] _middleParts;

	private string[] _nounReceivers;

	private void Start()
	{
		_nouns = Resources.Load<TextAsset>("hostNouns").text.Trim().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		_middleParts = Resources.Load<TextAsset>("hostMiddleParts").text.Trim().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		_nounReceivers = Resources.Load<TextAsset>("hostNounReceivers").text.Trim().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		MonoBehaviour.print(_nouns[1]);
		MonoBehaviour.print(_middleParts[1]);
	}

	public string GetRandomHostName()
	{
		string text = _nouns[UnityEngine.Random.Range(0, _nouns.Length)];
		string text2 = _middleParts[UnityEngine.Random.Range(0, _middleParts.Length)];
		if (!text2.StartsWith(",") && !text2.StartsWith("'s"))
		{
			text += " ";
		}
		text += text2;
		return text + " " + _nounReceivers[UnityEngine.Random.Range(0, _nounReceivers.Length)];
	}
}
