using System;
using System.Globalization;
using UnityEngine;
using Bolt;

public class WeaponDebugger : MonoBehaviour
{
    [Serializable]
    public class WeaponBinding
    {
        public KeyCode Key;
        public WeaponType Weapon;
        public int Ammo = 99;
    }

    [Header("Configure your test keybinds here")]
    public WeaponBinding[] Bindings = new WeaponBinding[]
    {
        new WeaponBinding { Key = KeyCode.Alpha1, Weapon = WeaponType.None,     Ammo = 0 },
        new WeaponBinding { Key = KeyCode.Alpha2, Weapon = WeaponType.Sword,    Ammo = 99 },
        new WeaponBinding { Key = KeyCode.Alpha3, Weapon = WeaponType.Crossbow, Ammo = 99 },
        new WeaponBinding { Key = KeyCode.Alpha4, Weapon = WeaponType.Reindeer, Ammo = 99 },
        new WeaponBinding { Key = KeyCode.Alpha5, Weapon = WeaponType.Pistol,   Ammo = 99 },
    };

    [Header("Optional: give full HP + reset stamina on equip, for repeat testing")]
    // Refills to the controller's StartHitpoints, so there's one health number to tune.
    public bool RefillHealthOnEquip = true;

    // Only the host may write entity state, so a client encodes its request into a LogEvent
    // aimed at the server and WeaponDebuggerServer applies it there.
    public const string RequestPrefix = "#wpndbg ";

    private SantaCharacterController _controller;
    private BoltEntity _entity;

    private void Awake()
    {
        _controller = GetComponent<SantaCharacterController>();
        _entity = GetComponent<BoltEntity>();
    }

    private void Update()
    {
        if (_controller == null || _entity == null || !_controller.HasBeenUnderLocalControl())
        {
            return;
        }

        foreach (WeaponBinding binding in Bindings)
        {
            if (Input.GetKeyDown(binding.Key))
            {
                RequestWeapon(binding.Weapon, binding.Ammo);
                break;
            }
        }
    }

    private void RequestWeapon(WeaponType type, int ammo)
    {
        if (_entity.IsOwner())
        {
            ApplyWeapon(_controller, type, ammo, RefillHealthOnEquip);
            return;
        }

        // Send the intent, not a health value - the host resolves it from the target character.
        LogEvent request = LogEvent.Create(GlobalTargets.OnlyServer);
        request.message = string.Concat(
            RequestPrefix,
            ((int)type).ToString(CultureInfo.InvariantCulture), " ",
            ammo.ToString(CultureInfo.InvariantCulture), " ",
            (RefillHealthOnEquip ? "1" : "0"));
        request.Send();
        Debug.Log("WeaponDebugger: asked the host for " + type + " (ammo: " + ammo + ")");
    }

    public static bool IsRequest(string message)
    {
        return message != null && message.StartsWith(RequestPrefix, StringComparison.Ordinal);
    }

    public static bool TryParseRequest(string message, out WeaponType type, out int ammo, out bool refillHealth)
    {
        type = WeaponType.None;
        ammo = 0;
        refillHealth = false;
        if (!IsRequest(message))
        {
            return false;
        }
        string[] parts = message.Substring(RequestPrefix.Length).Split(' ');
        int weaponId;
        int refillFlag;
        if (parts.Length != 3
            || !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out weaponId)
            || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out ammo)
            || !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out refillFlag))
        {
            return false;
        }
        type = (WeaponType)weaponId;
        refillHealth = refillFlag != 0;
        return true;
    }

    // Must only be called on the host - clients cannot write entity state.
    public static void ApplyWeapon(SantaCharacterController controller, WeaponType type, int ammo, bool refillHealth)
    {
        if (controller == null || controller.entity == null || !controller.entity.isAttached)
        {
            return;
        }
        ISantaState state = controller.state;
        if (state == null)
        {
            Debug.LogWarning("WeaponDebugger: could not access ISantaState.");
            return;
        }

        state.EquippedWeapon = (int)type;
        state.CurrentWeaponAmmo = ammo;
        if (refillHealth)
        {
            // Same number the character spawns with, under Health / Death.
            state.HitPoints = controller.StartHitpoints;
        }

        Debug.Log("WeaponDebugger: equipped " + type + " (ammo: " + ammo + ")");
    }
}
