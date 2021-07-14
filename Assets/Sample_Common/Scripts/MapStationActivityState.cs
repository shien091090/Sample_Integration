using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapStationActivityState
{
    public ActivityState currentState;
    public int currentCycleNum;

    public static MapStationActivityState InitActivityState(int timeStamp, ActivityTimeSetting activityTimeSetting)
    {
        if (activityTimeSetting == null)
            return null;

        MapStationActivityState _activityState = new MapStationActivityState();

        int _start = activityTimeSetting.activityStartDay;
        int _end = activityTimeSetting.activityEndDay;
        int _keep = _end + activityTimeSetting.KeepDayCount;

        if (timeStamp >= _start && timeStamp <= _end)
        {
            _activityState.currentCycleNum = timeStamp - _start;
            _activityState.currentState = ActivityState.活動中;
            return _activityState;
        }
        else if (timeStamp > _end && timeStamp <= _keep)
        {
            _activityState.currentCycleNum = timeStamp;
            _activityState.currentState = ActivityState.保留中;
            return _activityState;
        }
        else
        {
            _activityState.currentCycleNum = -1;
            _activityState.currentState = ActivityState.已結束;
            return _activityState;
        }
    }
}
