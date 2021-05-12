using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonSample : MonoBehaviour
{
    public enum ParamType
    {
        INT,
        STRING
    }

    public ParamType type;
    public int param_int;
    public string param_str;

    public void BTN_Test()
    {
        switch (type)
        {
            case ParamType.INT:
                Debug.Log(IsDefault(param_int));
                break;

            case ParamType.STRING:
                Debug.Log(IsDefault(param_str));
                break;
        }

    }

    public bool IsDefault<T>(T v)
    {
        if (v.GetType() == typeof(string))
        {
            return (v.Equals(null) || v.Equals(string.Empty) || v.Equals(""));
        }
        else
        {
            return (v.Equals(default(T)));
        }

    }
}
