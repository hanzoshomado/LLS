using UnityEngine;

public class TextRenderer : MonoBehaviour
{
	public string TextContent;

	public Color TextColor = Color.green;

	public Color BorderColor = Color.black;

	public int OutlinePadding = 2;

	private GUIStyle _textStyle;

	private GUIStyle _textStyleBorder;

	private void Start()
	{
		_textStyle = new GUIStyle();
		_textStyle.fontSize = 20;
		_textStyle.alignment = TextAnchor.UpperCenter;
		_textStyle.normal.textColor = TextColor;
		_textStyleBorder = new GUIStyle();
		_textStyleBorder.fontSize = 20;
		_textStyleBorder.alignment = TextAnchor.UpperCenter;
		_textStyleBorder.normal.textColor = BorderColor;
	}

	public void OnGUI()
	{
		Vector3 vector = Camera.main.WorldToViewportPoint(base.transform.position);
		Rect position = new Rect(vector.x * (float)Screen.width - 50f, (1f - vector.y) * (float)Screen.height, 100f, 20f);
		position.x += OutlinePadding;
		position.y += OutlinePadding;
		GUI.Label(position, TextContent, _textStyleBorder);
		position.y -= OutlinePadding;
		GUI.Label(position, TextContent, _textStyleBorder);
		position.y -= OutlinePadding;
		GUI.Label(position, TextContent, _textStyleBorder);
		position.x -= OutlinePadding;
		GUI.Label(position, TextContent, _textStyleBorder);
		position.x -= OutlinePadding;
		GUI.Label(position, TextContent, _textStyleBorder);
		position.y += OutlinePadding;
		GUI.Label(position, TextContent, _textStyleBorder);
		position.y += OutlinePadding;
		GUI.Label(position, TextContent, _textStyleBorder);
		position.x += OutlinePadding;
		GUI.Label(position, TextContent, _textStyleBorder);
		position.y -= OutlinePadding;
		GUI.Label(position, TextContent, _textStyle);
	}
}
