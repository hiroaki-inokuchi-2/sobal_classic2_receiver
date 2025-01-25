using UnityEngine;
using UnityEngine.UI;

public class SlideOperationScript : MonoBehaviour
{
    // パネルのImageコンポーネント (Inspector で設定)
    public Image panelImage;

    // スライドとして使用する画像の配列 (Inspector で設定)
    public Sprite[] slideSprites;

    // 現在表示しているスライドのインデックス
    private int currentSlideIndex = 0;

    void Start()
    {
        // パネルのImageが設定されていない場合はエラーを出力
        if (panelImage == null)
        {
            Debug.LogError("PanelのImageコンポーネントが設定されていません。");
            return;
        }

        // スライド画像が存在する場合、最初のスライドを表示
        if (slideSprites.Length > 0)
        {
            panelImage.sprite = slideSprites[0];
        }
        else
        {
            Debug.LogWarning("スライド画像が設定されていません。");
        }
    }

    void Update()
    {
        // 右矢印キー (→) が押されたら次のスライドへ
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextSlide();
        }

        // 左矢印キー (←) が押されたら前のスライドへ
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PreviousSlide();
        }
    }

    // 次のスライドへ切り替えるメソッド（最後のスライドでは止まる）
    public void NextSlide()
    {
        // スライドが1枚もない、または最後のスライドなら何もしない
        if (slideSprites.Length == 0 || currentSlideIndex >= slideSprites.Length - 1)
        {
            Debug.Log("これ以上次のスライドはありません。");
            return;
        }

        // インデックスを1つ進める
        currentSlideIndex++;
        panelImage.sprite = slideSprites[currentSlideIndex];
    }

    // 前のスライドへ切り替えるメソッド（最初のスライドでは止まる）
    public void PreviousSlide()
    {
        // スライドが1枚もない、または最初のスライドなら何もしない
        if (slideSprites.Length == 0 || currentSlideIndex <= 0)
        {
            Debug.Log("これ以上前のスライドはありません。");
            return;
        }

        // インデックスを1つ戻す
        currentSlideIndex--;
        panelImage.sprite = slideSprites[currentSlideIndex];
    }
}