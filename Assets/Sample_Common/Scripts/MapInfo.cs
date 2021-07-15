using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class MapInfo
{
    public Action<int> OnPlayerPosUpdated;
    public Action<Dictionary<int, StationRewardInfo>> OnStationRewardUpdated;
    public List<RegionInfo> regionData;
    public List<StationInfo> stationData;

    public int currentStationId;
    public int GetMaxRegionNum { private set; get; }
    public int GoalStationId { private set; get; }

    public MapInfo(List<RegionInfo> regionSetting, List<StationRewardSetting> rewardSetting)
    {
        if (regionSetting == null)
            return;

        OnPlayerPosUpdated = null;
        OnStationRewardUpdated = null;

        regionData = regionSetting;
        GetMaxRegionNum = regionSetting.Count - 1;
        GoalStationId = regionSetting[regionSetting.Count - 1].upper;

        stationData = new List<StationInfo>();
        for (int i = 0; i < regionSetting.Count; i++)
        {
            List<StationInfo> _stations = BuildStationList(i, regionSetting[i]);
            stationData.AddRange(_stations);
        }

        Dictionary<int, StationRewardInfo> dict_rewardInfo = new Dictionary<int, StationRewardInfo>();
        dict_rewardInfo = rewardSetting.ToDictionary(reward => reward.stationId, reward => reward.rewardInfo);
        OnStationRewardUpdated.Invoke(dict_rewardInfo);
    }

    private List<StationInfo> BuildStationList(int _regionId, RegionInfo _rangeInfo)
    {
        List<StationInfo> _resultStations = new List<StationInfo>();

        int _stationCount = _rangeInfo.GetStationCount();
        int _startId = _rangeInfo.bottom + 1;
        int _cost = _rangeInfo.pillCost;

        for (int i = 0; i < _stationCount; i++)
        {
            StationInfo _station = new StationInfo
            {
                stationId = i + _startId,
                pillCost = _cost
            };

            OnPlayerPosUpdated += _station.UpdatePosToUnlockStation;
            OnStationRewardUpdated += _station.UpdateRewardContent;

            _resultStations.Add(_station);
        }

        return _resultStations;
    }

    private bool IsRegionUnlock(int regionId)
    {
        if (regionId <= 0 || regionId > regionData.Count - 1)
            return false;

        RegionInfo _targetRegion = regionData[regionId];
        int _lastStationId = _targetRegion.upper;

        foreach (StationInfo _station in stationData)
        {
            if (_station.stationId > _lastStationId)
                break;

            if (!_station.isStationUnlocked)
                return false;
        }

        return true;
    }

    private int GetRegionIdByStationId(int stationId)
    {
        for (int i = 0; i < regionData.Count; i++)
        {
            int _upp = regionData[i].upper;
            int _bot = regionData[i].bottom;

            if (_bot < stationId && stationId <= _upp)
            {
                return i;
            }
        }

        return -1;
    }

    private void UpdateCurrentStationAndMapState(int updateStaionId)
    {
        currentStationId = updateStaionId;

        if (OnPlayerPosUpdated != null)
            OnPlayerPosUpdated.Invoke(updateStaionId);
    }

    private List<StationRewardInfo> GetRewardContentsAfterMove(int stepCount)
    {
        List<StationRewardInfo> _rewardContents = new List<StationRewardInfo>();

        if (stepCount <= 0)
            return _rewardContents;

        Dictionary<int, StationInfo> parseDictStationInfo = stationData.ToDictionary(info => info.stationId);
        for (int i = currentStationId + 1; i <= currentStationId + stepCount; i++)
        {
            if (!parseDictStationInfo.ContainsKey(i))
                break;

            StationInfo _station = parseDictStationInfo[i];
            StationRewardInfo _reward = _station.rewardContent;

            if (_reward != null)
                _rewardContents.Add(_reward);
        }

        return _rewardContents;
    }

    private Queue<StepMovement> GetMovementQueueAfterMove(int stepCount)
    {
        Queue<StepMovement> _resultQueue = new Queue<StepMovement>();

        if (stepCount <= 0)
            return _resultQueue;

        Dictionary<int, StationInfo> parseDictStationInfo = stationData.ToDictionary(info => info.stationId);
        for (int i = currentStationId + 1; i <= currentStationId + stepCount; i++)
        {
            StepMovement _movement = new StepMovement();

            StationInfo _station = parseDictStationInfo[i];
            int _currentRegion = GetRegionIdByStationId(i);
            int _lastStation = regionData[_currentRegion].upper;

            _movement.isCrossRegion = i == _lastStation;
            _movement.isReachGoal = i == GoalStationId;
            _movement.reward = _station.rewardContent;

            _resultQueue.Enqueue(_movement);

            if (_movement.isReachGoal)
                break;
        }

        return _resultQueue;
    }

}