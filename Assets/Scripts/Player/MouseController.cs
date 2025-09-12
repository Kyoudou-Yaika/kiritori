using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 後回し
public class MouseController : MonoBehaviour
{
    [SerializeField]
    private Camera _targetCamera;

    MeshCut2D.CutController MC;
    Vector2 StartPos, EndPos, Centerpos;
    public int distance = 15;          // Rayの飛ばせる距離

    private RaycastHit2D[] _hits = new RaycastHit2D[10];

    public float BoxAngle = 0f;


    // Start is called before the first frame update
    void Start()
    {
        MC = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartPos = Input.mousePosition;
            Debug.Log("StartPos" + Camera.main.ScreenToWorldPoint(StartPos));
        }

        if (Input.GetMouseButtonUp(0))
        {
            EndPos = Input.mousePosition;
            Debug.Log("EndPos" + Camera.main.ScreenToWorldPoint(EndPos));
            RayFind();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        MC = collision.GetComponent<MeshCut2D.CutController>();
        Debug.Log(UnityEditor.ObjectNames.GetClassName(collision));
    }

    // Rayを用いてobjectを探す
    void RayFind()
    { 
        // 切り取り線の開始と終わり
        var midScreenPosition = (StartPos + EndPos) / 2f;
        var midRay = _targetCamera.ScreenPointToRay(midScreenPosition);

        //  レイを書く
        Debug.DrawRay(midRay.origin, midRay.direction * 100f, Color.red, 2f);
        // BoxCastの中心をRayのちょっと前方にする（5単位前）
        var boxCenter = midRay.origin + midRay.direction * 10f;
        // スクリーン座標をワールド座標に変換して方向を出す
        var startWorld = _targetCamera.ScreenToWorldPoint(new Vector2(StartPos.x, StartPos.y));
        var endWorld = _targetCamera.ScreenToWorldPoint(new Vector2(EndPos.x, EndPos.y));
        Vector2 dragDir = (endWorld - startWorld).normalized;
        var forward = _targetCamera.transform.forward;
        // カット面の法線（スワイプ方向とカメラ前方の外積）
        Vector2 sliceNormal = Vector3.Cross(dragDir, forward).normalized;
        // Boxの回転をカット面に合わせる
        Quaternion boxRotation = Quaternion.LookRotation(sliceNormal, dragDir);

        // Boxのサイズを調整（厚みや幅は好みで）
        Vector2 boxHalfExtents = new Vector2(Vector2.Distance(startWorld, endWorld) / 2f, 0.05f);// 2f, 0.05f, 1f
        // BoxCast実行
        var hitCount = Physics2D.BoxCastNonAlloc(boxCenter, boxHalfExtents, BoxAngle , forward, _hits, distance);
        Debug.Log("ボックスの中心　" + boxCenter);// 57
        Debug.Log("各軸についてのボックスサイズの半分　" + boxHalfExtents);
        Debug.Log("ボックスを投射する方向　" + forward);
        Debug.Log("結果の保存　" + _hits);
        Debug.Log("ボックスの回転　" + boxRotation);

        // CutControllerにデータを渡す
        for (int i = 0; i < hitCount; i++)
        {
            MC = _hits[i].transform.GetComponent<MeshCut2D.CutController>();
            if (MC != null)
            {
                CC(hitCount);
            }
            Debug.Log($"カット対象: {_hits[i].transform.name}, Normal={sliceNormal}");
        }

        if (hitCount == 0)
        {
            Debug.Log("BoxCastでヒットなし。");
        }
    }

    void CC(int Count)
    {
        MC.index = Count;
        MC.Startx = _targetCamera.ScreenToWorldPoint(StartPos).x;
        MC.Starty = _targetCamera.ScreenToWorldPoint(StartPos).y;
        MC.Endx = _targetCamera.ScreenToWorldPoint(EndPos).x;
        MC.Endy = _targetCamera.ScreenToWorldPoint(EndPos).y;
    }
}

