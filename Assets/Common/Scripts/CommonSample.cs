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
        mainNumTable = new Dictionary<string, int>();
        mainNumTable.Add("A", 1);
        mainNumTable.Add("B", 3);

        numStorageA = new NumStorage(mainNumTable);
        numStorageB = new NumStorage(mainNumTable);

        mainNumTable["A"] = 10;

        Debug.Log(numStorageA.numTable["A"]);

        numStorageA.numTable["B"] = 15;

        Debug.Log(numStorageB.numTable["B"]);
    }
}
