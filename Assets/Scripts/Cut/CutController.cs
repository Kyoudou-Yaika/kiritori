using System.Collections.Generic;
using UnityEngine;

// 後回し
namespace MeshCut2D
{
    public class CutController : MonoBehaviour
    {
        [SerializeField]
        private Sprite sprite;
        IList<Vector3> Vertices;                  // 頂点           (階級投影)
        IList<Color32> Colors;                    // 色             ()
        IList<Vector2> UV;                        // UV             ()
        IList<int> Indices;                       // インデックス   ()
        public int index;                         // インデックス数 ()
        public float Startx, Starty, Endx, Endy;  // LinePoint1     (スタートとエンドポイント)
        MeshCutResult ResultsA, ResultsB;         // 結果Aと結果B   ()
       
        // Start is called before the first frame update
        void Start()
        {
            Mesh mesh = GetComponent<MeshFilter>().mesh;
            Vertices = mesh.vertices;
            // Vertices = GetComponent<SpriteRenderer>().sprite.vertices;
            UV = sprite.uv;

            ResultsA = new MeshCutResult();
            ResultsB = new MeshCutResult();
        }

        // Update is called once per frame
        void Update()
        {
            MeshCut2D.Cut(Vertices, Colors, UV, Indices, index, Startx, Starty, Endx, Endy, ResultsA, ResultsB);
            if (ResultsA != null && ResultsB != null)
            {
                Debug.Log("not null");
            }
        }
    }

    // MeshCutの結果を管理するクラス
    // MeshCutの結果
    public class MeshCutResult
    {
        public List<Vector3> vertices = new List<Vector3>();
        public List<Color32> colors = new List<Color32>();
        public List<int> indices = new List<int>();
        public List<Vector2> uv = new List<Vector2>();

        // クリア(リセット)
        public void Clear()
        {
            vertices.Clear();
            colors.Clear();
            uv.Clear();
            indices.Clear();
        }

        // 三角形の追加
        public void AddTriangle(
            float x1, float y1,
            float x2, float y2,
            float x3, float y3,
            float uv1X, float uv1Y,
            float uv2X, float uv2Y,
            float uv3X, float uv3Y,
            Color color)
        {
            int vertexCount = vertices.Count;
            vertices.Add(new Vector3(x1, y1, 0));
            vertices.Add(new Vector3(x2, y2, 0));
            vertices.Add(new Vector3(x3, y3, 0));
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
            uv.Add(new Vector2(uv1X, uv1Y));
            uv.Add(new Vector2(uv2X, uv2Y));
            uv.Add(new Vector2(uv3X, uv3Y));
            indices.Add(vertexCount + 2);
            indices.Add(vertexCount + 1);
            indices.Add(vertexCount + 0);
        }

        // 長方形の追加
        public void AddRectangle(
            float x1, float y1,
            float x2, float y2,
            float x3, float y3,
            float x4, float y4,
            float uv1_X, float uv1_Y,
            float uv2_X, float uv2_Y,
            float uv3_X, float uv3_Y,
            float uv4_X, float uv4_Y,
            Color color)
        {
            int vertexCount = vertices.Count;
            vertices.Add(new Vector3(x1, y1, 0));
            vertices.Add(new Vector3(x2, y2, 0));
            vertices.Add(new Vector3(x3, y3, 0));
            vertices.Add(new Vector3(x4, y4, 0));
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
            uv.Add(new Vector2(uv1_X, uv1_Y));
            uv.Add(new Vector2(uv2_X, uv2_Y));
            uv.Add(new Vector2(uv3_X, uv3_Y));
            uv.Add(new Vector2(uv4_X, uv4_Y));
            indices.Add(vertexCount + 2);
            indices.Add(vertexCount + 1);
            indices.Add(vertexCount + 0);
            indices.Add(vertexCount + 0);
            indices.Add(vertexCount + 3);
            indices.Add(vertexCount + 2);
        }
    }

    public class MeshCut2D
    {
        // Cut()を呼び出す
        public static void Cut(
            IList<Vector3> vertices,    // 頂点
            IList<Color32> colors,      // 色
            IList<Vector2> uv,          // UV
            IList<int> indices,         // インデックス()
            int indexCount,             // インデックス数
            float x1,                   // LinePoint1
            float y1,                   // LinePoint1
            float x2,                   // LinePoint2
            float y2,                   // LinePoint2
            MeshCutResult _resultsA,    // 結果A
            MeshCutResult _resultsB)    // 結果B
        {
            _resultsA.Clear();
            _resultsB.Clear();

            for (int i = 0; i < indexCount; i += 3)
            {
                // 使いやすいように変数に代入しているだけ
                int indexA = indices[i + 0];
                int indexB = indices[i + 1];
                int indexC = indices[i + 2];
                Vector3 a = vertices[indexA];
                Vector3 b = vertices[indexB];
                Vector3 c = vertices[indexC];
                Color color = colors[indexA];
                float uvA_X = uv[indexA].x;
                float uvA_Y = uv[indexA].y;
                float uvB_X = uv[indexB].x;
                float uvB_Y = uv[indexB].y;
                float uvC_X = uv[indexC].x;
                float uvC_Y = uv[indexC].y;

                bool aSide = IsClockWise(x1, y1, x2, y2, a.x, a.y);
                bool bSide = IsClockWise(x1, y1, x2, y2, b.x, b.y);
                bool cSide = IsClockWise(x1, y1, x2, y2, c.x, c.y);
                if (aSide == bSide && aSide == cSide)
                {
                    // 三項演算子を使用
                    // 「?」:trueの時「:」:falseの時
                    var triangleResult = aSide ? _resultsA : _resultsB;
                    triangleResult.AddTriangle(
                        a.x, a.y, b.x, b.y, c.x, c.y,
                        uvA_X, uvA_Y, uvB_X, uvB_Y, uvC_X, uvC_Y,
                        color);
                }
                else if (aSide != bSide && aSide != cSide)
                {
                    float abX, abY, caX, caY, uvAB_X, uvAB_Y, uvCA_X, uvCA_Y;
                    GetIntersectionLineAndLineStrip(
                        x1, y1,
                        x2, y2,
                        a.x, a.y,
                        b.x, b.y,
                        uvA_X, uvA_Y,
                        uvB_X, uvB_Y,
                        out abX, out abY,
                        out uvAB_X, out uvAB_Y);
                    GetIntersectionLineAndLineStrip(
                        x1, y1,
                        x2, y2,
                        c.x, c.y,
                        a.x, a.y,
                        uvC_X, uvC_Y,
                        uvA_X, uvA_Y,
                        out caX, out caY,
                        out uvCA_X, out uvCA_Y);
                    var triangleResult = aSide ? _resultsA : _resultsB;
                    var rectangleResult = aSide ? _resultsB : _resultsA;
                    triangleResult.AddTriangle(
                        a.x, a.y,
                        abX, abY,
                        caX, caY,
                        uvA_X, uvA_Y,
                        uvAB_X, uvAB_Y,
                        uvCA_X, uvCA_Y,
                        color);
                    rectangleResult.AddRectangle(
                        b.x, b.y,
                        c.x, c.y,
                        caX, caY,
                        abX, abY,
                        uvB_X, uvB_Y,
                        uvC_X, uvC_Y,
                        uvCA_X, uvCA_Y,
                        uvAB_X, uvAB_Y,
                        color);
                }
                else if (bSide != aSide && bSide != cSide)
                {
                    float abX, abY, bcX, bcY, uvAB_X, uvAB_Y, uvBC_X, uvBC_Y;
                    GetIntersectionLineAndLineStrip(
                        x1, y1,
                        x2, y2,
                        a.x, a.y,
                        b.x, b.y,
                        uvA_X, uvA_Y,
                        uvB_X, uvB_Y,
                        out abX, out abY,
                        out uvAB_X, out uvAB_Y);
                    GetIntersectionLineAndLineStrip(
                        x1, y1,
                        x2, y2,
                        b.x, b.y,
                        c.x, c.y,
                        uvB_X, uvB_Y,
                        uvC_X, uvC_Y,
                        out bcX, out bcY,
                        out uvBC_X, out uvBC_Y);
                    var triangleResult = bSide ? _resultsA : _resultsB;
                    var rectangleResult = bSide ? _resultsB : _resultsA;
                    triangleResult.AddTriangle(
                        b.x, b.y,
                        bcX, bcY,
                        abX, abY,
                        uvB_X, uvB_Y,
                        uvBC_X, uvBC_Y,
                        uvAB_X, uvAB_Y,
                        color);
                    rectangleResult.AddRectangle(
                        c.x, c.y,
                        a.x, a.y,
                        abX, abY,
                        bcX, bcY,
                        uvC_X, uvC_Y,
                        uvA_X, uvA_Y,
                        uvAB_X, uvAB_Y,
                        uvBC_X, uvBC_Y,
                        color);
                }
                else if (cSide != aSide && cSide != bSide)
                {
                    float bcX, bcY, caX, caY, uvBC_X, uvBC_Y, uvCA_X, uvCA_Y;
                    GetIntersectionLineAndLineStrip(
                        x1, y1,
                        x2, y2,
                        b.x, b.y,
                        c.x, c.y,
                        uvB_X, uvB_Y,
                        uvC_X, uvC_Y,
                        out bcX, out bcY,
                        out uvBC_X, out uvBC_Y);
                    GetIntersectionLineAndLineStrip(
                        x1, y1,
                        x2, y2,
                        c.x, c.y,
                        a.x, a.y,
                        uvC_X, uvC_Y,
                        uvA_X, uvA_Y,
                        out caX, out caY,
                        out uvCA_X, out uvCA_Y);
                    var triangleResult = cSide ? _resultsA : _resultsB;
                    var rectangleResult = cSide ? _resultsB : _resultsA;
                    triangleResult.AddTriangle(
                        c.x, c.y,
                        caX, caY,
                        bcX, bcY,
                        uvC_X, uvC_Y,
                        uvCA_X, uvCA_Y,
                        uvBC_X, uvBC_Y,
                        color);
                    rectangleResult.AddRectangle(
                        a.x, a.y,
                        b.x, b.y,
                        bcX, bcY,
                        caX, caY,
                        uvA_X, uvA_Y,
                        uvB_X, uvB_Y,
                        uvBC_X, uvBC_Y,
                        uvCA_X, uvCA_Y,
                        color);
                }
            }
        }

        // 交差点とラインストリップを取得する
        private static void GetIntersectionLineAndLineStrip(
            float x1, float y1, // Line Point
            float x2, float y2, // Line Point
            float x3, float y3, // Line Strip Point
            float x4, float y4, // Line Strip Point
            float uv3_X, float uv3_Y,
            float uv4_X, float uv4_Y,
            out float x, out float y,
            out float uvX, out float uvY)
        {
            float s1 = (x2 - x1) * (y3 - y1) - (y2 - y1) * (x3 - x1);
            float s2 = (x2 - x1) * (y1 - y4) - (y2 - y1) * (x1 - x4);

            float c = s1 / (s1 + s2);

            x = x3 + (x4 - x3) * c;
            y = y3 + (y4 - y3) * c;

            uvX = uv3_X + (uv4_X - uv3_X) * c;
            uvY = uv3_Y + (uv4_Y - uv3_Y) * c;
        }

        // 時計回りのときtrueを返す
        private static bool IsClockWise(
            float x1, float y1,
            float x2, float y2,
            float x3, float y3)
        {
            return (x2 - x1) * (y3 - y2) - (y2 - y1) * (x3 - x2) > 0;
        }
    }
}
