using UnityEngine;

public class FocusManager : Singleton<FocusManager>
{
	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void Update()
	{
		if (Singleton<UIRoot>.Instance.ShouldUnlockMouse())
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			return;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		if (Input.GetMouseButtonDown(0))
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}

	public bool HasFocus()
	{
		return Cursor.lockState == CursorLockMode.Locked;
	}
}
