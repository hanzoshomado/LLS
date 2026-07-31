using UnityEngine;
using UnityEngine.Rendering;

// Draws where a thrown grenade will land, for the local player only.
//
// The simulation deliberately mirrors what SnowballProjectile actually does to the grenade
// rigidbody: gravity is disabled on the prefab and re-applied as Physics.gravity * GravityMultiplier
// once per FixedUpdate, drag is 0, and the grenade detonates on its first collision. So stepping
// symplectic Euler at the physics timestep and stopping at the first cast hit reproduces the real
// flight path, not just an approximation of it.
//
// Owned and driven by SantaCharacterController; it builds its own renderers at runtime so nothing
// has to be wired up on the player prefab.
public class GrenadeArcPreview
{
    private const int MaxPoints = 256;
    private const int MarkerSegments = 40;

    // Pushed in from the controller's inspector fields every frame so live tweaks take effect.
    public Material ArcMaterial;
    public Color ArcColor = new Color(1f, 0.45f, 0.15f, 0.9f);
    public float ArcWidth = 0.06f;
    public float MaxSimulationTime = 3f;
    public float PointInterval = 0.05f;
    public float ProjectileRadius = 0.2f;
    public float GravityMultiplier = 1f;
    public float ExplosionRadius;
    public bool ShowImpactMarker = true;
    public LayerMask CollisionMask = ~(1 << 2);

    // Both buffers are oversized and reused, because this whole thing re-runs every frame the
    // grenade is held.
    private readonly Vector3[] _points = new Vector3[MaxPoints];
    private readonly RaycastHit[] _hitBuffer = new RaycastHit[16];
    private readonly Transform _parent;
    private static Material _fallbackMaterial;
    private int _pointCount;
    private LineRenderer _arc;
    private LineRenderer _marker;

    public GrenadeArcPreview(Transform parent)
    {
        _parent = parent;
    }

    public void Show(Vector3 origin, Vector3 velocity, SantaCharacterController thrower)
    {
        RaycastHit impact;
        bool hitSomething = Simulate(origin, velocity, thrower, out impact);

        if (_pointCount < 2)
        {
            Hide();
            return;
        }

        EnsureRenderers();
        ApplyStyle(_arc);
        _arc.positionCount = _pointCount;
        // Set one at a time rather than SetPositions: the buffer is oversized and reused, and
        // SetPositions wants an array whose length matches positionCount exactly.
        for (int i = 0; i < _pointCount; i++)
        {
            _arc.SetPosition(i, _points[i]);
        }
        _arc.enabled = true;

        if (hitSomething && ShowImpactMarker)
        {
            ApplyStyle(_marker);
            DrawImpactMarker(impact);
        }
        else if (_marker != null)
        {
            _marker.enabled = false;
        }
    }

    public void Hide()
    {
        if (_arc != null)
        {
            _arc.enabled = false;
        }
        if (_marker != null)
        {
            _marker.enabled = false;
        }
    }

    private bool Simulate(Vector3 origin, Vector3 velocity, SantaCharacterController thrower, out RaycastHit impact)
    {
        _pointCount = 0;
        _points[_pointCount++] = origin;
        impact = default(RaycastHit);

        float step = Time.fixedDeltaTime;
        if (step <= 0f)
        {
            step = 0.02f;
        }
        // Integrating at the physics step but only casting every few steps keeps the predicted
        // positions exact while holding the per-frame cast count down.
        int stepsPerPoint = Mathf.Max(1, Mathf.RoundToInt(PointInterval / step));
        int maxSteps = Mathf.Max(1, Mathf.CeilToInt(MaxSimulationTime / step));

        Vector3 gravity = Physics.gravity * GravityMultiplier;
        Vector3 position = origin;
        Vector3 segmentStart = origin;

        for (int i = 1; i <= maxSteps && _pointCount < MaxPoints; i++)
        {
            // Velocity first, then position: the order PhysX integrates a force added with
            // ForceMode.Acceleration during FixedUpdate.
            velocity += gravity * step;
            position += velocity * step;

            if (i % stepsPerPoint != 0 && i != maxSteps)
            {
                continue;
            }

            if (TryCastSegment(segmentStart, position, thrower, out impact))
            {
                _points[_pointCount++] = impact.point;
                return true;
            }
            _points[_pointCount++] = position;
            segmentStart = position;
        }
        return false;
    }

    private bool TryCastSegment(Vector3 from, Vector3 to, SantaCharacterController thrower, out RaycastHit hit)
    {
        hit = default(RaycastHit);
        Vector3 delta = to - from;
        float distance = delta.magnitude;
        if (distance <= 0.0001f)
        {
            return false;
        }
        Vector3 direction = delta / distance;

        int count = ((!(ProjectileRadius > 0f)) ?
            Physics.RaycastNonAlloc(from, direction, _hitBuffer, distance, CollisionMask, QueryTriggerInteraction.Ignore) :
            Physics.SphereCastNonAlloc(from, ProjectileRadius, direction, _hitBuffer, distance, CollisionMask, QueryTriggerInteraction.Ignore));

        bool found = false;
        float nearest = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            RaycastHit candidate = _hitBuffer[i];
            if (candidate.collider == null || candidate.distance > nearest)
            {
                continue;
            }
            // The throw starts inside the thrower's own capsule, so without this the arc would
            // collapse onto the muzzle every frame.
            if (thrower != null && candidate.collider.GetComponentInParent<SantaCharacterController>() == thrower)
            {
                continue;
            }
            nearest = candidate.distance;
            hit = candidate;
            found = true;
        }

        if (found && hit.distance <= 0f)
        {
            // A cast that starts already overlapping reports point/normal as zero; fall back to
            // something drawable so the marker doesn't jump to the world origin.
            hit.point = from;
            hit.normal = -direction;
        }
        return found;
    }

    private void DrawImpactMarker(RaycastHit impact)
    {
        float radius = ((ExplosionRadius > 0f) ? ExplosionRadius : 0.5f);
        Vector3 normal = ((impact.normal.sqrMagnitude > 0.0001f) ? impact.normal.normalized : Vector3.up);
        Vector3 center = impact.point + normal * 0.05f;

        Vector3 tangent = Vector3.Cross(normal, Vector3.up);
        if (tangent.sqrMagnitude < 0.001f)
        {
            tangent = Vector3.Cross(normal, Vector3.forward);
        }
        tangent.Normalize();
        Vector3 bitangent = Vector3.Cross(normal, tangent);

        // The renderer is set to loop, so the ring closes itself without a duplicate last point.
        _marker.positionCount = MarkerSegments;
        for (int i = 0; i < MarkerSegments; i++)
        {
            float angle = (float)i / (float)MarkerSegments * Mathf.PI * 2f;
            _marker.SetPosition(i, center + (tangent * Mathf.Cos(angle) + bitangent * Mathf.Sin(angle)) * radius);
        }
        _marker.enabled = true;
    }

    private void EnsureRenderers()
    {
        if (_arc == null)
        {
            _arc = CreateLine("GrenadeArcPreview");
        }
        if (_marker == null)
        {
            _marker = CreateLine("GrenadeArcImpactMarker");
            _marker.loop = true;
        }
    }

    private LineRenderer CreateLine(string name)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(_parent, false);
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.receiveShadows = false;
        lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        lineRenderer.lightProbeUsage = LightProbeUsage.Off;
        lineRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        lineRenderer.alignment = LineAlignment.View;
        lineRenderer.textureMode = LineTextureMode.Stretch;
        lineRenderer.numCapVertices = 2;
        lineRenderer.numCornerVertices = 2;
        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;
        return lineRenderer;
    }

    private void ApplyStyle(LineRenderer lineRenderer)
    {
        Material material = ((ArcMaterial != null) ? ArcMaterial : ResolveFallbackMaterial());
        if (material != null && lineRenderer.sharedMaterial != material)
        {
            lineRenderer.sharedMaterial = material;
        }
        lineRenderer.startColor = ArcColor;
        lineRenderer.endColor = ArcColor;
        lineRenderer.startWidth = ArcWidth;
        lineRenderer.endWidth = ArcWidth;
    }

    private static Material ResolveFallbackMaterial()
    {
        if (_fallbackMaterial != null)
        {
            return _fallbackMaterial;
        }
        // No material assigned in the inspector, so find any unlit vertex-coloured shader that is
        // actually present. If none of them are, the LineRenderer keeps its own default material.
        string[] candidates = new string[]
        {
            "Sprites/Default",
            "Particles/Standard Unlit",
            "Legacy Shaders/Particles/Alpha Blended",
            "Unlit/Color"
        };
        for (int i = 0; i < candidates.Length; i++)
        {
            Shader shader = Shader.Find(candidates[i]);
            if (shader != null)
            {
                _fallbackMaterial = new Material(shader);
                return _fallbackMaterial;
            }
        }
        return null;
    }
}
