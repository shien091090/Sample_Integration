using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollMapManager : MonoBehaviour
{
    private static ScrollMapManager _instance;
    public static ScrollMapManager Instance { get { return _instance; } }

    public ScrollMapView viewManager;
    public List<StationInfo> stationsData;
    public int currentStaionId;
    public int maxRegionNum;
    public List<int> lockInStationId;
    public List<StepPerformanceInfo> performanceInfos;

    void Start()
    {
        if (_instance == null)
            _instance = this;

        TEST_InitScrollMap();
    }

    [ContextMenu("InitScrollMap")]
    public void TEST_InitScrollMap()
    {
        if (Application.isPlaying)
            viewManager.SetupScrollMap(stationsData, currentStaionId);
    }

    [ContextMenu("PlayMovingAction")]
    public void TEST_PlayMovingAction()
    {
        if (Application.isPlaying)
        {
            Queue<StepPerformanceInfo> performancesQueue = new Queue<StepPerformanceInfo>();

            for (int i = 0; i < performanceInfos.Count; i++)
            {
                performancesQueue.Enqueue(performanceInfos[i]);
            }

            viewManager.PlayMovingAction(performancesQueue);
        }
            
    }

    public int GetUnlockRegionNum(int stationId)
    {
        int _unlockNum = -1;

        for (int i = 0; i < lockInStationId.Count; i++)
        {
            if (stationId >= lockInStationId[i])
                _unlockNum = i;
            else
                break;
        }

        return _unlockNum;
    }

}
