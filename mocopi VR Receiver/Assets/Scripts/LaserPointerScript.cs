using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LaserPointerScript : MonoBehaviour
{
    [SerializeField] private RectTransform slideArea;   // **スライド領域の RectTransform**
    [SerializeField] private Camera uiCamera;          // **UI をレンダリングするカメラ**
    [SerializeField] private Image pointerImage;       // **光の点（ポインター）**
    [SerializeField] private GameObject trailPrefab;   // **軌跡用の TrailRenderer を持つオブジェクト**
    
    private GameObject activeTrail; // **現在の軌跡オブジェクト**
    private Vector2 pointerPosition; // **ポインターの現在位置**

    void Update()
    {
        UpdatePointerPosition();
    }

    /// <summary>
    /// **ポインター位置をマウスカーソルの位置に合わせて更新**
    /// </summary>
    private void UpdatePointerPosition()
    {
        // **マウス位置を UI のワールド座標に変換**
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(slideArea, Input.mousePosition, uiCamera, out pointerPosition))
        {
            // **ポインターの位置をスライド内のローカル座標に設定**
            pointerImage.rectTransform.anchoredPosition = pointerPosition;

            // **UI のポインター位置をワールド座標に変換**
            Vector3 worldPos = slideArea.transform.TransformPoint(pointerPosition);

            // **軌跡を描画**
            UpdateTrail(worldPos);
        }
    }

    /// <summary>
    /// **ポインターの軌跡を更新**
    /// </summary>
    private void UpdateTrail(Vector3 worldPosition)
    {
        if (activeTrail == null)
        {
            // **新しい Trail を作成（ワールド空間に配置）**
            activeTrail = Instantiate(trailPrefab, worldPosition, Quaternion.identity);
        }

        // **Trail の位置を更新**
        activeTrail.transform.position = worldPosition;
    }

    /// <summary>
    /// **将来的に mocopi からデータを受け取るためのメソッド**
    /// </summary>
    public void SetPointerPosition(Vector2 newPosition)
    {
        pointerPosition = newPosition;
        pointerImage.rectTransform.anchoredPosition = newPosition;
        
        // **UI のポインター位置をワールド座標に変換**
        Vector3 worldPos = slideArea.transform.TransformPoint(newPosition);
        UpdateTrail(worldPos);
    }
}