﻿using UnityEngine;
using UnityEngine.Events;

public class TriggerEventSource : MonoBehaviour
{
    public event TriggerEventSourceEventHandler OnTrigger;

    private void OnTriggerEnter(Collider other)
    {
        OnTrigger.Invoke(other.gameObject);
    }
}

public delegate void TriggerEventSourceEventHandler(GameObject other);
