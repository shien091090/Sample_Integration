using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class MapStationGame
{
    public PillManager pillInfo;
    public MapStationActivityState activityState;
    public MapInfo mapInfo;

    public Action<int> OnDayUpdated;
    public Action<ActivityState> OnActivityStateUpdated;

    //Model Init
    public MapStationGame()
    {
        InitSettingData(() =>
        {
            RequestActivityState((activityStateRes) =>
            {
                //TODO : Verify Error
                if (activityStateRes == null)
                {
                    Debug.Log("[ERROR] ActivityData Get Failed");
                    activityState = new MapStationActivityState() { currentState = ActivityState.已結束 };
                }

                if (activityState.currentState == ActivityState.已結束)
                    return;

                InitMemberActivityState();
            });

        });

    }

    private void InitSettingData(Action callback)
    {
        RequestMapSetting((mapInfoRes) =>
        {
            if (mapInfoRes == null)
                throw new Exception("[ERROR] MapSetting Init Failed");

            RequestPillSetting((pillRes) =>
            {
                if (pillRes == null)
                    throw new Exception("[ERROR] PillManagerSetting Init Failed");

                callback.Invoke();
            });

        });
    }

    private void InitMemberActivityState()
    {
        RequestMemberPos((posIdRes) =>
        {
            //TODO : Verify Error
            if (posIdRes == default)
            {
                Debug.Log("[ERROR] Get Member Pos Failed");
                return;
            }

            mapInfo.UpdateCurrentStationAndMapState(posIdRes);

            RequestMemberPillRecord((memberRecord) =>
            {
                pillInfo.dict_pillRecord = memberRecord.ToDictionary(record => record.cycleNum);
            });

        });
    }

    private void RequestActivityState(Action<MapStationActivityState> callback)
    {
        //TODO : Get From Server
        int _currentDay = CommonSample.Instance.currentDay;
        ActivityTimeSetting _activitySetting = CommonSample.Instance.activityTimeSetting;

        MapStationActivityState _activityState = MapStationActivityState.InitActivityState(_currentDay, _activitySetting);

        callback.Invoke(_activityState);
    }

    private void CheckAndUpdateActivityState()
    {
        RequestActivityState((activityInfoRes)=> 
        {
            //Check Error Code
            if (activityInfoRes == null)
            {
                activityState = null;
                return;
            }

            bool _updateDay = 
            activityState == null ||
            activityState.currentCycleNum != activityInfoRes.currentCycleNum;

            bool _updateState =
            activityState == null ||
            activityState.currentState != activityInfoRes.currentState;

            activityState = activityInfoRes;
            if (_updateDay)
                OnDayUpdated(activityState.currentCycleNum);

            if (_updateState)
                OnActivityStateUpdated(activityState.currentState);

        });
    }

    private void RequestPillSetting(Action<PillManager> callback)
    {
        //TODO : Get From Server
        List<AccumulationPillThresHold> _betThresHoldSetting = CommonSample.Instance.betThresHoldSetting;
        List<int> _scoreThresHoldSetting = CommonSample.Instance.scoreThresHoldSetting;
        List<VipFreePillSetting> _vipFreePillSetting = CommonSample.Instance.vipFreePillSetting;

        pillInfo = new PillManager(_betThresHoldSetting, _scoreThresHoldSetting, _vipFreePillSetting);

        callback.Invoke(pillInfo);
    }

    private void RequestMemberPillRecord(Action<List<MemberCyclePillRecord>> callback)
    {
        //TODO : Get From Server
        List<MemberCyclePillRecord> _memberRecord = CommonSample.Instance.memberPillRecord;

        callback.Invoke(_memberRecord);
    }

    private void RequestMapSetting(Action<MapInfo> callback)
    {
        //TODO : Get From Server
        List<int> regionSort = CommonSample.Instance.regionSort;
        List<RegionRangeInfo> regionRangeSetting = CommonSample.Instance.regionRangeSetting;
        List<int> pillCost = CommonSample.Instance.pillCost;
        List<StationRewardSetting> rewardSetting = CommonSample.Instance.stationRewardSetting;

        mapInfo = MapInfo.InitMap(regionSort[regionSort.Count - 1], regionRangeSetting, pillCost, rewardSetting);

        callback.Invoke(mapInfo);
    }

    private void RequestMemberPos(Action<int> callback)
    {
        //TODO : Get From Server
        int _nowStationId = CommonSample.Instance.nowStationId;

        //TODO : Verify Error

        callback.Invoke(_nowStationId);
    }

}