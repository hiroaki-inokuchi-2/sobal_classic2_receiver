using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxColiderEventListener : MonoBehaviour
{
    public void OnHitWristLeftEvent(Collider col) 
    {
        if (col.name == "LeftHand") 
        {
            Debug.Log("Hit WRIST L");
        }
    }

    public void OnHitWristRightEvent(Collider col) 
    {
        if (col.name == "RightHand") 
        {
            Debug.Log("Hit WRIST R");
        }
    }

    public void OnHitAnkleLeftEvent(Collider col) 
    {
        if (col.name == "LeftLeg")
        {
            Debug.Log("Hit ANKLE L");
        }
    }

    public void OnHitAnkleRightEvent(Collider col) 
    {
        if (col.name == "RightLeg") 
        {
            Debug.Log("Hit ANKLE R");
        }
    }
}
