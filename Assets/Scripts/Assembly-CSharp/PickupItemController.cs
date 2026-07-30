using Bolt;
using UnityEngine;

public class PickupItemController : EntityBehaviour<IPickupItemState>
{
	public WeaponType WeaponType;

	public FollowWorldTransform PressKeyPrefab;

	public float PickupRange;

	public int StartAmmo;

	private FollowWorldTransform _pressKeyPrefab;

	private float _velocityY;

	public override void Attached()
	{
		base.state.SetTransforms(base.state.Transform, base.transform);
		if (base.entity.IsOwner())
		{
			base.state.Ammo = StartAmmo;
		}
	}

	public void SetAmmo(int amount)
	{
		base.state.Ammo = amount;
	}

	private void OnDestroy()
	{
		hideKeyPressUI();
	}

	public override void SimulateOwner()
	{
		bool flag = false;
		float num = 0f;
		RaycastHit hitInfo;
		if (Physics.Raycast(new Ray(base.transform.position + new Vector3(0f, 10f, 0f), -Vector3.up), out hitInfo, 20f, GetEnvironmentLayerMask()))
		{
			flag = true;
			num = base.transform.position.y - hitInfo.point.y;
		}
		else if (base.transform.position.y < -100f)
		{
			BoltNetwork.Destroy(base.gameObject);
			return;
		}
		float num2 = 0f;
		if (!flag || num > 0f)
		{
			_velocityY += Singleton<PhysicsManager>.Instance.Gravity * BoltNetwork.frameDeltaTime;
			num2 = _velocityY * BoltNetwork.frameDeltaTime;
			if (flag)
			{
				num2 = Mathf.Max(num2, 0f - num);
			}
		}
		else
		{
			_velocityY = 0f;
			num2 = 0f - num;
		}
		base.transform.position += new Vector3(0f, num2, 0f);
	}

	private static int GetEnvironmentLayerMask()
	{
		return 1 << LayerMask.NameToLayer("Environment");
	}

	private void Update()
	{
		if (isLocalPlayerInRange())
		{
			showKeyPressUI();
		}
		else
		{
			hideKeyPressUI();
		}
	}

	private void showKeyPressUI()
	{
		if (_pressKeyPrefab == null)
		{
			_pressKeyPrefab = Object.Instantiate(PressKeyPrefab);
			_pressKeyPrefab.transform.SetParent(Singleton<UIRoot>.Instance.transform, false);
			_pressKeyPrefab.transform.SetAsFirstSibling();
			_pressKeyPrefab.TargetTransform = base.transform;
		}
	}

	private void hideKeyPressUI()
	{
		if (_pressKeyPrefab != null)
		{
			Object.Destroy(_pressKeyPrefab.gameObject);
			_pressKeyPrefab = null;
		}
	}

	private bool isLocalPlayerInRange()
	{
		SantaCharacterController santaWithControl = CharacterTracker.Instance.GetSantaWithControl();
		if (santaWithControl != null)
		{
			return IsInRangeOf(santaWithControl.transform.position);
		}
		return false;
	}

	public bool IsInRangeOf(Vector3 position)
	{
		float magnitude = (position - base.transform.position).magnitude;
		return magnitude <= PickupRange;
	}
}
