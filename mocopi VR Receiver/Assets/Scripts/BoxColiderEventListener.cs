using System.Collections;
using System.Collections.Generic;
using Mocopi.Receiver;
using UnityEngine;
using UnityEngine.UI;

public class BoxColiderEventListener : MonoBehaviour
{
    // 画像スライド用クラス
    [SerializeField]
    private ImageAnimation _imageAnimation;

    [SerializeField]
    private MocopiAvatar _mocopiAvatar;

    [SerializeField]
    private LaserPointerScript _lazerPointer;

    // 拍手SEのAudioSource（未指定なら実行時に取得/生成）
    [SerializeField]
    private AudioSource _clapSource;

    // 拍手SEのAudioClip（Inspectorで設定）
    [SerializeField]
    private AudioClip _clapClip;

    // 連続発火を防ぐクールダウン（秒）
    [SerializeField]
    private float _clapCooldown = 0.4f;

    // 拍手デバッグ表示のオン/オフ
    [SerializeField]
    private bool _showClapDebugOverlay = false;

    // デバッグ表示を載せるCanvas（未指定なら自動検索）
    [SerializeField]
    private Canvas _clapDebugCanvas;

    // デバッグ表示用のText（未指定なら自動生成）
    [SerializeField]
    private Text _clapDebugText;

    // デバッグ表示の背景（未指定なら自動生成）
    [SerializeField]
    private Image _clapDebugBackground;

    // デバッグ表示の文字色
    [SerializeField]
    private Color _clapDebugTextColor = Color.yellow;

    // デバッグ背景色
    [SerializeField]
    private Color _clapDebugBackgroundColor = new Color(0f, 0f, 0f, 0.6f);

    // デバッグ表示の文字サイズ
    [SerializeField]
    private int _clapDebugFontSize = 18;

    // デバッグ表示の位置（Canvas左上基準）
    [SerializeField]
    private Vector2 _clapDebugPosition = new Vector2(10f, -220f);

    // 背景の余白
    [SerializeField]
    private Vector2 _clapDebugPadding = new Vector2(6f, 4f);

    // 直近の拍手再生時刻
    private float _lastClapTime = -999f;

    // デバッグ表示は複数インスタンスで共有する
    private static bool s_clapDebugInitialized;
    private static Text s_clapDebugText;
    private static Image s_clapDebugBackground;
    private static float s_lastClapDetectedTime = -999f;
    private static float s_lastClapPlayedTime = -999f;
    private static string s_lastClapCollider = "None";
    private static string s_lastClapStatus = "None";
    private static int s_clapDetectedCount;
    private static int s_clapPlayedCount;

    private void Start()
    {
        // デバッグ表示が必要なら初期化しておく
        if (_showClapDebugOverlay)
        {
            EnsureClapDebugOverlay();
        }
    }

    private void Update()
    {
        // 拍手の検知状況を画面に更新する
        UpdateClapDebugOverlay();
    }

    public void OnHitWristLeftEvent(Collider col) 
    {
        if (this._mocopiAvatar.IsFirstMotionReceived && col.name == "LeftHand") 
        {
            // 左手を横に伸ばすとレーザーポインターの切り替え
            Debug.Log("Hit WRIST L");
            this._lazerPointer.TogglePointer();
            //StartCoroutine(this._imageAnimation.Slide(1));
        }

        // 左手のコライダーが右手に触れたら拍手扱いにする
        TryPlayClap(col);
    }

    public void OnHitWristRightEvent(Collider col) 
    {
        if (this._mocopiAvatar.IsFirstMotionReceived && col.name == "RightHand") 
        {
            Debug.Log("Hit WRIST R");
            //StartCoroutine(this._imageAnimation.Slide(-1));
        }

        // 右手のコライダーが左手に触れたら拍手扱いにする
        TryPlayClap(col);
    }

    public void OnHitAnkleLeftEvent(Collider col) 
    {
        if (this._mocopiAvatar.IsFirstMotionReceived && col.name == "LeftLeg")
        {
            // 左手を横に伸ばすと次のスライドへ
            Debug.Log("Hit ANKLE L");
            StartCoroutine(this._imageAnimation.Slide(1));
        }
    }

    public void OnHitAnkleRightEvent(Collider col) 
    {
        if (this._mocopiAvatar.IsFirstMotionReceived && col.name == "RightLeg") 
        {
            // 右手を伸ばすと前のスライドへ
            Debug.Log("Hit ANKLE R");
            StartCoroutine(this._imageAnimation.Slide(-1));
        }
    }

    private void TryPlayClap(Collider col)
    {
        // Mocopiの初期受信前は無効にする（誤作動抑止）
        if (_mocopiAvatar == null || !_mocopiAvatar.IsFirstMotionReceived)
        {
            // 受信準備ができていないことをデバッグ表示に記録する
            RecordClapDetected(col, "NotReady");
            return;
        }

        // コライダーが渡ってこない場合は検知失敗として記録する
        if (col == null)
        {
            RecordClapDetected(col, "NullCollider");
            return;
        }

        // 右手/左手以外との衝突は拍手扱いにしない
        if (!IsClapCollider(col))
        {
            // 手以外との衝突をデバッグ表示に記録する
            RecordClapDetected(col, "NotWrist");
            return;
        }

        // 拍手検知のデバッグ情報を記録する
        RecordClapDetected(col, "Detected");

        // 短時間の連続再生を防ぐ
        if (Time.time - _lastClapTime < _clapCooldown)
        {
            RecordClapDetected(col, "Cooldown");
            return;
        }

        EnsureClapSource();
        if (_clapSource == null)
        {
            RecordClapDetected(col, "NoAudioSource");
            return;
        }

        if (_clapClip == null)
        {
            Debug.LogWarning("Clap SE is not set. Assign an AudioClip in the inspector.");
            RecordClapDetected(col, "NoClip");
            return;
        }

        // SEを再生し、時刻を更新する
        _clapSource.PlayOneShot(_clapClip);
        _lastClapTime = Time.time;
        RecordClapPlayed(col);
    }

    private void EnsureClapSource()
    {
        // Inspector未設定時は同一GameObjectから取得する
        if (_clapSource == null)
        {
            _clapSource = GetComponent<AudioSource>();
        }

        // AudioSourceが無い場合は追加して使う
        if (_clapSource == null)
        {
            _clapSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private static bool IsClapCollider(Collider col)
    {
        if (col == null)
        {
            return false;
        }

        // 両手が触れたときだけ拍手とみなす
        return col.name == "WRIST RIGHT" || col.name == "WRIST LEFT";
    }

    private void RecordClapDetected(Collider col, string status)
    {
        // 検知情報をグローバルに保持して表示する
        s_lastClapDetectedTime = Time.time;
        s_lastClapCollider = col != null ? col.name : "None";
        s_lastClapStatus = status;
        s_clapDetectedCount++;
    }

    private void RecordClapPlayed(Collider col)
    {
        // 再生情報をグローバルに保持して表示する
        s_lastClapPlayedTime = Time.time;
        s_lastClapCollider = col != null ? col.name : "None";
        s_lastClapStatus = "Played";
        s_clapPlayedCount++;
    }

    private void UpdateClapDebugOverlay()
    {
        if (!_showClapDebugOverlay)
        {
            return;
        }

        EnsureClapDebugOverlay();
        if (s_clapDebugText == null)
        {
            return;
        }

        // 検知/再生の経過時間を計算する
        float detectedAgo = s_lastClapDetectedTime < 0f ? -1f : Time.time - s_lastClapDetectedTime;
        float playedAgo = s_lastClapPlayedTime < 0f ? -1f : Time.time - s_lastClapPlayedTime;

        // 画面表示の文言を組み立てる
        s_clapDebugText.text =
            "Clap Debug\n" +
            $"Detected: {s_lastClapDetectedTime:F2}s ago={detectedAgo:F2} status={s_lastClapStatus}\n" +
            $"Played: {s_lastClapPlayedTime:F2}s ago={playedAgo:F2}\n" +
            $"Collider: {s_lastClapCollider}\n" +
            $"Counts: detected={s_clapDetectedCount} played={s_clapPlayedCount}\n" +
            $"Cooldown: {_clapCooldown:F2}s";
    }

    private void EnsureClapDebugOverlay()
    {
        if (s_clapDebugInitialized)
        {
            return;
        }

        // Canvasが未指定ならシーンから探す
        if (_clapDebugCanvas == null)
        {
            _clapDebugCanvas = FindObjectOfType<Canvas>();
        }

        if (_clapDebugCanvas == null)
        {
            return;
        }

        if (_clapDebugText == null)
        {
            // 実行時にTextを生成して表示する
            GameObject textObject = new GameObject("ClapDebugText");
            textObject.transform.SetParent(_clapDebugCanvas.transform, false);

            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = _clapDebugPosition;
            rectTransform.sizeDelta = new Vector2(900f, 120f);

            _clapDebugText = textObject.AddComponent<Text>();
            _clapDebugText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _clapDebugText.fontSize = _clapDebugFontSize;
            _clapDebugText.color = _clapDebugTextColor;
            _clapDebugText.alignment = TextAnchor.UpperLeft;
            _clapDebugText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _clapDebugText.verticalOverflow = VerticalWrapMode.Overflow;
        }

        if (_clapDebugBackground == null && _clapDebugText != null)
        {
            // Textの背面に背景Imageを生成する
            GameObject backgroundObject = new GameObject("ClapDebugBackground");
            backgroundObject.transform.SetParent(_clapDebugText.transform.parent, false);
            backgroundObject.transform.SetAsFirstSibling();

            _clapDebugBackground = backgroundObject.AddComponent<Image>();
            _clapDebugBackground.color = _clapDebugBackgroundColor;
        }

        // 共有参照を更新する
        s_clapDebugText = _clapDebugText;
        s_clapDebugBackground = _clapDebugBackground;

        if (s_clapDebugText != null)
        {
            s_clapDebugText.color = _clapDebugTextColor;
            s_clapDebugText.fontSize = _clapDebugFontSize;
        }

        if (s_clapDebugBackground != null && s_clapDebugText != null)
        {
            RectTransform textRect = s_clapDebugText.rectTransform;
            RectTransform bgRect = s_clapDebugBackground.rectTransform;
            bgRect.anchorMin = textRect.anchorMin;
            bgRect.anchorMax = textRect.anchorMax;
            bgRect.pivot = textRect.pivot;
            bgRect.anchoredPosition = textRect.anchoredPosition;
            bgRect.sizeDelta = textRect.sizeDelta + _clapDebugPadding * 2f;
            s_clapDebugBackground.color = _clapDebugBackgroundColor;
        }

        s_clapDebugInitialized = true;
    }
}
