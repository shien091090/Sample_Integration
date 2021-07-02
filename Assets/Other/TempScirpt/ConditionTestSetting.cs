using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;
using UnityEditor;

namespace SCGLobby
{
    public class ConditionTestSetting : ScriptableObject
    {
        private static ConditionTestSetting _instance;
        public static ConditionTestSetting Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

#if UNITY_EDITOR
                string[] guids = AssetDatabase.FindAssets("t:ConditionTestSetting");

                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _instance = (ConditionTestSetting)AssetDatabase.LoadAssetAtPath(path, typeof(ConditionTestSetting));
                }
#endif

                return _instance;

            }
        }

        [System.Serializable]
        public class ParamField
        {
            public string paramValue;
            public string paramName;

            public ParamField(FieldInfo _fieldInfo)
            {
                paramName = _fieldInfo.Name;
            }

            public ParamField(ParameterInfo _paramInfo)
            {
                paramName = _paramInfo.Name;
            }
        }

        [System.Serializable]
        public class Comparison
        {
            public string className;
            public List<ParamField> fieldList;

            public Comparison(Type classType)
            {
                className = classType.Name;
                FieldInfo[] _fieldInfo = classType.GetFields();

                if (_fieldInfo == null || _fieldInfo.Length <= 0)
                    return;

                fieldList = new List<ParamField>();

                for (int i = 0; i < _fieldInfo.Length; i++)
                {
                    ParamField paramField = new ParamField(_fieldInfo[i]);
                    fieldList.Add(paramField);
                }
            }

            public bool CheckIntegrity()
            {
                Type _type = Type.GetType(NotificationModel.CLASS_NAMESPACE + className);

                if (_type == null)
                    return false;

                FieldInfo[] _fieldInfos = _type.GetFields();
                if (_fieldInfos.Length != fieldList.Count)
                    return false;

                for (int i = 0; i < _fieldInfos.Length; i++)
                {
                    if (_fieldInfos[i].Name != fieldList[i].paramName)
                        return false;
                    else
                        fieldList[i].paramName = _fieldInfos[i].Name;
                }

                return true;
            }

            public int GetFieldIndex(string fieldName)
            {
                for (int i = 0; i < fieldList.Count; i++)
                {
                    if (fieldList[i].paramName == fieldName)
                        return i;
                }

                return -1;
            }
        }

        //----------------------------------------------------------

        private const string CLASS_NAME_HEAD = "Cond_";

        [Header("[Debug訊息]")]
        [SerializeField] private Color titleColor;
        [SerializeField] private bool showDebugMessage = false;
        [HideInInspector] public NotificationModel.ConditionType[] inputTypes;
        [HideInInspector] public List<Comparison> comparisonList;
        [HideInInspector] public NotificationModel.BroadcastRawData rawData;
        [HideInInspector] public int panelType;

        public Action dropdownMergingAction;

        public Color GetTitleColor { get { return titleColor; } }
        public bool ShowDebugMessage { get { return showDebugMessage; } }
        public NotificationModel.BroadcastRawData GetRawData
        {
            get
            {
                if (rawData == null)
                    rawData = new NotificationModel.BroadcastRawData();

                return rawData;
            }
        }

        //----------------------------------------------------------

        void OnEnable()
        {
            if (_instance == null)
                _instance = this;
        }

        public List<Comparison> UpdateComparisonList()
        {
            if (comparisonList == null)
                comparisonList = new List<Comparison>();

            RepairComparisonData(comparisonList);

            List<Comparison> _updateComparisonList = new List<Comparison>();
            for (int i = 0; i < inputTypes.Length; i++)
            {
                string _className = CLASS_NAME_HEAD + inputTypes[i].ToString();
                Type _type = Type.GetType(NotificationModel.CLASS_NAMESPACE + _className);

                if (_type == null)
                    continue;

                int _existCount = comparisonList
                    .Where(comp => comp.className == _className)
                    .Count();

                if (_existCount == 0)
                {
                    _updateComparisonList.Add(new Comparison(_type));
                }
                else
                {
                    IEnumerable<Comparison> comparisonQuery =
                        from comp in comparisonList
                        where comp.className == _className
                        select comp;

                    Comparison _comp = comparisonQuery.ToArray()[0];

                    _updateComparisonList.Add(_comp);
                }
            }

            return _updateComparisonList;
        }

        public List<ConditionData> GetConditionDatas()
        {
            List<ConditionData> _resultDatas = new List<ConditionData>();

            if (inputTypes == null || inputTypes.Length <= 0)
                return _resultDatas;

            comparisonList = UpdateComparisonList();

            for (int i = 0; i < comparisonList.Count; i++)
            {
                Type _type = Type.GetType(NotificationModel.CLASS_NAMESPACE + comparisonList[i].className);

                ConditionData _data = (ConditionData)Activator.CreateInstance(_type);
                FieldInfo[] _fieldInfos = _type.GetFields();
                for (int j = 0; j < _fieldInfos.Length; j++)
                {
                    if (_fieldInfos[j].Name == comparisonList[i].fieldList[j].paramName)
                    {
                        string _value = comparisonList[i].fieldList[j].paramValue;
                        _fieldInfos[j].SetValue(_data, NotificationModel.ConvertData(_value, _fieldInfos[j].FieldType));
                    }
                }

                _resultDatas.Add(_data);

            }

            return _resultDatas;
        }

        private void RepairComparisonData(List<Comparison> dataList)
        {
            for (int i = 0; i < dataList.Count; i++)
            {
                if (!dataList[i].CheckIntegrity())
                {
                    dataList.RemoveAt(i);
                    RepairComparisonData(dataList);
                    break;
                }
            }
        }

        public void FocusObject()
        {
#if UNITY_EDITOR
            Selection.activeObject = _instance;
            EditorGUIUtility.PingObject(_instance);
#endif
        }
    }
}
