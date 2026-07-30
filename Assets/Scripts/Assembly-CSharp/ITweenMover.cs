using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ITweenMover : MonoBehaviour
{
	private Image _image;

	private RectTransform _rectTransform;

	private Light _light;

	private Action _moveToCompleteCallback;

	private Action _moveXToCompleteCallback;

	private Action _moveYToCompleteCallback;

	private Action _moveZToCompleteCallback;

	private Action _scaleCompleteCallback;

	private Action _rotateToCompleteCallback;

	private Action _lightIntensityToCompleteCallback;

	public void StopAllTweens()
	{
		_image = null;
		_rectTransform = null;
		_light = null;
		_moveToCompleteCallback = null;
		_moveXToCompleteCallback = null;
		_moveYToCompleteCallback = null;
		_moveZToCompleteCallback = null;
		_scaleCompleteCallback = null;
		_rotateToCompleteCallback = null;
		_lightIntensityToCompleteCallback = null;
		iTween[] components = GetComponents<iTween>();
		for (int i = 0; i < components.Length; i++)
		{
			UnityEngine.Object.Destroy(components[i]);
		}
	}

	public void MoveTo(Vector3 targetPosition, ITweenHash iTweenHash, Action completeCallback = null)
	{
		_moveToCompleteCallback = completeCallback;
		Hashtable hash = iTweenHash.GetHash();
		hash["x"] = targetPosition.x;
		hash["y"] = targetPosition.y;
		hash["z"] = targetPosition.z;
		hash["oncomplete"] = "onMoveToComplete";
		iTween.MoveTo(base.gameObject, hash);
	}

	public void MoveTo(Vector3[] targetPositions, ITweenHash iTweenHash, Action completeCallback = null)
	{
		_moveToCompleteCallback = completeCallback;
		Hashtable hash = iTweenHash.GetHash();
		hash["path"] = targetPositions;
		hash["oncomplete"] = "onMoveToComplete";
		iTween.MoveTo(base.gameObject, hash);
	}

	private void onMoveToComplete()
	{
		if (_moveToCompleteCallback != null)
		{
			Action moveToCompleteCallback = _moveToCompleteCallback;
			_moveToCompleteCallback = null;
			moveToCompleteCallback();
		}
	}

	public void ScaleTo(Vector3 targetScale, ITweenHash iTweenHash, Action scaleCompleteCallback = null)
	{
		_scaleCompleteCallback = scaleCompleteCallback;
		Hashtable hash = iTweenHash.GetHash();
		hash["x"] = targetScale.x;
		hash["y"] = targetScale.y;
		hash["z"] = targetScale.z;
		hash["oncomplete"] = "onScaleToComplete";
		iTween.ScaleTo(base.gameObject, hash);
	}

	private void onScaleToComplete()
	{
		if (_scaleCompleteCallback != null)
		{
			Action scaleCompleteCallback = _scaleCompleteCallback;
			_scaleCompleteCallback = null;
			scaleCompleteCallback();
		}
	}

	public void MoveXTo(float targetX, ITweenHash iTweenHash, Action completeCallback = null)
	{
		_moveXToCompleteCallback = completeCallback;
		Hashtable hash = iTweenHash.GetHash();
		hash["from"] = base.transform.position.x;
		hash["to"] = targetX;
		hash["onupdate"] = "onUpdateTransformX";
		hash["oncomplete"] = "onMoveXToComplete";
		iTween.ValueTo(base.gameObject, hash);
	}

	private void onUpdateTransformX(float value)
	{
		Vector3 position = base.transform.position;
		position.x = value;
		base.transform.position = position;
	}

	private void onMoveXToComplete()
	{
		if (_moveXToCompleteCallback != null)
		{
			Action moveXToCompleteCallback = _moveXToCompleteCallback;
			_moveXToCompleteCallback = null;
			moveXToCompleteCallback();
		}
	}

	public void MoveYTo(float targetY, ITweenHash iTweenHash, Action completeCallback = null)
	{
		_moveYToCompleteCallback = completeCallback;
		Hashtable hash = iTweenHash.GetHash();
		hash["from"] = base.transform.position.y;
		hash["to"] = targetY;
		hash["onupdate"] = "onUpdateTransformY";
		hash["oncomplete"] = "onMoveYToComplete";
		iTween.ValueTo(base.gameObject, hash);
	}

	private void onUpdateTransformY(float value)
	{
		Vector3 position = base.transform.position;
		position.y = value;
		base.transform.position = position;
	}

	private void onMoveYToComplete()
	{
		if (_moveYToCompleteCallback != null)
		{
			Action moveYToCompleteCallback = _moveYToCompleteCallback;
			_moveYToCompleteCallback = null;
			moveYToCompleteCallback();
		}
	}

	public void MoveZTo(float targetZ, ITweenHash iTweenHash, Action completeCallback = null)
	{
		_moveZToCompleteCallback = completeCallback;
		Hashtable hash = iTweenHash.GetHash();
		hash["from"] = base.transform.position.z;
		hash["to"] = targetZ;
		hash["onupdate"] = "onUpdateTransformZ";
		hash["oncomplete"] = "onMoveZToComplete";
		iTween.ValueTo(base.gameObject, hash);
	}

	private void onUpdateTransformZ(float value)
	{
		Vector3 position = base.transform.position;
		position.z = value;
		base.transform.position = position;
	}

	private void onMoveZToComplete()
	{
		if (_moveZToCompleteCallback != null)
		{
			Action moveZToCompleteCallback = _moveZToCompleteCallback;
			_moveZToCompleteCallback = null;
			moveZToCompleteCallback();
		}
	}

	public void RotateTo(Vector3 eulerAngles, ITweenHash iTweenHash, Action completeCallback = null)
	{
		_rotateToCompleteCallback = completeCallback;
		Hashtable hash = iTweenHash.GetHash();
		hash["rotation"] = eulerAngles;
		hash["oncomplete"] = "onRotateToComplete";
		iTween.RotateTo(base.gameObject, hash);
	}

	private void onRotateToComplete()
	{
		if (_rotateToCompleteCallback != null)
		{
			Action rotateToCompleteCallback = _rotateToCompleteCallback;
			_rotateToCompleteCallback = null;
			rotateToCompleteCallback();
		}
	}

	public void FieldOfViewTo(float targetValue, ITweenHash iTweenHash)
	{
		Hashtable hash = iTweenHash.GetHash();
		hash["from"] = GetComponent<Camera>().fieldOfView;
		hash["to"] = targetValue;
		hash["onupdate"] = "onUpdateFieldOfView";
		iTween.ValueTo(base.gameObject, hash);
	}

	private void onUpdateFieldOfView(float value)
	{
		GetComponent<Camera>().fieldOfView = value;
	}

	public void ImageFillTo(float targetValue, ITweenHash iTweenHash)
	{
		_image = GetComponent<Image>();
		Hashtable hash = iTweenHash.GetHash();
		hash["from"] = _image.fillAmount;
		hash["to"] = targetValue;
		hash["onupdate"] = "onUpdateImageFill";
		iTween.ValueTo(base.gameObject, hash);
	}

	private void onUpdateImageFill(float value)
	{
		_image.fillAmount = value;
	}

	public void ImageAlphaTo(float targetValue, ITweenHash iTweenHash)
	{
		_image = GetComponent<Image>();
		Hashtable hash = iTweenHash.GetHash();
		hash["from"] = _image.color.a;
		hash["to"] = targetValue;
		hash["onupdate"] = "onUpdateImageAlpha";
		iTween.ValueTo(base.gameObject, hash);
	}

	private void onUpdateImageAlpha(float value)
	{
		Color color = _image.color;
		color.a = value;
		_image.color = color;
	}

	public void RectTransformWidthTo(float targetValue, ITweenHash iTweenHash)
	{
		_rectTransform = GetComponent<RectTransform>();
		Hashtable hash = iTweenHash.GetHash();
		hash["from"] = _rectTransform.sizeDelta.x;
		hash["to"] = targetValue;
		hash["onupdate"] = "onUpdateRectTransformWidth";
		iTween.ValueTo(base.gameObject, hash);
	}

	private void onUpdateRectTransformWidth(float value)
	{
		Vector2 sizeDelta = _rectTransform.sizeDelta;
		sizeDelta.x = value;
		_rectTransform.sizeDelta = sizeDelta;
	}

	public void AnchoredPositionTo(Vector2 targetPosition, ITweenHash iTweenHash)
	{
		_rectTransform = GetComponent<RectTransform>();
		Hashtable hash = iTweenHash.GetHash();
		hash["from"] = _rectTransform.anchoredPosition;
		hash["to"] = targetPosition;
		hash["onupdate"] = "onUpdateAnchoredPosition";
		iTween.ValueTo(base.gameObject, hash);
	}

	private void onUpdateAnchoredPosition(Vector2 value)
	{
		_rectTransform.anchoredPosition = value;
	}

	public void LightShadowStrengthTo(float targetValue, ITweenHash iTweenHash)
	{
		_light = GetComponent<Light>();
		Hashtable hash = iTweenHash.GetHash();
		hash["from"] = _light.shadowStrength;
		hash["to"] = targetValue;
		hash["onupdate"] = "onUpdateLightShadowStrength";
		iTween.ValueTo(base.gameObject, hash);
	}

	private void onUpdateLightShadowStrength(float value)
	{
		_light.shadowStrength = value;
	}

	public void LightIntensityTo(float targetValue, ITweenHash iTweenHash, Action completeCompleteCallback = null)
	{
		_light = GetComponent<Light>();
		Hashtable hash = iTweenHash.GetHash();
		hash["from"] = _light.intensity;
		hash["to"] = targetValue;
		hash["onupdate"] = "onUpdateLightIntensity";
		hash["oncomplete"] = "onLightIntensityToComplete";
		_lightIntensityToCompleteCallback = completeCompleteCallback;
		iTween.ValueTo(base.gameObject, hash);
	}

	private void onUpdateLightIntensity(float value)
	{
		_light.intensity = value;
	}

	private void onLightIntensityToComplete()
	{
		if (_lightIntensityToCompleteCallback != null)
		{
			Action lightIntensityToCompleteCallback = _lightIntensityToCompleteCallback;
			_lightIntensityToCompleteCallback = null;
			lightIntensityToCompleteCallback();
		}
	}
}
