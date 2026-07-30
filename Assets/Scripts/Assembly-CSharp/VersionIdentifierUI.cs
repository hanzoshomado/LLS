using UnityEngine;
using UnityEngine.UI;

public class VersionIdentifierUI : MonoBehaviour
{
	public Text VersionIdentifier;

	private void Start()
	{
		VersionIdentifier.text = Singleton<VersionNumberManager>.Instance.GetUserFacingVersionDescription();
	}
}
