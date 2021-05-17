using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonSample : MonoBehaviour
{

    public enum ActionType
    {
        Get,
        Catch,
        Kick,
        Throw
    }

    public void BTN_Test()
    {
        BestDictionary _dict = new BestDictionary("Name", "Type", "ID");
        _dict.Add("Peter", ActionType.Catch, 100001);
        _dict.Add("Andy", ActionType.Throw, 100002);
        _dict.Add("Cindy", ActionType.Get, 100003);
        _dict.Add("Shien", ActionType.Kick, 100004);
        _dict.Add("Glen", ActionType.Catch, 100005);

        bool _checkUniform = _dict.CheckUniform();
        Debug.Log("CheckUniform = " + _checkUniform);

        BestDictionary.Extracting _ex = _dict.ExtractToList("Type", ActionType.Throw, ActionType.Catch);
        for (int i = 0; i < _ex.BodyList.Count; i++)
        {
            Debug.Log(string.Format("[{0}] = {1}", _ex.IndexList[i], _ex.BodyList[i]));
        }
    }
}
