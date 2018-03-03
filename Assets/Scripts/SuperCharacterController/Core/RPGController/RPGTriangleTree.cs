using System;
using System.Collections.Generic;
using UnityEngine;

public class RPGTriangleTree
{
    public readonly float Size = float.MinValue;
    public readonly int TriangleCount;
    public readonly Triangle[] Triangles;
    public readonly int VertexCount;
    public readonly Vector3[] Vertices;

    public Node Root;

    public RPGTriangleTree(MeshCollider mc)
    {
        var mesh = mc.sharedMesh;
        var tris = mesh.triangles;
        var verts = mesh.vertices;

        Vertices = verts;
        VertexCount = verts.Length;
        TriangleCount = tris.Length / 3;
        Triangles = new Triangle[TriangleCount];

        var size = mc.bounds.extents * 2f;
        Size = Mathf.Max(Size, Mathf.Ceil(size.x));
        Size = Mathf.Max(Size, Mathf.Ceil(size.y));
        Size = Mathf.Max(Size, Mathf.Ceil(size.z));

        Root.Init(Vector3.zero, new Vector3(Size, Size, Size));

        var t = new Triangle();
        var pts = new Vector3[3];

        var n = 0;

        for (var i = 0; i < tris.Length; ++i)
        {
            t = new Triangle();
            t.Index0 = tris[i];
            t.Index1 = tris[++i];
            t.Index2 = tris[++i];

            pts[0] = verts[t.Index0];
            pts[1] = verts[t.Index1];
            pts[2] = verts[t.Index2];

            FromPoints(pts, out t.Center, out t.Extents);

            Triangles[n++] = t;

            Node.Insert(ref Root, Triangles, n - 1);
        }
    }

    public void GetTrianglePoints(int n, out Vector3 p0, out Vector3 p1, out Vector3 p2)
    {
        var t = Triangles[n];
        p0 = Vertices[t.Index0];
        p1 = Vertices[t.Index1];
        p2 = Vertices[t.Index2];
    }

    public Vector3[] GetTrianglePoints(int n)
    {
        var ps = new Vector3[3];
        GetTrianglePoints(n, out ps[0], out ps[1], out ps[2]);
        return ps;
    }

    public static void FromPoints(Vector3[] points, out Vector3 center, out Vector3 extents)
    {
        var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        for (var i = 0; i < points.Length; ++i)
        {
            min = Vector3.Min(min, points[i]);
            max = Vector3.Max(max, points[i]);
        }

        center = 0.5f * (min + max);
        extents = 0.5f * (max - min);
    }

    public void DrawGizmos()
    {
        DrawGizmos(ref Root);
    }

    public void FindClosestNodes(Vector3 p, float r, List<Node> result)
    {
        Node.FindClosestNodes(ref Root, ref p, r, result);
    }

    public void FindClosestTriangles(Vector3 p, float r, List<int> result)
    {
        Node.FindClosestTriangles(ref Root, ref p, r, result);
    }

    private void DrawGizmos(ref Node node)
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(node.Center, node.Extents * 2f);

        for (var i = 0; i < node.Triangles.Length; ++i)
        {
            var t = Triangles[node.Triangles[i]];

            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vertices[t.Index0], Vertices[t.Index1]);
            Gizmos.DrawLine(Vertices[t.Index1], Vertices[t.Index2]);
            Gizmos.DrawLine(Vertices[t.Index2], Vertices[t.Index0]);
        }

        if (node.Children != null)
            for (var i = 0; i < 8; ++i)
                DrawGizmos(ref node.Children[i]);
    }

    public struct Triangle
    {
        public int Index0;
        public int Index1;
        public int Index2;

        public Vector3 Center;
        public Vector3 Extents;

        public Vector3 Min
        {
            get { return Center - Extents; }
        }

        public Vector3 Max
        {
            get { return Center + Extents; }
        }
    }

    public struct Node
    {
        public const int LEFT_TOP_FRONT = 0;
        public const int RIGHT_TOP_FRONT = 1;
        public const int LEFT_TOP_BACK = 2;
        public const int RIGHT_TOP_BACK = 3;
        public const int LEFT_BOTTOM_FRONT = 4;
        public const int RIGHT_BOTTOM_FRONT = 5;
        public const int LEFT_BOTTOM_BACK = 6;
        public const int RIGHT_BOTTOM_BACK = 7;

        public Vector3 Center;
        public Vector3 Extents;
        public Node[] Children;
        public int[] Triangles;

        public Vector3 Min
        {
            get { return Center - Extents; }
        }

        public Vector3 Max
        {
            get { return Center + Extents; }
        }

        public void Init(Vector3 center, Vector3 extents)
        {
            Center = center;
            Extents = extents;
            Triangles = new int[0];
        }

        public static void Split(ref Node node)
        {
            var x2 = node.Extents.x / 2;
            var y2 = node.Extents.y / 2;
            var z2 = node.Extents.z / 2;

            var extents = new Vector3(x2, y2, z2);
            var right = node.Center.x + x2;
            var left = node.Center.x - x2;
            var top = node.Center.y + y2;
            var bottom = node.Center.y - y2;
            var front = node.Center.z + z2;
            var back = node.Center.z - z2;

            node.Children = new Node[8];
            node.Children[LEFT_TOP_FRONT].Init(new Vector3(left, top, front), extents);
            node.Children[RIGHT_TOP_FRONT].Init(new Vector3(right, top, front), extents);
            node.Children[LEFT_TOP_BACK].Init(new Vector3(left, top, back), extents);
            node.Children[RIGHT_TOP_BACK].Init(new Vector3(right, top, back), extents);
            node.Children[LEFT_BOTTOM_FRONT].Init(new Vector3(left, bottom, front), extents);
            node.Children[RIGHT_BOTTOM_FRONT].Init(new Vector3(right, bottom, front), extents);
            node.Children[LEFT_BOTTOM_BACK].Init(new Vector3(left, bottom, back), extents);
            node.Children[RIGHT_BOTTOM_BACK].Init(new Vector3(right, bottom, back), extents);
        }

        public static void Insert(ref Node n, Triangle[] ts, int t)
        {
            if (n.Extents.x / 2f > 0.5f && n.Children == null)
                Split(ref n);

            if (IntersectsTriangle(ref n, ref ts[t]))
                if (n.Children == null)
                {
                    Array.Resize(ref n.Triangles, n.Triangles.Length + 1);
                    n.Triangles[n.Triangles.Length - 1] = t;
                }
                else
                {
                    for (var i = 0; i < 8; ++i)
                        Insert(ref n.Children[i], ts, t);
                }
        }

        public static void FindClosestNodes(ref Node n, ref Vector3 p, float r, List<Node> result)
        {
            if (IntersectsSphere(ref n, ref p, r))
            {
                if (n.Triangles.Length > 0)
                    result.Add(n);

                if (n.Children != null)
                    for (var i = 0; i < 8; ++i)
                        FindClosestNodes(ref n.Children[i], ref p, r, result);
            }
        }

        public static void FindClosestTriangles(ref Node n, ref Vector3 p, float r, List<int> result)
        {
            if (IntersectsSphere(ref n, ref p, r))
            {
                if (n.Triangles.Length > 0)
                    foreach (var triangle in n.Triangles)
                        if (!result.Contains(triangle))
                            result.Add(triangle);

                if (n.Children != null)
                    for (var i = 0; i < 8; ++i)
                        FindClosestTriangles(ref n.Children[i], ref p, r, result);
            }
        }

        public static bool IntersectsTriangle(ref Node n, ref Triangle t)
        {
            var nMin = n.Min;
            var nMax = n.Max;

            var tMin = t.Min;
            var tMax = t.Max;

            if (nMin.x > tMax.x || tMin.x > nMax.x)
                return false;

            if (nMin.y > tMax.y || tMin.y > nMax.y)
                return false;

            if (nMin.z > tMax.z || tMin.z > nMax.z)
                return false;

            return true;
        }

        public static bool IntersectsSphere(ref Node node, ref Vector3 p, float radius)
        {
            var v = clampVector(p, node.Min, node.Max);
            return (p - v).sqrMagnitude <= radius * radius;
        }

        private static Vector3 clampVector(Vector3 value, Vector3 min, Vector3 max)
        {
            var x = value.x;
            x = x > max.x ? max.x : x;
            x = x < min.x ? min.x : x;

            var y = value.y;
            y = y > max.y ? max.y : y;
            y = y < min.y ? min.y : y;

            var z = value.z;
            z = z > max.z ? max.z : z;
            z = z < min.z ? min.z : z;

            return new Vector3(x, y, z);
        }
    }
}