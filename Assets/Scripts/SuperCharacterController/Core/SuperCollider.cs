﻿using UnityEngine;

public static class SuperCollider
{
    public static Vector3 ClosestPointOnSurface(Collider collider, Vector3 to, float radius)
    {
        if (collider is BoxCollider)
            return ClosestPointOnSurface((BoxCollider) collider, to);
        if (collider is SphereCollider)
            return ClosestPointOnSurface((SphereCollider) collider, to);
        if (collider is CapsuleCollider)
            return ClosestPointOnSurface((CapsuleCollider) collider, to);
        if (collider is MeshCollider)
        {
            var bsp = collider.GetComponent<BSPTree>();

            if (bsp != null)
                return bsp.ClosestPointOn(to, radius);

            var bfm = collider.GetComponent<BruteForceMesh>();

            if (bfm != null)
                return bfm.ClosestPointOn(to);
        }

        return Vector3.zero;
    }

    public static Vector3 ClosestPointOnSurface(SphereCollider collider, Vector3 to)
    {
        Vector3 p;

        p = to - collider.transform.position;
        p.Normalize();

        p *= collider.radius * collider.transform.localScale.x;
        p += collider.transform.position;

        return p;
    }

    public static Vector3 ClosestPointOnSurface(BoxCollider collider, Vector3 to)
    {
        // Cache the collider transform
        var ct = collider.transform;

        // Firstly, transform the point into the space of the collider
        var local = ct.InverseTransformPoint(to);

        // Now, shift it to be in the center of the box
        local -= collider.center;

        //Pre multiply to save operations.
        var halfSize = collider.size * 0.5f;

        // Clamp the points to the collider's extents
        var localNorm = new Vector3(
            Mathf.Clamp(local.x, -halfSize.x, halfSize.x),
            Mathf.Clamp(local.y, -halfSize.y, halfSize.y),
            Mathf.Clamp(local.z, -halfSize.z, halfSize.z)
        );

        //Calculate distances from each edge
        var dx = Mathf.Min(Mathf.Abs(halfSize.x - localNorm.x), Mathf.Abs(-halfSize.x - localNorm.x));
        var dy = Mathf.Min(Mathf.Abs(halfSize.y - localNorm.y), Mathf.Abs(-halfSize.y - localNorm.y));
        var dz = Mathf.Min(Mathf.Abs(halfSize.z - localNorm.z), Mathf.Abs(-halfSize.z - localNorm.z));

        // Select a face to project on
        if (dx < dy && dx < dz)
            localNorm.x = Mathf.Sign(localNorm.x) * halfSize.x;
        else if (dy < dx && dy < dz)
            localNorm.y = Mathf.Sign(localNorm.y) * halfSize.y;
        else if (dz < dx && dz < dy)
            localNorm.z = Mathf.Sign(localNorm.z) * halfSize.z;

        // Now we undo our transformations
        localNorm += collider.center;

        // Return resulting point
        return ct.TransformPoint(localNorm);
    }

    // Courtesy of Moodie
    public static Vector3 ClosestPointOnSurface(CapsuleCollider collider, Vector3 to)
    {
        var ct = collider.transform; // Transform of the collider

        var lineLength = collider.height - collider.radius * 2;
        // The length of the line connecting the center of both sphere
        var dir = Vector3.up;

        var upperSphere = dir * lineLength * 0.5f + collider.center;
        // The position of the radius of the upper sphere in local coordinates
        var lowerSphere = -dir * lineLength * 0.5f + collider.center;
        // The position of the radius of the lower sphere in local coordinates

        var local = ct.InverseTransformPoint(to); // The position of the controller in local coordinates

        var p = Vector3.zero; // Contact point
        var pt = Vector3.zero;
        // The point we need to use to get a direction vector with the controller to calculate contact point

        if (local.y < lineLength * 0.5f && local.y > -lineLength * 0.5f)
            // Controller is contacting with cylinder, not spheres
            pt = dir * local.y + collider.center;
        else if (local.y > lineLength * 0.5f) // Controller is contacting with the upper sphere 
            pt = upperSphere;
        else if (local.y < -lineLength * 0.5f) // Controller is contacting with lower sphere
            pt = lowerSphere;

        //Calculate contact point in local coordinates and return it in world coordinates
        p = local - pt;
        p.Normalize();
        p = p * collider.radius + pt;
        return ct.TransformPoint(p);
    }
}