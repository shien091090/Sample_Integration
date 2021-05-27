using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DataSearchingManager : MonoBehaviour
{
    public int elementListCount = 10;
    public int elementLength = 3;
    public MyStopwatch.TimeUnit stopwatchTimeUnit;

    public List<string> strList;
    private List<int> intList;

    private List<int> charNumberRecords;

    void Start()
    {
        SetCharNumbers();
    }

    public void BTN_Test()
    {
        MyStopwatch.TimerTest(() =>
        {
            strList = CreateRandomStringList(elementListCount, elementLength);
        }, stopwatchTimeUnit, "CreateRandomStringList");


    }

    private void SetCharNumbers()
    {
        string _charLine = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        char[] _charGroup = _charLine.ToCharArray();

        charNumberRecords = new List<int>();
        for (int i = 0; i < _charGroup.Length; i++)
        {
            try
            {
                int _charNumber = Convert.ToInt32(_charGroup[i]);
                charNumberRecords.Add(_charNumber);
            }
            catch (Exception _exception)
            {
                Debug.Log(_exception);
                continue;
            }

        }
    }

    private string GetRandomChar()
    {
        string _result = string.Empty;

        if (charNumberRecords == null || charNumberRecords.Count <= 0)
            return _result;

        bool _repeatError = false;
        while (true)
        {
            try
            {
                int _dice = UnityEngine.Random.Range(0, charNumberRecords.Count);
                int _charNum = charNumberRecords[_dice];

                string _char = char.ConvertFromUtf32(_charNum);

                return _char;
            }
            catch (Exception _exception)
            {
                Debug.Log(_exception);

                if (_repeatError)
                    return _result;

                _repeatError = true;
                continue;
            }
        }
    }

    private List<string> CreateRandomStringList(int count, int elementLength)
    {
        if (count <= 0 || elementLength <= 0)
            return null;

        List<string> _result = new List<string>();

        int _errorTimes = 0;
        for (int i = 0; i < count; i++)
        {
            if (_errorTimes > 3)
                throw new Exception("RandomElement Function Error");

            string _element = string.Empty;

            for (int j = 0; j < elementLength; j++)
            {
                string _char = GetRandomChar();

                if (string.IsNullOrEmpty(_char))
                {
                    i--;
                    _errorTimes++;
                    break;
                }
                else
                    _element += _char;

            }

            if (_element.Length == elementLength)
                _result.Add(_element);
        }

        return _result;
    }
}

