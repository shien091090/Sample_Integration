using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

namespace SNShien.Common.DataTools
{
    public class RandomDatasCreator
    {
        private static List<int> charNumberRecords;

        public static List<T> CreateRandomValueList<T>(int count, int elementLength)
        {
            if (count <= 0 || elementLength <= 0)
                return null;

            List<T> _result = new List<T>();

            int _errorTimes = 0;
            for (int i = 0; i < count; i++)
            {
                if (_errorTimes > 3)
                    throw new Exception("RandomElement Function Error");

                string _elementContent = string.Empty;

                for (int j = 0; j < elementLength; j++)
                {
                    T _unit = default;

                    if (GetRandomIndivisible(out _unit))
                    {
                        _elementContent += _unit.ToString();
                    }
                    else
                    {
                        i--;
                        _errorTimes++;
                        break;
                    }
                }

                if (_elementContent.Length == elementLength)
                {
                    T _element = (T)Convert.ChangeType(_elementContent, typeof(T));
                    _result.Add(_element);
                }
                    
            }

            return _result;
        }

        private static void SetCharNumbers()
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
                    UnityEngine.Debug.Log(_exception);
                    continue;
                }

            }
        }

        private static bool GetRandomIndivisible<T>(out T value)
        {
            bool _repeatError = false;
            while (true)
            {
                try
                {
                    if (typeof(T) == typeof(int))
                    {
                        int _result = UnityEngine.Random.Range(0, 10);
                        value = (T)Convert.ChangeType(_result, typeof(T));
                        return true;
                    }
                    else if (typeof(T) == typeof(string))
                    {
                        if (charNumberRecords == null || charNumberRecords.Count <= 0)
                            SetCharNumbers();

                        int _dice = UnityEngine.Random.Range(0, charNumberRecords.Count);
                        int _charNum = charNumberRecords[_dice];

                        string _result = char.ConvertFromUtf32(_charNum);

                        value = (T)Convert.ChangeType(_result, typeof(T));
                        return true;
                    }

                    value = default;
                    return false;
                }
                catch (Exception _exception)
                {
                    UnityEngine.Debug.Log(_exception);

                    if (_repeatError)
                    {
                        value = default;
                        return false;
                    }
                        
                    _repeatError = true;
                    continue;
                }
            }
        }
    }
}