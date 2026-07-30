using System.Collections.Generic;
using UnityEngine;

public class PlayAudioOnce : MonoBehaviour
{
	public List<AudioClip> Clips;

	private void Start()
	{
		GetComponent<AudioSource>().clip = Clips[Random.Range(0, Clips.Count)];
		GetComponent<AudioSource>().Play();
	}
}
