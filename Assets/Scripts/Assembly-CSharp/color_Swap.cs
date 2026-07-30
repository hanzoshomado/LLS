using UnityEngine;

public class color_Swap : MonoBehaviour
{
	public Renderer Rend;

	public Texture2D ColorSwap_LogicGate;

	private void Start()
	{
		InitializeColors();
	}

	public void InitializeColors()
	{
		Rend = GetComponent<Renderer>();
		Rend.material.SetTexture("_SwapGate", ColorSwap_LogicGate);
	}
}
