using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class BestDictionary
{
    public class Extracting
    {
        public List<int> IndexList { private set; get; }
        public List<object> BodyList { private set; get; }

        public Extracting(List<int> _indexList, List<object> _bodyList)
        {
            IndexList = _indexList;
            BodyList = _bodyList;
        }
    }

    private List<object>[] valueGroup;
    private Dictionary<string, int> keyIndexTable;
    public int Width { private set; get; }

    public BestDictionary(params string[] keyNames)
    {
        Width = keyNames.Length;
        valueGroup = new List<object>[Width];

        keyIndexTable = new Dictionary<string, int>();
        for (int i = 0; i < Width; i++)
        {
            keyIndexTable.Add(keyNames[i], i);
        }
    }

    public void Add(params object[] valueTicket)
    {
        if (valueTicket.Length != Width)
            throw new System.OverflowException("Over Length");

        for (int i = 0; i < Width; i++)
        {
            if (valueGroup[i] == null)
                valueGroup[i] = new List<object>();

            valueGroup[i].Add(valueTicket[i]);
        }

    }

    public object GetElement(object searchKey, string parallelKey)
    {
        for (int i = 0; i < valueGroup.Length; i++)
        {
            if (valueGroup[i].Contains(searchKey))
            {
                int _index = valueGroup[i].IndexOf(searchKey);

                if (!keyIndexTable.ContainsKey(parallelKey))
                    return null;

                return valueGroup[keyIndexTable[parallelKey]][_index];
            }
        }

        return null;
    }

    public object GetElement(int index, string parallelKey)
    {
        if (!keyIndexTable.ContainsKey(parallelKey))
            return null;

        if (valueGroup[keyIndexTable[parallelKey]].Count <= index)
            return null;

        return valueGroup[keyIndexTable[parallelKey]][index];
    }

    public object GetElement(int index, int parallelIndex)
    {
        if (valueGroup.Length <= parallelIndex)
            return null;

        if (valueGroup[parallelIndex].Count <= index)
            return null;

        return valueGroup[parallelIndex][index];
    }

    public bool CheckUniform(bool debugPrint = false)
    {
        if (valueGroup == null)
            return true;

        if (valueGroup.Length <= 0)
            return true;

        int _listCount = valueGroup[0].Count;
        for (int i = 1; i < valueGroup.Length; i++)
        {
            if (_listCount != valueGroup[i].Count)
                return false;
        }

        if (_listCount == 0)
            return true;

        bool _uniformState = true;

        List<Type> _types = new List<Type>();
        string[] _keyNames = keyIndexTable.Keys.ToArray();

        for (int i = 0; i < _listCount; i++)
        {
            string _parallelElement = string.Empty;
            for (int j = 0; j < valueGroup.Length; j++)
            {
                if (i == 0)
                    _types.Add(valueGroup[j][i].GetType());
                else if (i > 0 && valueGroup[j][i].GetType() != _types[j])
                    _uniformState = false;

                _parallelElement += string.Format("{0}({1})", _keyNames[j], valueGroup[j][i].GetType());

                if (j < valueGroup.Length - 1)
                    _parallelElement += ", ";
            }

            if (debugPrint)
                Debug.Log(string.Format("[{0}] -- {1}", i, _parallelElement));
        }

        return _uniformState;
    }

    public Extracting ExtractToList(string extractKey, params object[] blockItems)
    {
        if (!keyIndexTable.ContainsKey(extractKey))
            return null;

        List<object> _bodyList = new List<object>();
        List<int> _indexList = new List<int>();

        List<object> _extractList = valueGroup[keyIndexTable[extractKey]];
        for (int i = 0; i < _extractList.Count; i++)
        {
            if (blockItems.Contains(_extractList[i]))
                continue;

            _bodyList.Add(_extractList[i]);
            _indexList.Add(i);
        }

        Extracting _result = new Extracting(_indexList, _bodyList);
        return _result;
    }
}
