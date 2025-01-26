using System.Collections;
using System.Collections.Generic;
using Mocopi.Receiver;
using UnityEngine;

public class BoxColiderEventListener : MonoBehaviour
{
    // 画像スライド用クラス
    [SerializeField]
    private ImageAnimation _imageAnimation;

    [SerializeField]
    private MocopiAvatar _mocopiAvatar;

    public void OnHitWristLeftEvent(Collider col) 
    {
        if (this._mocopiAvatar.IsFirstMotionReceived && col.name == "LeftHand") 
        {
            // 左手を横に伸ばすと次のスライドへ
            Debug.Log("Hit WRIST L");
            StartCoroutine(this._imageAnimation.Slide(1));
        }
    }

    public void OnHitWristRightEvent(Collider col) 
    {
        if (this._mocopiAvatar.IsFirstMotionReceived && col.name == "RightHand") 
        {
            // 右手を伸ばすと前のスライドへ
            Debug.Log("Hit WRIST R");
            StartCoroutine(this._imageAnimation.Slide(-1));
        }
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
}
