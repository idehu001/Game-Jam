using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class PlayerMovement : MonoBehaviour
{
    // ------------------------------------------------------------------ inspector

    [Header("Ground movement")]
    [Tooltip("Horizontal push force while grounded.")]
    [Min(0f)] public float groundMoveForce = 40f;

    [Range(0f, 1f)]
    [Tooltip("Fraction of groundMoveForce still applied while airborne and out of water.")]
    public float airControlMultiplier = 0.5f;

    [Header("Swimming")]
    [Min(0f)] public float swimForce = 30f;

    [Header("Layers")]
    [Tooltip("Everything that can be stood on.")]
    public LayerMask groundMask;

    [Header("Ground probe")]
    [Tooltip("How far below the body to look for ground.")]
    [Min(0.005f)] public float groundProbeDistance = 0.08f;

    [Range(0f, 80f)]
    [Tooltip("Set it at or above the steepest slope you want walkable.")]
    public float maxSlopeAngle = 80f;

    [Min(0.01f)]
    [Tooltip("How far ahead of the body to look for a slope.")]
    public float slopeLookAhead = 0.05f;

    [Header("Steps")]
    [Tooltip("Ledges up to this tall (world units). " +
             "0 disables stepping.")]
    [Min(0f)] public float maxStepHeight = 0.25f;

    [Min(0.01f)]
    [Tooltip("How far past the body's front edge a step counts as \"in the way.\" " +
             "Also how far onto the ledge you are placed.")]
    public float stepProbeDistance = 0.06f;

    [Header("Water")]
    [Range(0f, 1f)]
    [Tooltip("Fraction of horizontal speed kept as a one-off impulse when breaking the surface.")]
    public float entryKeepHorizontal = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("Fraction of vertical speed kept as a one-off impulse when breaking the surface.")]
    public float entryKeepVertical = 0.25f;

    [Header("Cracking")]
    [Tooltip("Speed while climbing through a crack, in any direction.")]
    [Min(0f)] public float climbMaxSpeed = 4f;

    // ------------------------------------------------------------------ events

    public event Action WaterEntered;
    public event Action WaterExited;
    public event Action CrackEntered;
    public event Action CrackExited;

    // ------------------------------------------------------------------ state

    [Header("Animator")]
    public Animator animator;
    [System.NonSerialized] public Rigidbody2D _rb;
    [System.NonSerialized] public float oldGravityScale;
    Collider2D[] _cols;
    ContactFilter2D _groundFilter;
    readonly RaycastHit2D[] _hits = new RaycastHit2D[16];
    readonly HashSet<Collider2D> _ignoredOneWay = new HashSet<Collider2D>();

    float _moveInput;       // -1..1, from the Move action's .x
    float _vertInput;       // -1..1, from the Float action's .y

    bool _grounded;
    Collider2D _groundCollider;
    Vector2 _groundNormal = Vector2.up;
    Collider2D _body;
    readonly Collider2D[] _overlap = new Collider2D[4];

    int _waterOverlapCount;  // handles overlapping water triggers without needing to rank them by surface height
    int _climbOverlapCount;  // crack triggers touching the body
    bool _climbing;          // climbing: gravity off, velocity driven by input

    // ------------------------------------------------------------------ public read-only

    public bool IsGrounded => _grounded && !IsSwimming && !IsClimbing;
    public bool IsSwimming => _waterOverlapCount > 0;
    public bool IsClimbing => _climbing;
    public Vector2 Velocity => _rb != null ? _rb.linearVelocity : Vector2.zero;
    public int Facing => Mathf.Abs(_moveInput) < 0.05f ? 0 : (int)Mathf.Sign(_moveInput);    //0 or 1. Used for flipping the sprite.

    // ------------------------------------------------------------------ input API

    public void SetMoveInput(float horizontal) => _moveInput = Mathf.Clamp(horizontal, -1f, 1f);
    public void SetVerticalInput(float vertical) => _vertInput = Mathf.Clamp(vertical, -1f, 1f);

    // ------------------------------------------------------------------ lifecycle

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        oldGravityScale = _rb.gravityScale;

        if (_rb.gravityScale <= 0f)
        {
            Debug.LogWarning("[PlayerMovement] Gravity Scale is 0. " +
                             "Controller needs gravity gravity for falling and for buoyancy. " +
                             "Set Gravity Scale in Nu's RigidBody2D to a positive value (2-4).", this);
        }
        _rb.freezeRotation = true;

        int n = Mathf.Max(1, _rb.attachedColliderCount);
        _cols = new Collider2D[n];
        _rb.GetAttachedColliders(_cols);

        RefreshFilter();
    }

    public void RefreshFilter()
    {
        _groundFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = groundMask,
            useTriggers = false
        };
    }

    void FixedUpdate()
    {
        ProbeGround();
        UpdateClimb();

        animator.SetFloat("yVelocity", _rb.linearVelocity.y);
        animator.SetFloat("xVelocity", Mathf.Abs(_rb.linearVelocity.x));
        animator.SetBool("Grounded", _grounded);

        // ---------------- inside a crack: velocity-driven ----------------
        if (_climbing)
        {
            DriveClimb();
            return;
        }

        bool swimming = IsSwimming;
        bool grounded = _grounded && !swimming;

        // ---------------- horizontal forces ----------------
        if (Mathf.Abs(_moveInput) > 0.01f)
        {
            if (grounded)
            {
                // On the ground the push is aimed along the surface, and adjusted so slopes feel like flat ground.
                // See GroundDriveForce.
                float dir = Mathf.Sign(_moveInput);
                _rb.AddForce(GroundDriveForce(dir, groundMoveForce * Mathf.Abs(_moveInput)), ForceMode2D.Force);
                TryStepUp(dir);
            }
            else
            {
                float force = swimming ? groundMoveForce : groundMoveForce * airControlMultiplier;
                _rb.AddForce(Vector2.right * _moveInput * force, ForceMode2D.Force);
            }
        }

        // ---------------- vertical forces ----------------
        if (swimming && Mathf.Abs(_vertInput) > 0.01f)
        {
            _rb.AddForce(Vector2.up * _vertInput * swimForce, ForceMode2D.Force);
        }
    }

    // ------------------------------------------------------------------ ground

    void ProbeGround()
    {
        _grounded = false;
        _groundCollider = null;
        _groundNormal = Vector2.up;

        int count = _rb.Cast(Vector2.down, _groundFilter, _hits, groundProbeDistance);
        float bestUp = -1f;

        for (int i = 0; i < count; i++)
        {
            RaycastHit2D h = _hits[i];
            if (h.collider == null) continue;
            if (!IsWalkable(h.normal)) continue;

            if (h.normal.y > bestUp)
            {
                bestUp = h.normal.y;
                _groundCollider = h.collider;
                _groundNormal = h.normal;
            }
        }

        _grounded = _groundCollider != null;
    }

    // ------------------------------------------------------------------ slopes & steps

    bool IsWalkable(Vector2 normal) =>
        normal.y > 0f && Vector2.Angle(normal, Vector2.up) <= maxSlopeAngle + 0.5f; // The half-degree slack stops a slope at exactly maxSlopeAngle failing on float error.

    // Unit vector along a surface with this normal, pointing the way we want to go (dir is -1 or 1).
    static Vector2 Tangent(Vector2 normal, float dir) => new Vector2(normal.y, -normal.x) * dir;


    ///  1. Aimed along the surface. A sideways push into a slope also presses the body into it,
    ///     which raises friction, and only cos(angle) of it is left pointing uphill.
    ///
    ///  2. Adjusted so a slope feels like flat ground. Top speed here is wherever the push balances
    ///     friction and damping, so anything that changes friction or adds gravity along the surface changes Nu's speed.
    Vector2 GroundDriveForce(float dir, float push)
    {
        // Surface to move along
        Vector2 n = _groundNormal;
        Collider2D surface = _groundCollider;

        int count = _rb.Cast(new Vector2(dir, 0f), _groundFilter, _hits, slopeLookAhead);
        for (int i = 0; i < count; i++)
        {
            RaycastHit2D h = _hits[i];
            if (h.collider == null || _ignoredOneWay.Contains(h.collider)) continue;
            if (!IsWalkable(h.normal)) continue;
            if (Tangent(h.normal, dir).y > Tangent(n, dir).y)
            {
                n = h.normal;
                surface = h.collider;
            }
        }

        Vector2 t = Tangent(n, dir);

        Vector2 weight = Physics2D.gravity * (_rb.gravityScale * _rb.mass);
        float mu = Mathf.Sqrt(Mathf.Max(0f, _body != null ? _body.friction : 0f) *
                              Mathf.Max(0f, surface != null ? surface.friction : 0f)); // the solver combines friction as sqrt(a * b)
        float pressing = Mathf.Max(0f, Vector2.Dot(-weight, n)); // weight * cos(angle) - what friction acts on

        float total = push
                    - Vector2.Dot(weight, t) // gravity along the slope
                    - mu * (weight.magnitude - pressing);
        return t * Mathf.Max(0f, total);
    }

    /// If the obstacle at foot level is no taller than maxStepHeight, has a walkable top and has headroom above it, lift the body onto it.
    void TryStepUp(float dir)
    {
        if (maxStepHeight <= 0f || _body == null) return;

        const float skin = 0.02f;
        Bounds b = _body.bounds;
        float footY = b.min.y;
        float frontX = dir > 0f ? b.max.x : b.min.x;
        Vector2 forward = new Vector2(dir, 0f);

        float reach = stepProbeDistance + Mathf.Abs(_rb.linearVelocity.x) * Time.fixedDeltaTime;

        // 1. Something solid and steeper than a slope at foot level.
        Vector2 lowOrigin = new Vector2(frontX - dir * skin, footY + skin);
        if (!ClosestHit(lowOrigin, forward, reach + skin, out RaycastHit2D low)) return;
        if (IsWalkable(low.normal)) return;

        // 2. If still there at full step height -> resolved as a wall.
        Vector2 highOrigin = new Vector2(frontX - dir * skin, footY + maxStepHeight + skin);
        if (ClosestHit(highOrigin, forward, reach + skin, out _)) return;

        // 3. Find the top of the ledge just past its face.
        float gap = Mathf.Max(0f, (low.point.x - frontX) * dir);
        float nudge = gap + stepProbeDistance;
        Vector2 topOrigin = new Vector2(frontX + dir * nudge, footY + maxStepHeight + skin);
        if (!ClosestHit(topOrigin, Vector2.down, maxStepHeight + skin * 2f, out RaycastHit2D top)) return;
        if (!IsWalkable(top.normal)) return;

        float rise = top.point.y - footY;
        if (rise <= 0.001f || rise > maxStepHeight + skin) return;

        // 4. Calculate whether there is enough room for Nu's collider
        Vector2 shift = new Vector2(dir * nudge, rise + skin);
        Vector2 size = new Vector2(b.size.x - skin, b.size.y - skin);
        if (Physics2D.OverlapBox((Vector2)b.center + shift, size, 0f, _groundFilter, _overlap) > 0) return;

        _rb.position += shift;
    }

    /// Nearest ground-filtered ray hit. Triggers (water) are skipped.
    bool ClosestHit(Vector2 origin, Vector2 direction, float distance, out RaycastHit2D closest)
    {
        closest = default;
        int count = Physics2D.Raycast(origin, direction, _groundFilter, _hits, distance);
        float best = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            RaycastHit2D h = _hits[i];
            if (h.collider == null || _ignoredOneWay.Contains(h.collider)) continue;
            if (h.distance < best) { best = h.distance; closest = h; }
        }
        return closest.collider != null;
    }

    // ------------------------------------------------------------------ water

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<BuoyancyEffector2D>() != null)
        {
            bool wasDry = _waterOverlapCount == 0;
            _waterOverlapCount++;
            if (!wasDry) return;   // already counted via a different collider on the same or another water body

            // One-off entry impulse
            Vector2 v = _rb.linearVelocity;
            Vector2 target = new Vector2(v.x * entryKeepHorizontal, v.y * entryKeepVertical);
            _rb.AddForce((target - v) * _rb.mass, ForceMode2D.Impulse);

            WaterEntered?.Invoke();
        }

        if (other.CompareTag("Cracks"))
        {
            _climbOverlapCount++;
            CrackEntered?.Invoke();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<BuoyancyEffector2D>() != null)
        {
            _waterOverlapCount = Mathf.Max(0, _waterOverlapCount - 1);
            if (_waterOverlapCount == 0) WaterExited?.Invoke();
        }

        if (other.CompareTag("Cracks"))
        {
            _climbOverlapCount = Mathf.Max(0, _climbOverlapCount - 1);
            CrackExited?.Invoke();
        }
    }

    // ------------------------------------------------------------------ cracks

    void UpdateClimb()
    {
        bool touching = _climbOverlapCount > 0;

        if (!_climbing)
        {
            if (touching) StartClimb();
        }
        else if (!touching || _grounded)
        {
            StopClimb();   // fully out
        }
    }

    void StartClimb()
    {
        _climbing = true;
        oldGravityScale = _rb.gravityScale;
        _rb.gravityScale = 0f;
    }

    void StopClimb()
    {
        _climbing = false;
        _rb.gravityScale = oldGravityScale;   // velocity is kept, so leaving upward carries you out with a small bounce
    }

    void DriveClimb()
    {
        Vector2 input = new Vector2(_moveInput, _vertInput);
        input = Vector2.ClampMagnitude(input, 1f); // diagonals aren't faster

        _rb.linearVelocity = input * climbMaxSpeed;
    }

    // ------------------------------------------------------------------ editor aids

    void OnDrawGizmosSelected()
    {
        Collider2D c = GetComponent<Collider2D>();
        if (c == null) return;

        Bounds b = c.bounds;
        Gizmos.color = _grounded ? Color.green : Color.red;
        Gizmos.DrawLine(new Vector3(b.min.x, b.min.y), new Vector3(b.min.x, b.min.y - groundProbeDistance));
        Gizmos.DrawLine(new Vector3(b.max.x, b.min.y), new Vector3(b.max.x, b.min.y - groundProbeDistance));

        if (_waterOverlapCount > 0)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(b.center, b.size * 1.05f);
        }
    }
}
