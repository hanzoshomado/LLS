using System.Collections;
using UnityEngine;

public class FollowWorldPosition : MonoBehaviour
{
	public bool AlwaysVisible;

	public GameObject[] ObjectsToHideWhenBehind;

	public GameObject[] ObjectsToShowWhenBehind;

	public Vector2 AnchoredPositionWhenBehind;

	public float ScaleWhenBehind;

	public Vector2 AnchorPositionOffset;

	public Vector3 WorldPositionOffset;

	public float ScaleMultiplierPerDistance = 30f;

	public float BaseScaleMultiplier = 1f;

	public bool ScaleWithDistance = true;

	public bool ScaleWithFieldOfView;

	public float ScaleDividerPerFieldOfView = 20f;

	public bool RespectCameraRect;

	private RectTransform _rectTransform;

	private Transform _cameraTransform;

	private CanvasRenderer _canvasRenderer;

	protected Vector3 _targetPosition;

	private bool _isVisible;

	private bool _forceHidden;

	private Camera _mainCamera;

	public float DBG_DistanceToCamera;

	private float _dynamicScaleMultiplier;

	private void Awake()
	{
		_rectTransform = (RectTransform)base.transform;
		_canvasRenderer = GetComponent<CanvasRenderer>();
		_dynamicScaleMultiplier = 1f;
		_isVisible = true;
	}

	public void SetTargetPosition(Vector3 targetPosition)
	{
		_targetPosition = targetPosition;
	}

	public void SetForceHidden(bool shouldHide)
	{
		_forceHidden = shouldHide;
	}

	protected virtual void Update()
	{
		UpdatePosition();
	}

	public void UpdatePosition()
	{
		_mainCamera = Camera.main;
		if (!(_mainCamera != null))
		{
			return;
		}
		_cameraTransform = _mainCamera.transform;
		Vector3 viewportPosition = Camera.main.WorldToViewportPoint(_targetPosition + WorldPositionOffset);
		if (viewportPosition.z <= 0f || (AlwaysVisible && (viewportPosition.x < 0f || viewportPosition.x > 1f || viewportPosition.y < -0f || viewportPosition.y > 1f)))
		{
			if (AlwaysVisible)
			{
				setVisible(true);
				setvisibilityOfObjects(ObjectsToHideWhenBehind, false);
				setvisibilityOfObjects(ObjectsToShowWhenBehind, true);
				_rectTransform.anchoredPosition = new Vector2(Singleton<UIManager>.Instance.UIRoot.sizeDelta.x / 2f, AnchoredPositionWhenBehind.y);
				_rectTransform.localScale = ScaleWhenBehind * Vector3.one;
			}
			else
			{
				setvisibilityOfObjects(ObjectsToHideWhenBehind, false);
				setVisible(false);
			}
			return;
		}
		setvisibilityOfObjects(ObjectsToHideWhenBehind, !_forceHidden);
		setvisibilityOfObjects(ObjectsToShowWhenBehind, false);
		setVisible(true);
		Vector2 uIRootAnchoredPositionFromViewportPosition = Singleton<UIManager>.Instance.GetUIRootAnchoredPositionFromViewportPosition(viewportPosition, RespectCameraRect);
		_rectTransform.anchoredPosition = uIRootAnchoredPositionFromViewportPosition + AnchorPositionOffset;
		if (ScaleWithDistance || ScaleWithFieldOfView)
		{
			Vector3 localScale = _dynamicScaleMultiplier * Vector3.one;
			if (ScaleWithDistance)
			{
				float num = (DBG_DistanceToCamera = (_cameraTransform.position - _targetPosition).magnitude);
				localScale *= BaseScaleMultiplier * ScaleMultiplierPerDistance / num;
			}
			if (ScaleWithFieldOfView && !_mainCamera.orthographic)
			{
				localScale *= ScaleDividerPerFieldOfView / _mainCamera.fieldOfView;
			}
			_rectTransform.localScale = localScale;
		}
	}

	private void setvisibilityOfObjects(GameObject[] objects, bool visible)
	{
		for (int i = 0; i < objects.Length; i++)
		{
			objects[i].SetActive(visible);
		}
	}

	private void setVisible(bool visible)
	{
		if (_isVisible != visible)
		{
			_canvasRenderer.SetAlpha((!visible) ? 0f : 1f);
			_isVisible = visible;
		}
	}

	public void PlayScaleAnimation(float scaleTime = 0.5f)
	{
		Hashtable hashtable = iTween.Hash();
		hashtable["from"] = 0f;
		hashtable["to"] = 1f;
		hashtable["onupdate"] = "onUpdateDynamicScale";
		hashtable["time"] = scaleTime;
		_dynamicScaleMultiplier = 0f;
		_rectTransform.localScale = Vector3.zero;
		iTween.ValueTo(base.gameObject, hashtable);
	}

	private void onUpdateDynamicScale(float value)
	{
		_dynamicScaleMultiplier = value;
	}
}
