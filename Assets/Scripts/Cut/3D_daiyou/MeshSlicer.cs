using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshSlicer : MonoBehaviour
{
    public static (GameObject, GameObject) Slice(Transform targetObject, Vector3 slicePosition, Vector3 sliceUp, Material slicedMaterial)
    {
        var meshFilter = targetObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter not found.");
            return (null, null);
        }

        var meshRenderer = targetObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("MeshRenderer not found.");
            return (null, null);
        }
        var mesh = meshFilter.sharedMesh;
        Plane plane = new Plane(sliceUp, slicePosition);
        var originalVerts = mesh.vertices;
        var originalUVs = mesh.uv;
        var upperVerts = new List<Vector3>();
        var upperTris = new List<int>();
        var upperUVs = new List<Vector2>();
        var lowerVerts = new List<Vector3>();
        var lowerTris = new List<int>();
        var lowerUVs = new List<Vector2>();
        var cutEdges = new List<(Vector3 pos, Vector2 uv)>();
        var capVertsTop = new List<Vector3>();
        var capTrisTop = new List<int>();
        var capUVsTop = new List<Vector2>();
        var capVertsBottom = new List<Vector3>();
        var capTrisBottom = new List<int>();
        var capUVsBottom = new List<Vector2>();
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            int i0 = mesh.triangles[i];
            int i1 = mesh.triangles[i + 1];
            int i2 = mesh.triangles[i + 2];
            Vector3[] v = new Vector3[] {
                targetObject.TransformPoint(originalVerts[i0]),
                targetObject.TransformPoint(originalVerts[i1]),
                targetObject.TransformPoint(originalVerts[i2])
            };
            Vector2[] uv = new Vector2[] {
                originalUVs[i0],
                originalUVs[i1],
                originalUVs[i2]
            };
            bool[] side = new bool[] {
                plane.GetSide(v[0]),
                plane.GetSide(v[1]),
                plane.GetSide(v[2])
            };
            int sideCount = side.Count(s => s);
            if (sideCount == 3)
                AddTriangle(upperVerts, upperTris, upperUVs, v[0], v[1], v[2], uv[0], uv[1], uv[2]);
            else if (sideCount == 0)
                AddTriangle(lowerVerts, lowerTris, lowerUVs, v[0], v[1], v[2], uv[0], uv[1], uv[2]);
            else
                SliceTriangleStable(plane, v, uv, side, upperVerts, upperTris, upperUVs, lowerVerts, lowerTris, lowerUVs, cutEdges);
        }
        // 上面（正順）
        FillCutFace(cutEdges, -plane.normal, capVertsTop, capTrisTop, capUVsTop);
        // 下面（逆順）
        FillCutFace(cutEdges, plane.normal, capVertsBottom, capTrisBottom, capUVsBottom);
        var baseMaterial = meshRenderer.sharedMaterial;
        var upperObject =
            CreateMesh("UpperHull", upperVerts, upperTris, upperUVs, capVertsTop, capTrisTop, capUVsTop, baseMaterial, slicedMaterial);

        var lowerObject =
            CreateMesh("LowerHull", lowerVerts, lowerTris, lowerUVs, capVertsBottom, capTrisBottom, capUVsBottom, baseMaterial, slicedMaterial);
        Destroy(targetObject.gameObject);
        return (upperObject, lowerObject);
    }
    static void SliceTriangleStable(Plane plane, Vector3[] v, Vector2[] uv, bool[] side,
        List<Vector3> upperVerts, List<int> upperTris, List<Vector2> upperUVs,
        List<Vector3> lowerVerts, List<int> lowerTris, List<Vector2> lowerUVs,
        List<(Vector3, Vector2)> cutEdges)
    {
        List<(Vector3, Vector2)> upper = new List<(Vector3, Vector2)>();
        List<(Vector3, Vector2)> lower = new List<(Vector3, Vector2)>();
        (Vector3, Vector2)[] intersections = new (Vector3, Vector2)[2];
        int interCount = 0;
        for (int i = 0; i < 3; i++)
        {
            int next = (i + 1) % 3;
            if (side[i]) upper.Add((v[i], uv[i]));
            else lower.Add((v[i], uv[i]));
            if (side[i] != side[next])
            {
                Vector3 pos = GetIntersection(plane, v[i], v[next]);
                float t = (pos - v[i]).magnitude / (v[next] - v[i]).magnitude;
                Vector2 interpolatedUV = Vector2.Lerp(uv[i], uv[next], t);
                var pair = (pos, interpolatedUV);
                upper.Add(pair);
                lower.Add(pair);
                intersections[interCount++] = pair;
            }
        }
        cutEdges.AddRange(intersections);
        if (upper.Count == 3)
            AddTriangle(upperVerts, upperTris, upperUVs, upper[0], upper[1], upper[2]);
        else if (upper.Count == 4)
        {
            AddTriangle(upperVerts, upperTris, upperUVs, upper[0], upper[1], upper[2]);
            AddTriangle(upperVerts, upperTris, upperUVs, upper[0], upper[2], upper[3]);
        }
        if (lower.Count == 3)
            AddTriangle(lowerVerts, lowerTris, lowerUVs, lower[0], lower[1], lower[2]);
        else if (lower.Count == 4)
        {
            AddTriangle(lowerVerts, lowerTris, lowerUVs, lower[0], lower[1], lower[2]);
            AddTriangle(lowerVerts, lowerTris, lowerUVs, lower[0], lower[2], lower[3]);
        }
    }
    private static void FillCutFace(List<(Vector3 pos, Vector2 uv)> cutEdges, Vector3 normal,
        List<Vector3> capVerts, List<int> capTris, List<Vector2> capUVs)
    {
        Vector3 center = cutEdges.Aggregate(Vector3.zero, (c, p) => c + p.pos) / cutEdges.Count;
        Vector2 centerUV = new Vector2(0.5f, 0.5f);
        Vector3 axisX = Vector3.Cross(normal, Vector3.up).normalized;
        if (axisX == Vector3.zero)
            axisX = Vector3.Cross(normal, Vector3.forward).normalized;
        var ordered = cutEdges.OrderBy(p =>
            Mathf.Atan2(Vector3.Dot(Vector3.Cross(axisX, p.pos - center), normal),
                        Vector3.Dot(axisX, p.pos - center))
        ).ToList();
        int centerIndex = capVerts.Count;
        capVerts.Add(center);
        capUVs.Add(centerUV);
        for (int i = 0; i < ordered.Count; i++)
        {
            var a = ordered[i];
            var b = ordered[(i + 1) % ordered.Count];
            int aIdx = capVerts.Count;
            int bIdx = capVerts.Count + 1;
            capVerts.Add(a.pos);
            capVerts.Add(b.pos);
            capUVs.Add(new Vector2(0.5f + (a.pos - center).x * 0.5f, 0.5f + (a.pos - center).z * 0.5f));
            capUVs.Add(new Vector2(0.5f + (b.pos - center).x * 0.5f, 0.5f + (b.pos - center).z * 0.5f));
            capTris.Add(centerIndex);
            capTris.Add(aIdx);
            capTris.Add(bIdx);
        }
    }
    private static Vector3 GetIntersection(Plane plane, Vector3 a, Vector3 b)
    {
        Ray ray = new Ray(a, b - a);
        plane.Raycast(ray, out float enter);
        return ray.GetPoint(enter);
    }
    private static void AddTriangle(List<Vector3> verts, List<int> tris, List<Vector2> uvs,
        Vector3 a, Vector3 b, Vector3 c, Vector2 uva, Vector2 uvb, Vector2 uvc)
    {
        int idx = verts.Count;
        verts.AddRange(new[] { a, b, c });
        uvs.AddRange(new[] { uva, uvb, uvc });
        tris.AddRange(new[] { idx, idx + 1, idx + 2 });
    }
    private static void AddTriangle(List<Vector3> verts, List<int> tris, List<Vector2> uvs,
        (Vector3, Vector2) a, (Vector3, Vector2) b, (Vector3, Vector2) c)
    {
        int idx = verts.Count;
        verts.AddRange(new[] { a.Item1, b.Item1, c.Item1 });
        uvs.AddRange(new[] { a.Item2, b.Item2, c.Item2 });
        tris.AddRange(new[] { idx, idx + 1, idx + 2 });
    }
    private static GameObject CreateMesh(string name,
        List<Vector3> verts, List<int> tris, List<Vector2> uvs,
        List<Vector3> capVerts, List<int> capTris, List<Vector2> capUVs,
        Material baseMaterial, Material slicedMaterial)
    {
        if (verts.Count == 0 && capVerts.Count == 0)
        {
            Debug.LogWarning($"[{name}] Mesh skipped: no vertices.");
            return null;
        }

        // 生成物のcomponent指定
        var obj = new GameObject(name, typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider),
            typeof(Rigidbody), typeof(MeshSlicer), typeof(AfterNewObject));
        var mesh = new Mesh();
        var allVerts = verts.Concat(capVerts).ToList();
        var allUVs = uvs.Concat(capUVs).ToList();
        // 頂点とUVをペアでチェック
        var filtered = allVerts.Zip(allUVs, (v, uv) => new { v, uv })
            .Where(p => !(float.IsNaN(p.v.x) || float.IsNaN(p.v.y) || float.IsNaN(p.v.z)))
            .ToList();
        if (filtered.Count != allVerts.Count)
        {
            Debug.LogWarning($"[{name}] Some vertices contained NaN and were removed.");
        }
        var cleanVerts = filtered.Select(p => obj.transform.InverseTransformPoint(p.v)).ToList();
        var cleanUVs = filtered.Select(p => p.uv).ToList();
        mesh.SetVertices(cleanVerts);
        mesh.subMeshCount = 2;
        mesh.SetTriangles(tris, 0);
        mesh.SetTriangles(capTris.Select(i => i + verts.Count).ToList(), 1);
        mesh.SetUVs(0, cleanUVs);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        obj.GetComponent<MeshFilter>().mesh = mesh;
        var meshCollider = obj.GetComponent<MeshCollider>();
        meshCollider.convex = true;
        meshCollider.sharedMesh = mesh;
        var renderer = obj.GetComponent<MeshRenderer>();
        renderer.materials = new[] { baseMaterial, slicedMaterial };
        return obj;
    }
}
