using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NTPTimeTester : MonoBehaviour
{

    public void BTN_NTPTest()
    {
        StartCoroutine(NTPTiming.Instance.Cor_GetNTPTime());
    }


}
