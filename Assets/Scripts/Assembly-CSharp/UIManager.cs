using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
	public RectTransform UIRoot;

	public Vector2 GetUIRootAnchoredPosition(Vector3 worldPosition)
	{
		Vector3 viewportPosition = Camera.main.WorldToViewportPoint(worldPosition);
		return GetUIRootAnchoredPositionFromViewportPosition(viewportPosition);
	}

	public Vector2 GetUIRootAnchoredPositionFromViewportPosition(Vector3 viewportPosition, bool respectCameraRect = false)
	{
		if (respectCameraRect)
		{
			float num = UIRoot.rect.width * viewportPosition.x;
			float num2 = UIRoot.rect.width * Camera.main.rect.xMin;
			float num3 = UIRoot.rect.width * Camera.main.rect.xMax;
			float num4 = UIRoot.rect.height * viewportPosition.y;
			float num5 = UIRoot.rect.height * Camera.main.rect.yMin;
			float num6 = UIRoot.rect.height * Camera.main.rect.yMax;
			return new Vector2(viewportPosition.x * (num3 - num2), viewportPosition.y * (num6 - num5));
		}
		return new Vector2(UIRoot.rect.width * viewportPosition.x, UIRoot.rect.height * viewportPosition.y);
	}

	public Vector2 GetViewportPositionFromAnchoredPosition(Vector2 anchoredPosition)
	{
		return new Vector2(anchoredPosition.x / UIRoot.rect.width, anchoredPosition.y / UIRoot.rect.height);
	}

	public bool IsMouseOverUIElement()
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.position = Input.mousePosition;
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointerEventData, list);
		for (int i = 0; i < list.Count; i++)
		{
			GameObject gameObject = list[i].gameObject;
			if (gameObject.GetComponent<Image>() != null || gameObject.GetComponent<Text>() != null || gameObject.GetComponent<InputField>() != null)
			{
				UIManager componentInParent = gameObject.transform.GetComponentInParent<UIManager>();
				return componentInParent != null;
			}
		}
		return false;
	}

	public bool IsMouseOverUIElement(GameObject uiElement)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.position = Input.mousePosition;
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointerEventData, list);
		for (int i = 0; i < list.Count; i++)
		{
			GameObject gameObject = list[i].gameObject;
			if (gameObject == uiElement)
			{
				return true;
			}
		}
		return false;
	}
}
