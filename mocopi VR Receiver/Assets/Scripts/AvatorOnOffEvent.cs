using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatorOnOffEvent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.planeDistance = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ShowAvator();
        }
    }

    public void ShowAvator()
    {
        Canvas canvas = GetComponent<Canvas>();

        if (canvas.planeDistance == 100)
        {
            return;
        }

        Debug.Log("ShowAvator()");
        canvas.planeDistance = 100;
    }
}
