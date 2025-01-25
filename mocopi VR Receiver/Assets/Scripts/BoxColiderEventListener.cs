using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxColiderEventListener : MonoBehaviour
{
    public void OnHitWristLeftEvent(Collider col) 
    {
        if (col.name == "human_low:_l_hand") 
        {
            Debug.Log("Hit WRIST L");
        }
    }

    public void OnHitWristRightEvent(Collider col) 
    {
        if (col.name == "human_low:_r_hand") 
        {
            Debug.Log("Hit WRIST R");
        }
    }

    public void OnHitAnkleLeftEvent(Collider col) 
    {
        if (col.name == "human_low:_l_low_leg")
        {
            Debug.Log("Hit ANKLE L");
        }
    }

    public void OnHitAnkleRightEvent(Collider col) 
    {
        if (col.name == "human_low:_r_low_leg") 
        {
            Debug.Log("Hit ANKLE R");
        }
    }
}
