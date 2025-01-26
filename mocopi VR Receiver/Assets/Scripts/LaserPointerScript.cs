using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LaserPointerScript : MonoBehaviour
{
    [SerializeField] private RectTransform slideArea;   // **スライド領域の RectTransform**
    [SerializeField] private Camera uiCamera;          // **UI をレンダリングするカメラ**
    [SerializeField] private Image pointerImage;       // **光の点（ポインター）**
    [SerializeField] private GameObject trailPrefab;   // **軌跡用の TrailRenderer を持つオブジェクト**
    [SerializeField] private Transform avatarRightHand; // **mocopi のアバター右手の Transform**
    [SerializeField] private Transform avatarRightForearm; // **mocopi のアバター右前腕（肘〜手）**
    [SerializeField] private LayerMask slideLayer; // **スライドと衝突判定するレイヤー**
    
    private GameObject activeTrail; // **現在の軌跡オブジェクト**
    private Vector2 pointerPosition; // **ポインターの現在位置**
    private bool isPointerEnabled = false; // **ポインターのオンオフ状態（デフォルトはオフ）**

    void Update()
    {
        // **Ctrl（Windows） or Command（Mac）＋L でポインター表示を切り替え**
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
             Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)) &&
            Input.GetKeyDown(KeyCode.L))
        {
            TogglePointer();
        }

        if (isPointerEnabled && avatarRightHand != null && avatarRightForearm != null)
        {
            // **mocopi の右前腕の向きに基づいてポインター位置を計算**
            UpdatePointerWithMocopi();
        }
    }

    /// <summary>
    /// **ポインターのオンオフを切り替える**
    /// </summary>
    public void TogglePointer()
    {
        isPointerEnabled = !isPointerEnabled;
        pointerImage.gameObject.SetActive(isPointerEnabled);
        if (!isPointerEnabled && activeTrail != null)
        {
            Destroy(activeTrail);
        }
        Debug.Log("Pointer Toggled: " + isPointerEnabled);
    }

    /// <summary>
    /// **mocopi のアバターの腕の向きに基づいてポインターを動かす**
    /// </summary>
    private void UpdatePointerWithMocopi()
    {
        // **mocopi の右手のワールド座標を取得**
        Vector3 handWorldPosition = avatarRightHand.position;
        
        // **前腕（肘→手）の向きを基準に Ray を発射する方向を計算**
        Vector3 forwardDirection = (avatarRightHand.position - avatarRightForearm.position).normalized;
        Debug.DrawRay(handWorldPosition, forwardDirection * 10, Color.blue, 1.0f);

        // **Ray を発射してスライドと衝突する位置を求める**
        RaycastHit hit;
        if (Physics.Raycast(handWorldPosition, forwardDirection, out hit, Mathf.Infinity, slideLayer))
        {
            //Debug.Log("Raycast hit: " + hit.collider.name);

            // **スライド上のヒットポイントを取得**
            Vector3 hitPosition = hit.point;

            // **ワールド座標 → スクリーン座標に変換**
            Vector3 screenPosition = uiCamera.WorldToScreenPoint(hitPosition);
            //Debug.Log("Screen Position: " + screenPosition);

            // **スクリーン座標 → UI のローカル座標に変換**
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(slideArea, screenPosition, uiCamera, out pointerPosition))
            {
                //Debug.Log("Pointer Position: " + pointerPosition);

                // **ポインターを移動**
                pointerImage.rectTransform.anchoredPosition = pointerPosition;
                UpdateTrail(slideArea.transform.TransformPoint(pointerPosition));
            }
        }
        else
        {
            //Debug.Log("Raycast did not hit anything.");
        }
    }

    /// <summary>
    /// **ポインターの軌跡を更新**
    /// </summary>
    private void UpdateTrail(Vector3 worldPosition)
    {
        worldPosition+= new Vector3(0,0,1);
        if (!isPointerEnabled) return;

        if (activeTrail == null)
        {
            // **新しい Trail を作成（ワールド空間に配置）**
            activeTrail = Instantiate(trailPrefab, worldPosition, Quaternion.identity);
        }
        
        // **Trail の位置を更新**
        activeTrail.transform.position = worldPosition;
    }
}