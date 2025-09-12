using UnityEngine;
using System.Collections.Generic;

public class SpriteCut : MonoBehaviour
{
    // å„âÒÇµ(AI)

    public SpriteRenderer targetSpriteRenderer;

    private Vector2 lineStart;
    private Vector2 lineEnd;
    private bool isDragging = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lineStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            lineEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isDragging = false;
            CutSprite();
        }
    }

    void CutSprite()
    {
        Sprite sprite = targetSpriteRenderer.sprite;
        Vector2[] positions = sprite.vertices;
        ushort[] triangles = sprite.triangles;
        Vector2[] uvs = sprite.uv;

        List<Vector3> leftVerts = new List<Vector3>();
        List<Vector2> leftUVs = new List<Vector2>();
        List<int> leftTris = new List<int>();

        List<Vector3> rightVerts = new List<Vector3>();
        List<Vector2> rightUVs = new List<Vector2>();
        List<int> rightTris = new List<int>();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i0 = triangles[i];
            int i1 = triangles[i + 1];
            int i2 = triangles[i + 2];

            bool left0 = IsLeftOfLine(positions[i0], lineStart, lineEnd);
            bool left1 = IsLeftOfLine(positions[i1], lineStart, lineEnd);
            bool left2 = IsLeftOfLine(positions[i2], lineStart, lineEnd);

            int leftCount = (left0 ? 1 : 0) + (left1 ? 1 : 0) + (left2 ? 1 : 0);

            if (leftCount == 3)
            {
                AddTriangle(leftVerts, leftUVs, leftTris,
                    positions[i0], positions[i1], positions[i2],
                    uvs[i0], uvs[i1], uvs[i2]);
            }
            else if (leftCount == 0)
            {
                AddTriangle(rightVerts, rightUVs, rightTris,
                    positions[i0], positions[i1], positions[i2],
                    uvs[i0], uvs[i1], uvs[i2]);
            }
            else
            {
                SplitMixedTriangle(positions, uvs, i0, i1, i2, lineStart, lineEnd,
                    leftVerts, leftUVs, leftTris, rightVerts, rightUVs, rightTris);
            }
        }

        CreateMeshObject("LeftPart", leftVerts, leftUVs, leftTris);
        CreateMeshObject("RightPart", rightVerts, rightUVs, rightTris);
    }

    bool IsLeftOfLine(Vector2 p, Vector2 a, Vector2 b)
    {
        return ((b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x)) > 0;
    }

    void LineIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2, out Vector2 intersection)
    {
        intersection = Vector2.zero;
        Vector2 r = p2 - p1;
        Vector2 s = q2 - q1;
        float rxs = r.x * s.y - r.y * s.x;
        if (Mathf.Approximately(rxs, 0f)) return;

        float t = ((q1 - p1).x * s.y - (q1 - p1).y * s.x) / rxs;
        intersection = p1 + t * r;
    }

    Vector2 InterpolateUV(Vector2 uvA, Vector2 uvB, Vector2 pA, Vector2 pB, Vector2 inter)
    {
        float total = Vector2.Distance(pA, pB);
        float part = Vector2.Distance(pA, inter);
        float t = part / total;
        return Vector2.Lerp(uvA, uvB, t);
    }

    void AddTriangle(List<Vector3> verts, List<Vector2> uvs, List<int> tris,
                     Vector2 v0, Vector2 v1, Vector2 v2,
                     Vector2 uv0, Vector2 uv1, Vector2 uv2)
    {
        int index = verts.Count;
        verts.Add(v0);
        verts.Add(v1);
        verts.Add(v2);
        uvs.Add(uv0);
        uvs.Add(uv1);
        uvs.Add(uv2);
        tris.Add(index);
        tris.Add(index + 1);
        tris.Add(index + 2);
    }

    void CreateMeshObject(string name, List<Vector3> verts, List<Vector2> uvs, List<int> tris)
    {
        GameObject obj = new GameObject(name);
        Mesh mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshFilter mf = obj.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = obj.AddComponent<MeshRenderer>();
        mr.material = targetSpriteRenderer.material;

        obj.transform.position = targetSpriteRenderer.transform.position;
    }

    void SplitMixedTriangle(
        Vector2[] positions, Vector2[] uvs,
        int i0, int i1, int i2,
        Vector2 lineStart, Vector2 lineEnd,
        List<Vector3> leftVerts, List<Vector2> leftUVs, List<int> leftTris,
        List<Vector3> rightVerts, List<Vector2> rightUVs, List<int> rightTris)
    {
        int[] indices = new int[] { i0, i1, i2 };
        List<int> left = new List<int>();
        List<int> right = new List<int>();

        foreach (int i in indices)
        {
            Vector2 p = positions[i];
            if (IsLeftOfLine(p, lineStart, lineEnd))
                left.Add(i);
            else
                right.Add(i);
        }

        if (left.Count == 1 && right.Count == 2)
        {
            int li = left[0];
            int ri0 = right[0];
            int ri1 = right[1];

            Vector2 lp = positions[li];
            Vector2 rp0 = positions[ri0];
            Vector2 rp1 = positions[ri1];
            Vector2 luv = uvs[li];
            Vector2 ruv0 = uvs[ri0];
            Vector2 ruv1 = uvs[ri1];

            Vector2 inter0, inter1;
            LineIntersect(lp, rp0, lineStart, lineEnd, out inter0);
            LineIntersect(lp, rp1, lineStart, lineEnd, out inter1);
            Vector2 uvInter0 = InterpolateUV(luv, ruv0, lp, rp0, inter0);
            Vector2 uvInter1 = InterpolateUV(luv, ruv1, lp, rp1, inter1);

            AddTriangle(leftVerts, leftUVs, leftTris, lp, inter0, inter1, luv, uvInter0, uvInter1);
            AddTriangle(rightVerts, rightUVs, rightTris, rp0, rp1, inter0, ruv0, ruv1, uvInter0);
            AddTriangle(rightVerts, rightUVs, rightTris, rp1, inter1, inter0, ruv1, uvInter1, uvInter0);
        }
        else if (right.Count == 1 && left.Count == 2)
        {
            int ri = right[0];
            int li0 = left[0];
            int li1 = left[1];

            Vector2 rp = positions[ri];
            Vector2 lp0 = positions[li0];
            Vector2 lp1 = positions[li1];
            Vector2 ruv = uvs[ri];
            Vector2 luv0 = uvs[li0];
            Vector2 luv1 = uvs[li1];

            Vector2 inter0, inter1;
            LineIntersect(rp, lp0, lineStart, lineEnd, out inter0);
            LineIntersect(rp, lp1, lineStart, lineEnd, out inter1);
            Vector2 uvInter0 = InterpolateUV(ruv, luv0, rp, lp0, inter0);
            Vector2 uvInter1 = InterpolateUV(ruv, luv1, rp, lp1, inter1);

            // âEë§ÅFrp, inter0, inter1
            AddTriangle(rightVerts, rightUVs, rightTris, rp, inter0, inter1, ruv, uvInter0, uvInter1);

            // ç∂ë§ÅFlp0, lp1, inter0 Ç∆ lp1, inter1, inter0
            AddTriangle(leftVerts, leftUVs, leftTris, lp0, lp1, inter0, luv0, luv1, uvInter0);
            AddTriangle(leftVerts, leftUVs, leftTris, lp1, inter1, inter0, luv1, uvInter1, uvInter0);
        }
    }
}
