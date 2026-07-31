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
	public int StartHitpoints=130;
	public int HitpointsHealedPerKill = 25; // set to 0 to turn heal-on-kill off
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
	public AudioSource LightningAudioSource; // assign in Inspector, put it on/near the muzzle
	// The beam FX is authored to fly far further than the gun can hit, so its particles get their
	// lifetimes scaled down to put the tip at the end of the shot.
	public bool LightningBeamStopsAtHit = true; // off = always drawn out to the full range
	public float LightningBeamLengthScale = 1f; // nudge if the tip doesn't quite land on the impact
	public bool PlayLightningAudioForOtherPlayers = true; // their beams play positionally; yours stays 2D

	[Header("SnowballLauncher")]
	public RangedWeaponStats SnowballStats; // reuse SpawnDistance, TimeBetweenAttacks, MovementMultiplier, MuzzlePoint, MuzzleFlash
	public int SnowballDamage;
	public float SnowballExplosionRadius;
	public float SnowballThrowForce;

	[Header("Grenade")]
	public RangedWeaponStats GrenadeStats;
	public int GrenadeDamage;
	public float GrenadeExplosionRadius;
	public float GrenadeThrowForce;

	[Header("Grenade Throw")]
	// Hold fire to wind up, release to throw. The wind-up pose is a placeholder offset applied to
	// the grenade model; the animator hooks below are for when real clips exist.
	public Vector3 GrenadeWindupLocalOffset = new Vector3(0.02f, 0.05f, -0.09f);
	public Vector3 GrenadeWindupLocalEuler = new Vector3(-25f, 0f, 0f);
	public float GrenadeWindupTime = 0.15f;
	public float GrenadeWindupFOVBoost = 6f; // positive pulls the view back; negative zooms in
	public float GrenadeWindupMovementMultiplier = 0.6f; // matches the other aimed weapons
	// Optional UpperAnimator state to Play() on release, e.g. an authored throw clip. The
	// "IsPreparingGrenade" bool and "ThrowGrenade" trigger are driven too, but only if the
	// controller actually defines them.
	public string GrenadeThrowAnimatorState = "";

	[Header("Grenade Trajectory Preview")]
	public bool ShowGrenadeArc = true;
	public bool ShowGrenadeArcOnlyWhileCooking = true; // arc appears once you start winding up
	// Must match GravityMultiplier on the grenade projectile prefab's SnowballProjectile, or the
	// preview will predict a different flight than the thrown grenade actually takes.
	public float GrenadeArcGravityMultiplier = 2f;
	public float GrenadeArcProjectileRadius = 0.2f; // the grenade prefab's capsule radius
	public float GrenadeArcMaxTime = 3f;
	public float GrenadeArcPointInterval = 0.05f;
	public float GrenadeArcWidth = 0.06f;
	public Color GrenadeArcColor = new Color(1f, 0.45f, 0.15f, 0.9f);
	public Material GrenadeArcMaterial; // optional; a built-in unlit shader is used when empty
	public bool ShowGrenadeArcImpactMarker = true; // ring showing the blast radius where it lands
	public LayerMask GrenadeArcCollisionMask = ~(1 << 2); // everything except Ignore Raycast

	[Header("Boxing Gloves")]
	public int BoxingGlovesDamage;
	public float TimeBetweenBoxingGlovesAttacks;
	public float BoxingGlovesMovementMultiplier;



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
	private float _lastLightningFireTime = -999f;
	private bool _isFiringLightning;
	private bool _wasFiringLightningLastFrame;
	private float _lightningAmmoAccumulator;
	private float _lightningDamageAccumulator;
	private GrenadeArcPreview _grenadeArcPreview;
	private bool _wasCookingGrenade;
	private bool _aimStateIsGrenadeWindup;
	private float _grenadeWindupBlend;
	private Transform _grenadeModel;
	private Vector3 _grenadeModelBasePosition;
	private Quaternion _grenadeModelBaseRotation;
	private bool _grenadeModelSearched;
	private AnimatorControllerParameter[] _upperAnimatorParameters;
	private ParticleSystem[] _lightningBeamSystems;
	private float[] _lightningBeamAuthoredLifetimes;
	private float _lightningBeamAuthoredReach;
	private float _lightningBeamAppliedLength = -1f;
	private bool _lightningBeamSystemsSearched;
	private readonly RaycastHit[] _lightningBeamHits = new RaycastHit[16];

	// Rides along on the replicated AttackDirection field. By the time ExecutingAttackID reaches
	// remote clients the weapon has already been cleared to None everywhere, so the equipped
	// weapon can't tell them the attack was a grenade throw. Deliberately outside CharacterDirection.
	private const int GrenadeThrowAttackDirection = 100;

	// How long a copy of a character that isn't simulating input keeps showing the lightning as
	// firing after the last attack tick reached it. Long enough to bridge the gap between
	// snapshots, short enough that the beam stops when the shooter lets go.
	private const float LightningRemoteFiringHoldTime = 0.15f;

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

		// The grenade wind-up shares the replicated IsAiming flag but is a different pose, so it
		// drives its own animator parameter and never claims the aim-down-sights one.
		bool isGrenadeWindup = _isInAimState && HasGrenade();
		UpperAnimator.SetBool("IsAiming", _isInAimState && !isGrenadeWindup);
		setUpperAnimatorBool("IsPreparingGrenade", isGrenadeWindup);

		if (!HasBeenUnderLocalControl())
		{
			return;
		}
		if (!isInAimState && _isInAimState)
		{
			// Winding up a throw must not pull the camera into the ADS position.
			_aimStateIsGrenadeWindup = isGrenadeWindup;
			if (_aimStateIsGrenadeWindup)
			{
				return;
			}
			PlayerCamera.GetComponent<ITweenMover>().RotateTo(Vector3.zero, AimInHash);
			PlayerCamera.GetComponent<ITweenMover>().MoveTo(AimCameraPosition.localPosition, AimInHash, delegate
			{
				Head.SetActive(false);
			});
		}
		else if (isInAimState && !_isInAimState)
		{
			// Checked against how the aim state was entered, because the grenade is already gone
			// from EquippedWeapon by the time the throw ends it.
			bool wasGrenadeWindup = _aimStateIsGrenadeWindup;
			_aimStateIsGrenadeWindup = false;
			if (wasGrenadeWindup)
			{
				return;
			}
			Head.SetActive(true);
			PlayerCamera.GetComponent<ITweenMover>().MoveTo(_cameraMeleePosition, AimOutHash);
			PlayerCamera.GetComponent<ITweenMover>().RotateTo(_cameraMeleeLocalEulerAngles, AimOutHash);
		}
	}

	// Setting a parameter the controller doesn't define logs a warning on every call, and the
	// grenade parameters only exist once someone authors clips for them in the Animator window.
	private bool upperAnimatorHasParameter(string parameterName)
	{
		if (_upperAnimatorParameters == null)
		{
			// Animator.parameters allocates a fresh array per call, so it gets cached once.
			_upperAnimatorParameters = UpperAnimator.parameters;
		}
		for (int i = 0; i < _upperAnimatorParameters.Length; i++)
		{
			if (_upperAnimatorParameters[i].name == parameterName)
			{
				return true;
			}
		}
		return false;
	}

	private void setUpperAnimatorBool(string parameterName, bool value)
	{
		if (upperAnimatorHasParameter(parameterName))
		{
			UpperAnimator.SetBool(parameterName, value);
		}
	}

	private void setUpperAnimatorTrigger(string parameterName)
	{
		if (upperAnimatorHasParameter(parameterName))
		{
			UpperAnimator.SetTrigger(parameterName);
		}
	}

	private void OnEquippedWeaponChanged()
	{
		// Debug.Log("OnEquippedWeaponChanged fired, weapon = " + (WeaponType)base.state.EquippedWeapon);
		WeaponType equippedWeapon = (WeaponType)base.state.EquippedWeapon;
		//Debug.Log("Equipped weapon is now: " + equippedWeapon + " | Array length: " + WeaponModels.Length);
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
		UpperAnimator.SetBool("HasSnowballLauncher", equippedWeapon == WeaponType.SnowballLauncher);
		setUpperAnimatorBool("HasGrenade", equippedWeapon == WeaponType.Grenade);
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
		float moveMultiplier = GetMoveMultiplier(playerMoveCommand);
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

	private float GetMoveMultiplier(PlayerMoveCommand cmd)
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
		if (HasSnowballLauncher())
		{
			return SnowballStats.MovementMultiplier;
		}
		if (HasBoxingGloves())
		{
			return BoxingGlovesMovementMultiplier;
		}
		if (HasGrenade())
		{
			// Keyed off the command input rather than the wind-up flag, because this also runs
			// while a client re-simulates old commands and the input is the only version of it
			// that rewinds with them.
			return (cmd.Input.Attack1Held ? GrenadeWindupMovementMultiplier : GrenadeStats.MovementMultiplier);
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
		if (_isFiringLightning)
		{
			_lastLightningFireTime = Time.time;
		}
		if (base.entity.IsOwner())
		{
			executeLightningCommand(cmd);
		}


		if (HasPistol() && cmd.Input.Attack1Held)
		{
			_lastPistolFireInputTime = Time.time;
		}
		bool isFiring = HasPistol() && (Time.time - _lastPistolFireInputTime < 0.15f);
		UpperAnimator.SetBool("IsFiringPistol", isFiring);
		//Debug.Log("Frame: " + Time.frameCount + " | Attack1Held: " + cmd.Input.Attack1Held + " | IsFiringPistol: " + isFiring);

		// The grenade has no aim-down-sights: holding fire winds the throw up and releasing it
		// throws. PlayerMoveCommand only carries the held state, so the release is the edge
		// between two commands - tracked on first execution so a re-simulated command can't
		// report a second one.
		bool isCookingGrenade = cmd.Input.Attack1Held && HasGrenade();
		bool releasedGrenade = _wasCookingGrenade && HasGrenade() && !cmd.Input.Attack1Held;
		if (cmd.IsFirstExecution)
		{
			_wasCookingGrenade = isCookingGrenade;
		}

		bool flag = (cmd.Input.Attack2Held && HasAimableRangedWeapon()) || isCookingGrenade;
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
		if (HasGrenade())
		{
			// Throws on release, so it never runs the press-to-attack path below - that would
			// fire an attack every frame the grenade is held.
			if (releasedGrenade)
			{
				throwGrenade(cmd);
			}
			return;
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
			if (HasSnowballLauncher())
			{
				base.state.CurrentWeaponAmmo--;
				spawnSnowball(cmd.ServerFrame);
				if (base.state.CurrentWeaponAmmo == 0)
				{
					base.state.EquippedWeapon = 0;
				}
			}
		}
		tryRenderAttackID(attackID, attackDirection);
	}

	private void throwGrenade(PlayerMoveCommand cmd)
	{
		int attackID = base.state.ExecutingAttackID + 1;
		if (base.entity.IsOwner())
		{
			base.state.AttackStartTime = BoltNetwork.serverTime;
			base.state.AttackDirection = GrenadeThrowAttackDirection;
			base.state.ExecutingAttackID++;
			spawnGrenade(cmd.ServerFrame);
			base.state.EquippedWeapon = 0; // instantly gone after one throw
		}
		tryRenderAttackID(attackID, (CharacterDirection)GrenadeThrowAttackDirection);
	}

	private void spawnCrossbowBolt(int commandServerFrame)
	{
		Vector3 spawnPosition = AimCameraPosition.position + AimCameraPosition.forward * CrossBowSpawnDistance;
		BoltEntity boltEntity = BoltNetwork.Instantiate(BoltPrefabs.CrossbowBolt);
		boltEntity.GetComponent<CrossbowProjectile>().Intialize(this, base.state.ExecutingAttackID, spawnPosition, AimCameraPosition.eulerAngles, commandServerFrame);
	}

	private void spawnSnowball(int commandServerFrame)
	{
		// Spawn on the aim axis like the crossbow, so the shot converges on the crosshair
		// instead of flying parallel to it from the muzzle.
		Vector3 spawnPosition = AimCameraPosition.position + AimCameraPosition.forward * SnowballStats.SpawnDistance;
		Vector3 throwVelocity = AimCameraPosition.forward * SnowballThrowForce;

		if (SnowballStats.MuzzleFlash != null)
		{
			SnowballStats.MuzzleFlash.Play();
		}

		BoltEntity boltEntity = BoltNetwork.Instantiate(MakePrefabId(15), spawnPosition, Quaternion.identity);
		SnowballProjectile projectile = boltEntity.GetComponent<SnowballProjectile>();
		projectile.Damage = SnowballDamage;
		projectile.ExplosionRadius = SnowballExplosionRadius;
		projectile.Initialize(this, base.state.ExecutingAttackID, throwVelocity);
	}

	public static PrefabId MakePrefabId(int id)
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
		Vector3 spawnPosition = AimCameraPosition.position + AimCameraPosition.forward * PistolStats.SpawnDistance;

		if (PistolStats.MuzzleFlash != null)
		{
			PistolStats.MuzzleFlash.Play();
		}

		BoltEntity boltEntity = BoltNetwork.Instantiate(MakePrefabId(12));
		boltEntity.GetComponent<CrossbowProjectile>().Intialize(this, base.state.ExecutingAttackID, spawnPosition, AimCameraPosition.eulerAngles, commandServerFrame);
	}

	// Single source of truth for the throw so the preview arc and the grenade that actually
	// spawns can't drift apart.
	private void getGrenadeThrowVector(out Vector3 spawnPosition, out Vector3 throwVelocity)
	{
		spawnPosition = GrenadeStats.MuzzlePoint.position + GrenadeStats.MuzzlePoint.forward * GrenadeStats.SpawnDistance;
		throwVelocity = AimCameraPosition.forward * GrenadeThrowForce;
	}

	private void spawnGrenade(int commandServerFrame)
	{
		Vector3 spawnPosition;
		Vector3 throwVelocity;
		getGrenadeThrowVector(out spawnPosition, out throwVelocity);

		if (GrenadeStats.MuzzleFlash != null)
		{
			GrenadeStats.MuzzleFlash.Play();
		}

		BoltEntity boltEntity = BoltNetwork.Instantiate(MakePrefabId(17), spawnPosition, Quaternion.identity); // next free ID
		SnowballProjectile projectile = boltEntity.GetComponent<SnowballProjectile>();
		projectile.Damage = GrenadeDamage;
		projectile.ExplosionRadius = GrenadeExplosionRadius;
		projectile.Initialize(this, base.state.ExecutingAttackID, throwVelocity);
	}

	// Authoritative lightning tick. Runs on the owner (the host) for every player, from inside
	// the command, so the hit test can be rewound to the frame the shooter actually saw.
	private void executeLightningCommand(PlayerMoveCommand cmd)
	{
		if (!_isFiringLightning)
		{
			_lightningAmmoAccumulator = 0f;
			_lightningDamageAccumulator = 0f;
			return;
		}
		if (base.state.CurrentWeaponAmmo <= 0)
		{
			base.state.EquippedWeapon = 0;
			_lightningAmmoAccumulator = 0f;
			_lightningDamageAccumulator = 0f;
			return;
		}
		if (LightningStats.MuzzlePoint != null)
		{
			applyLightningDamage(cmd);
		}
		_lightningAmmoAccumulator += LightningStats.AmmoDrainPerSecond * BoltNetwork.frameDeltaTime;
		if (_lightningAmmoAccumulator >= 1f)
		{
			int num = Mathf.FloorToInt(_lightningAmmoAccumulator);
			base.state.CurrentWeaponAmmo = Mathf.Max(0, base.state.CurrentWeaponAmmo - num);
			_lightningAmmoAccumulator -= num;
		}
	}

	private void applyLightningDamage(PlayerMoveCommand cmd)
	{
		// Hit test runs down the aim axis like the crossbow; the beam still renders from the muzzle.
		Vector3 origin = AimCameraPosition.position;
		Vector3 direction = AimCameraPosition.forward;
		Ray ray = new Ray(origin, direction);
		BoltPhysicsHits boltPhysicsHits = BoltNetwork.RaycastAll(ray, cmd.ServerFrame);
		SantaCharacterController santaCharacterController = null;
		BoltPhysicsHit hit = default(BoltPhysicsHit);
		for (int i = 0; i < boltPhysicsHits.count; i++)
		{
			BoltPhysicsHit boltPhysicsHit = boltPhysicsHits.GetHit(i);
			if (boltPhysicsHit.distance > LightningStats.Range || boltPhysicsHit.hitbox.hitboxType == BoltHitboxType.Proximity)
			{
				continue;
			}
			SantaCharacterController component = boltPhysicsHit.body.GetComponent<SantaCharacterController>();
			if (component != null && component != this)
			{
				santaCharacterController = component;
				hit = boltPhysicsHit;
				break;
			}
		}
		if (santaCharacterController == null)
		{
			return;
		}
		int layerMask = 1 << LayerMask.NameToLayer("Environment");
		RaycastHit hitInfo;
		if (Physics.Raycast(ray, out hitInfo, LightningStats.Range, layerMask) && hitInfo.distance < hit.distance)
		{
			return;
		}
		_lightningDamageAccumulator += LightningStats.DamagePerSecond * BoltNetwork.frameDeltaTime;
		if (_lightningDamageAccumulator < 1f)
		{
			return;
		}
		int num = Mathf.FloorToInt(_lightningDamageAccumulator);
		_lightningDamageAccumulator -= num;
		Vector3 worldImpactPosition = origin + direction * hit.distance;
		if (hit.hitbox.hitboxType == BoltHitboxType.Body)
		{
			santaCharacterController.TryTakeDamageFromAttack(this, num, direction, base.state.ExecutingAttackID, WeaponType.LightningGun, worldImpactPosition);
		}
		else
		{
			santaCharacterController.TryTakeReindeerDamageFromAttack(this, num, direction, base.state.ExecutingAttackID, WeaponType.LightningGun, worldImpactPosition);
		}
	}

	// Proxies never run ExecuteCommand, so _isFiringLightning is only ever set on the copies that
	// simulate input. What every copy does get is ExecutingAttackID ticking up while the trigger
	// is held (TimeBetweenAttacks is 0 for this weapon), which tryRenderAttackID turns into
	// _lastLightningFireTime - the same signal the firing animation already runs on.
	private bool isFiringLightningForDisplay()
	{
		if (HasBeenUnderLocalControl())
		{
			return _isFiringLightning;
		}
		return HasLightningGun() && Time.time - _lastLightningFireTime < LightningRemoteFiringHoldTime;
	}

	private void UpdateLightningBeam(bool isFiring)
	{
		if (!isFiring)
		{
			if (LightningBeamInstance != null)
			{
				LightningBeamInstance.SetActive(false);
			}
			if (LightningAudioSource != null && LightningAudioSource.isPlaying)
			{
				LightningAudioSource.Stop();
			}
			_wasFiringLightningLastFrame = false;
			return;
		}

		if (LightningBeamInstance == null || LightningStats.MuzzlePoint == null)
		{
			return;
		}

		if (base.state.CurrentWeaponAmmo <= 0)
		{
			LightningBeamInstance.SetActive(false);
			if (LightningAudioSource != null && LightningAudioSource.isPlaying)
			{
				LightningAudioSource.Stop();
			}
			_wasFiringLightningLastFrame = false;
			return;
		}

		if (!_wasFiringLightningLastFrame)
		{
			LightningBeamInstance.SetActive(true);
			ParticleSystem ps = LightningBeamInstance.GetComponent<ParticleSystem>();
			if (ps != null)
			{
				ps.Play();
			}
			if (LightningAudioSource != null && LightningStats.MuzzlePoint != null && (HasBeenUnderLocalControl() || PlayLightningAudioForOtherPlayers))
			{
				AudioClipDefinition clipDef = Singleton<AudioLibrary>.Instance.LightningFire[0]; // or randomize like PlayClipAtTransform likely does
				LightningAudioSource.clip = clipDef.Clip; // adjust field name based on AudioClipDefinition's actual structure
				LightningAudioSource.loop = true;
				if (!HasBeenUnderLocalControl())
				{
					// The source on the prefab is 2D, which is what you want for your own gun but
					// would put every other player's beam at full volume in your ears.
					LightningAudioSource.spatialBlend = 1f;
				}
				LightningAudioSource.Play();
			}
		}
		_wasFiringLightningLastFrame = true;

		// Hit detection and ammo drain are authoritative and live in executeLightningCommand;
		// from here down this method is purely beam presentation.
		Vector3 beamOrigin;
		Vector3 beamDirection;
		getLightningBeamAxis(out beamOrigin, out beamDirection);
		LightningBeamInstance.transform.position = beamOrigin;
		LightningBeamInstance.transform.rotation = Quaternion.LookRotation(beamDirection);
		updateLightningBeamLength(beamOrigin, beamDirection);
	}

	// The beam is drawn from the muzzle, so the same axis is what the length test has to use -
	// anything else and the beam stops somewhere other than where it visibly meets geometry.
	private void getLightningBeamAxis(out Vector3 origin, out Vector3 direction)
	{
		origin = LightningStats.MuzzlePoint.position;
		if (HasBeenUnderLocalControl())
		{
			direction = AimCameraPosition.forward;
			return;
		}
		// AimCameraPosition hangs off CameraRoot, and only ExecuteCommand ever turns that - which
		// proxies don't run, so it would be stuck at its authored pitch. TiltRoot is given the
		// same rotation and is replicated, so it is the aim axis that survives the trip.
		direction = TiltRoot.forward;
	}

	// The FX prefab throws its particles much further than LightningStats.Range, so the beam
	// visibly reaches past anything the gun can actually damage. Scaling the lifetime of the
	// travelling particles pulls the tip back to where the shot really ends; every layer gets the
	// same factor, so the beam keeps its authored proportions and just gets shorter.
	private void updateLightningBeamLength(Vector3 beamOrigin, Vector3 beamDirection)
	{
		if (!cacheLightningBeamSystems())
		{
			return;
		}
		float length = getLightningBeamLength(beamOrigin, beamDirection);
		if (Mathf.Approximately(length, _lightningBeamAppliedLength))
		{
			return;
		}
		_lightningBeamAppliedLength = length;
		float lifetimeScale = length / _lightningBeamAuthoredReach;
		for (int i = 0; i < _lightningBeamSystems.Length; i++)
		{
			if (_lightningBeamAuthoredLifetimes[i] <= 0f)
			{
				continue;
			}
			ParticleSystem.MainModule main = _lightningBeamSystems[i].main;
			main.startLifetimeMultiplier = _lightningBeamAuthoredLifetimes[i] * lifetimeScale;
		}
	}

	private float getLightningBeamLength(Vector3 beamOrigin, Vector3 beamDirection)
	{
		float distance = LightningStats.Range;
		if (LightningBeamStopsAtHit)
		{
			// Presentation only - the authoritative, lag-compensated hit test is in
			// applyLightningDamage. This just needs to know where the beam visually stops.
			int hitCount = Physics.RaycastNonAlloc(beamOrigin, beamDirection, _lightningBeamHits, distance, ~(1 << 2), QueryTriggerInteraction.Ignore);
			for (int i = 0; i < hitCount; i++)
			{
				RaycastHit hit = _lightningBeamHits[i];
				if (hit.collider == null || hit.distance >= distance)
				{
					continue;
				}
				// The ray starts at the muzzle, inside the shooter, so their own colliders would
				// otherwise pin the beam to zero length.
				if (hit.collider.GetComponentInParent<SantaCharacterController>() == this)
				{
					continue;
				}
				distance = hit.distance;
			}
		}
		return distance * LightningBeamLengthScale;
	}

	private bool cacheLightningBeamSystems()
	{
		if (!_lightningBeamSystemsSearched)
		{
			_lightningBeamSystemsSearched = true;
			ParticleSystem[] systems = LightningBeamInstance.GetComponentsInChildren<ParticleSystem>(true);
			_lightningBeamSystems = systems;
			_lightningBeamAuthoredLifetimes = new float[systems.Length];
			for (int i = 0; i < systems.Length; i++)
			{
				ParticleSystem.MainModule main = systems[i].main;
				float lifetime = main.startLifetimeMultiplier;
				float speed = main.startSpeedMultiplier;
				// Only particles that travel define how far the beam reaches; the muzzle flash and
				// glow sit still, and shortening their lifetime would just make them flicker.
				if (speed <= 0f)
				{
					continue;
				}
				_lightningBeamAuthoredLifetimes[i] = lifetime;
				_lightningBeamAuthoredReach = Mathf.Max(_lightningBeamAuthoredReach, speed * lifetime);
			}
		}
		return _lightningBeamAuthoredReach > 0f;
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
	    if (HasSnowballLauncher())
		{
			return SnowballStats.TimeBetweenAttacks;
		}
		if (HasBoxingGloves())
		{
			return TimeBetweenBoxingGlovesAttacks;
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
		if ((int)attackDirection == GrenadeThrowAttackDirection)
		{
			// Checked before the weapon branches: EquippedWeapon is already None by now, on every
			// client, so this is the only thing that still identifies the throw.
			Singleton<AudioManager>.Instance.PlayClipAtTransform(Singleton<AudioLibrary>.Instance.SnowballThrow, UpperAnimator.transform);
			setUpperAnimatorTrigger("ThrowGrenade");
			if (!string.IsNullOrEmpty(GrenadeThrowAnimatorState))
			{
				UpperAnimator.Play(GrenadeThrowAnimatorState, 0, 0f);
			}
			return;
		}
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
		else if (HasSnowballLauncher())
		{
			  Debug.Log("Snowball fire branch reached, clips: " + Singleton<AudioLibrary>.Instance.SnowballThrow.Length);
			Singleton<AudioManager>.Instance.PlayClipAtTransform(Singleton<AudioLibrary>.Instance.SnowballThrow, UpperAnimator.transform);
			UpperAnimator.SetTrigger("FireSnowball");
		}
		else if (HasLightningGun())
		{
			// Continuous fire, so no trigger - this keeps the pose alive for remote viewers,
			// who only learn about the shot when ExecutingAttackID replicates.
			_lastLightningFireTime = Time.time;
		}
		else if (!HasAnyEquippedWeapon() || HasBoxingGloves())
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
	public bool HasSnowballLauncher()
	{
		return base.state.EquippedWeapon == (int)WeaponType.SnowballLauncher;
	}
	public bool HasGrenade()
	{
		return base.state.EquippedWeapon == (int)WeaponType.Grenade;
	}
	public bool HasBoxingGloves()
	{
		return base.state.EquippedWeapon == (int)WeaponType.BoxingGloves;
	}

	// Weapons that aim down sights on Attack2 and fire along AimCameraPosition.
	public bool HasAimableRangedWeapon()
	{
		return HasCrossbow() || HasPistol() || HasSnowballLauncher() || HasLightningGun();
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

		// Bolt throws on any state access once an entity is detached, and these objects keep
		// ticking for a frame or two while the session tears down on the way back to the menu.
		if (_isDetached || base.entity == null || !base.entity.isAttached)
		{
			hideGrenadeArc();
			return;
		}

		UpperAnimator.SetBool("IsFiringLightning", HasLightningGun() && Time.time - _lastLightningFireTime < LightningRemoteFiringHoldTime);

		// These run for every copy of the character, not just the local one, so other players can
		// see the wind-up and the beam too.
		updateGrenadeWindupPose();
		UpdateLightningBeam(isFiringLightningForDisplay());

		if (HasBeenUnderLocalControl())
		{
			// IsSprinting is just "sprint key held with stamina left", so on its own it fires
			// while standing still. The speed boost and stamina drain both require movement
			// too (flag2 && flag5 in ExecuteCommand); this keeps the FOV consistent with them.
			bool isSprintingAndMoving = base.state.IsSprinting && base.state.IsMoving;
			float targetFOV = _baseFOV;
			if (isSprintingAndMoving)
			{
				targetFOV += SprintFOVBoost;
			}
			if (IsPreparingGrenadeThrow())
			{
				targetFOV += GrenadeWindupFOVBoost;
			}
			PlayerCamera.fieldOfView = Mathf.Lerp(PlayerCamera.fieldOfView, targetFOV, Time.deltaTime * FOVLerpSpeed);

			updateGrenadeArc();
		}
		else
		{
			hideGrenadeArc();
		}
	}

	// True wherever the wind-up should be showing. The local controller sets _isInAimState itself
	// rather than waiting for IsAiming to round-trip through the owner; proxies get it from the
	// replicated flag via OnIsAimingChanged.
	public bool IsPreparingGrenadeThrow()
	{
		return HasGrenade() && _isInAimState;
	}

	// Placeholder for a real throw animation: eases the grenade model back into a cocked pose
	// while the throw is being held, and back to its authored pose when it isn't.
	private void updateGrenadeWindupPose()
	{
		float target = (IsPreparingGrenadeThrow() ? 1f : 0f);
		if (target == 0f && _grenadeWindupBlend == 0f)
		{
			// Resting pose - leave the transform alone rather than rewriting it every frame.
			return;
		}
		Transform grenadeModel = getGrenadeModel();
		if (grenadeModel == null)
		{
			return;
		}
		if (!HasGrenade())
		{
			// Thrown or swapped away: the model is hidden now, so snap rather than ease - the
			// next grenade has to start from the authored pose.
			_grenadeWindupBlend = 0f;
		}
		else if (GrenadeWindupTime > 0f)
		{
			_grenadeWindupBlend = Mathf.MoveTowards(_grenadeWindupBlend, target, Time.deltaTime / GrenadeWindupTime);
		}
		else
		{
			_grenadeWindupBlend = target;
		}
		grenadeModel.localPosition = _grenadeModelBasePosition + GrenadeWindupLocalOffset * _grenadeWindupBlend;
		grenadeModel.localRotation = _grenadeModelBaseRotation * Quaternion.Euler(GrenadeWindupLocalEuler * _grenadeWindupBlend);
	}

	private Transform getGrenadeModel()
	{
		if (!_grenadeModelSearched)
		{
			_grenadeModelSearched = true;
			for (int i = 0; i < WeaponModels.Length; i++)
			{
				if (WeaponModels[i] != null && WeaponModels[i].WeaponType == WeaponType.Grenade)
				{
					_grenadeModel = WeaponModels[i].transform;
					// Captured before anything offsets it, so this is the authored pose.
					_grenadeModelBasePosition = _grenadeModel.localPosition;
					_grenadeModelBaseRotation = _grenadeModel.localRotation;
					break;
				}
			}
		}
		return _grenadeModel;
	}

	// Only the throwing player sees their own arc; remote copies of this character never draw one.
	private void updateGrenadeArc()
	{
		if (!ShowGrenadeArc || !HasGrenade() || !IsAlive() || GrenadeStats.MuzzlePoint == null || AimCameraPosition == null)
		{
			hideGrenadeArc();
			return;
		}
		if (ShowGrenadeArcOnlyWhileCooking && !IsPreparingGrenadeThrow())
		{
			hideGrenadeArc();
			return;
		}
		if (_grenadeArcPreview == null)
		{
			_grenadeArcPreview = new GrenadeArcPreview(base.transform);
		}
		_grenadeArcPreview.ArcMaterial = GrenadeArcMaterial;
		_grenadeArcPreview.ArcColor = GrenadeArcColor;
		_grenadeArcPreview.ArcWidth = GrenadeArcWidth;
		_grenadeArcPreview.MaxSimulationTime = GrenadeArcMaxTime;
		_grenadeArcPreview.PointInterval = GrenadeArcPointInterval;
		_grenadeArcPreview.ProjectileRadius = GrenadeArcProjectileRadius;
		_grenadeArcPreview.GravityMultiplier = GrenadeArcGravityMultiplier;
		_grenadeArcPreview.ExplosionRadius = GrenadeExplosionRadius;
		_grenadeArcPreview.ShowImpactMarker = ShowGrenadeArcImpactMarker;
		_grenadeArcPreview.CollisionMask = GrenadeArcCollisionMask;

		Vector3 spawnPosition;
		Vector3 throwVelocity;
		getGrenadeThrowVector(out spawnPosition, out throwVelocity);
		_grenadeArcPreview.Show(spawnPosition, throwVelocity, this);
	}

	private void hideGrenadeArc()
	{
		if (_grenadeArcPreview != null)
		{
			_grenadeArcPreview.Hide();
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
		if (HasBoxingGloves())
		{
			return BoxingGlovesDamage;
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
				if (attackingCharacter != this)
				{
					attackingCharacter.gainKillHeal();
					PlayerStatsManager.ReportKill(attackingCharacter);
				}
				dropCurrentWeapon();
				destroyThisAndCreateRagdoll(damageDirection);
			}
			else if (weaponUsed == WeaponType.Crossbow)
			{
				createCrossBowStickEvent(damageDirection, false, worldImpactPosition);
			}
		}
	}

	// Called on this character when it lands a killing blow. Host-only: the caller already
	// runs behind BoltNetwork.isServer, and HitPoints replicates the result to everyone.
	private void gainKillHeal()
	{
		if (HitpointsHealedPerKill <= 0 || _isDetached || !IsAlive())
		{
			return;
		}
		base.state.HitPoints = Mathf.Clamp(base.state.HitPoints + HitpointsHealedPerKill, 0, StartHitpoints);
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
