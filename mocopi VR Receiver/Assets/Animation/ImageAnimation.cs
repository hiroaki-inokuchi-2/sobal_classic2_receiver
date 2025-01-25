using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageAnimation : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;

    // Start is called before the first frame update
    void Start()
    {
        StartTurning(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartTurning(bool isReverse)
    {
        Debug.Log("StartTurning(): isReverse = " + isReverse);
        StartCoroutine(Turning(isReverse));
    }

    IEnumerator Turning(bool isReverse)
    {
        float moveValue;
        if (isReverse)
        {
            moveValue = 50f;
        }
        else
        {
            moveValue = -50f;
        }

        for (int i = 0; i < 40; i++)
        {
            targetObject.transform.localPosition += new Vector3(moveValue, 0, 0);
            yield return null;
        }
    }
}
