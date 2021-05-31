using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using SNShien.Common.TesterTools;
using SNShien.Common.DataTools;

namespace SNShien.DataSearching
{
    public class SearchTarget : MonoBehaviour
    {
        public int elementListCount = 10;
        public int elementLength = 3;
        public MyStopwatch.TimeUnit stopwatchTimeUnit;
        public DataGroupType groupType;
        public DataValueType valueType;

        public DataGroup dataGroup;
        public Button btn_reset;
        public Button btn_searching;

        public List<string> strList;
        public List<int> intList;

        void Start()
        {
            Init();
        }

        private void Init()
        {
            InitialDatas();

            SetButtonEvent(btn_reset, "ResetDatas", () =>
            {
                MyStopwatch.TimerTest(()=> 
                {
                    InitialDatas();
                }, stopwatchTimeUnit, "CreateDatas");
                
            });
        }

        private void InitialDatas()
        {
            Func<DataGroupType, DataGroup> DataGroupInstanceCallback = null;

            switch (valueType)
            {
                case DataValueType.INT:
                    DataGroupInstanceCallback = CreateDataGroupInstance<int>;
                    break;

                case DataValueType.STRING:
                    DataGroupInstanceCallback = CreateDataGroupInstance<string>;
                    break;
            }

            dataGroup = DataGroupInstanceCallback.Invoke(groupType);
        }

        private void SetButtonEvent(Button target, string buttonCaption, UnityAction callback)
        {
            Text _caption = target.gameObject.GetComponentInChildren<Text>();

            if (_caption != null)
                _caption.text = buttonCaption;

            target.onClick.RemoveAllListeners();
            target.onClick.AddListener(callback);
        }

        private DataGroup CreateDataGroupInstance<T>(DataGroupType groupType)
        {
            switch (groupType)
            {
                case DataGroupType.List:
                    {
                        List<T> _inputDatas = RandomDatasCreator.CreateRandomValueList<T>(elementListCount, elementLength);
                        DataGroup_List<T> _result = new DataGroup_List<T>(_inputDatas);

                        strList = new List<string>();
                        intList = new List<int>();
                        if (typeof(T) == typeof(string))
                        {
                            strList = (List<string>)Convert.ChangeType(_inputDatas, typeof(List<string>));
                        }
                        else if (typeof(T) == typeof(int))
                        {
                            intList = (List<int>)Convert.ChangeType(_inputDatas, typeof(List<int>));
                        }

                        return _result;
                    }

                    //case DataGroupType.Dictionary:
                    //    return new DataGroup_List<T>();
            }

            return null;
        }


    }
}