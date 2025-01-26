using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ImageAnimation : MonoBehaviour
{
    private float SLIDE_WIDTH = 1920f; // **スライドの幅 (画面幅)**
    
    // **スライド用の3つのパネル (UIオブジェクト)**
    [SerializeField] private GameObject[] slides = new GameObject[3]; // **3つのスライド**
    [SerializeField] private Image[] slideImages = new Image[3]; // **スライドの画像**
    
    public Sprite[] slideSprites; // **スライド用の画像リスト**
    private int currentImageIndex = 0; // **現在表示中のスライド画像のインデックス**
    private bool isSliding = false; // **スライド中かどうかのフラグ**

    void Start()
    {
        SLIDE_WIDTH = Screen.currentResolution.width;

        // **スライドの初期位置を設定**
        slides[0].transform.localPosition = new Vector3(-SLIDE_WIDTH, 0, 0); // 左
        slides[1].transform.localPosition = Vector3.zero; // 中央
        slides[2].transform.localPosition = new Vector3(SLIDE_WIDTH, 0, 0); // 右

        // **最初のスライド画像をセット**
        if (slideSprites.Length > 0)
        {
            slideImages[0].sprite = (currentImageIndex > 0) ? slideSprites[currentImageIndex - 1] : null;
            slideImages[1].sprite = slideSprites[currentImageIndex];
            slideImages[2].sprite = (currentImageIndex < slideSprites.Length - 1) ? slideSprites[currentImageIndex + 1] : null;
        }
    }

    void Update()
    {
        if (isSliding) return;

        // **右矢印キーで前のスライドへ**
        if (Input.GetKeyDown(KeyCode.RightArrow) && currentImageIndex > 0)
        {
            StartCoroutine(Slide(-1));
        }
        // **左矢印キーで次のスライドへ**
        if (Input.GetKeyDown(KeyCode.LeftArrow) && currentImageIndex < slideSprites.Length - 1)
        {
            StartCoroutine(Slide(1));
        }
    }

    /// <summary>
    /// **スライドを行うアニメーション**
    /// `direction` が 1 の場合、右へスライド（次の画像へ）
    /// `direction` が -1 の場合、左へスライド（前の画像へ）
    /// </summary>
    public IEnumerator Slide(int direction)
    {
        if (isSliding) yield break;
        isSliding = true;

        float duration = 0.2f; // **スライド時間（秒）**
        float elapsedTime = 0f;

        // **スライド前の位置を記録**
        Vector3[] startPositions = slides.Select(s => s.transform.localPosition).ToArray();
        Vector3[] targetPositions = new Vector3[3];

        for (int i = 0; i < 3; i++)
        {
            targetPositions[i] = startPositions[i] + new Vector3(-SLIDE_WIDTH * direction, 0, 0);
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            for (int i = 0; i < 3; i++)
            {
                slides[i].transform.localPosition = Vector3.Lerp(startPositions[i], targetPositions[i], t);
            }

            yield return null;
        }

        // **スライド完了後の更新**
        UpdateSlidesAfterMove(direction);
        isSliding = false;
    }

    /// <summary>
    /// **スライド後にオブジェクトの位置と画像を更新**
    /// `direction` が 1 の場合、左へスライド（次の画像へ）
    /// `direction` が -1 の場合、右へスライド（前の画像へ）
    /// </summary>
    void UpdateSlidesAfterMove(int direction)
{
    if (direction == 1) // **左へスライド（次の画像へ）**
    {
        // **現在の `slides[0]` を右端に移動**
        slides[0].transform.localPosition = new Vector3(SLIDE_WIDTH, 0, 0);

        // **スライドと画像の順番を同時に入れ替える**
        (slides[0], slides[1], slides[2]) = (slides[1], slides[2], slides[0]);
        (slideImages[0], slideImages[1], slideImages[2]) = (slideImages[1], slideImages[2], slideImages[0]);

        // **現在のスライド画像を更新**
        currentImageIndex++;

        // **新しい `slides[2]` に次の画像をセット**
        if (currentImageIndex < slideSprites.Length - 1)
        {
            slideImages[2].sprite = slideSprites[currentImageIndex + 1];
        }
        else
        {
            slideImages[2].sprite = null; // 画像がない場合は空白に
        }
    }
    else if (direction == -1) // **右へスライド（前の画像へ）**
    {
        // **現在の `slides[2]` を左端に移動**
        slides[2].transform.localPosition = new Vector3(-SLIDE_WIDTH, 0, 0);

        // **スライドと画像の順番を同時に入れ替える**
        (slides[2], slides[1], slides[0]) = (slides[1], slides[0], slides[2]);
        (slideImages[2], slideImages[1], slideImages[0]) = (slideImages[1], slideImages[0], slideImages[2]);

        // **現在のスライド画像を更新**
        currentImageIndex--;

        // **新しい `slides[0]` に前の画像をセット**
        if (currentImageIndex > 0)
        {
            slideImages[0].sprite = slideSprites[currentImageIndex - 1];
        }
        else
        {
            slideImages[0].sprite = null; // 画像がない場合は空白に
        }
    }
}
}
