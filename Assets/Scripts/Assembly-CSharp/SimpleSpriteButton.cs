using System;
using UnityEngine;

public class SimpleSpriteButton : MonoBehaviour
{
	public Sprite UpSprite;

	public Sprite DownSprite;

	public Sprite DisabledSprite;

	private bool _enabled = true;

	private Action _buttonPressedListener;

	public void SetListener(Action listener)
	{
		_buttonPressedListener = listener;
	}

	public void Enable()
	{
		_enabled = true;
		GetComponent<SpriteRenderer>().sprite = UpSprite;
	}

	public void Disable(bool changeLabel = false)
	{
		_enabled = false;
		if (changeLabel)
		{
			GetComponent<SpriteRenderer>().sprite = DisabledSprite;
		}
	}

	public void OnMouseUp()
	{
		if (_enabled)
		{
			GetComponent<SpriteRenderer>().sprite = UpSprite;
			if (_buttonPressedListener != null)
			{
				_buttonPressedListener();
			}
		}
	}

	public void OnMouseDown()
	{
		if (_enabled)
		{
			GetComponent<SpriteRenderer>().sprite = DownSprite;
		}
	}
}
