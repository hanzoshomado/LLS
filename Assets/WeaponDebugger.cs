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
    public bool RefillHealthOnEquip = true;
    public int FullHitpoints = 100; // set to match StartHitpoints on SantaCharacterController if you want exact parity

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
        int hitpoints = RefillHealthOnEquip ? FullHitpoints : 0;

        if (_entity.IsOwner())
        {
            ApplyWeapon(_controller, type, ammo, hitpoints);
            return;
        }

        LogEvent request = LogEvent.Create(GlobalTargets.OnlyServer);
        request.message = string.Concat(
            RequestPrefix,
            ((int)type).ToString(CultureInfo.InvariantCulture), " ",
            ammo.ToString(CultureInfo.InvariantCulture), " ",
            hitpoints.ToString(CultureInfo.InvariantCulture));
        request.Send();
        Debug.Log("WeaponDebugger: asked the host for " + type + " (ammo: " + ammo + ")");
    }

    public static bool IsRequest(string message)
    {
        return message != null && message.StartsWith(RequestPrefix, StringComparison.Ordinal);
    }

    public static bool TryParseRequest(string message, out WeaponType type, out int ammo, out int hitpoints)
    {
        type = WeaponType.None;
        ammo = 0;
        hitpoints = 0;
        if (!IsRequest(message))
        {
            return false;
        }
        string[] parts = message.Substring(RequestPrefix.Length).Split(' ');
        int weaponId;
        if (parts.Length != 3
            || !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out weaponId)
            || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out ammo)
            || !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out hitpoints))
        {
            return false;
        }
        type = (WeaponType)weaponId;
        return true;
    }

    // Must only be called on the host - clients cannot write entity state.
    public static void ApplyWeapon(SantaCharacterController controller, WeaponType type, int ammo, int hitpoints)
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
        if (hitpoints > 0)
        {
            state.HitPoints = hitpoints;
        }

        Debug.Log("WeaponDebugger: equipped " + type + " (ammo: " + ammo + ")");
    }
}
