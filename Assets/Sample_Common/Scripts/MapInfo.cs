using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class MapInfo
{
    public List<RegionInfo> regionData;
    public Action<int> OnPlayerPosUpdated;
    public Action<Dictionary<int, StationRewardInfo>> OnStationRewardUpdated;
    public int GetMaxRegionNum { private set; get; }

    public MapInfo(int maxRegionId, List<RegionRangeInfo> regionRangeSetting, List<int> pillCostInfo, List<StationRewardSetting> rewardSetting)
    {
        if (regionRangeSetting == null || pillCostInfo == null)
            return;

        if (regionRangeSetting.Count < maxRegionId + 1 || pillCostInfo.Count < maxRegionId + 1)
            return;

        OnPlayerPosUpdated = null;
        GetMaxRegionNum = maxRegionId;

        List<RegionInfo> _regionData = new List<RegionInfo>();
        for (int i = 0; i <= maxRegionId; i++)
        {
            int _regionId = i;
            RegionRangeInfo _rangeInfo = regionRangeSetting[_regionId];
            int _cost = pillCostInfo[_regionId];
            List<StationInfo> _stations = BuildStationList(i, _rangeInfo, _cost);

            RegionInfo _region = new RegionInfo();
            _region.regionId = _regionId;
            _region.rangeInfo = _rangeInfo;
            _region.stationData = _stations;

            OnPlayerPosUpdated += _region.UpdatePosToUnlockRegion;

            _regionData.Add(_region);
        }

        Dictionary<int, StationRewardInfo> dict_rewardInfo = new Dictionary<int, StationRewardInfo>();
        dict_rewardInfo = rewardSetting.ToDictionary(reward => reward.stationId, reward => reward.rewardInfo);
        OnStationRewardUpdated.Invoke(dict_rewardInfo);

        regionData = _regionData;
    }

    private List<StationInfo> BuildStationList(int _regionId, RegionRangeInfo _rangeInfo, int _cost)
    {
        List<StationInfo> _result = new List<StationInfo>();

        int _stationCount = _rangeInfo.GetStationCount();
        int _startId = _rangeInfo.bottom + 1;

        for (int i = 0; i < _stationCount; i++)
        {
            StationInfo _station = new StationInfo
            {
                stationId = i + _startId,
                pillCost = _cost
            };

            OnPlayerPosUpdated += _station.UpdatePosToUnlockStation;
            OnStationRewardUpdated += _station.UpdateRewardContent;

            _result.Add(_station);
        }

        return _result;
    }



    //Debug
    public void PrintMapInfo()
    {
        //string _log = "=== PrintMapInfo ===\n";

        ////_log += string.Format();
        //_log += "--- Map簡易資訊 ---\n";
        //_log += string.Format("共 {0} 個Region\n");
        //_log += string.Format("Region {0} : 共 {1} 個Station, Cost {2} Pill\n");

        //_log = "--- Map解鎖 ---\n";
        //_log += string.Format("Region {0} : {1}");

        //_log = "--- Station解鎖 ---\n";
        //_log += string.Format("Region-Station {0} : {1}");
    }

    public bool IsRegionUnlock(int regionId)
    {
        RegionInfo[] _filterRegionInfo = regionData
            .Where(x => x.regionId == regionId)
            .ToArray();

        if (_filterRegionInfo.Length != 1)
        {
            Debug.Log("[ERROR] Region取得數量錯誤 : " + _filterRegionInfo.Length);
            return false;
        }

        return _filterRegionInfo[0].isRegionUnlocked;
    }

    public int GetRegionIdByStationId(int stationId)
    {
        if (regionData == null || regionData.Count <= 0)
            return -1;

        for (int i = 0; i < regionData.Count; i++)
        {
            int _upp = regionData[i].rangeInfo.upper;
            int _bot = regionData[i].rangeInfo.bottom;

            if (_bot < stationId && stationId <= _upp)
            {
                return regionData[i].regionId;
            }
        }

        return -1;
    }

    public void UpdateMap(int currentStationId)
    {
        if (OnPlayerPosUpdated != null)
            OnPlayerPosUpdated.Invoke(currentStationId);
    }

}