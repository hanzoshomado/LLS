using Bolt;
using UnityEngine;

public class SpectatorController : EntityBehaviour<ISpectatorState>
{
	public float CursorRotationSpeedHorizontal;

	public float CursorRotationSpeedVertical;

	public float TiltRotationSpeed;

	public float MoveSpeed;

	public float SprintSpeedMultiplier;

	public float MoveUpDownSpeed;

	public float MinCameraTilt;

	public float MaxCameraTilt;

	public float MinFieldOfView;

	public float MaxFieldOfView;

	public float FieldOfViewMultiplier;

	public float MouseSensitivity;

	public bool InvertMouse;

	public Camera Camera;

	private bool _isUnderLocalControl;

	public override void Attached()
	{
		Camera.gameObject.SetActive(false);
	}

	public override void ControlGained()
	{
		Camera.gameObject.SetActive(true);
		_isUnderLocalControl = true;
	}

	public override void ControlLost()
	{
		Camera.gameObject.SetActive(false);
		_isUnderLocalControl = false;
	}

	private void Update()
	{
		if (!Singleton<FocusManager>.Instance.HasFocus() || !_isUnderLocalControl)
		{
			return;
		}
		float num = ((!Input.GetKey(KeyCode.LeftShift)) ? 1f : SprintSpeedMultiplier);
		if (Input.GetKey(KeyCode.W))
		{
			base.transform.position += base.transform.forward * MoveSpeed * Time.fixedDeltaTime * num;
		}
		else if (Input.GetKey(KeyCode.S))
		{
			base.transform.position -= base.transform.forward * MoveSpeed * Time.fixedDeltaTime * num;
		}
		if (Input.GetKey(KeyCode.D))
		{
			base.transform.position += base.transform.right * MoveUpDownSpeed * Time.fixedDeltaTime * num;
		}
		else if (Input.GetKey(KeyCode.A))
		{
			base.transform.position -= base.transform.right * MoveUpDownSpeed * Time.fixedDeltaTime * num;
		}
		if (Input.GetKey(KeyCode.Space))
		{
			base.transform.position += Vector3.up * MoveUpDownSpeed * Time.fixedDeltaTime * num;
		}
		else if (Input.GetKey(KeyCode.LeftControl))
		{
			base.transform.position -= Vector3.up * MoveUpDownSpeed * Time.fixedDeltaTime * num;
		}
		float z = 0f;
		if (Input.GetKey(KeyCode.E))
		{
			z = TiltRotationSpeed * Time.fixedDeltaTime;
		}
		else if (Input.GetKey(KeyCode.Q))
		{
			z = (0f - TiltRotationSpeed) * Time.fixedDeltaTime;
		}
		if (!Singleton<UIRoot>.Instance.ShouldUnlockMouse() || Input.GetMouseButton(1))
		{
			float mouseSensitivity = MouseSensitivity;
			int num2 = ((!InvertMouse) ? 1 : (-1));
			float num3 = Input.GetAxis("Mouse Y") * mouseSensitivity * (float)num2;
			float num4 = Input.GetAxis("Mouse X") * mouseSensitivity;
			float y = num4 * CursorRotationSpeedHorizontal;
			base.transform.localEulerAngles += new Vector3(0f, y, z);
			float num5 = base.transform.localEulerAngles.x + num3 * CursorRotationSpeedVertical;
			if (num5 > 180f)
			{
				num5 -= 360f;
			}
			if (num5 < -180f)
			{
				num5 += 360f;
			}
			num5 = Mathf.Max(MinCameraTilt, num5);
			num5 = Mathf.Min(MaxCameraTilt, num5);
			base.transform.localEulerAngles = new Vector3(num5, base.transform.localEulerAngles.y, base.transform.localEulerAngles.z);
		}
		float axis = Input.GetAxis("Mouse ScrollWheel");
		if (Camera != null && axis != 0f)
		{
			float b = Camera.fieldOfView + axis * FieldOfViewMultiplier * Time.fixedDeltaTime;
			b = Mathf.Max(MinFieldOfView, b);
			b = Mathf.Min(MaxFieldOfView, b);
			Camera.fieldOfView = b;
		}
	}
}
