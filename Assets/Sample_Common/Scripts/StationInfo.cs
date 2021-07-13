using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationInfo
{
    //public int regionId;
    public int stationId;
    public int pillCost;
    public StationRewardInfo rewardContent;
    public bool isStationUnlocked;

    public void UpdatePosToUnlockStation(int newStationId)
    {
        if (newStationId >= stationId)
            isStationUnlocked = true;
    }

    //更新獎勵內容方案之一(Observer Pattern)
    public void UpdateRewardContent(Dictionary<int, StationRewardInfo> rewardSetting)
    {
        if (rewardSetting.ContainsKey(stationId) && stationId > 0)
            rewardContent = rewardSetting[stationId];
        else
            rewardContent = null;
    }
}