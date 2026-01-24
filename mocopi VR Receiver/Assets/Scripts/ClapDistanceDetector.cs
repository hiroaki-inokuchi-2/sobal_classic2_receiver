using System.Collections;
using System.Collections.Generic;
using Mocopi.Receiver;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ClapDistanceDetector : MonoBehaviour
{
    // 右手/左手のTransformを参照して距離で拍手を判定する。
    // コライダーは使わず、手同士の距離だけで判定する。
    [SerializeField] private Transform rightHand;
    [SerializeField] private Transform leftHand;

    // Mocopiの受信状態を見たい場合に設定する（未設定なら無視）。
    [SerializeField] private MocopiAvatar mocopiAvatar;

    // これ以下の距離になったら拍手とみなす（単位はメートル想定）。
    [SerializeField] private float clapDistanceThreshold = 0.18f;

    // 連続発火を防ぐクールダウン（秒）。
    [SerializeField] private float clapCooldown = 0.4f;

    // 連続して何回「拍手検知」したらSEを鳴らすか。
    [SerializeField] private int requiredConsecutiveClaps = 3;

    // 連続検知の時間制限（秒）。この時間内に規定回数が必要。
    [SerializeField] private float consecutiveClapWindow = 2f;

    // 両手を上げた判定に使う高さ（ワールドY）。
    [SerializeField] private float handsUpHeightThreshold = 1.5f;

    // 両手上げSEのクールダウン（秒）。
    [SerializeField] private float handsUpCooldown = 2.0f;

    // 両手上げで再生する歓声＋拍手SE（Inspectorで設定）。
    [SerializeField] private AudioClip handsUpCheerClip;

    // 拍手SEのAudioSource（未指定なら実行時に取得/生成）。
    [SerializeField] private AudioSource clapSource;

    // 拍手SEのAudioClip（Inspectorで設定）。
    [SerializeField] private AudioClip clapClip;

    // 拍手動作を外部アプリへ通知するかどうか。
    [SerializeField] private bool notifyOnClap = true;

    // 通知先のURL（Sobal-Classic-3のサーバーなど）。
    [SerializeField] private string notifyUrl = "http://localhost:3000/api/bell";

    // 通知の連続送信を防ぐクールダウン（秒）。
    [SerializeField] private float notifyCooldown = 0.5f;

    // 通知を送るために必要な「連続拍手」回数。
    [SerializeField] private int notifyRequiredConsecutiveClaps = 2;

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
    [SerializeField] private Vector2 debugPosition = new Vector2(10f, -360f);

    // 背景の余白。
    [SerializeField] private Vector2 debugPadding = new Vector2(6f, 4f);

    // 直近の拍手再生時刻（クールダウン判定用）。
    private float lastClapTime = -999f;

    // 直前フレームで距離が閾値以内だったかどうか。
    private bool wasWithinThreshold;

    // デバッグ表示の初期化済みフラグ。
    private bool debugInitialized;

    // デバッグ表示用の内部状態。
    private float lastDistance;
    private string lastStatus = "None";
    private int detectedCount;
    private int playedCount;
    private int consecutiveClapCount;

    private float lastHandsUpTime = -999f;
    private bool wasHandsUp;
    private string lastHandsUpStatus = "None";
    private int handsUpDetectedCount;
    private int handsUpPlayedCount;

    // 直近の通知送信時刻（連続送信防止）。
    private float lastNotifyTime = -999f;

    // 直近の拍手検知時刻を保持する（時間ウィンドウ判定用）。
    private readonly List<float> clapDetectedTimes = new List<float>();

    private void Start()
    {
        // デバッグ表示が必要なら初期化しておく。
        if (showDebugOverlay)
        {
            EnsureDebugOverlay();
        }
    }

    private void Update()
    {
        // Mocopiの受信前なら判定を止める（誤検知抑止）。
        if (mocopiAvatar != null && !mocopiAvatar.IsFirstMotionReceived)
        {
            lastStatus = "NotReady";
            UpdateDebugOverlay();
            return;
        }

        // 片手でも未設定なら判定できない。
        if (rightHand == null || leftHand == null)
        {
            lastStatus = "MissingHand";
            UpdateDebugOverlay();
            return;
        }

        // 現在の手同士の距離を測る。
        lastDistance = Vector3.Distance(rightHand.position, leftHand.position);
        bool withinThreshold = lastDistance <= clapDistanceThreshold;

        // 両手を上げたときのSE判定。
        CheckHandsUp();

        // 距離が閾値を超えたら「離れた」とみなして次の拍手に備える。
        if (!withinThreshold)
        {
            wasWithinThreshold = false;
            lastStatus = "Apart";
            UpdateDebugOverlay();
            return;
        }

        // 既に近接中なら連続発火を避ける。
        if (wasWithinThreshold)
        {
            lastStatus = "Holding";
            UpdateDebugOverlay();
            return;
        }

        // クールダウン中なら再生しない。
        if (Time.time - lastClapTime < clapCooldown)
        {
            lastStatus = "Cooldown";
            UpdateDebugOverlay();
            return;
        }

        // 検知カウントを増やす。
        detectedCount++;
        lastStatus = "Detected";

        // 今回の検知時刻を追加し、ウィンドウ外の古い検知を削除する。
        clapDetectedTimes.Add(Time.time);
        for (int i = clapDetectedTimes.Count - 1; i >= 0; i--)
        {
            if (Time.time - clapDetectedTimes[i] > consecutiveClapWindow)
            {
                clapDetectedTimes.RemoveAt(i);
            }
        }

        // 現在のウィンドウ内の検知回数を更新する。
        consecutiveClapCount = clapDetectedTimes.Count;

        // ここで「両手を合わせた」通知を送る（再生とは独立）。
        // 連続回数の条件を満たした時のみ送信する。
        TryNotifyClap();

        // 連続回数が足りない場合は再生せず、次回の検知を待つ。
        if (consecutiveClapCount < requiredConsecutiveClaps)
        {
            wasWithinThreshold = true;
            UpdateDebugOverlay();
            return;
        }

        // AudioSourceが無ければ準備する。
        EnsureClapSource();
        if (clapSource == null)
        {
            lastStatus = "NoAudioSource";
            UpdateDebugOverlay();
            return;
        }

        // クリップ未設定なら再生できない。
        if (clapClip == null)
        {
            lastStatus = "NoClip";
            UpdateDebugOverlay();
            return;
        }

        // 動作が無効なら判定のみ行い、再生は抑止する。
        if (!GestureActionToggle.ActionsEnabled)
        {
            lastClapTime = Time.time;
            lastStatus = "Suppressed";
            wasWithinThreshold = true;
            consecutiveClapCount = 0;
            clapDetectedTimes.Clear();
            UpdateDebugOverlay();
            return;
        }

        // SEを再生し、時刻と状態を更新する。
        clapSource.PlayOneShot(clapClip);
        lastClapTime = Time.time;
        playedCount++;
        lastStatus = "Played";

        // 今回は近接状態として記録する。
        wasWithinThreshold = true;
        consecutiveClapCount = 0;
        clapDetectedTimes.Clear();

        // デバッグ表示を更新する。
        UpdateDebugOverlay();
    }

    private void CheckHandsUp()
    {
        bool handsUp = rightHand.position.y >= handsUpHeightThreshold &&
                       leftHand.position.y >= handsUpHeightThreshold;

        if (!handsUp)
        {
            wasHandsUp = false;
            lastHandsUpStatus = "Down";
            return;
        }

        if (wasHandsUp)
        {
            lastHandsUpStatus = "Holding";
            return;
        }

        handsUpDetectedCount++;
        lastHandsUpStatus = "Detected";

        if (Time.time - lastHandsUpTime < handsUpCooldown)
        {
            lastHandsUpStatus = "Cooldown";
            wasHandsUp = true;
            return;
        }

        EnsureClapSource();
        if (clapSource == null)
        {
            lastHandsUpStatus = "NoAudioSource";
            wasHandsUp = true;
            return;
        }

        if (handsUpCheerClip == null)
        {
            lastHandsUpStatus = "NoHandsUpClip";
            wasHandsUp = true;
            return;
        }

        // 動作が無効なら判定のみ行い、再生は抑止する。
        if (!GestureActionToggle.ActionsEnabled)
        {
            lastHandsUpTime = Time.time;
            lastHandsUpStatus = "Suppressed";
            wasHandsUp = true;
            return;
        }

        clapSource.PlayOneShot(handsUpCheerClip);
        lastHandsUpTime = Time.time;
        handsUpPlayedCount++;
        lastHandsUpStatus = "Played";
        wasHandsUp = true;
    }

    private void EnsureClapSource()
    {
        // Inspector未設定時は同一GameObjectから取得する。
        if (clapSource == null)
        {
            clapSource = GetComponent<AudioSource>();
        }

        // AudioSourceが無い場合は追加して使う。
        if (clapSource == null)
        {
            clapSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void TryNotifyClap()
    {
        // 通知が不要なら何もしない。
        if (!notifyOnClap)
        {
            return;
        }

        // 連続回数が足りない場合は通知しない。
        if (consecutiveClapCount < notifyRequiredConsecutiveClaps)
        {
            return;
        }

        // URLが未設定なら通知を行わない。
        if (string.IsNullOrWhiteSpace(notifyUrl))
        {
            return;
        }

        // 送信クールダウン中ならスキップする。
        if (Time.time - lastNotifyTime < notifyCooldown)
        {
            return;
        }

        lastNotifyTime = Time.time;
        StartCoroutine(SendClapNotification());
    }

    private IEnumerator SendClapNotification()
    {
        // シンプルなJSONで通知する（サーバー側のログに残しやすくする）。
        string payload = "{\"source\":\"unity\",\"event\":\"clap\",\"time\":" + Time.time.ToString("F3") + "}";
        using (UnityWebRequest request = new UnityWebRequest(notifyUrl, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(payload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // 失敗してもゲーム側の挙動は止めない。
            yield return request.SendWebRequest();
        }
    }

    private void UpdateDebugOverlay()
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

        float cooldownRemaining = Mathf.Max(0f, clapCooldown - (Time.time - lastClapTime));

        // 表示用に手の名前を事前に確定しておく（構文の複雑化を避ける）。
        string rightHandName = rightHand != null ? rightHand.name : "None";
        string leftHandName = leftHand != null ? leftHand.name : "None";

        // 画面表示の文言を組み立てる。
        debugText.text =
            "Clap Distance Debug\n" +
            "HandsUp: " + lastHandsUpStatus + " heightY=" + handsUpHeightThreshold.ToString("F2") + "\n" +
            "HandsUp Counts: detected=" + handsUpDetectedCount + " played=" + handsUpPlayedCount + " cooldown=" + handsUpCooldown.ToString("F2") + "s\n" +
            "Status: " + lastStatus + "\n" +
            "Distance: " + lastDistance.ToString("F3") + "  Threshold: " + clapDistanceThreshold.ToString("F3") + "\n" +
            "CooldownRemaining: " + cooldownRemaining.ToString("F2") + "s\n" +
            "Counts: detected=" + detectedCount + " played=" + playedCount + "\n" +
            "Consecutive: " + consecutiveClapCount + " / " + requiredConsecutiveClaps + " window=" + consecutiveClapWindow.ToString("F2") + "s\n" +
            "Hands: R=" + rightHandName + " L=" + leftHandName;
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
            GameObject textObject = new GameObject("ClapDistanceDebugText");
            textObject.transform.SetParent(debugCanvas.transform, false);

            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = debugPosition;
            rectTransform.sizeDelta = new Vector2(900f, 120f);

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
            GameObject backgroundObject = new GameObject("ClapDistanceDebugBackground");
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
