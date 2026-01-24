using UnityEngine;

public class GestureActionToggle : MonoBehaviour
{
    // 全てのジェスチャー動作（拍手/スワイプ/両手上げ歓声）を一括で有効/無効にする。
    // セッション中だけの切り替え想定なので、静的フラグで保持する。
    public static bool ActionsEnabled { get; private set; } = true;

    // Tabキーで切り替える想定（必要ならInspectorで変更可能）。
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    // 起動時のデフォルト状態（シーン読み込み時に1回だけ適用）。
    [SerializeField] private bool startEnabled = true;

    private static bool hasInitialized;

    private void Awake()
    {
        // どのシーンでも最初の1回だけ初期値を反映する。
        if (!hasInitialized)
        {
            ActionsEnabled = startEnabled;
            hasInitialized = true;
        }
    }

    private void Update()
    {
        // PC実行時のみTab入力を有効にする（Editor/Standalone）。
        if (!IsPcPlatform())
        {
            return;
        }

        // Tabキーが押されたら状態を反転する。
        if (Input.GetKeyDown(toggleKey))
        {
            ActionsEnabled = !ActionsEnabled;
        }
    }

    private static bool IsPcPlatform()
    {
        if (Application.isEditor)
        {
            return true;
        }

        return Application.platform == RuntimePlatform.WindowsPlayer ||
               Application.platform == RuntimePlatform.OSXPlayer ||
               Application.platform == RuntimePlatform.LinuxPlayer;
    }
}
