using UnityEngine;

public class Draggable : MonoBehaviour
{
	public bool IsDraggingEnabled = true;

	public float ZWhenDragging = -1f;

	public float ZNormal;

	private bool _isDragging;

	private bool _draggingHasMovedPosition;

	private Vector3 _initialMouseOffset;

	private void Update()
	{
		if (!_isDragging)
		{
			return;
		}
		Vector3 vector = GetWorldPosFromTapPosition() - _initialMouseOffset;
		if ((base.transform.position - vector).magnitude > 0.05f)
		{
			if (!_draggingHasMovedPosition)
			{
				_draggingHasMovedPosition = true;
				base.gameObject.SendMessage("OnDraggingStarted");
			}
			base.transform.position = vector;
		}
		base.gameObject.SendMessage("OnDraggingUpdated");
	}

	private Vector3 GetWorldPosFromTapPosition()
	{
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.z = 10f;
		Vector3 result = Camera.main.ScreenToWorldPoint(mousePosition);
		result.z = ZWhenDragging;
		return result;
	}

	private void OnMouseDown()
	{
		if (IsDraggingEnabled)
		{
			_isDragging = true;
			base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, ZWhenDragging);
			_initialMouseOffset = GetWorldPosFromTapPosition() - base.transform.position;
			_draggingHasMovedPosition = false;
		}
	}

	private void OnMouseUp()
	{
		if (_isDragging)
		{
			_isDragging = false;
			base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, ZNormal);
			if (!_draggingHasMovedPosition)
			{
				base.gameObject.SendMessage("OnTapOnDragObject");
			}
			else
			{
				base.gameObject.SendMessage("OnDraggingEnded");
			}
		}
	}

	private void OnDraggingStarted()
	{
	}

	private void OnDraggingEnded()
	{
	}

	private void OnDraggingUpdated()
	{
	}

	private void OnTapOnDragObject()
	{
	}
}
