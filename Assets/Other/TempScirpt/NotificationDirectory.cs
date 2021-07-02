using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Reflection;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using SCGLobby.DataStruct;

namespace SCGLobby
{
    public class NotificationDirectory
    {
        private enum LogicalOperator
        {
            And,
            Or
        }

        private class SubCompareResult
        {
            public LogicalOperator logicTag;
            public bool result;
        }

        //------------------------------------------------------------------

        public const string CLASS_TAG_CONDITION = "condition";
        public const string CLASS_TAG_RESULT = "result";
        public const string CHAR_TAG_IGNORE = "-";

        public const string COMPARE_TAG_RANGE = "*range";
        public const string COMPARE_TAG_GREATERTHEN = "*greaterThan";
        public const string COMPARE_TAG_SMALLERTHAN = "*smallerThan";
        public const string COMPARE_TAG_CONTAIN = "*contain";

        public const string LOGICAL_TAG_AND = "_and";
        public const string LOGICAL_TAG_OR = "_or";
        public const string SPLIT_REGEX_PATTERN = ",(?! )";

        private static Dictionary<Type, HashSet<int>> conditionClassIndexCollection;
        private static Dictionary<Type, List<int>> conditionClassColumns;
        private static List<int> resultClassColumns;
        private static List<string[]> notificationUnitStorage;

        //------------------------------------------------------------------

        public NotificationDirectory(string path)
        {
            LoadText(path);
        }

        public List<ResultData> ConditionCompare(List<ConditionData> conditions)
        {
            int[] _indexList = TypeFilter(conditions); //第一層判斷 : 是否存在指定複合類型的條件

            if (_indexList.Length <= 0)
                return null;

            List<ResultData> _resultGroup = new List<ResultData>();
            _resultGroup = CongruentFilter(_indexList, conditions); //第二層判斷 : 條件是否完全吻合

            if (_resultGroup.Count <= 0)
                return null;

            return _resultGroup;
        }

        private static void LoadText(string path)
        {
            TextAsset textData = AssetsManager.Instance.LoadAsset<TextAsset>(path);

            LoadFromBytes(textData.bytes);
        }

        private static void LoadFromBytes(byte[] bytes)
        {
            conditionClassIndexCollection = new Dictionary<Type, HashSet<int>>();
            conditionClassColumns = new Dictionary<Type, List<int>>();
            resultClassColumns = new List<int>();
            notificationUnitStorage = new List<string[]>();

            using (StreamReader sr = new StreamReader(new MemoryStream(bytes)))
            {
                string line = sr.ReadLine(); //讀第一列
                string[] paramTypeFields = line.Split(',');

                line = sr.ReadLine(); //讀第二列
                string[] classTypeFields = line.Split(',');

                for (int i = 0; i < classTypeFields.Length; i++)
                {
                    if (!IsValidString(classTypeFields[i]))
                        continue;

                    if (paramTypeFields[i] == CLASS_TAG_CONDITION)
                        AddClassTypeColumnList(conditionClassColumns, classTypeFields[i], i);
                    else if (paramTypeFields[i] == CLASS_TAG_RESULT)
                        resultClassColumns.Add(i);
                }

                if (conditionClassColumns.Count <= 0)
                    return;

                int _index = 0;
                do //讀以下所有列
                {
                    line = sr.ReadLine();
                    Regex reg = new Regex(SPLIT_REGEX_PATTERN);
                    string[] datas = reg.Split(line);
                    notificationUnitStorage.Add(datas);

                    foreach (KeyValuePair<Type, List<int>> _pair in conditionClassColumns)
                    {
                        for (int i = 0; i < _pair.Value.Count; i++)
                        {
                            if (!IsValidString(notificationUnitStorage[_index][_pair.Value[i]])) continue;

                            if (conditionClassIndexCollection.ContainsKey(_pair.Key))
                                conditionClassIndexCollection[_pair.Key].Add(_index);
                            else
                                conditionClassIndexCollection[_pair.Key] = new HashSet<int>() { _index };
                        }
                    }

                    _index++;

                } while (!sr.EndOfStream);

            }
        }

        private static void AddClassTypeColumnList(Dictionary<Type, List<int>> dictionary, string typeName, int index)
        {
            Type _type = Type.GetType(NotificationModel.CLASS_NAMESPACE + typeName);

            if (_type == null)
                return;

            if (dictionary.ContainsKey(_type))
                dictionary[_type].Add(index);
            else
                dictionary[_type] = new List<int>() { index };
        }

        private static int[] TypeFilter(List<ConditionData> conditions)
        {
            int[] _resultIndex = new int[0];
            List<HashSet<int>> _hashSetGroup = new List<HashSet<int>>();

            for (int i = 0; i < conditions.Count; i++)
            {
                Type _type = conditions[i].GetType();
                if (!conditionClassIndexCollection.ContainsKey(_type)) continue;

                _hashSetGroup.Add(new HashSet<int>(conditionClassIndexCollection[_type]));
            }

            if (_hashSetGroup.Count <= 0)
                return _resultIndex;

            HashSet<int> _intersection = new HashSet<int>(_hashSetGroup[0]);
            for (int i = 1; i < _hashSetGroup.Count; i++)
            {
                _intersection.UnionWith(_hashSetGroup[i]);
            }

            if (_intersection.Count >= 1)
            {
                _resultIndex = new int[_intersection.Count];
                _intersection.CopyTo(_resultIndex);
            }

            return _resultIndex;
        }

        private static List<ResultData> CongruentFilter(int[] indexArray, List<ConditionData> conditions)
        {
            List<ResultData> _result = new List<ResultData>();

            for (int i = 0; i < indexArray.Length; i++)
            {
                string[] _line = notificationUnitStorage[indexArray[i]];
                List<Type> _compareTypeList = GetContainTypesInLine(_line);

                bool _isMatch = false;

                for (int j = 0; j < _compareTypeList.Count; j++)
                {
                    ConditionData _conditionUnit = null;

                    for (int k = 0; k < conditions.Count; k++)
                    {
                        if (conditions[k].GetType() == _compareTypeList[j])
                            _conditionUnit = conditions[k];
                    }

                    if (_conditionUnit == null)
                    {
                        _isMatch = false;
                        break;
                    }

                    int _fieldCount = conditionClassColumns[_conditionUnit.GetType()].Count;
                    string[] _paramField = new string[_fieldCount];

                    for (int k = 0; k < _fieldCount; k++)
                    {
                        int _columnIndex = conditionClassColumns[_conditionUnit.GetType()][k];
                        _paramField[k] = notificationUnitStorage[indexArray[i]][_columnIndex];
                    }

                    _isMatch = PropertyCompare(_paramField, _conditionUnit);

                    if (!_isMatch)
                        break;
                }

                if (_isMatch)
                {
                    ResultData _compareResult = Activator.CreateInstance<ResultData>();
                    FieldInfo[] _fieldInfos = typeof(ResultData).GetFields();

                    for (int j = 0; j < resultClassColumns.Count; j++)
                    {
                        int _columnIndex = resultClassColumns[j];
                        string _param = notificationUnitStorage[indexArray[i]][_columnIndex];

                        _fieldInfos[j].SetValue(_compareResult, NotificationModel.ConvertData(_param, _fieldInfos[j].FieldType));
                    }

                    _result.Add(_compareResult);
                }
            }

            return _result;
        }

        private static List<Type> GetContainTypesInLine(string[] line)
        {
            List<Type> _compareTypeList = new List<Type>();

            foreach (KeyValuePair<Type, List<int>> pair in conditionClassColumns)
            {
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    string _content = line[pair.Value[i]];

                    if (!string.IsNullOrEmpty(_content) && _content != CHAR_TAG_IGNORE && !_compareTypeList.Contains(pair.Key))
                    {
                        _compareTypeList.Add(pair.Key);
                        break;
                    }
                }
            }

            return _compareTypeList;
        }

        private static bool PropertyCompare(string[] conditionTable, ConditionData compareObj)
        {
            Type _type = compareObj.GetType();
            FieldInfo[] _fieldInfoArr = _type.GetFields();

            List<bool> orList = new List<bool>();
            List<bool> andList = new List<bool>();

            for (int i = 0; i < conditionTable.Length; i++)
            {
                if (conditionTable[i] == CHAR_TAG_IGNORE) continue;

                object _compareParam = _fieldInfoArr[i].GetValue(compareObj);
                SubCompareResult _compare = SpecificCompare(conditionTable[i], _compareParam, _fieldInfoArr[i]);

                switch (_compare.logicTag)
                {
                    case LogicalOperator.And:
                        andList.Add(_compare.result);
                        break;

                    case LogicalOperator.Or:
                        orList.Add(_compare.result);
                        break;
                }
            }

            if ((orList == null || orList.Count <= 0) && (andList == null || andList.Count <= 0))
                return false;

            for (int i = 0; i < andList.Count; i++)
            {
                if (!andList[i])
                    return false;
            }

            bool _or = false;

            if (orList.Count <= 0)
                return true;

            for (int i = 0; i < orList.Count; i++)
            {
                if (orList[i])
                    _or = true;
            }

            return _or;
        }

        private static SubCompareResult SpecificCompare(string tableParam, object compareParam, FieldInfo fieldInfo)
        {
            SubCompareResult _resultInfo = new SubCompareResult();
            string paramName = fieldInfo.Name;
            bool _result = false;

            if (tableParam.Contains(COMPARE_TAG_RANGE)) //在數值範圍內
            {
                tableParam = tableParam.Replace(COMPARE_TAG_RANGE, string.Empty);

                List<int> _range = (List<int>)NotificationModel.ConvertData(tableParam, typeof(int), true);
                int _min = _range[0];
                int _max = _range[1];
                int _value = (int)compareParam;

                _result = (_min <= _value) && (_value <= _max);
            }
            else if (tableParam.Contains(COMPARE_TAG_GREATERTHEN)) //大於指定數值
            {
                tableParam = tableParam.Replace(COMPARE_TAG_GREATERTHEN, string.Empty);

                int _param = (int)NotificationModel.ConvertData(tableParam, typeof(int));
                int _value = (int)compareParam;

                _result = (_value >= _param);
            }
            else if (tableParam.Contains(COMPARE_TAG_SMALLERTHAN)) //小於指定數值
            {
                tableParam = tableParam.Replace(COMPARE_TAG_SMALLERTHAN, string.Empty);

                int _param = (int)NotificationModel.ConvertData(tableParam, typeof(int));
                int _value = (int)compareParam;

                _result = (_value <= _param);
            }
            else if (tableParam.Contains(COMPARE_TAG_CONTAIN)) //包含指定值
            {
                tableParam = tableParam.Replace(COMPARE_TAG_CONTAIN, string.Empty);

                IList _param = (IList)NotificationModel.ConvertData(tableParam, fieldInfo.FieldType, true);

                _result = _param.Contains(compareParam);
            }
            else //未指定的狀況, 檢查是否相等
            {
                object _param = NotificationModel.ConvertData(tableParam, fieldInfo.FieldType);

                _result = _param.Equals(compareParam);
            }

            _resultInfo.result = _result;

            if (paramName.Contains(LOGICAL_TAG_OR)) //加入"OR"邏輯判斷清單
                _resultInfo.logicTag = LogicalOperator.Or;
            else //加入"AND"邏輯判斷清單
                _resultInfo.logicTag = LogicalOperator.And;

            return _resultInfo;
        }

        private static bool IsValidString(string _str)
        {
            return
                !string.IsNullOrEmpty(_str) &&
                _str != CHAR_TAG_IGNORE;
        }
    }
}

