using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SNShien.DataSearching
{
    public class DataGroup
    {
        public int DatasCount { protected set; get; }
    }

    public class DataGroup_List<T> : DataGroup
    {
        public List<T> datas;

        public DataGroup_List(List<T> inputDatas)
        {
            datas = inputDatas;
            DatasCount = inputDatas.Count;
        }
    }

}