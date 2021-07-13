using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class CommonSample : MonoBehaviour
{
    static private CommonSample _instance;
    static public CommonSample Instance { get { return _instance; } }

    public int getPillCount;

    [Header("MemberData")]
    public int nowStationId;
    public int nowPillCount;

    [Header("MapSetting ")]
    public List<int> regionSort;
    public List<RegionRangeInfo> regionRangeSetting;
    public List<int> pillCost;
    public List<StationRewardSetting> stationRewardSetting;

    private MapStationGame MapStationGameModel;

    void Start()
    {
        if (_instance == null)
            _instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            string _log = "=== 測試用操作說明 ====\n";
            _log += "F2 : Model Init\n";
            _log += string.Format("F3 : 取得仙丹 {0} 個\n", getPillCount);
            _log += "F4 : 前進\n";

            Debug.Log(_log);
        }

        if (Input.GetKeyDown(KeyCode.F2))
            MapStationGameModel = new MapStationGame();

        if (Input.GetKeyDown(KeyCode.F3))
            MapStationGameModel.AddPill(getPillCount);

        if (Input.GetKeyDown(KeyCode.F4))
        {

        }
    }

}

[System.Serializable]
public class StationRewardSetting
{
    public int stationId;
    public StationRewardInfo rewardInfo;
}