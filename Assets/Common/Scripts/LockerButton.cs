using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class LockerButton : Button
{
    public float waitSeconds;
    public bool isPrintDebugLog;
    public bool IsUILock { private set; get; }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (IsUILock)
            return;
        else
            IsUILock = true;

        base.OnPointerClick(eventData);

        try
        {
            WaitForUnlock(waitSeconds);
        }
        catch (Exception e)
        {
            Debug.Log("LockerButton Error : " + e);
            Unlock();
        }
    }

    private void Unlock()
    {
        PrintDebugLog(string.Format("gameObject : {0} , is Unlock", gameObject.name));
        IsUILock = false;
    }

    private void WaitForUnlock(float sec)
    {
        StartCoroutine(Cor_WaitTimes(sec, Unlock));
    }

    private IEnumerator Cor_WaitTimes(float sec, Action callback)
    {
        PrintDebugLog(string.Format("gameObject : {0} , WaitForSeconds = {1}", gameObject.name, sec));
        yield return new WaitForSeconds(waitSeconds);
        PrintDebugLog(string.Format("gameObject : {0} , WaitForSeconds Is TimeOut", gameObject.name, sec));

        callback.Invoke();
    }

    private void PrintDebugLog(string log)
    {
        if (!isPrintDebugLog)
            return;

        Debug.Log("[UILocker] " + log);
    }
}
