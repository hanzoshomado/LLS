using System;
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

        if (!_entity.IsOwner())
        {
            return;
        }

        foreach (WeaponBinding binding in Bindings)
        {
            if (Input.GetKeyDown(binding.Key))
            {
                EquipWeapon(binding.Weapon, binding.Ammo);
                break;
            }
        }
    }

    private void EquipWeapon(WeaponType type, int ammo)
{
    ISantaState state = _entity.GetState<ISantaState>();
    if (state == null)
    {
        Debug.LogWarning("WeaponDebugger: could not access ISantaState.");
        return;
    }

    Debug.Log("WeaponDebugger EquipWeapon called with type=" + type + " (int value: " + (int)type + ")");

    state.EquippedWeapon = (int)type;
    state.CurrentWeaponAmmo = ammo;

        if (RefillHealthOnEquip)
        {
            state.HitPoints = FullHitpoints;
        }

        Debug.Log("WeaponDebugger: equipped " + type + " (ammo: " + ammo + ")");
    }
}