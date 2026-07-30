using UnityEngine;

public class UIRoot : Singleton<UIRoot>
{
	public HealthBarUI HealthBarUI;

	public RespawnButtonMenu RespawnButtonMenu;

	public DisconnectedPanel DisconnectedPanel;

	public EscMenu EscMenu;

	private void Start()
	{
		DisconnectedPanel.Hide();
		EscMenu.Hide();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (EscMenu.gameObject.activeSelf)
			{
				EscMenu.Hide();
			}
			else
			{
				EscMenu.Show();
			}
		}
	}

	public bool ShouldUnlockMouse()
	{
		return DisconnectedPanel.gameObject.activeInHierarchy || EscMenu.gameObject.activeInHierarchy;
	}
}
