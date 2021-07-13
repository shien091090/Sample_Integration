using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MapStationGame
{
    public int pillCount;
    public int nowStationId;
    public MapInfo mapInfo;

    //Model Init
    public MapStationGame()
    {
        RequestMapSetting(() =>
        {
            RequestMemberData(() =>
            {
                mapInfo.UpdateMap(nowStationId);
                Debug.Log("Success");
            });

        });

    }

    public void RequestMapSetting(Action callback)
    {
        //TODO : Get From Server

        List<int> regionSort = CommonSample.Instance.regionSort;
        List<RegionRangeInfo> regionRangeSetting = CommonSample.Instance.regionRangeSetting;
        List<int> pillCost = CommonSample.Instance.pillCost;
        List<StationRewardSetting> rewardSetting = CommonSample.Instance.stationRewardSetting;

        mapInfo = new MapInfo(regionSort[regionSort.Count - 1], regionRangeSetting, pillCost, rewardSetting);

        //TODO : Verify Error

        callback.Invoke();
    }

    public void RequestMemberData(Action callback)
    {
        //TODO : Get From Server
        nowStationId = CommonSample.Instance.nowStationId;
        pillCount = CommonSample.Instance.nowPillCount;

        //TODO : Verify Error

        callback.Invoke();
    }

    public void Bet()
    {

    }

}