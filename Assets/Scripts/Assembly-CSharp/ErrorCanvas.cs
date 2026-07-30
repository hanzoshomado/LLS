using UnityEngine;
using UnityEngine.UI;

public class ErrorCanvas : MonoBehaviour
{
	public Text StackTraceText;

	public void OnExitButtonClicked()
	{
		Application.Quit();
	}
}
