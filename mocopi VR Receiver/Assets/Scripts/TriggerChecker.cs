using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerChecker : MonoBehaviour
{
    public UnityEvent<Collider> OnColiderEnter;

    private void OnTriggerEnter(Collider col) 
    {
        OnColiderEnter?.Invoke(col);
    }
}
