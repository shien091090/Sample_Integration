using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollMapManager : MonoBehaviour
{
    public ScrollMap scrollMap;
    public RectTransform[] lockObjects;

    public int lockNumber;

    private void Start()
    {
        if (scrollMap != null && lockObjects != null)
            scrollMap.SetLockPos(lockObjects);
    }

    [ContextMenu("Unlock")]
    public void TEST_Unlock()
    {
        scrollMap.Unlock(lockNumber);
    }

}
