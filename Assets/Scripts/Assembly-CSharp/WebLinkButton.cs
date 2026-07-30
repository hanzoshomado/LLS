using UnityEngine;

public class WebLinkButton : MonoBehaviour
{
	public string URL;

	public void OnButtonClicked()
	{
		Application.OpenURL(URL);
	}
}
