using UnityEngine;
using UnityEngine.UI;

public class RightHandSwipeSlideController : MonoBehaviour
{
    // 右手/左手のTransformを参照してスワイプ方向を判定する。
    // 右手: 右→左 でスライドを進める / 左手: 左→右 でスライドを戻す。
    [SerializeField] private Transform rightHand;
    [SerializeField] private Transform leftHand;
    // スライド切り替えアニメーションの実体
    [SerializeField] private ImageAnimation slideAnimation;
    // 位置計測の基準座標系（未設定ならワールド座標で判定）
    [SerializeField] private Transform swipeReference;
    // 胸の前など、スワイプを許可する空間を使うかどうか
    [SerializeField] private bool requireSwipeZone = true;
    // 胸の「高さ」だけを制限し、前後/左右は許容するかどうか
    [SerializeField] private bool limitToChestHeightOnly = true;
    // スワイプを許可する空間の中心（swipeReference基準のローカル座標）
    [SerializeField] private Vector3 swipeZoneCenter = new Vector3(0f, 1.175f, 0.25f); // 高さ帯1.0〜1.35の中心((1.0+1.35)/2)
    // スワイプを許可する空間のサイズ（swipeReference基準のローカル座標）
    [SerializeField] private Vector3 swipeZoneSize = new Vector3(0.6f, 0.35f, 0.4f); // 高さ帯1.0〜1.35の高さ(1.35-1.0)
    // X方向の最小移動量。この距離以上でスワイプとみなす。
    [SerializeField] private float swipeMinDistance = 0.20f;
    // スワイプ動作の許容時間（秒）
    [SerializeField] private float swipeMaxDuration = 0.02f;
    // 素早い動きのみを許可するための最低速度（X方向の距離/秒）
    [SerializeField] private float minSwipeSpeed = 0.5f;
    // 連続スワイプの誤検知防止のクールダウン時間（秒）
    [SerializeField] private float swipeCooldown = 0.6f;
    // 縦方向のブレ許容（これを超えるとスワイプ無効）
    [SerializeField] private float swipeMaxVertical = 0.15f;
    // 奥行き方向のブレ許容（これを超えるとスワイプ無効）
    [SerializeField] private float swipeMaxDepth = 0.2f;
    [SerializeField] private string leftHandName = "WRIST LEFT"; // leftHand未設定時に自動検索するTransform名
    [SerializeField] private bool showDebugOverlay = true; // 画面上のデバッグ表示
    [SerializeField] private Canvas debugCanvas; // デバッグ表示を載せるCanvas（未指定なら自動検索）
    [SerializeField] private Text debugText; // デバッグ表示用のText（未指定なら自動生成）
    [SerializeField] private int debugFontSize = 20; // デバッグ表示の文字サイズ
    [SerializeField] private Color debugTextColor = Color.red; // デバッグ表示の文字色
    [SerializeField] private Image debugBackground; // デバッグ表示の背景（未指定なら自動生成）
    [SerializeField] private Color debugBackgroundColor = new Color(0f, 0f, 0f, 0.6f); // デバッグ背景色
    [SerializeField] private Vector2 debugPadding = new Vector2(8f, 6f); // 背景の余白

    private class SwipeState
    {
        // スワイプ開始位置（基準座標系のローカル座標）
        public Vector3 SwipeStartLocalPos;
        // スワイプ開始時刻
        public float SwipeStartTime;
        // 最後にスワイプを確定した時刻
        public float LastSwipeTime = -999f;
        // 現在トラッキング中かどうか
        public bool SwipeTracking;
        // 最新の計測値（デバッグ表示用）
        public Vector3 CurrentLocalPos;
        public Vector3 Delta;
        public float Elapsed;
        public bool IsInZone;
        public Vector3 ZoneLocalPos;
    }

    private readonly SwipeState rightSwipe = new SwipeState();
    private readonly SwipeState leftSwipe = new SwipeState();
    private bool warnedMissingLeftHand;
    private bool debugTextInitialized;

    // ClapHandsUpDetectorなどで同じ座標基準を使えるように公開する。
    // swipeReferenceが未設定の場合はnullのままになる。
    public Transform SwipeReference => swipeReference;

    void Start()
    {
        if (slideAnimation == null)
        {
            slideAnimation = FindObjectOfType<ImageAnimation>();
        }

        if (leftHand == null && rightHand != null)
        {
            // Inspector未設定時はアバター階層から左手を検索する
            leftHand = FindChildByName(rightHand.root, leftHandName);
        }

        if (showDebugOverlay)
        {
            EnsureDebugText();
        }
    }

    void Update()
    {
        HandleSwipeInput();
        UpdateDebugOverlay();
    }

    void OnGUI()
    {
        if (!showDebugOverlay || debugText != null)
        {
            return;
        }

        GUI.Label(new Rect(10, 10, 900, 140), BuildDebugMessage());
    }

    private void HandleSwipeInput()
    {
        // 右手の右→左スワイプで次へ進む
        HandleSwipeForHand(
            rightHand,
            rightSwipe,
            delta => delta.x <= -swipeMinDistance,
            1);

        if (leftHand == null && rightHand != null)
        {
            // アバターの読み込みが遅れる場合に備えて再検索する
            leftHand = FindChildByName(rightHand.root, leftHandName);
            if (leftHand == null && !warnedMissingLeftHand)
            {
                Debug.LogWarning($"Left hand not found with name '{leftHandName}'. Assign it in the inspector if needed.");
                warnedMissingLeftHand = true;
            }
        }

        // 左手の左→右スワイプで前へ戻る
        HandleSwipeForHand(
            leftHand,
            leftSwipe,
            delta => delta.x >= swipeMinDistance,
            -1);
    }

    private void UpdateDebugOverlay()
    {
        if (!showDebugOverlay)
        {
            return;
        }

        EnsureDebugText();
        if (debugText != null)
        {
            // Inspectorで色やサイズを変更した場合に反映されるよう毎フレーム更新する
            debugText.color = debugTextColor;
            debugText.fontSize = debugFontSize;
            debugText.text = BuildDebugMessage();
        }

        if (debugBackground != null && debugText != null)
        {
            // 背景の色とサイズをTextに追従させる
            debugBackground.color = debugBackgroundColor;
            RectTransform textRect = debugText.rectTransform;
            RectTransform bgRect = debugBackground.rectTransform;
            bgRect.anchorMin = textRect.anchorMin;
            bgRect.anchorMax = textRect.anchorMax;
            bgRect.pivot = textRect.pivot;
            bgRect.anchoredPosition = textRect.anchoredPosition;
            bgRect.sizeDelta = textRect.sizeDelta + debugPadding * 2f;
        }
    }

    private void EnsureDebugText()
    {
        if (debugTextInitialized)
        {
            return;
        }

        if (debugCanvas == null)
        {
            // シーン内のCanvasを探す（未指定の場合）
            debugCanvas = FindObjectOfType<Canvas>();
        }

        if (debugCanvas == null)
        {
            return;
        }

        if (debugText == null)
        {
            // 実行時にデバッグ用Textを生成する
            GameObject textObject = new GameObject("SwipeDebugText");
            textObject.transform.SetParent(debugCanvas.transform, false);

            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = new Vector2(10f, -10f);
            rectTransform.sizeDelta = new Vector2(900f, 160f);

            debugText = textObject.AddComponent<Text>();
            debugText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            debugText.fontSize = debugFontSize;
            debugText.color = debugTextColor;
            debugText.alignment = TextAnchor.UpperLeft;
            debugText.horizontalOverflow = HorizontalWrapMode.Wrap;
            debugText.verticalOverflow = VerticalWrapMode.Overflow;
        }
        else
        {
            // 既存のTextがある場合もInspector設定を反映する
            debugText.fontSize = debugFontSize;
            debugText.color = debugTextColor;
        }

        if (debugBackground == null && debugText != null)
        {
            // Textの背面に背景用Imageを生成する
            GameObject backgroundObject = new GameObject("SwipeDebugBackground");
            backgroundObject.transform.SetParent(debugText.transform.parent, false);
            backgroundObject.transform.SetAsFirstSibling();

            debugBackground = backgroundObject.AddComponent<Image>();
            debugBackground.color = debugBackgroundColor;
        }

        debugTextInitialized = true;
    }

    private string BuildDebugMessage()
    {
        string rightHandNameLabel = rightHand != null ? rightHand.name : "None";
        string leftHandNameLabel = leftHand != null ? leftHand.name : "None";
        float rightCooldown = Mathf.Max(0f, swipeCooldown - (Time.time - rightSwipe.LastSwipeTime));
        float leftCooldown = Mathf.Max(0f, swipeCooldown - (Time.time - leftSwipe.LastSwipeTime));

        return "Swipe Debug\n" +
               $"RightHand: {rightHandNameLabel}\n" +
               $"LeftHand: {leftHandNameLabel}\n" +
               $"Right Tracking: {rightSwipe.SwipeTracking} InZone: {rightSwipe.IsInZone} ZonePos: {rightSwipe.ZoneLocalPos}\n" +
               $"Left Tracking: {leftSwipe.SwipeTracking} InZone: {leftSwipe.IsInZone} ZonePos: {leftSwipe.ZoneLocalPos}\n" +
               $"Right Elapsed: {rightSwipe.Elapsed:F3}s Delta: {rightSwipe.Delta}\n" +
               $"Left Elapsed: {leftSwipe.Elapsed:F3}s Delta: {leftSwipe.Delta}\n" +
               $"Cooldown R: {rightCooldown:F2}s L: {leftCooldown:F2}s\n" +
               $"Thresholds: MinX={swipeMinDistance} MaxT={swipeMaxDuration} MaxY={swipeMaxVertical} MaxZ={swipeMaxDepth}\n" +
               $"Zone: Enabled={requireSwipeZone} HeightOnly={limitToChestHeightOnly} Center={swipeZoneCenter} Size={swipeZoneSize}";
    }

    private void HandleSwipeForHand(Transform hand, SwipeState state, System.Func<Vector3, bool> isSwipe, int slideDirection)
    {
        if (hand == null)
        {
            state.SwipeTracking = false;
            return;
        }

        // 現在の手の位置を取得（必要なら基準座標へ変換）
        Vector3 currentLocal = GetSwipeLocalPosition(hand);
        state.CurrentLocalPos = currentLocal;

        // 胸の前など指定領域外ならスワイプ追跡を止める
        if (!IsWithinSwipeZone(hand, state))
        {
            state.SwipeTracking = false;
            return;
        }

        if (!state.SwipeTracking)
        {
            // トラッキング開始。最初の位置と時刻を記録する
            state.SwipeStartLocalPos = currentLocal;
            state.SwipeStartTime = Time.time;
            state.SwipeTracking = true;
            return;
        }

        float elapsed = Time.time - state.SwipeStartTime;
        state.Elapsed = elapsed;
        if (elapsed > swipeMaxDuration)
        {
            // 許容時間を超えたらスワイプ開始位置を更新して再トラッキングする
            state.SwipeStartLocalPos = currentLocal;
            state.SwipeStartTime = Time.time;
            return;
        }

        // 開始位置からの移動量を計測
        Vector3 delta = currentLocal - state.SwipeStartLocalPos;
        state.Delta = delta;
        if (Mathf.Abs(delta.y) > swipeMaxVertical || Mathf.Abs(delta.z) > swipeMaxDepth)
        {
            // 縦方向/奥行き方向のブレが大きい場合はトラッキングをリセットする
            state.SwipeStartLocalPos = currentLocal;
            state.SwipeStartTime = Time.time;
            return;
        }

        float speed = elapsed > 0f ? Mathf.Abs(delta.x) / elapsed : 0f;
        if (isSwipe(delta) && speed >= minSwipeSpeed && Time.time - state.LastSwipeTime >= swipeCooldown)
        {
            if (slideAnimation != null && slideAnimation.CanSlide(slideDirection))
            {
                StartCoroutine(slideAnimation.Slide(slideDirection));
            }
            else
            {
                if (slideAnimation == null)
                {
                    Debug.LogWarning("Slide animation not set for swipe input.");
                }
            }

            // スワイプ確定後はクールダウン管理のため時刻を更新
            state.LastSwipeTime = Time.time;
            state.SwipeStartLocalPos = currentLocal;
            state.SwipeStartTime = Time.time;
        }
    }

    private Vector3 GetSwipeLocalPosition(Transform hand)
    {
        if (swipeReference != null)
        {
            // 指定基準からのローカル座標で判定する
            return swipeReference.InverseTransformPoint(hand.position);
        }

        // 基準未設定時はワールド座標で判定する
        return hand.position;
    }

    private bool IsWithinSwipeZone(Transform hand, SwipeState state)
    {
        if (!requireSwipeZone)
        {
            // ゾーン制限を使わない場合は常に許可
            state.IsInZone = true;
            return true;
        }

        Transform zoneReference = GetZoneReference();
        if (zoneReference == null || hand == null)
        {
            // 参照がない場合は安全側（許可）に倒す
            state.IsInZone = true;
            return true;
        }

        // ゾーン基準のローカル座標で位置を判定する
        Vector3 localPos = zoneReference.InverseTransformPoint(hand.position);
        state.ZoneLocalPos = localPos;

        Vector3 halfSize = swipeZoneSize * 0.5f;
        Vector3 delta = localPos - swipeZoneCenter;

        bool inZone;
        if (limitToChestHeightOnly)
        {
            // 胸の高さ帯に入っているかだけを見る（前後/左右は許容）
            inZone = Mathf.Abs(delta.y) <= halfSize.y;
        }
        else
        {
            // 箱型の許可ゾーンに完全に入っているかを見る
            inZone =
                Mathf.Abs(delta.x) <= halfSize.x &&
                Mathf.Abs(delta.y) <= halfSize.y &&
                Mathf.Abs(delta.z) <= halfSize.z;
        }

        state.IsInZone = inZone;
        return inZone;
    }

    private Transform GetZoneReference()
    {
        if (swipeReference != null)
        {
            // 明示された基準がある場合はそれを使う
            return swipeReference;
        }

        if (rightHand != null)
        {
            // 右手のルート（アバターのルート）を基準にする
            return rightHand.root;
        }

        return null;
    }

    private static Transform FindChildByName(Transform root, string targetName)
    {
        if (root == null)
        {
            return null;
        }

        // 階層内を全探索して名前一致のTransformを返す
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == targetName)
            {
                return child;
            }
        }

        return null;
    }
}
