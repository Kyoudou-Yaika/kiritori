using UnityEngine;

public class CustomCursorController : MonoBehaviour
{

    void Start()
    {
        // システムカーソル非表示
        Cursor.visible = false;
    }

    void Update()
    {
        // デバッグ用キー
        if (Input.GetKeyDown(KeyCode.J))
        {
            Cursor.visible = true;
        }

        // マウスのスクリーン座標を取得
        Vector3 mousePosition = Input.mousePosition;

        // カメラからの距離（Z軸）を設定（2Dならカメラの距離を適切に）
        mousePosition.z = 5f; // 例えばカメラから10の位置

        // スクリーン座標をワールド座標に変換
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        // 自分自身（スプライト）をマウス位置に移動
        transform.position = worldPosition;
    }
}

