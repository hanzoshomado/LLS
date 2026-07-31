using System;
using UnityEngine;
using Bolt;

public class SnowballProjectile : MonoBehaviour
{
    public int Damage;
    public float ExplosionRadius;
    public GameObject ImpactEffect;
    public float GravityMultiplier = 1f; // tune per-weapon on each prefab (Snowball vs Grenade, etc.)

    private SantaCharacterController _owner;
    private int _attackID;
    private bool _hasExploded;
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false; // disable built-in gravity, apply our own scaled version instead
    }

    private void FixedUpdate()
    {
        _rb.AddForce(Physics.gravity * GravityMultiplier, ForceMode.Acceleration);
    }

    public void Initialize(SantaCharacterController owner, int attackID, Vector3 velocity)
    {
        _owner = owner;
        _attackID = attackID;
        _rb.velocity = velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!BoltNetwork.isServer) return;
        if (_hasExploded) return;
        _hasExploded = true;

        if (_owner == null)
        {
            SafeDestroy();
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, ExplosionRadius);
        foreach (Collider hit in hits)
        {
            SantaCharacterController target = hit.GetComponentInParent<SantaCharacterController>();
            if (target != null)
            {
                Vector3 dir = (target.transform.position - transform.position).normalized;
                target.TryTakeDamageFromAttack(_owner, Damage, dir, _attackID, WeaponType.SnowballLauncher, transform.position);
            }
        }

        if (ImpactEffect != null)
        {
            GameObject effect = Instantiate(ImpactEffect, transform.position, Quaternion.identity);
            effect.transform.localScale = Vector3.one * ExplosionRadius;
            Destroy(effect, 3f);
        }

        SafeDestroy();
    }

    private void SafeDestroy()
    {
        try
        {
            BoltNetwork.Destroy(gameObject);
        }
        catch (Exception)
        {
            // already detached/destroyed by Bolt internally - safe to ignore
        }
    }
}