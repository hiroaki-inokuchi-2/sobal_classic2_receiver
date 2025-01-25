using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageAnimation : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;

    // Start is called before the first frame update
    void Start()
    {
        StartAnimation();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartAnimation()
    {
        Debug.Log("StartAnimation()");
        StartCoroutine("TurningReverse");

    }

    IEnumerator Turning()
    {
        Debug.Log("Turning()");
        for (int i = 0; i < 40; i++)
        {
            targetObject.transform.position += new Vector3(50f, 0, 0);
            yield return null;
        }
    }

    IEnumerator TurningReverse()
    {
        Debug.Log("TurningReverse()");
        for (int i = 0; i < 40; i++)
        {
            targetObject.transform.position += new Vector3(-50f, 0, 0);
            yield return null;
        }
    }
}
