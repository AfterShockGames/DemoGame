// External release version 2.0.0

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Custom character controller, to be used by attaching the component to an object
///     and writing scripts attached to the same object that recieve the "SuperUpdate" message
/// </summary>
public class SuperCharacterController : MonoBehaviour
{
    private const float Tolerance = 0.05f;
    private const float TinyTolerance = 0.01f;
    private const string TemporaryLayer = "TempCast";
    private const int MaxPushbackIterations = 2;

    private static SuperCollisionType defaultCollisionType;

    [SerializeField] private readonly CollisionSphere[] spheres =
        new CollisionSphere[3]
        {
            new CollisionSphere(0.5f, true, false),
            new CollisionSphere(1.0f, false, false),
            new CollisionSphere(1.5f, false, true)
        };

    private bool clamping = true;

    [SerializeField] private bool debugGrounding;

    [SerializeField] public Vector3 debugMove = Vector3.zero;

    [SerializeField] private bool debugPushbackMesssages;

    [SerializeField] private bool debugSpheres;

    private float fixedDeltaTime;

    [SerializeField] private bool fixedTimeStep;

    [SerializeField] private int fixedUpdatesPerSecond;

    private Vector3 groundOffset;

    private List<Collider> ignoredColliders;
    private List<IgnoredCollider> ignoredColliderStack;

    private Vector3 initialPosition;
    private Vector3 lastGroundPosition;

    [SerializeField] private Collider ownCollider;

    [SerializeField] public float radius = 0.5f;

    private bool slopeLimiting = true;

    private int TemporaryLayerIndex;

    public LayerMask Walkable;

    public float deltaTime { get; protected set; }
    public SuperGround currentGround { get; private set; }
    public CollisionSphere feet { get; private set; }
    public CollisionSphere head { get; private set; }

    /// <summary>
    ///     Total height of the controller from the bottom of the feet to the top of the head
    /// </summary>
    public float height
    {
        get { return Vector3.Distance(SpherePosition(head), SpherePosition(feet)) + radius * 2; }
    }

    public Vector3 up
    {
        get { return transform.up; }
    }

    public Vector3 down
    {
        get { return -transform.up; }
    }

    public List<SuperCollision> collisionData { get; private set; }
    public Transform currentlyClampedTo { get; set; }
    public float heightScale { get; set; }
    public float radiusScale { get; set; }

    private void Awake()
    {
        collisionData = new List<SuperCollision>();

        TemporaryLayerIndex = LayerMask.NameToLayer(TemporaryLayer);

        ignoredColliders = new List<Collider>();
        ignoredColliderStack = new List<IgnoredCollider>();

        currentlyClampedTo = null;

        fixedDeltaTime = 1.0f / fixedUpdatesPerSecond;

        heightScale = 1.0f;

        if (ownCollider)
            IgnoreCollider(ownCollider);

        foreach (var sphere in spheres)
        {
            if (sphere.isFeet)
                feet = sphere;

            if (sphere.isHead)
                head = sphere;
        }

        if (feet == null)
            Debug.LogError("[SuperCharacterController] Feet not found on controller");

        if (head == null)
            Debug.LogError("[SuperCharacterController] Head not found on controller");

        if (defaultCollisionType == null)
            defaultCollisionType =
                new GameObject("DefaultSuperCollisionType", typeof(SuperCollisionType)).GetComponent<SuperCollisionType>
                    ();

        currentGround = new SuperGround(Walkable, this);

        gameObject.SendMessage("SuperStart", SendMessageOptions.DontRequireReceiver);
    }

    private void Update()
    {
        Debug.Log("I am not called anymore!");
        // If we are using a fixed timestep, ensure we run the main update loop
        // a sufficient number of times based on the Time.deltaTime

        if (!fixedTimeStep)
        {
            deltaTime = Time.deltaTime;

            SingleUpdate();
        }
        else
        {
            var delta = Time.deltaTime;

            while (delta > fixedDeltaTime)
            {
                deltaTime = fixedDeltaTime;

                SingleUpdate();

                delta -= fixedDeltaTime;
            }

            if (delta > 0f)
            {
                deltaTime = delta;

                SingleUpdate();
            }
        }
    }

    protected void SingleUpdate()
    {
        // Check if we are clamped to an object implicity or explicity
        var isClamping = clamping || currentlyClampedTo != null;
        var clampedTo = currentlyClampedTo != null ? currentlyClampedTo : currentGround.transform;

        if (isClamping && clampedTo != null && clampedTo.position - lastGroundPosition != Vector3.zero)
            transform.position += clampedTo.position - lastGroundPosition;

        initialPosition = transform.position;

        ProbeGround(1);

        transform.position += debugMove * deltaTime;

        gameObject.SendMessage("SuperUpdate", SendMessageOptions.DontRequireReceiver);

        RecursivePushback(0, MaxPushbackIterations);

        ProbeGround(2);

        if (slopeLimiting)
            SlopeLimit();

        ProbeGround(3);

        if (clamping)
            ClampToGround();

        isClamping = clamping || currentlyClampedTo != null;
        clampedTo = currentlyClampedTo != null ? currentlyClampedTo : currentGround.transform;

        if (isClamping)
            lastGroundPosition = clampedTo.position;

        if (debugGrounding)
            currentGround.DebugGround(true, true, true, true, true);
    }

    private void ProbeGround(int iter)
    {
        PushIgnoredColliders();
        currentGround.ProbeGround(SpherePosition(feet), iter);
        PopIgnoredColliders();
    }

    /// <summary>
    ///     Prevents the player from walking up slopes of a larger angle than the object's SlopeLimit.
    /// </summary>
    /// <returns>True if the controller attemped to ascend a too steep slope and had their movement limited</returns>
    private bool SlopeLimit()
    {
        var n = currentGround.PrimaryNormal();
        var a = Vector3.Angle(n, up);

        if (a > currentGround.superCollisionType.SlopeLimit)
        {
            var absoluteMoveDirection = Math3d.ProjectVectorOnPlane(n, transform.position - initialPosition);

            // Retrieve a vector pointing down the slope
            var r = Vector3.Cross(n, down);
            var v = Vector3.Cross(r, n);

            var angle = Vector3.Angle(absoluteMoveDirection, v);

            if (angle <= 90.0f)
                return false;

            // Calculate where to place the controller on the slope, or at the bottom, based on the desired movement distance
            var resolvedPosition = Math3d.ProjectPointOnLine(initialPosition, r, transform.position);
            var direction = Math3d.ProjectVectorOnPlane(n, resolvedPosition - transform.position);

            RaycastHit hit;

            // Check if our path to our resolved position is blocked by any colliders
            if (Physics.CapsuleCast(SpherePosition(feet), SpherePosition(head), radius, direction.normalized, out hit,
                direction.magnitude, Walkable))
                transform.position += v.normalized * hit.distance;
            else
                transform.position += direction;

            return true;
        }

        return false;
    }

    private void ClampToGround()
    {
        var d = currentGround.Distance();
        transform.position -= up * d;
    }

    public void EnableClamping()
    {
        clamping = true;
    }

    public void DisableClamping()
    {
        clamping = false;
    }

    public void EnableSlopeLimit()
    {
        slopeLimiting = true;
    }

    public void DisableSlopeLimit()
    {
        slopeLimiting = false;
    }

    public bool IsClamping()
    {
        return clamping;
    }

    /// <summary>
    ///     Provides raycast data based on where a SphereCast would contact the specified normal
    ///     Raycasting downwards from a point along the controller's bottom sphere, based on the provided
    ///     normal
    /// </summary>
    /// <param name="groundNormal">Normal of a triangle assumed to be directly below the controller</param>
    /// <param name="hit">Simulated SphereCast data</param>
    /// <returns>True if the raycast is successful</returns>
    private bool SimulateSphereCast(Vector3 groundNormal, out RaycastHit hit)
    {
        var groundAngle = Vector3.Angle(groundNormal, up) * Mathf.Deg2Rad;

        var secondaryOrigin = transform.position + up * Tolerance;

        if (!Mathf.Approximately(groundAngle, 0))
        {
            var horizontal = Mathf.Sin(groundAngle) * radius;
            var vertical = (1.0f - Mathf.Cos(groundAngle)) * radius;

            // Retrieve a vector pointing up the slope
            var r2 = Vector3.Cross(groundNormal, down);
            var v2 = -Vector3.Cross(r2, groundNormal);

            secondaryOrigin += Math3d.ProjectVectorOnPlane(up, v2).normalized * horizontal + up * vertical;
        }

        if (Physics.Raycast(secondaryOrigin, down, out hit, Mathf.Infinity, Walkable))
        {
            // Remove the tolerance from the distance travelled
            hit.distance -= Tolerance;

            return true;
        }
        return false;
    }

    /// <summary>
    ///     Check if any of the CollisionSpheres are colliding with any walkable objects in the world.
    ///     If they are, apply a proper pushback and retrieve the collision data
    /// </summary>
    private void RecursivePushback(int depth, int maxDepth)
    {
        PushIgnoredColliders();

        collisionData.Clear();

        var contact = false;

        foreach (var sphere in spheres)
        foreach (var col in Physics.OverlapSphere(SpherePosition(sphere), radius, Walkable))
        {
            if (col.isTrigger)
                continue;

            var position = SpherePosition(sphere);
            var contactPoint = SuperCollider.ClosestPointOnSurface(col, position, radius);

            if (contactPoint != Vector3.zero)
            {
                if (debugPushbackMesssages)
                    DebugDraw.DrawMarker(contactPoint, 2.0f, Color.cyan, 0.0f, false);

                var v = contactPoint - position;

                if (v != Vector3.zero)
                {
                    // Cache the collider's layer so that we can cast against it
                    var layer = col.gameObject.layer;

                    col.gameObject.layer = TemporaryLayerIndex;

                    // Check which side of the normal we are on
                    var facingNormal = Physics.SphereCast(new Ray(position, v.normalized), TinyTolerance,
                        v.magnitude + TinyTolerance, 1 << TemporaryLayerIndex);

                    col.gameObject.layer = layer;

                    // Orient and scale our vector based on which side of the normal we are situated
                    if (facingNormal)
                        if (Vector3.Distance(position, contactPoint) < radius)
                            v = v.normalized * (radius - v.magnitude) * -1;
                        else
                            continue;
                    else
                        v = v.normalized * (radius + v.magnitude);

                    contact = true;

                    transform.position += v;

                    col.gameObject.layer = TemporaryLayerIndex;

                    // Retrieve the surface normal of the collided point
                    RaycastHit normalHit;

                    Physics.SphereCast(new Ray(position + v, contactPoint - (position + v)), TinyTolerance,
                        out normalHit, 1 << TemporaryLayerIndex);

                    col.gameObject.layer = layer;

                    var superColType = col.gameObject.GetComponent<SuperCollisionType>();

                    if (superColType == null)
                        superColType = defaultCollisionType;

                    // Our collision affected the collider; add it to the collision data
                    var collision = new SuperCollision
                    {
                        collisionSphere = sphere,
                        superCollisionType = superColType,
                        gameObject = col.gameObject,
                        point = contactPoint,
                        normal = normalHit.normal
                    };

                    collisionData.Add(collision);
                }
            }
        }

        PopIgnoredColliders();

        if (depth < maxDepth && contact)
            RecursivePushback(depth + 1, maxDepth);
    }

    private void PushIgnoredColliders()
    {
        ignoredColliderStack.Clear();

        for (var i = 0; i < ignoredColliders.Count; i++)
        {
            var col = ignoredColliders[i];
            ignoredColliderStack.Add(new IgnoredCollider(col, col.gameObject.layer));
            col.gameObject.layer = TemporaryLayerIndex;
        }
    }

    private void PopIgnoredColliders()
    {
        for (var i = 0; i < ignoredColliderStack.Count; i++)
        {
            var ic = ignoredColliderStack[i];
            ic.collider.gameObject.layer = ic.layer;
        }

        ignoredColliderStack.Clear();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (debugSpheres)
            if (spheres != null)
            {
                if (heightScale == 0) heightScale = 1;

                foreach (var sphere in spheres)
                {
                    Gizmos.color = sphere.isFeet ? Color.green : (sphere.isHead ? Color.yellow : Color.cyan);
                    Gizmos.DrawWireSphere(SpherePosition(sphere), radius);
                }
            }
    }

    public Vector3 SpherePosition(CollisionSphere sphere)
    {
        if (sphere.isFeet)
            return transform.position + sphere.offset * up;
        return transform.position + sphere.offset * up * heightScale;
    }

    public bool PointBelowHead(Vector3 point)
    {
        return Vector3.Angle(point - SpherePosition(head), up) > 89.0f;
    }

    public bool PointAboveFeet(Vector3 point)
    {
        return Vector3.Angle(point - SpherePosition(feet), down) > 89.0f;
    }

    public void IgnoreCollider(Collider col)
    {
        ignoredColliders.Add(col);
    }

    public void RemoveIgnoredCollider(Collider col)
    {
        ignoredColliders.Remove(col);
    }

    public void ClearIgnoredColliders()
    {
        ignoredColliders.Clear();
    }

    /// <summary>
    ///     Describes the Transform of the object we are standing on as well as it's CollisionType, as well
    ///     as how far the ground is below us and what angle it is in relation to the controller.
    /// </summary>
    [SerializeField]
    public struct Ground
    {
        public RaycastHit hit { get; set; }
        public RaycastHit nearHit { get; set; }
        public RaycastHit farHit { get; set; }
        public RaycastHit secondaryHit { get; set; }
        public SuperCollisionType collisionType { get; set; }
        public Transform transform { get; set; }

        public Ground(RaycastHit hit, RaycastHit nearHit, RaycastHit farHit, RaycastHit secondaryHit,
            SuperCollisionType superCollisionType, Transform hitTransform)
        {
            this.hit = hit;
            this.nearHit = nearHit;
            this.farHit = farHit;
            this.secondaryHit = secondaryHit;
            collisionType = superCollisionType;
            transform = hitTransform;
        }
    }

    protected struct IgnoredCollider
    {
        public Collider collider;
        public int layer;

        public IgnoredCollider(Collider collider, int layer)
        {
            this.collider = collider;
            this.layer = layer;
        }
    }

    public class SuperGround
    {
        private const float groundingUpperBoundAngle = 60.0f;
        private const float groundingMaxPercentFromCenter = 0.85f;
        private const float groundingMinPercentFromcenter = 0.50f;
        private readonly SuperCharacterController controller;

        private readonly LayerMask walkable;
        private GroundHit farGround;
        private GroundHit flushGround;
        private GroundHit nearGround;

        private GroundHit primaryGround;
        private GroundHit stepGround;

        public SuperGround(LayerMask walkable, SuperCharacterController controller)
        {
            this.walkable = walkable;
            this.controller = controller;
        }

        public SuperCollisionType superCollisionType { get; private set; }
        public Transform transform { get; private set; }

        /// <summary>
        ///     Scan the surface below us for ground. Follow up the initial scan with subsequent scans
        ///     designed to test what kind of surface we are standing above and handle different edge cases
        /// </summary>
        /// <param name="origin">Center of the sphere for the initial SphereCast</param>
        /// <param name="iter">
        ///     Debug tool to print out which ProbeGround iteration is being run (3 are run each frame for the
        ///     controller)
        /// </param>
        public void ProbeGround(Vector3 origin, int iter)
        {
            ResetGrounds();

            var up = controller.up;
            var down = -up;

            var o = origin + up * Tolerance;

            // Reduce our radius by Tolerance squared to avoid failing the SphereCast due to clipping with walls
            var smallerRadius = controller.radius - Tolerance * Tolerance;

            RaycastHit hit;

            if (Physics.SphereCast(o, smallerRadius, down, out hit, Mathf.Infinity, walkable))
            {
                var superColType = hit.collider.gameObject.GetComponent<SuperCollisionType>();

                if (superColType == null)
                    superColType = defaultCollisionType;

                superCollisionType = superColType;
                transform = hit.transform;

                // By reducing the initial SphereCast's radius by Tolerance, our casted sphere no longer fits with
                // our controller's shape. Reconstruct the sphere cast with the proper radius
                SimulateSphereCast(hit.normal, out hit);

                primaryGround = new GroundHit(hit.point, hit.normal, hit.distance);

                // If we are standing on a perfectly flat surface, we cannot be either on an edge,
                // On a slope or stepping off a ledge
                if (
                    Vector3.Distance(
                        Math3d.ProjectPointOnPlane(controller.up, controller.transform.position, hit.point),
                        controller.transform.position) < TinyTolerance)
                    return;

                // As we are standing on an edge, we need to retrieve the normals of the two
                // faces on either side of the edge and store them in nearHit and farHit

                var toCenter = Math3d.ProjectVectorOnPlane(up,
                    (controller.transform.position - hit.point).normalized * TinyTolerance);

                var awayFromCenter = Quaternion.AngleAxis(-80.0f, Vector3.Cross(toCenter, up)) * -toCenter;

                var nearPoint = hit.point + toCenter + up * TinyTolerance;
                var farPoint = hit.point + awayFromCenter * 3;

                RaycastHit nearHit;
                RaycastHit farHit;

                Physics.Raycast(nearPoint, down, out nearHit, Mathf.Infinity, walkable);
                Physics.Raycast(farPoint, down, out farHit, Mathf.Infinity, walkable);

                nearGround = new GroundHit(nearHit.point, nearHit.normal, nearHit.distance);
                farGround = new GroundHit(farHit.point, farHit.normal, farHit.distance);

                // If we are currently standing on ground that should be counted as a wall,
                // we are likely flush against it on the ground. Retrieve what we are standing on
                if (Vector3.Angle(hit.normal, up) > superColType.StandAngle)
                {
                    // Retrieve a vector pointing down the slope
                    var r = Vector3.Cross(hit.normal, down);
                    var v = Vector3.Cross(r, hit.normal);

                    var flushOrigin = hit.point + hit.normal * TinyTolerance;

                    RaycastHit flushHit;

                    if (Physics.Raycast(flushOrigin, v, out flushHit, Mathf.Infinity, walkable))
                    {
                        RaycastHit sphereCastHit;

                        if (SimulateSphereCast(flushHit.normal, out sphereCastHit))
                            flushGround = new GroundHit(sphereCastHit.point, sphereCastHit.normal,
                                sphereCastHit.distance);
                    }
                }

                // If we are currently standing on a ledge then the face nearest the center of the
                // controller should be steep enough to be counted as a wall. Retrieve the ground
                // it is connected to at it's base, if there exists any
                if (Vector3.Angle(nearHit.normal, up) > superColType.StandAngle || nearHit.distance > Tolerance)
                {
                    var col = nearHit.collider.gameObject.GetComponent<SuperCollisionType>();

                    if (col == null)
                        col = defaultCollisionType;

                    // We contacted the wall of the ledge, rather than the landing. Raycast down
                    // the wall to retrieve the proper landing
                    if (Vector3.Angle(nearHit.normal, up) > superColType.StandAngle)
                    {
                        // Retrieve a vector pointing down the slope
                        var r = Vector3.Cross(nearHit.normal, down);
                        var v = Vector3.Cross(r, nearHit.normal);

                        RaycastHit stepHit;

                        if (Physics.Raycast(nearPoint, v, out stepHit, Mathf.Infinity, walkable))
                            stepGround = new GroundHit(stepHit.point, stepHit.normal, stepHit.distance);
                    }
                    else
                    {
                        stepGround = new GroundHit(nearHit.point, nearHit.normal, nearHit.distance);
                    }
                }
            }
            // If the initial SphereCast fails, likely due to the controller clipping a wall,
            // fallback to a raycast simulated to SphereCast data
            else if (Physics.Raycast(o, down, out hit, Mathf.Infinity, walkable))
            {
                var superColType = hit.collider.gameObject.GetComponent<SuperCollisionType>();

                if (superColType == null)
                    superColType = defaultCollisionType;

                superCollisionType = superColType;
                transform = hit.transform;

                RaycastHit sphereCastHit;

                if (SimulateSphereCast(hit.normal, out sphereCastHit))
                    primaryGround = new GroundHit(sphereCastHit.point, sphereCastHit.normal, sphereCastHit.distance);
                else
                    primaryGround = new GroundHit(hit.point, hit.normal, hit.distance);
            }
            else
            {
                Debug.LogError(
                    "[SuperCharacterComponent]: No ground was found below the player; player has escaped level");
            }
        }

        private void ResetGrounds()
        {
            primaryGround = null;
            nearGround = null;
            farGround = null;
            flushGround = null;
            stepGround = null;
        }

        public bool IsGrounded(bool currentlyGrounded, float distance)
        {
            Vector3 n;
            return IsGrounded(currentlyGrounded, distance, out n);
        }

        public bool IsGrounded(bool currentlyGrounded, float distance, out Vector3 groundNormal)
        {
            groundNormal = Vector3.zero;

            if (primaryGround == null || primaryGround.distance > distance)
                return false;

            // Check if we are flush against a wall
            if (farGround != null && Vector3.Angle(farGround.normal, controller.up) > superCollisionType.StandAngle)
            {
                if (flushGround != null &&
                    Vector3.Angle(flushGround.normal, controller.up) < superCollisionType.StandAngle &&
                    flushGround.distance < distance)
                {
                    groundNormal = flushGround.normal;
                    return true;
                }

                return false;
            }

            // Check if we are at the edge of a ledge, or on a high angle slope
            if (farGround != null && !OnSteadyGround(farGround.normal, primaryGround.point))
            {
                // Check if we are walking onto steadier ground
                if (nearGround != null && nearGround.distance < distance &&
                    Vector3.Angle(nearGround.normal, controller.up) < superCollisionType.StandAngle &&
                    !OnSteadyGround(nearGround.normal, nearGround.point))
                {
                    groundNormal = nearGround.normal;
                    return true;
                }

                // Check if we are on a step or stair
                if (stepGround != null && stepGround.distance < distance &&
                    Vector3.Angle(stepGround.normal, controller.up) < superCollisionType.StandAngle)
                {
                    groundNormal = stepGround.normal;
                    return true;
                }

                return false;
            }


            if (farGround != null)
                groundNormal = farGround.normal;
            else
                groundNormal = primaryGround.normal;

            return true;
        }

        /// <summary>
        ///     To help the controller smoothly "fall" off surfaces and not hang on the edge of ledges,
        ///     check that the ground below us is "steady", or that the controller is not standing
        ///     on too extreme of a ledge
        /// </summary>
        /// <param name="normal">Normal of the surface to test against</param>
        /// <param name="point">Point of contact with the surface</param>
        /// <returns>True if the ground is steady</returns>
        private bool OnSteadyGround(Vector3 normal, Vector3 point)
        {
            var angle = Vector3.Angle(normal, controller.up);

            var angleRatio = angle / groundingUpperBoundAngle;

            var distanceRatio = Mathf.Lerp(groundingMinPercentFromcenter, groundingMaxPercentFromCenter, angleRatio);

            var p = Math3d.ProjectPointOnPlane(controller.up, controller.transform.position, point);

            var distanceFromCenter = Vector3.Distance(p, controller.transform.position);

            return distanceFromCenter <= distanceRatio * controller.radius;
        }

        public Vector3 PrimaryNormal()
        {
            return primaryGround.normal;
        }

        public Vector3 Normal(bool isGrounded, float distance)
        {
            Vector3 n;
            IsGrounded(isGrounded, distance, out n);
            return n;
        }

        public float HitDistance()
        {
            return primaryGround.distance;
        }

        public float Distance()
        {
            return primaryGround.distance;
        }

        public void DebugGround(bool primary, bool near, bool far, bool flush, bool step)
        {
            if (primary && primaryGround != null)
                DebugDraw.DrawVector(primaryGround.point, primaryGround.normal, 2.0f, 1.0f, Color.yellow, 0, false);

            if (near && nearGround != null)
                DebugDraw.DrawVector(nearGround.point, nearGround.normal, 2.0f, 1.0f, Color.blue, 0, false);

            if (far && farGround != null)
                DebugDraw.DrawVector(farGround.point, farGround.normal, 2.0f, 1.0f, Color.red, 0, false);

            if (flush && flushGround != null)
                DebugDraw.DrawVector(flushGround.point, flushGround.normal, 2.0f, 1.0f, Color.cyan, 0, false);

            if (step && stepGround != null)
                DebugDraw.DrawVector(stepGround.point, stepGround.normal, 2.0f, 1.0f, Color.green, 0, false);
        }

        private bool SimulateSphereCast(Vector3 groundNormal, out RaycastHit hit)
        {
            var groundAngle = Vector3.Angle(groundNormal, controller.up) * Mathf.Deg2Rad;

            var secondaryOrigin = controller.transform.position + controller.up * Tolerance;

            if (!Mathf.Approximately(groundAngle, 0))
            {
                var horizontal = Mathf.Sin(groundAngle) * controller.radius;
                var vertical = (1.0f - Mathf.Cos(groundAngle)) * controller.radius;

                // Retrieve a vector pointing up the slope
                var r2 = Vector3.Cross(groundNormal, controller.down);
                var v2 = -Vector3.Cross(r2, groundNormal);

                secondaryOrigin += Math3d.ProjectVectorOnPlane(controller.up, v2).normalized * horizontal +
                                   controller.up * vertical;
            }

            if (Physics.Raycast(secondaryOrigin, controller.down, out hit, Mathf.Infinity, walkable))
            {
                // Remove the tolerance from the distance travelled
                hit.distance -= Tolerance;

                return true;
            }
            return false;
        }

        private class GroundHit
        {
            public GroundHit(Vector3 point, Vector3 normal, float distance)
            {
                this.point = point;
                this.normal = normal;
                this.distance = distance;
            }

            public Vector3 point { get; private set; }
            public Vector3 normal { get; private set; }
            public float distance { get; private set; }
        }
    }
}

[Serializable]
public class CollisionSphere
{
    public bool isFeet;
    public bool isHead;
    public float offset;

    public CollisionSphere(float offset, bool isFeet, bool isHead)
    {
        this.offset = offset;
        this.isFeet = isFeet;
        this.isHead = isHead;
    }
}

public struct SuperCollision
{
    public CollisionSphere collisionSphere;
    public SuperCollisionType superCollisionType;
    public GameObject gameObject;
    public Vector3 point;
    public Vector3 normal;
}