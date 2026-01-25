using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HeadBowDetector : MonoBehaviour
{
    // 頭のTransform（Inspectorで必ず設定する想定）。
    [SerializeField] private Transform headTransform;

    // 高さ判定の基準Transform（未設定ならワールドYで判定）。
    [SerializeField] private Transform referenceTransform;

    // 頭を下げたとみなす高さ（referenceTransformのローカルY or ワールドY）。
    [SerializeField] private float bowHeightThreshold = 1.25f;

    // 頭を上げたと判定するリセット高さ（これ以上で次の検知を許可）。
    [SerializeField] private float bowResetHeight = 1.35f;

    // 通知を送るかどうか。
    [SerializeField] private bool notifyOnBow = true;

    // 通知先URL（Sobal-Classic-3のサーバー）。
    [SerializeField] private string notifyUrl = "http://localhost:3000/api/worship";

    // 通知のクールダウン（連続送信を防ぐ）。
    [SerializeField] private float notifyCooldown = 3.0f;

    // デバッグ表示のオン/オフ。
    [SerializeField] private bool showDebugOverlay = true;

    // デバッグ表示を載せるCanvas（未指定なら自動検索）。
    [SerializeField] private Canvas debugCanvas;

    // デバッグ表示用のText（未指定なら自動生成）。
    [SerializeField] private Text debugText;

    // デバッグ表示の背景（未指定なら自動生成）。
    [SerializeField] private Image debugBackground;

    // デバッグ表示の文字色。
    [SerializeField] private Color debugTextColor = Color.cyan;

    // デバッグ背景色。
    [SerializeField] private Color debugBackgroundColor = new Color(0f, 0f, 0f, 0.6f);

    // デバッグ表示の文字サイズ。
    [SerializeField] private int debugFontSize = 18;

    // デバッグ表示の位置（Canvas左上基準）。
    [SerializeField] private Vector2 debugPosition = new Vector2(10f, -520f);

    // 背景の余白。
    [SerializeField] private Vector2 debugPadding = new Vector2(6f, 4f);

    // 直近の通知時刻。
    private float lastNotifyTime = -999f;

    // 既に頭を下げている状態かどうか。
    private bool isBowHolding;

    // 直近の通知結果をデバッグ表示するための状態。
    private string lastNotifyStatus = "None";
    private long lastNotifyHttpCode;
    private string lastNotifyError;
    private float lastNotifyResultTime = -999f;

    // デバッグ表示の初期化済みフラグ。
    private bool debugInitialized;

    private void Update()
    {
        // 頭が未設定なら何もしない（誤動作防止）。
        if (headTransform == null)
        {
            UpdateDebugOverlay(null, "MissingHead");
            return;
        }

        float headY = GetHeadY();

        // まずは「頭が上がった」状態を判定してリセットする。
        if (headY >= bowResetHeight)
        {
            // 上がったら次の検知を許可する。
            isBowHolding = false;
            UpdateDebugOverlay(headY, "Reset");
            return;
        }

        // 閾値より上なら検知しない。
        if (headY > bowHeightThreshold)
        {
            UpdateDebugOverlay(headY, "AboveThreshold");
            return;
        }

        // 既に下げた状態なら連続検知を防ぐ。
        if (isBowHolding)
        {
            UpdateDebugOverlay(headY, "Holding");
            return;
        }

        // ここで初回の「頭を下げた」判定。
        isBowHolding = true;
        UpdateDebugOverlay(headY, "Detected");

        // クールダウン中なら通知を送らない。
        if (Time.time - lastNotifyTime < notifyCooldown)
        {
            UpdateDebugOverlay(headY, "Cooldown");
            return;
        }

        lastNotifyTime = Time.time;

        // 通知が有効ならHTTP送信を行う。
        if (notifyOnBow)
        {
            UpdateDebugOverlay(headY, "Notify");
            StartCoroutine(SendBowNotification());
        }
    }

    private float GetHeadY()
    {
        // 基準Transformがあればローカル座標で判定する。
        if (referenceTransform != null)
        {
            return referenceTransform.InverseTransformPoint(headTransform.position).y;
        }

        // ない場合はワールドYで判定する。
        return headTransform.position.y;
    }

    private IEnumerator SendBowNotification()
    {
        // シンプルなJSONで通知する（後からサーバー側で識別しやすい）。
        string payload = "{\"source\":\"unity\",\"event\":\"bow\",\"time\":" + Time.time.ToString("F3") + "}";
        using (UnityWebRequest request = new UnityWebRequest(notifyUrl, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(payload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // 失敗してもゲーム進行は止めない。
            yield return request.SendWebRequest();

            // 通知結果を保持してデバッグ表示できるようにする。
            lastNotifyHttpCode = request.responseCode;
            lastNotifyResultTime = Time.time;
            if (request.result == UnityWebRequest.Result.Success)
            {
                lastNotifyStatus = "Success";
                lastNotifyError = null;
            }
            else
            {
                lastNotifyStatus = request.result.ToString();
                lastNotifyError = request.error;
            }
        }
    }

    private void UpdateDebugOverlay(float? headY, string state)
    {
        if (!showDebugOverlay)
        {
            return;
        }

        EnsureDebugOverlay();
        if (debugText == null)
        {
            return;
        }

        float cooldownRemaining = Mathf.Max(0f, notifyCooldown - (Time.time - lastNotifyTime));
        string headName = headTransform != null ? headTransform.name : "None";
        string referenceName = referenceTransform != null ? referenceTransform.name : "World";
        string headYText = headY.HasValue ? headY.Value.ToString("F3") : "N/A";
        string notifyErrorText = string.IsNullOrEmpty(lastNotifyError) ? "None" : lastNotifyError;
        float notifyResultAgo = lastNotifyResultTime >= 0f ? Time.time - lastNotifyResultTime : -1f;

        // 表示文言をまとめて更新する（状態確認をしやすくする）。
        debugText.text =
            "HeadBow Debug\n" +
            "State: " + state + " Holding=" + isBowHolding + "\n" +
            "Head: " + headName + " Y=" + headYText + "\n" +
            "Threshold: " + bowHeightThreshold.ToString("F3") + " Reset: " + bowResetHeight.ToString("F3") + "\n" +
            "Notify: enabled=" + notifyOnBow + " cooldown=" + notifyCooldown.ToString("F2") + " remain=" + cooldownRemaining.ToString("F2") + "s\n" +
            "NotifyResult: " + lastNotifyStatus + " code=" + lastNotifyHttpCode + " ago=" + notifyResultAgo.ToString("F2") + "s\n" +
            "NotifyError: " + notifyErrorText + "\n" +
            "NotifyUrl: " + notifyUrl + "\n" +
            "Reference: " + referenceName;
    }

    private void EnsureDebugOverlay()
    {
        if (debugInitialized)
        {
            return;
        }

        // Canvasが未指定ならシーンから探す。
        if (debugCanvas == null)
        {
            debugCanvas = FindObjectOfType<Canvas>();
        }

        if (debugCanvas == null)
        {
            return;
        }

        if (debugText == null)
        {
            // 実行時にTextを生成して表示する。
            GameObject textObject = new GameObject("HeadBowDebugText");
            textObject.transform.SetParent(debugCanvas.transform, false);

            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = debugPosition;
            rectTransform.sizeDelta = new Vector2(900f, 140f);

            debugText = textObject.AddComponent(typeof(Text)) as Text;
            debugText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            debugText.fontSize = debugFontSize;
            debugText.color = debugTextColor;
            debugText.alignment = TextAnchor.UpperLeft;
            debugText.horizontalOverflow = HorizontalWrapMode.Wrap;
            debugText.verticalOverflow = VerticalWrapMode.Overflow;
        }

        if (debugBackground == null && debugText != null)
        {
            // Textの背面に背景Imageを生成する。
            GameObject backgroundObject = new GameObject("HeadBowDebugBackground");
            backgroundObject.transform.SetParent(debugText.transform.parent, false);
            backgroundObject.transform.SetAsFirstSibling();

            debugBackground = backgroundObject.AddComponent(typeof(Image)) as Image;
            debugBackground.color = debugBackgroundColor;
        }

        // 背景をTextのサイズに合わせる。
        if (debugBackground != null && debugText != null)
        {
            RectTransform textRect = debugText.rectTransform;
            RectTransform bgRect = debugBackground.rectTransform;
            bgRect.anchorMin = textRect.anchorMin;
            bgRect.anchorMax = textRect.anchorMax;
            bgRect.pivot = textRect.pivot;
            bgRect.anchoredPosition = textRect.anchoredPosition;
            bgRect.sizeDelta = textRect.sizeDelta + debugPadding * 2f;
            debugBackground.color = debugBackgroundColor;
        }

        debugInitialized = true;
    }
}
