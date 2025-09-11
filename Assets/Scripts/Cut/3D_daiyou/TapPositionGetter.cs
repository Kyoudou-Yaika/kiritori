using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 参考元https://hacchi-man.hatenablog.com/entry/2025/04/08/220000
[RequireComponent(typeof(LineRenderer))]
public class TapPositionGetter : MonoBehaviour
{
    [SerializeField]
    private Camera _targetCamera;
    [SerializeField]
    private Material _slicedMaterial;
    [SerializeField]
    private LineRenderer _lineRenderer;
    private Vector3 _startPosition;
    private Vector3 _endPosition;
    private bool _isDragging = false;
    private RaycastHit[] _hits = new RaycastHit[10];
    // 操作用
    public bool touch;
    RaycastHit hit;
    Vector3 touchObject;

    private void Start()
    {
        if (_targetCamera == null)
            _targetCamera = Camera.main;
        // LineRendererの初期設定
        _lineRenderer.positionCount = 2;
        _lineRenderer.enabled = false;
        _lineRenderer.widthMultiplier = 0.05f;
    }

    private void Update()
    {
        // ドラッグ開始
        if (Input.GetMouseButtonDown(0))
        {
            _startPosition = Input.mousePosition;
            _isDragging = true;
            _lineRenderer.enabled = true;
            // 当たり判定による条件式
            // Rayを飛ばして当たり判定をチェック
            Ray ray = Camera.main.ScreenPointToRay(_startPosition);    // カメラからマウスカーソルの位置のRayを作成
            if (Physics.Raycast(ray, out hit))
            {
                touch = true;   // フラグ
                _lineRenderer.enabled = false;
            }
        }

        // ドラッグ中：線の更新
        if (_isDragging && !touch)
        {
            _endPosition = Input.mousePosition;
            var startWorld = GetWorldPosition(_startPosition);
            var endWorld = GetWorldPosition(_endPosition);
            startWorld.z -= 1;
            endWorld.z -= 1;
            _lineRenderer.SetPosition(0, startWorld);
            _lineRenderer.SetPosition(1, endWorld);
        }
        else if (_isDragging && touch)
        {
            touchObject = GetWorldPosition(Input.mousePosition);
            touchObject.z = 0f;
            hit.collider.gameObject.transform.position = touchObject;
        }

        if (Input.GetMouseButtonUp(0) && _isDragging)
        {
            _isDragging = false;
            _lineRenderer.enabled = false;
            if (touch)
            {
                touch = false;
                return;
            }

            // 切り取り線の開始と終わり
            var midScreenPosition = (_startPosition + _endPosition) / 2f;
            var midRay = _targetCamera.ScreenPointToRay(midScreenPosition);

            //  レイを書く
            Debug.DrawRay(midRay.origin, midRay.direction * 1000f, Color.red, 2f);
            // BoxCastの中心をRayのちょっと前方にする（5単位前）
            var boxCenter = midRay.origin + midRay.direction * 10f;
            // スクリーン座標をワールド座標に変換して方向を出す
            var startWorld = _targetCamera.ScreenToWorldPoint(new Vector3(_startPosition.x, _startPosition.y, 5f));
            var endWorld = _targetCamera.ScreenToWorldPoint(new Vector3(_endPosition.x, _endPosition.y, 5f));
            Vector3 dragDir = (endWorld - startWorld).normalized;
            var forward = _targetCamera.transform.forward;
            // カット面の法線（スワイプ方向とカメラ前方の外積）
            Vector3 sliceNormal = Vector3.Cross(dragDir, forward).normalized;
            // Boxの回転をカット面に合わせる
            Quaternion boxRotation = Quaternion.LookRotation(sliceNormal, dragDir);

            // Boxのサイズを調整（厚みや幅は好みで）
            Vector3 boxHalfExtents = new Vector3(Vector3.Distance(startWorld, endWorld) / 2f, 0.05f, 500f);// 2f, 0.05f, 1f
            // BoxCast実行
            var hitCount = Physics.BoxCastNonAlloc(boxCenter, boxHalfExtents, forward, _hits, boxRotation, 0.01f);
            Debug.Log("ボックスの中心　" + boxCenter);// 57
            Debug.Log("各軸についてのボックスサイズの半分　" + boxHalfExtents);
            Debug.Log("ボックスを投射する方向　" + forward);
            Debug.Log("結果の保存　" + _hits);
            Debug.Log("ボックスの回転　" + boxRotation);

            // MeshSlicerにデータを渡す
            for (int i = 0; i < hitCount; i++)
            {
                if (_hits[i].transform.name != "jimen")
                {
                    MeshSlicer.Slice(_hits[i].transform, boxCenter, sliceNormal, _slicedMaterial);
                    Debug.Log($"カット対象: {_hits[i].transform.name}, Normal={sliceNormal}");
                }
            }

            if (hitCount == 0)
            {
                Debug.Log("BoxCastでヒットなし。");
            }

        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    private Vector3 GetWorldPosition(Vector3 screenPosition)
    {
        var ray = _targetCamera.ScreenPointToRay(screenPosition);
        return ray.origin + ray.direction * 5f;
    }
}
