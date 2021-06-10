using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonSample : MonoBehaviour
{
    public struct NumStorage
    {
        public Dictionary<string, int> numTable;

        public NumStorage(Dictionary<string, int> _numTable)
        {
            numTable = new Dictionary<string, int>(_numTable);

        }
    }

    private Dictionary<string, int> mainNumTable;
    private NumStorage numStorageA;
    private NumStorage numStorageB;

    public void BTN_Test()
    {
        FunctionB(10);
    }

    public void FunctionA()
    {
        Debug.Log("FunctionA");
    }

    public void FunctionB(int a)
    {
        Debug.Log("FunctionB = " + a);
    }
}
