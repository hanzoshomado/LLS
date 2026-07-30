using System;
using System.Collections;
using Bolt;
using UnityEngine;
using System.Reflection; 

 
[Serializable]
public class RangedWeaponStats
{
    public float SpawnDistance;
    public float TimeBetweenAttacks;
    public float MovementMultiplier;
    public Transform MuzzlePoint;
    public ParticleSystem MuzzleFlash;
    public float Range;
    public LineRenderer Beam;
    public float AmmoDrainPerSecond;
    public float DamagePerSecond;
}

public class SantaCharacterController : EntityEventListener<ISantaState>
{
	[Header("Core References")]
	public Camera PlayerCamera;
	private float _baseFOV;
	public float SprintFOVBoost = 10f;
	public float FOVLerpSpeed = 8f;
	public WeaponModel[] WeaponModels;
	public Animator UpperAnimator;
	public Animator LowerAnimator;
	public Transform CameraRoot;
	public Transform TiltRoot;
	public Transform LegsRoot;
	public Transform NameTagAnchorPoint;
	public GameObject Head;

	[Header("Movement")]
	public float MoveSpeed;
	public float AnimatorRunSpeed;
	public float CharacterCollisionRadius;
	public float JumpVelocity;
	public float FallHeightToKillPlayer;
	public float MinTiltX;
	public float MaxTiltX;
	public float MouseRotationSpeedY = 2f;
	public float MouseRotationSpeedX = 2f;

	[Header("Ground / Wall Detection")]
	public float WallRaycastDistance;
	public float WallSlipFactor;
	public float WallSlipMinimum;
	public float FootRaycastDistance;
	public Transform FootRaycastOrigin;
	public Transform WallRaycastPointsOrigin;
	public Transform[] WallRaycastPoints;

	[Header("Stamina / Sprint")]
	public float StaminaToStartSprinting;
	public float SprintMoveMultiplier;
	public float SprintingStaminaDrainedPerSecond;
	public float StaminaRegenPerSecond;
	public float StartStamina;

	[Header("Health / Death")]
	public int StartHitpoints;
	public Transform BloodSpawnPoint;
	public SantaCharacterRagdollSource SantaCharacterRagdollSource;
	public float DeathFallVelocity;
	public float DeathFallVelocityY;

	[Header("Fists")]
	public int PunchDamage;
	public float TimeBetweenFistAttacks;

	[Header("Sword")]
	public int SwordDamage;
	public float TimeBetweenSwordAttacks;
	public float SwordMovementMultiplier;

	[Header("Crossbow")]
	public float CrossBowSpawnDistance;
	public float TimeBetweenCrossbowAttacks;
	public float CrossbowMovementMultiplier;
	public CrossbowAttachPoint CrossbowAttachPointHead;
	public CrossbowAttachPoint CrossbowAttachPointBody;
	public CrossbowAttachPoint CrossbowAttachPointReindeer;
	public Transform CrossbowAttachedPrefab;
	public Transform AimCameraPosition;
	public ITweenHash AimInHash;
	public ITweenHash AimOutHash;

	[Header("Reindeer")]
	public Transform ReindeerCameraPosition;
	public ReindeerPlayerAnimator ReindeerAnimator;
	public float ReindeerMovementMultiplier;
	public float ReindeerRotationYPerSecond;
	public float ReindeerRotationYPerSecondWhileSprinting;
	public float ReindeerTiltSpeedMultiplier;
	public float ReindeerGroundOffset;
	public float ReindeerSprintMultiplier;
	public float ReindeerStaminaToStartSprinting;
	public int ReindeerHornDamage;
	public float ReindeerStaminaDrain;
	public ITweenHash ReindeerCameraInHash;
	public ITweenHash ReindeerCameraOutHash;

	[Header("Pistol")]
	public RangedWeaponStats PistolStats;

	[Header("Lightning")]
	public RangedWeaponStats LightningStats;
	public GameObject LightningBeamInstance;

	[Header("Win Animation")]
	public float WinCameraRotationMultiplier;

	// ---- Private runtime state (not shown in Inspector) ----

	private Vector3 _lastSlipVector;
	private bool _forward;
	private bool _backward;
	private bool _left;
	private bool _right;
	private bool _jump;
	private bool _pickupButtonDown;
	private bool _putDownButtonDown;
	private bool _attack1Held;
	private bool _attack2Held;
	private bool _sprintKeyHeld;
	private float _mouseDeltaX;
	private float _mouseDeltaY;
	private Vector3 _velocity;
	private int _lastDamagedByAttackID;
	private int _reindeerLastDamagedByAttackID;
	private BoltEntity _lastDamagedByEntity;
	private int _lastAttackIDRendered;
	private bool _hasBeenUnderLocalControl;
	private float _stamina;
	private float _timeOfLastItemPickup;
	private bool _isDetached;
	private Vector3 _cameraMeleePosition;
	private Vector3 _cameraMeleeLocalEulerAngles;
	private bool _isInAimState;
	private float _timeStartedTurningReideer = -1f;
	private GroundType _groundType;
	private GroundType _lastGroundType;
	private float _lastGroundLandTime;
	private bool _wasOnGround;
	private bool _isPlayingWinAnimation;
	private float _lastPistolFireInputTime = -999f;
	private bool _isFiringLightning;
	private bool _wasFiringLightningLastFrame;
	private float _lightningAmmoAccumulator;
	private float _lightningDamageAccumulator;

	public GroundType GetGroundType()
	{
		return (!HasBeenUnderLocalControl()) ? ((GroundType)base.state.IsOnGroundType) : _groundType;
	}

	private void Awake()
	{
		PlayerCamera.gameObject.SetActive(false);
		_cameraMeleePosition = PlayerCamera.transform.localPosition;
		_cameraMeleeLocalEulerAngles = PlayerCamera.transform.localEulerAngles;
	    _baseFOV = PlayerCamera.fieldOfView;
	}

	private void Start()
	{
		UpperAnimator.SetFloat("MoveSpeed", AnimatorRunSpeed);
		LowerAnimator.SetFloat("MoveSpeed", AnimatorRunSpeed);
	}

	public override void Attached()
	{
		base.state.SetTransforms(base.state.Transform, base.transform);
		base.state.SetTransforms(base.state.TiltRootTransform, TiltRoot);
		base.state.AddCallback("EquippedWeapon", OnEquippedWeaponChanged);
		base.state.AddCallback("IsMoving", OnIsMovingChanged);
		base.state.AddCallback("IsOnGroundType", OnIsOnGroundTypeChanged);
		base.state.AddCallback("IsSprinting", OnIsSprintingChanged);
		base.state.AddCallback("IsAiming", OnIsAimingChanged);
		base.state.AddCallback("ExecutingAttackID", OnExecutingAttackIDChanged);
		if (base.entity.IsOwner())
		{
			base.state.HitPoints = StartHitpoints;
			base.state.EquippedWeapon = 0;
		}
		_stamina = StartStamina;
		OnEquippedWeaponChanged();
		StartCoroutine(TryCreateNameTagAfterWait());
	}

	public override void Detached()
	{
		_isDetached = true;
	}

	public bool IsDetached()
	{
		return _isDetached;
	}

	private IEnumerator TryCreateNameTagAfterWait()
	{
		yield return new WaitForSeconds(1f);
		if (!HasBeenUnderLocalControl())
		{
			SantaCharacterNameTagManager.Instance.CreateNameTagFor(this);
		}
	}

	public bool IsControlledBy(BoltConnection connection)
	{
		return base.entity.controller == connection;
	}

	public bool IsAlive()
	{
		return base.state.HitPoints > 0;
	}

	public override void ControlGained()
	{
		PlayerCamera.gameObject.SetActive(true);
		_hasBeenUnderLocalControl = true;
		Singleton<GlobalEventManager>.Instance.Dispatch("CharacterWithControlSpawned", this);
		SendSteamNameSetEvent();
	}

	private void SendSteamNameSetEvent()
	{
		ClientSetUsernameEvent clientSetUsernameEvent = ClientSetUsernameEvent.Create(base.entity);
		clientSetUsernameEvent.SteamUsername = Singleton<SteamManager>.Instance.GetSteamUsername();
		clientSetUsernameEvent.Send();
	}

	public bool HasBeenUnderLocalControl()
	{
		return _hasBeenUnderLocalControl;
	}

	private void OnIsMovingChanged()
	{
		UpperAnimator.SetBool("IsMoving", base.state.IsMoving);
		LowerAnimator.SetBool("IsMoving", base.state.IsMoving);
		if (HasReindeer())
		{
			ReindeerAnimator.SetAnimatorBool("IsMoving", base.state.IsMoving);
		}
	}

	private void OnIsSprintingChanged()
	{
		if (HasReindeer())
		{
			ReindeerAnimator.SetAnimatorBool("IsSprinting", base.state.IsSprinting);
		}
	}

	private void OnIsAimingChanged()
	{
		   Debug.Log("OnIsAimingChanged fired | HasBeenUnderLocalControl: " + HasBeenUnderLocalControl() + " | state.IsAiming: " + base.state.IsAiming);
		if (!HasBeenUnderLocalControl())
		{
			setIsAiming(base.state.IsAiming);
		}
	}

	public bool IsAimingCrossBow()
	{
		return _isInAimState;
	}

 
	private void setIsAiming(bool value)
	{
		bool isInAimState = _isInAimState;
		_isInAimState = value;

		 Debug.Log("setIsAiming called | value: " + value + " | HasLightningGun: " + HasLightningGun());

		if (HasLightningGun())
		{
			UpperAnimator.SetBool("IsFiringLightning", _isInAimState);
			        Debug.Log("Set IsFiringLightning to " + _isInAimState);
		}
		else
		{
			   UpperAnimator.SetBool("IsAiming", _isInAimState);
			UpperAnimator.SetBool("IsAiming", _isInAimState);
		}

		if (!HasBeenUnderLocalControl())
		{
			return;
		}
		if (HasLightningGun())
		{
			return; // no camera zoom for Lightning Gun
		}
		if (!isInAimState && _isInAimState)
		{
			PlayerCamera.GetComponent<ITweenMover>().RotateTo(Vector3.zero, AimInHash);
			PlayerCamera.GetComponent<ITweenMover>().MoveTo(AimCameraPosition.localPosition, AimInHash, delegate
			{
				Head.SetActive(false);
			});
		}
		else if (isInAimState && !_isInAimState)
		{
			Head.SetActive(true);
			PlayerCamera.GetComponent<ITweenMover>().MoveTo(_cameraMeleePosition, AimOutHash);
			PlayerCamera.GetComponent<ITweenMover>().RotateTo(_cameraMeleeLocalEulerAngles, AimOutHash);
		}
	}

	private void OnEquippedWeaponChanged()
	{
		 Debug.Log("OnEquippedWeaponChanged fired, weapon = " + (WeaponType)base.state.EquippedWeapon);
		WeaponType equippedWeapon = (WeaponType)base.state.EquippedWeapon;
		Debug.Log("Equipped weapon is now: " + equippedWeapon + " | Array length: " + WeaponModels.Length);
		for (int i = 0; i < WeaponModels.Length; i++)
		{
			bool shouldBeActive = WeaponModels[i].WeaponType == equippedWeapon;
			Debug.Log("  Model[" + i + "]: " + WeaponModels[i].name + " | Type: " + WeaponModels[i].WeaponType + " | Match: " + shouldBeActive);
			WeaponModels[i].gameObject.SetActive(shouldBeActive);
		}
		UpperAnimator.SetBool("HasSword", equippedWeapon == WeaponType.Sword);
		UpperAnimator.SetBool("HasCrossbow", equippedWeapon == WeaponType.Crossbow);
		UpperAnimator.SetBool("HasReindeer", equippedWeapon == WeaponType.Reindeer);
		UpperAnimator.SetBool("HasPistol", equippedWeapon == WeaponType.Pistol);
		UpperAnimator.SetBool("HasLightningGun", equippedWeapon == WeaponType.LightningGun);
		UpperAnimator.SetBool("IsUnarmed", equippedWeapon == WeaponType.None);
		LowerAnimator.SetBool("HasReindeer", equippedWeapon == WeaponType.Reindeer);
		if (HasBeenUnderLocalControl())
		{
			if (HasReindeer())
			{
				PlayerCamera.GetComponent<ITweenMover>().MoveTo(ReindeerCameraPosition.localPosition, ReindeerCameraInHash);
			}
			else
			{
				PlayerCamera.GetComponent<ITweenMover>().MoveTo(_cameraMeleePosition, ReindeerCameraOutHash);
			}
		}
	}

	public override void SimulateOwner()
	{
		if (IsAlive() && base.transform.position.y < FallHeightToKillPlayer)
		{
			base.state.HitPoints = 0;
			destroyThisAndCreateRagdoll(Vector3.up);
		}
	}

	public override void SimulateController()
	{
		if (IsAlive())
		{
			PollKeys(false);
			IPlayerMoveCommandInput playerMoveCommandInput = PlayerMoveCommand.Create();
			playerMoveCommandInput.Forward = _forward;
			playerMoveCommandInput.Back = _backward;
			playerMoveCommandInput.Left = _left;
			playerMoveCommandInput.Right = _right;
			playerMoveCommandInput.Jump = _jump;
			playerMoveCommandInput.PickupButtonDown = _pickupButtonDown;
			playerMoveCommandInput.PutDownButtonDown = _putDownButtonDown;
			playerMoveCommandInput.MouseDeltaX = _mouseDeltaX;
			playerMoveCommandInput.MouseDeltaY = _mouseDeltaY;
			playerMoveCommandInput.Attack1Held = _attack1Held;
			playerMoveCommandInput.Attack2Held = _attack2Held;
			playerMoveCommandInput.SprintKeyHeld = _sprintKeyHeld;
			try
			{
				base.entity.QueueInput(playerMoveCommandInput);
			}
			catch (Exception)
			{
				MonoBehaviour.print("caught exception");
			}
			_mouseDeltaX = 0f;
			_mouseDeltaY = 0f;
			_jump = false;
			_pickupButtonDown = false;
			_putDownButtonDown = false;
		}
	}

	public override void ExecuteCommand(Command command, bool resetState)
	{
		PlayerMoveCommand playerMoveCommand = (PlayerMoveCommand)command;
		if (resetState)
		{
			_velocity = playerMoveCommand.Result.Velocity;
			base.transform.localPosition = playerMoveCommand.Result.Position;
			base.transform.localEulerAngles = new Vector3(0f, playerMoveCommand.Result.RotationY, 0f);
			if (HasReindeer())
			{
				TiltRoot.localEulerAngles = Vector3.zero;
			}
			else
			{
				TiltRoot.localEulerAngles = new Vector3(playerMoveCommand.Result.RotationX, 0f, 0f);
			}
			CameraRoot.localEulerAngles = new Vector3(playerMoveCommand.Result.RotationX, CameraRoot.localEulerAngles.y, 0f);
			_stamina = playerMoveCommand.Result.Stamina;
			return;
		}
		Vector3 zero = Vector3.zero;
		bool flag = GameModeManager.Instance.IsGameModeInfoLoaded() && GameModeManager.Instance.IsRoundActive() && !_isPlayingWinAnimation;
		if (flag)
		{
			if (HasReindeer())
			{
				if (playerMoveCommand.Input.Forward || playerMoveCommand.Input.Attack1Held || playerMoveCommand.Input.SprintKeyHeld)
				{
					zero.z = 1f;
				}
				else if (playerMoveCommand.Input.Back)
				{
					zero.z = -0.5f;
				}
			}
			else
			{
				if (playerMoveCommand.Input.Forward)
				{
					zero.z = 1f;
				}
				if (playerMoveCommand.Input.Back)
				{
					zero.z = -1f;
				}
				if (playerMoveCommand.Input.Right)
				{
					zero.x = 1f;
				}
				if (playerMoveCommand.Input.Left)
				{
					zero.x = -1f;
				}
			}
		}
		zero.Normalize();
		bool flag2 = zero.magnitude > 0.01f;
		bool flag3 = playerMoveCommand.Input.SprintKeyHeld || (HasReindeer() && playerMoveCommand.Input.Attack1Held);
		float num = ((!HasReindeer()) ? StaminaToStartSprinting : ReindeerStaminaToStartSprinting);
		bool flag4 = _stamina > 0f && (base.state.IsSprinting || _stamina > num);
		bool flag5 = flag3 && flag4;
		if (base.entity.IsOwner())
		{
			base.state.IsMoving = flag2;
			if (flag5 && HasReindeer() && !base.state.IsSprinting)
			{
				base.state.ExecutingAttackID++;
			}
			base.state.IsSprinting = flag5;
		}
		executeRotationCommand(playerMoveCommand);
		Vector3 moveDirection = base.transform.forward * zero.z + base.transform.right * zero.x;
		moveDirection = HandleEnvironmentCollision(moveDirection);
		float num2 = 1f;
		if (flag2 && flag5)
		{
			num2 = ((!HasReindeer()) ? SprintMoveMultiplier : ReindeerSprintMultiplier);
			_stamina -= ((!HasReindeer()) ? SprintingStaminaDrainedPerSecond : ReindeerStaminaDrain) * BoltNetwork.frameDeltaTime;
		}
		_stamina += StaminaRegenPerSecond * BoltNetwork.frameDeltaTime;
		_stamina = Mathf.Clamp(_stamina, 0f, StartStamina);
		float moveMultiplier = GetMoveMultiplier();
		Vector3 vector = BoltNetwork.frameDeltaTime * MoveSpeed * moveMultiplier * num2 * moveDirection;
		bool flag6 = false;
		float num3 = 0f;
		float frameDeltaTime = BoltNetwork.frameDeltaTime;
		RaycastHit hitInfo;
		if (Physics.Raycast(new Ray(base.transform.position + new Vector3(0f, 10f, 0f), -Vector3.up), out hitInfo, 20f, GetEnvironmentLayerMask()))
		{
			num3 = base.transform.position.y - hitInfo.point.y;
			if (HasReindeer())
			{
				num3 -= ReindeerGroundOffset;
			}
			float num4 = 0.15f;
			if (num3 > num4)
			{
				_velocity.y += frameDeltaTime * Singleton<PhysicsManager>.Instance.Gravity;
			}
			else
			{
				vector.y -= num3;
				_velocity.y = Mathf.Max(0f, _velocity.y);
			}
			flag6 = num3 < num4;
		}
		else
		{
			_velocity.y += frameDeltaTime * Singleton<PhysicsManager>.Instance.Gravity;
		}
		if (flag6)
		{
			_velocity = Vector3.zero;
		}
		if (flag6 && playerMoveCommand.Input.Jump && !HasReindeer())
		{
			_velocity = Vector3.up * JumpVelocity;
			flag6 = false;
		}
		GroundType groundType;
		if (flag6)
		{
			groundType = GroundType.Snow;
			if (hitInfo.collider != null)
			{
				Ground component = hitInfo.collider.GetComponent<Ground>();
				if (component != null)
				{
					groundType = component.GroundType;
					bool flag7 = !_wasOnGround && Time.time - _lastGroundLandTime > 1f;
					if (HasBeenUnderLocalControl() && flag7 && !HasReindeer())
					{
						PlayStepForGroundType(groundType);
						_lastGroundLandTime = Time.time;
						_wasOnGround = true;
					}
				}
			}
		}
		else
		{
			groundType = GroundType.None;
			_wasOnGround = false;
		}
		if (HasBeenUnderLocalControl())
		{
			_groundType = groundType;
		}
		if (base.entity.IsOwner())
		{
			base.state.IsOnGroundType = (int)groundType;
		}
		HandleJumpPadCollision(moveDirection, playerMoveCommand.IsFirstExecution);
		vector += _velocity * frameDeltaTime;
		if (_velocity.y < 0f && num3 > 0f)
		{
			vector.y = Mathf.Max(vector.y, 0f - num3);
		}
		Vector3 potentialNewPosition = base.transform.localPosition + vector;
		Vector3 overlappingCharacters = CharacterTracker.Instance.GetOverlappingCharacters(this, potentialNewPosition);
		base.transform.localPosition = overlappingCharacters;
		if (flag)
		{
			if (!HasReindeer())
			{
				ExecuteAttackCommands(playerMoveCommand);
			}
			if (BoltNetwork.isServer)
			{
				if (playerMoveCommand.Input.PickupButtonDown)
				{
					tryPickUpNearbyItem();
				}
				if (playerMoveCommand.Input.PutDownButtonDown)
				{
					tryPutDownWeapon();
				}
			}
		}
		playerMoveCommand.Result.Position = base.transform.localPosition;
		playerMoveCommand.Result.RotationX = CameraRoot.localEulerAngles.x;
		playerMoveCommand.Result.RotationY = base.transform.localEulerAngles.y;
		playerMoveCommand.Result.Velocity = _velocity;
		playerMoveCommand.Result.Stamina = _stamina;
	}

	private float GetMoveMultiplier()
	{
		if (HasSword())
		{
			return SwordMovementMultiplier;
		}
		if (HasReindeer())
		{
			return ReindeerMovementMultiplier;
		}
		if (HasCrossbow())
		{
			return CrossbowMovementMultiplier;
		}
		if(HasPistol())
		{
			return PistolStats.MovementMultiplier;
		}
	    if (HasLightningGun())
		{
			return LightningStats.MovementMultiplier;
		}

		return 1f;
	}

	private void HandleJumpPadCollision(Vector3 moveDirection, bool isFirst)
	{
		Vector3 direction = moveDirection + Vector3.up * _velocity.y;
		Vector3 position = FootRaycastOrigin.position;
		if (HasReindeer())
		{
			position.y -= ReindeerGroundOffset;
		}
		RaycastHit hitInfo;
		if (!Physics.Raycast(new Ray(position, direction), out hitInfo, FootRaycastDistance + _velocity.magnitude * BoltNetwork.frameDeltaTime, GetHazardLayerMask()) || !(hitInfo.collider != null))
		{
			return;
		}
		JumpPad component = hitInfo.collider.GetComponent<JumpPad>();
		if (component != null)
		{
			_velocity = component.GetLaunchVelocity();
			if (isFirst)
			{
				component.PlayAndDispatchJumpEffect(HasBeenUnderLocalControl());
			}
		}
	}

	private void executeRotationCommand(PlayerMoveCommand cmd)
	{
		float num = 0f;
		float y = base.transform.localEulerAngles.y;
		float num2 = restrictRotationToPlusMinus180(CameraRoot.localEulerAngles.y);
		num = restrictRotationToPlusMinus180(CameraRoot.localEulerAngles.x);
		float num3 = ((!HasReindeer()) ? 1f : ReindeerTiltSpeedMultiplier);
		num += cmd.Input.MouseDeltaY * MouseRotationSpeedX * num3;
		num = Mathf.Clamp(num, MinTiltX, MaxTiltX);
		if (HasReindeer() && !_isPlayingWinAnimation)
		{
			float num4 = ((!base.state.IsSprinting) ? ReindeerRotationYPerSecond : ReindeerRotationYPerSecondWhileSprinting);
			float num5 = num4 * BoltNetwork.frameDeltaTime;
			float num6 = 0f;
			if (cmd.Input.Left || cmd.Input.Right)
			{
				float num7 = 0f;
				if (cmd.Input.Right)
				{
					num7 = 1f;
				}
				if (cmd.Input.Left)
				{
					num7 = -1f;
				}
				if (_timeStartedTurningReideer < 0f)
				{
					_timeStartedTurningReideer = Time.time;
				}
				num6 = num7 * num5;
			}
			else
			{
				_timeStartedTurningReideer = -1f;
			}
			y += num6;
			num2 = 0f;
		}
		else
		{
			y += cmd.Input.MouseDeltaX * MouseRotationSpeedY;
			if (!_isPlayingWinAnimation)
			{
				num2 = 0f;
			}
		}
		y %= 360f;
		if (_isPlayingWinAnimation)
		{
			num2 += Mathf.DeltaAngle(num2, 180f) * BoltNetwork.frameDeltaTime * WinCameraRotationMultiplier;
		}
		if (HasReindeer())
		{
			base.transform.localEulerAngles = new Vector3(0f, y, 0f);
			TiltRoot.localEulerAngles = Vector3.zero;
			CameraRoot.localEulerAngles = new Vector3(num, num2, 0f);
		}
		else
		{
			base.transform.localEulerAngles = new Vector3(0f, y, 0f);
			TiltRoot.localEulerAngles = new Vector3(num, 0f, 0f);
			CameraRoot.localEulerAngles = new Vector3(num, num2, 0f);
		}
	}

	private float restrictRotationToPlusMinus180(float rotation)
	{
		if (rotation > 180f)
		{
			rotation -= 360f;
		}
		if (rotation < -180f)
		{
			rotation += 360f;
		}
		return rotation;
	}

	private static int GetEnvironmentLayerMask()
	{
		return 1 << LayerMask.NameToLayer("Environment");
	}

	private static int GetHazardLayerMask()
	{
		return 1 << LayerMask.NameToLayer("Hazards");
	}

	private Vector3 HandleEnvironmentCollision(Vector3 moveDirection)
	{
		Vector3 raycastHitOrigin;
		RaycastHit? raycastHit = TryGetEnvironmentRaycastsHit(moveDirection, GetEnvironmentLayerMask(), out raycastHitOrigin);
		if (raycastHit.HasValue)
		{
			Vector3 vector = raycastHit.Value.normal * Vector3.Dot(moveDirection, raycastHit.Value.normal);
			Vector3 vector2 = moveDirection - vector;
			RaycastHit hitInfo;
			moveDirection = ((!(vector2.magnitude > WallSlipMinimum)) ? Vector3.zero : ((!Physics.Raycast(new Ray(WallRaycastPointsOrigin.position, vector2.normalized), out hitInfo, WallRaycastDistance, GetEnvironmentLayerMask())) ? vector2 : Vector3.zero));
		}
		return moveDirection;
	}

	private RaycastHit? TryGetEnvironmentRaycastsHit(Vector3 moveDirection, int environmentLayerMask, out Vector3 raycastHitOrigin)
	{
		WallRaycastPointsOrigin.eulerAngles = moveDirection;
		Transform[] wallRaycastPoints = WallRaycastPoints;
		foreach (Transform transform in wallRaycastPoints)
		{
			RaycastHit hitInfo;
			if (Physics.Raycast(new Ray(transform.position, moveDirection.normalized), out hitInfo, WallRaycastDistance, environmentLayerMask))
			{
				raycastHitOrigin = transform.position;
				return hitInfo;
			}
		}
		raycastHitOrigin = Vector3.zero;
		return null;
	}

	private void ExecuteAttackCommands(PlayerMoveCommand cmd)
	{
		_isFiringLightning = HasLightningGun() && cmd.Input.Attack1Held;

		
		if (HasPistol() && cmd.Input.Attack1Held)
		{
			_lastPistolFireInputTime = Time.time;
		}
		bool isFiring = HasPistol() && (Time.time - _lastPistolFireInputTime < 0.15f);
		UpperAnimator.SetBool("IsFiringPistol", isFiring);
		Debug.Log("Frame: " + Time.frameCount + " | Attack1Held: " + cmd.Input.Attack1Held + " | IsFiringPistol: " + isFiring);

		bool flag = (cmd.Input.Attack2Held && HasCrossbow()) || (cmd.Input.Attack1Held && HasLightningGun());
		if (flag && !base.state.IsAiming)
		{
			if (base.entity.IsOwner())
			{
				base.state.IsAiming = true;
			}
		}
		else if (!flag && base.state.IsAiming && base.entity.IsOwner())
		{
			base.state.IsAiming = false;
		}
		if (HasBeenUnderLocalControl() && cmd.IsFirstExecution)
		{
			setIsAiming(flag);
		}
		if (!cmd.Input.Attack1Held || !(BoltNetwork.serverTime - base.state.AttackStartTime > getTimeBetweenCurrentWeaponAttacks()))
		{
			return;
		}
		int attackID = base.state.ExecutingAttackID + 1;
		CharacterDirection attackDirection = CharacterDirection.Forward;
		if (cmd.Input.Left)
		{
			attackDirection = CharacterDirection.Left;
		}
		else if (cmd.Input.Right)
		{
			attackDirection = CharacterDirection.Right;
		}
		if (base.entity.IsOwner())
		{
			base.state.AttackStartTime = BoltNetwork.serverTime;
			base.state.AttackDirection = (int)attackDirection;
			base.state.ExecutingAttackID++;
			if (HasCrossbow())
			{
				base.state.CurrentWeaponAmmo--;
				spawnCrossbowBolt(cmd.ServerFrame);
				if (base.state.CurrentWeaponAmmo == 0)
				{
					base.state.EquippedWeapon = 0;
				}
			}
		
			if (HasPistol())
			{
				base.state.CurrentWeaponAmmo--;
				spawnPistolBullet(cmd.ServerFrame);
				if (base.state.CurrentWeaponAmmo == 0)
				{
					base.state.EquippedWeapon = 0;
				}
			}
		}
		tryRenderAttackID(attackID, attackDirection);
	}

	private void spawnCrossbowBolt(int commandServerFrame)
	{
		Vector3 spawnPosition = AimCameraPosition.position + AimCameraPosition.forward * CrossBowSpawnDistance;
		BoltEntity boltEntity = BoltNetwork.Instantiate(BoltPrefabs.CrossbowBolt);
		boltEntity.GetComponent<CrossbowProjectile>().Intialize(this, base.state.ExecutingAttackID, spawnPosition, AimCameraPosition.eulerAngles, commandServerFrame);
	}

	private static PrefabId MakePrefabId(int id)
	{
		ConstructorInfo ctor = typeof(PrefabId).GetConstructor(
			BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
			null,
			new System.Type[] { typeof(int) },
			null
		);
		return (PrefabId)ctor.Invoke(new object[] { id });
	}

	private void spawnPistolBullet(int commandServerFrame)
	{
		Vector3 spawnPosition = PistolStats.MuzzlePoint.position + PistolStats.MuzzlePoint.forward * PistolStats.SpawnDistance;

		if (PistolStats.MuzzleFlash != null)
		{
			PistolStats.MuzzleFlash.Play();
		}

		BoltEntity boltEntity = BoltNetwork.Instantiate(MakePrefabId(12));
		boltEntity.GetComponent<CrossbowProjectile>().Intialize(this, base.state.ExecutingAttackID, spawnPosition, AimCameraPosition.eulerAngles, commandServerFrame);
	}

	private void UpdateLightningBeam(bool isFiring)
	{
		if (!isFiring)
		{
			if (LightningBeamInstance != null)
			{
				LightningBeamInstance.SetActive(false);
			}
			_wasFiringLightningLastFrame = false;
			_lightningAmmoAccumulator = 0f;
			_lightningDamageAccumulator = 0f;
			return;
		}

		if (LightningBeamInstance == null || LightningStats.MuzzlePoint == null)
		{
			return;
		}

		if (base.entity.IsOwner())
		{
			if (base.state.CurrentWeaponAmmo <= 0)
			{
				base.state.EquippedWeapon = 0;
				_wasFiringLightningLastFrame = false;
				return;
			}
		}

		if (!_wasFiringLightningLastFrame)
		{
			LightningBeamInstance.SetActive(true);
			ParticleSystem ps = LightningBeamInstance.GetComponent<ParticleSystem>();
			if (ps != null)
			{
				ps.Play();
			}
		}
		_wasFiringLightningLastFrame = true;

		Vector3 origin = LightningStats.MuzzlePoint.position;
		Vector3 direction = AimCameraPosition.forward;

		RaycastHit hit;
		if (Physics.Raycast(origin, direction, out hit, LightningStats.Range))
		{
			if (base.entity.IsOwner())
			{
				SantaCharacterController target = hit.collider.GetComponentInParent<SantaCharacterController>();
				if (target != null)
				{
					_lightningDamageAccumulator += LightningStats.DamagePerSecond * Time.deltaTime;
					if (_lightningDamageAccumulator >= 1f)
					{
						int wholeDamage = Mathf.FloorToInt(_lightningDamageAccumulator);
						target.TryTakeDamageFromAttack(this, wholeDamage, direction, base.state.ExecutingAttackID, WeaponType.LightningGun, hit.point);
						_lightningDamageAccumulator -= wholeDamage;
					}
				}
			}
		}

		if (base.entity.IsOwner())
		{
			_lightningAmmoAccumulator += LightningStats.AmmoDrainPerSecond * Time.deltaTime;
			if (_lightningAmmoAccumulator >= 1f)
			{
				int wholeAmmoToRemove = Mathf.FloorToInt(_lightningAmmoAccumulator);
				base.state.CurrentWeaponAmmo = Mathf.Max(0, base.state.CurrentWeaponAmmo - wholeAmmoToRemove);
				_lightningAmmoAccumulator -= wholeAmmoToRemove;
			}
		}

		// just follow the muzzle, no stretching/scaling
		LightningBeamInstance.transform.position = origin;
		LightningBeamInstance.transform.rotation = Quaternion.LookRotation(direction);
	}
	private float getTimeBetweenCurrentWeaponAttacks()
	{
		if (HasSword())
		{
			return TimeBetweenSwordAttacks;
		}
		if (HasCrossbow())
		{
			return TimeBetweenCrossbowAttacks;
		}
		if(HasPistol())
		{
			 return PistolStats.TimeBetweenAttacks;
		}
		if (HasLightningGun())
		{
			return LightningStats.TimeBetweenAttacks;
		}
		return TimeBetweenFistAttacks;
	}

	private void OnExecutingAttackIDChanged()
	{
		tryRenderAttackID(base.state.ExecutingAttackID, (CharacterDirection)base.state.AttackDirection);
	}

	private void tryRenderAttackID(int attackID, CharacterDirection attackDirection)
	{
		if (_lastAttackIDRendered >= attackID)
		{
			return;
		}
		_lastAttackIDRendered = attackID;
		if (HasCrossbow())
		{
			UpperAnimator.SetTrigger("FireCrossBow");
		}
		else if (HasPistol())
		{
			Singleton<AudioManager>.Instance.PlayClipAtTransform(Singleton<AudioLibrary>.Instance.PistolFire, UpperAnimator.transform);
			Debug.Log("Setting FirePistol trigger");
			UpperAnimator.SetTrigger("FirePistol");
		}
		else if (!HasAnyEquippedWeapon())
		{
			int num = base.state.ExecutingAttackID % 2 + 1;
			Singleton<AudioManager>.Instance.PlayClipAtTransform(Singleton<AudioLibrary>.Instance.BeginPunch, UpperAnimator.transform);
			UpperAnimator.Play("FistStrike" + num, 0, 0f);
		}
		else if (HasSword())
		{
			Singleton<AudioManager>.Instance.PlayClipAtTransform(Singleton<AudioLibrary>.Instance.SwordSwing, UpperAnimator.transform);
			switch (attackDirection)
			{
			case CharacterDirection.Forward:
				UpperAnimator.Play("StrikeForward_Sword", 0, 0f);
				break;
			case CharacterDirection.Left:
				UpperAnimator.Play("StrikeLeft_Sword", 0, 0f);
				break;
			case CharacterDirection.Right:
				UpperAnimator.Play("StrikeRight_Sword", 0, 0f);
				break;
			}
		}
	}

	private void tryPickUpNearbyItem()
	{
		if (!(BoltNetwork.serverTime < _timeOfLastItemPickup + 0.5f))
		{
			PickupItemController pickupInRangeOf = WeaponDropManager.Instance.GetPickupInRangeOf(base.transform.position);
			if (pickupInRangeOf != null)
			{
				dropCurrentWeapon();
				base.state.EquippedWeapon = (int)pickupInRangeOf.WeaponType;
				base.state.CurrentWeaponAmmo = pickupInRangeOf.state.Ammo;
				BoltNetwork.Destroy(pickupInRangeOf.gameObject);
				_timeOfLastItemPickup = BoltNetwork.serverTime;
			}
		}
	}

	private void tryPutDownWeapon()
	{
		if (!(BoltNetwork.serverTime < _timeOfLastItemPickup + 0.5f) && HasAnyEquippedWeapon())
		{
			dropCurrentWeapon();
			_timeOfLastItemPickup = BoltNetwork.serverTime;
		}
	}

	private void dropCurrentWeapon()
	{
		if (HasAnyEquippedWeapon())
		{
			WeaponDropManager.Instance.SpawnPickup(getGroundPosition(), (WeaponType)base.state.EquippedWeapon, base.state.CurrentWeaponAmmo);
			base.state.EquippedWeapon = 0;
		}
	}

	public bool IsOwnedBy(BoltConnection connection)
	{
		return base.entity.IsOwner();
	}

	public float GetStamina()
	{
		return _stamina;
	}

	public bool HasAnyEquippedWeapon()
	{
		return base.state.EquippedWeapon != 0;
	}

	public bool HasSword()
	{
		return base.state.EquippedWeapon == 1;
	}

	public bool HasCrossbow()
	{
		return base.state.EquippedWeapon == 2;
	}

	public bool HasPistol()
	{
		return base.state.EquippedWeapon == (int)WeaponType.Pistol;
	}
	public bool HasLightningGun()
	{
		return base.state.EquippedWeapon == (int)WeaponType.LightningGun;
	}

	public bool HasReindeer()
	{
		return base.state.EquippedWeapon == 4;
	}
	

	public int GetEquippedWeaponTypeAsInt()
	{
		return base.state.EquippedWeapon;
	}

	private void Update()
	{
		PollKeys(true);

		if (HasBeenUnderLocalControl())
		{
			float targetFOV = base.state.IsSprinting ? _baseFOV + SprintFOVBoost : _baseFOV;
			PlayerCamera.fieldOfView = Mathf.Lerp(PlayerCamera.fieldOfView, targetFOV, Time.deltaTime * FOVLerpSpeed);

			UpdateLightningBeam(_isFiringLightning);
		}
	}

	private void PollKeys(bool mouse)
	{
		if (Singleton<FocusManager>.Instance != null && !Singleton<FocusManager>.Instance.HasFocus())
		{
			_forward = false;
			_backward = false;
			_left = false;
			_right = false;
			_jump = false;
			_pickupButtonDown = false;
			_putDownButtonDown = false;
			_attack1Held = false;
			_attack2Held = false;
			_sprintKeyHeld = false;
			return;
		}
		_forward = Input.GetKey(KeyCode.W);
		_backward = Input.GetKey(KeyCode.S);
		_left = Input.GetKey(KeyCode.A);
		_right = Input.GetKey(KeyCode.D);
		_jump = Input.GetKeyDown(KeyCode.Space) || _jump;
		_pickupButtonDown = Input.GetKeyDown(KeyCode.F) || _pickupButtonDown;
		_putDownButtonDown = Input.GetKeyDown(KeyCode.R) || _putDownButtonDown;
		_attack1Held = Input.GetMouseButtonDown(0) || Input.GetMouseButton(0);
		_attack2Held = Input.GetMouseButtonDown(1) || Input.GetMouseButton(1);
		_sprintKeyHeld = Input.GetKey(KeyCode.LeftShift);
		if (mouse)
		{
			_mouseDeltaX += Input.GetAxisRaw("Mouse X");
			_mouseDeltaY += 0f - Input.GetAxisRaw("Mouse Y");
		}
	}

	public void OnLandedMeleeStrike(SantaCharacterController punchedCharacter, bool hitReindeer)
	{
		if (!_isDetached)
		{
			Vector3 zero = Vector3.zero;
			Vector3 normalized = (punchedCharacter.transform.position - base.transform.position).normalized;
			if (hitReindeer)
			{
				punchedCharacter.TryTakeReindeerDamageFromAttack(this, getMeleeDamageOfCurrentWeapon(), normalized, base.state.ExecutingAttackID, (WeaponType)base.state.EquippedWeapon, zero);
			}
			else
			{
				punchedCharacter.TryTakeDamageFromAttack(this, getMeleeDamageOfCurrentWeapon(), normalized, base.state.ExecutingAttackID, (WeaponType)base.state.EquippedWeapon, zero);
			}
		}
	}

	private int getMeleeDamageOfCurrentWeapon()
	{
		if (HasSword())
		{
			return SwordDamage;
		}
		if (HasReindeer())
		{
			return ReindeerHornDamage;
		}
		if (!HasAnyEquippedWeapon())
		{
			return PunchDamage;
		}
		return 0;
	}

	public void TryTakeDamageFromAttack(SantaCharacterController attackingCharacter, int damage, Vector3 damageDirection, int attackID, WeaponType weaponUsed, Vector3 worldImpactPosition)
	{
		if (BoltNetwork.isServer && !_isDetached && !attackingCharacter._isDetached && (_lastDamagedByAttackID != attackID || !(_lastDamagedByEntity == attackingCharacter.entity)) && base.state.HitPoints != 0)
		{
			_lastDamagedByAttackID = attackID;
			_lastDamagedByEntity = attackingCharacter.entity;
			int value = base.state.HitPoints - damage;
			value = Mathf.Clamp(value, 0, StartHitpoints);
			base.state.HitPoints = value;
			createDamageVFXAndSound(damageDirection, weaponUsed, true);
			if (value == 0)
			{
				dropCurrentWeapon();
				destroyThisAndCreateRagdoll(damageDirection);
			}
			else if (weaponUsed == WeaponType.Crossbow)
			{
				createCrossBowStickEvent(damageDirection, false, worldImpactPosition);
			}
		}
	}

	private void destroyThisAndCreateRagdoll(Vector3 damageDirection)
	{
		BoltEntity boltEntity = BoltNetwork.Instantiate(BoltPrefabs.SantaCharacterRagdoll, base.transform.position, base.transform.rotation);
		boltEntity.GetComponent<SantaCharacterRagdoll>().MatchSanta(SantaCharacterRagdollSource);
		Vector3 velocity = damageDirection * DeathFallVelocity;
		velocity.y = DeathFallVelocityY;
		boltEntity.GetComponent<Rigidbody>().velocity = velocity;
		Singleton<GlobalEventManager>.Instance.Dispatch("PlayerKilledServer", this);
		BoltNetwork.Destroy(base.gameObject);
	}

	private void createCrossBowStickEvent(Vector3 damageDirection, bool wasReindeerHit, Vector3 impactPositionWorld)
	{
		CrossbowAttachPoint crossbowAttachPoint = null;
		if (wasReindeerHit)
		{
			crossbowAttachPoint = CrossbowAttachPointReindeer;
		}
		else
		{
			float sqrMagnitude = (impactPositionWorld - CrossbowAttachPointHead.transform.position).sqrMagnitude;
			float sqrMagnitude2 = (impactPositionWorld - CrossbowAttachPointBody.transform.position).sqrMagnitude;
			crossbowAttachPoint = ((!(sqrMagnitude2 < sqrMagnitude)) ? CrossbowAttachPointHead : CrossbowAttachPointBody);
		}
		CrossbowStickEvent crossbowStickEvent = CrossbowStickEvent.Create(base.entity);
		crossbowStickEvent.ImpactDirection = damageDirection;
		crossbowStickEvent.WasReindeerHit = wasReindeerHit;
		crossbowStickEvent.WasHeadHit = crossbowAttachPoint == CrossbowAttachPointHead;
		crossbowStickEvent.LocalPosition = crossbowAttachPoint.GetClosestLocalPosition(impactPositionWorld);
		crossbowStickEvent.Send();
	}

	public override void OnEvent(CrossbowStickEvent crossbowEvent)
	{
		CrossbowAttachPoint crossbowAttachPoint = null;
		Transform transform = UnityEngine.Object.Instantiate(parent: (crossbowEvent.WasReindeerHit ? CrossbowAttachPointReindeer : ((!crossbowEvent.WasHeadHit) ? CrossbowAttachPointBody : CrossbowAttachPointHead)).transform, original: CrossbowAttachedPrefab);
		transform.localScale = Vector3.one;
		transform.forward = crossbowEvent.ImpactDirection;
		transform.localPosition = crossbowEvent.LocalPosition;
		Singleton<AudioManager>.Instance.PlayClipAtTransform(Singleton<AudioLibrary>.Instance.CrossbowImpactBody, transform);
	}

	private void createDamageVFXAndSound(Vector3 damageDirection, WeaponType weaponUsed, bool showHitReaction)
	{
		Vector3 vector = new Vector3(damageDirection.x, 0f, damageDirection.z);
		float target = Mathf.Atan2(vector.z, vector.x) * 57.29578f;
		float current = Mathf.Atan2(base.transform.forward.z, base.transform.forward.x) * 57.29578f;
		float num = Mathf.DeltaAngle(current, target);
		if (showHitReaction)
		{
			HitReactionEvent hitReactionEvent = HitReactionEvent.Create(base.entity);
			hitReactionEvent.ImpactAngle = num;
			hitReactionEvent.Send();
		}
		float impactRotationY = base.transform.localEulerAngles.y - num;
		HitSoundAndVFXEvent hitSoundAndVFXEvent = HitSoundAndVFXEvent.Create();
		hitSoundAndVFXEvent.Position = BloodSpawnPoint.position;
		hitSoundAndVFXEvent.ImpactRotationY = impactRotationY;
		hitSoundAndVFXEvent.WeaponUsed = (int)weaponUsed;
		hitSoundAndVFXEvent.Send();
	}

	public void TryTakeReindeerDamageFromAttack(SantaCharacterController attackingCharacter, int damage, Vector3 damageDirection, int attackID, WeaponType weaponUsed, Vector3 worldImpactPosition)
	{
		if (_isDetached || !HasReindeer() || (_reindeerLastDamagedByAttackID == attackID && _lastDamagedByEntity == attackingCharacter.entity))
		{
			return;
		}
		_reindeerLastDamagedByAttackID = attackID;
		_lastDamagedByEntity = attackingCharacter.entity;
		createDamageVFXAndSound(damageDirection, weaponUsed, false);
		int num = base.state.CurrentWeaponAmmo - damage;
		if (num > 0)
		{
			if (weaponUsed == WeaponType.Crossbow)
			{
				createCrossBowStickEvent(damageDirection, true, worldImpactPosition);
			}
			base.state.CurrentWeaponAmmo = num;
		}
		else
		{
			BoltEntity boltEntity = BoltNetwork.Instantiate(BoltPrefabs.ReindeerRagDoll, base.transform.position, base.transform.rotation);
			Vector3 velocity = damageDirection * DeathFallVelocity;
			velocity.y = DeathFallVelocityY;
			boltEntity.GetComponent<Rigidbody>().velocity = velocity;
			base.state.EquippedWeapon = 0;
		}
	}

	private Vector3 getGroundPosition()
	{
		int layerMask = 1 << LayerMask.NameToLayer("Environment");
		RaycastHit hitInfo;
		if (Physics.Raycast(new Ray(base.transform.position + new Vector3(0f, 5f, 0f), -Vector3.up), out hitInfo, 20f, layerMask))
		{
			return hitInfo.point;
		}
		return base.transform.position;
	}

	public override void OnEvent(ClientSetUsernameEvent clientSetUsernameEvent)
	{
		if (base.entity.IsOwner())
		{
			base.state.SteamUsername = clientSetUsernameEvent.SteamUsername;
		}
	}

	public override void OnEvent(HitReactionEvent hitReactionEvent)
	{
		if (IsAlive())
		{
			string stateName = null;
			if (Mathf.Abs(Mathf.DeltaAngle(0f, hitReactionEvent.ImpactAngle)) < 45f)
			{
				stateName = "HitReactionForward";
			}
			else if (Mathf.Abs(Mathf.DeltaAngle(180f, hitReactionEvent.ImpactAngle)) < 45f)
			{
				stateName = "HitReactionBack";
			}
			else if (Mathf.Abs(Mathf.DeltaAngle(90f, hitReactionEvent.ImpactAngle)) < 45f)
			{
				stateName = "HitReactionLeft";
			}
			else if (Mathf.Abs(Mathf.DeltaAngle(-90f, hitReactionEvent.ImpactAngle)) < 45f)
			{
				stateName = "HitReactionRight";
			}
			UpperAnimator.Play(stateName, 0, 0f);
		}
	}

	public void PlayWinAnimation()
	{
		if (base.state.EquippedWeapon != 4)
		{
			base.state.EquippedWeapon = 0;
		}
		PlayWinAnimationEvent playWinAnimationEvent = PlayWinAnimationEvent.Create(base.entity);
		playWinAnimationEvent.Send();
	}

	public override void OnEvent(PlayWinAnimationEvent evnt)
	{
		setIsAiming(false);
		_isPlayingWinAnimation = true;
		UpperAnimator.Play("Winner_Intro", 0, 0f);
		LowerAnimator.Play("Winner_Intro", 0, 0f);
	}

	private void OnIsOnGroundTypeChanged()
	{
		if (!HasBeenUnderLocalControl())
		{
			PlayLandingSoundIfGroundTypeChanged();
		}
	}

	private void PlayLandingSoundIfGroundTypeChanged()
	{
		if (base.state.IsOnGroundType != (int)_lastGroundType)
		{
			if (HasReindeer())
			{
				PlayReindeerStep();
			}
			else
			{
				PlayStepForGroundType();
			}
		}
		_lastGroundType = (GroundType)base.state.IsOnGroundType;
	}

	public void PlayStepForGroundType()
	{
		PlayStepForGroundType(GetGroundType());
	}

	public void PlayStepForGroundType(GroundType groundType)
	{
		if (groundType != GroundType.None)
		{
			Singleton<AudioManager>.Instance.PlayClipAtTransform(Singleton<AudioLibrary>.Instance.GroundTypeToSound[groundType], base.transform);
		}
	}

	public void PlayReindeerStep()
	{
		if (GetGroundType() != GroundType.None)
		{
			Singleton<AudioManager>.Instance.PlayClipAtTransform(Singleton<AudioLibrary>.Instance.ReindeerTrot, base.transform);
		}
	}

	public void PlayReindeerRunStep()
	{
		if (GetGroundType() != GroundType.None)
		{
			Singleton<AudioManager>.Instance.PlayClipAtTransform(Singleton<AudioLibrary>.Instance.ReindeerRun, base.transform);
		}
	}
}
