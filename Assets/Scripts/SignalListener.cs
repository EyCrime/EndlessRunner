using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class SignalListener : MonoBehaviour
{
    public SignalObject signalObject;
    public UnityEvent signalEvent;
    public void OnSignalRaised()
    {
        signalEvent.Invoke();
    }
    private void OnEnable()
    {
        signalObject.RegisterListener(this);
    }
    private void OnDisable()
    {
        signalObject.DeregisterListener(this);
    }
}
