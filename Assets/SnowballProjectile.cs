using System;
using UnityEngine;
using Bolt;

public class SnowballProjectile : EntityBehaviour<IProjectileState>
{
    public int Damage;
    public float ExplosionRadius;
    public GameObject ImpactEffect;
    public float GravityMultiplier = 1f; // tune per-weapon on each prefab (Snowball vs Grenade, etc.)

    private SantaCharacterController _owner;
    private int _attackID;
    private bool _hasExploded;
    private bool _impactEffectPlayed;
    private Rigidbody _rb;

    private void Awake()
    {
        CacheRigidbody();
    }

    private void CacheRigidbody()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody>();
        }
        _rb.useGravity = false; // disable built-in gravity, apply our own scaled version instead
    }

    public override void Attached()
    {
        CacheRigidbody();

        // Without this the entity replicates nothing, so every remote copy sits at its
        // spawn point running unsynced local physics instead of following the throw.
        base.state.SetTransforms(base.state.Transform, base.transform);

        if (!base.entity.IsOwner())
        {
            // Bolt writes the replicated position straight onto the transform; local physics
            // would fight it and re-introduce the "grenade just drops" desync.
            _rb.isKinematic = true;
            _rb.detectCollisions = false;
        }
    }

    public override void Detached()
    {
        // The owner destroys the entity when it detonates, which detaches it everywhere,
        // so this is what gets the blast onto non-host screens.
        PlayImpactEffect();
    }

    private void FixedUpdate()
    {
        if (!IsSimulatingLocally()) return;
        _rb.AddForce(Physics.gravity * GravityMultiplier, ForceMode.Acceleration);
    }

    public void Initialize(SantaCharacterController owner, int attackID, Vector3 velocity)
    {
        _owner = owner;
        _attackID = attackID;
        _rb.velocity = velocity;
    }

    private bool IsSimulatingLocally()
    {
        return base.entity != null && base.entity.isAttached && base.entity.IsOwner();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsSimulatingLocally()) return;
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

        SafeDestroy();
    }

    private void PlayImpactEffect()
    {
        if (_impactEffectPlayed || ImpactEffect == null) return;
        _impactEffectPlayed = true;

        // Detach also fires while Bolt is tearing down (disconnect, shutdown); no blast then.
        if (!BoltNetwork.isRunning) return;

        GameObject effect = UnityEngine.Object.Instantiate(ImpactEffect, transform.position, Quaternion.identity);
        if (ExplosionRadius > 0f)
        {
            effect.transform.localScale = Vector3.one * ExplosionRadius;
        }
        UnityEngine.Object.Destroy(effect, 3f);
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
