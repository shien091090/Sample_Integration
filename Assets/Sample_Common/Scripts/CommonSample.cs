using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CommonSample : MonoBehaviour
{
    public Button button;

    public void BTN_A()
    {
        Debug.Log("BTN_A");
        Destroy(button.gameObject);
    }

    public void BTN_B()
    {
        Debug.Log("BTN_B");
    }

}
