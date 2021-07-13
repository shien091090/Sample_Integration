using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionInfo
{
    public int regionId;
    public bool isRegionUnlocked;
    public List<StationInfo> stationData;
    public RegionRangeInfo rangeInfo;

    public void UpdatePosToUnlockRegion(int newStationId)
    {
        if (newStationId > rangeInfo.upper)
            isRegionUnlocked = true;
    }
}