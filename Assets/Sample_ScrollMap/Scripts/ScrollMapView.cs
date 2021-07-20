using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(ScrollMap))]
public class ScrollMapView : MonoBehaviour
{
    [SerializeField] private RegionLocker[] regionLockers;
    [SerializeField] private StationUnit[] stations;
    [SerializeField] private RectTransform baoLittleSister;

    private Dictionary<int, RectTransform> dict_regionLocker;
    private Dictionary<int, Transform> dict_stationIdToPos;
    private Dictionary<int, StationUnit> dict_stationIdToUnit;

    [Range(0, 10)]
    public float stationMovingDuration;
    public Ease stationMovingCurveType;


    private ScrollMap _scrollMap;
    private ScrollMap ScrollMapController
    {
        get
        {
            if (_scrollMap == null)
                _scrollMap = this.gameObject.GetComponent<ScrollMap>();

            return _scrollMap;
        }
    }


    public void SetupScrollMap(List<StationInfo> stationList, int currentStationId)
    {
        SetStations(stationList);
        int unlockRegionNum = ScrollMapManager.Instance.GetUnlockRegionNum(currentStationId);
        SetRegionLock(unlockRegionNum);
        SetPlayerPos(currentStationId);
    }

    private void SetStations(List<StationInfo> stationList)
    {
        if (stations.Length != stationList.Count)
        {
            Debug.Log("[MapStation Error] 地圖Staion物件數量與輸入資料不符");
            return;
        }

        for (int i = 0; i < stations.Length; i++)
        {
            StationUnit _stationObj = stations[i];
            StationInfo _stationData = stationList[i];

            _stationObj.Init(_stationData);
        }

        dict_stationIdToPos = stations.ToDictionary(sta => sta.StationID, sta => sta.transform);
        dict_stationIdToUnit = stations.ToDictionary(sta => sta.StationID);
    }

    private void SetRegionLock(int unlockNum)
    {
        if (ScrollMapManager.Instance.maxRegionNum != regionLockers.Length - 1)
        {
            Debug.Log("[MapStation Error] 地圖Lock物件數量與Region設定不符");
            return;
        }

        dict_regionLocker = regionLockers.ToDictionary(locker => locker.number, locker => locker.objectRect);

        List<float> yPosList = new List<float>();
        for (int i = 0; i < regionLockers.Length; i++)
        {
            yPosList.Add(regionLockers[i].objectRect.localPosition.y);
        }
        ScrollMapController.SetLockPos(yPosList.ToArray());

        UnlockRegionLock(unlockNum);
    }

    private void UnlockRegionLock(int unlockNum)
    {
        if (unlockNum >= 0)
        {
            for (int i = 0; i <= unlockNum; i++)
            {
                ScrollMapController.UnlockScrollTenon(i);
                dict_regionLocker[i].gameObject.SetActive(false);
            }
        }
    }

    private void SetPlayerPos(int targetStationId)
    {
        if (!dict_stationIdToPos.ContainsKey(targetStationId))
        {
            Debug.Log("[MapStation Error] 找不到對應的StaionID位置");
            return;
        }

        Vector3 _targetPos = dict_stationIdToPos[targetStationId].position;
        baoLittleSister.position = _targetPos;
        ScrollMapController.FocusPos(baoLittleSister.localPosition.y);
    }

    public void PlayMovingAction(Queue<StepPerformanceInfo> performanceInfos)
    {
        ScrollMapController.CanDrag = false;

        StartCoroutine(Cor_PlayMovingAction(performanceInfos, () =>
        {
            Debug.Log("PlayMovingAction End");
            ScrollMapController.CanDrag = true;
        }));
    }

    private IEnumerator Cor_PlayMovingAction(Queue<StepPerformanceInfo> performanceInfos, Action callback)
    {
        while (performanceInfos.Count > 0)
        {
            StepPerformanceInfo _info = performanceInfos.Dequeue();

            int _targetStationId = _info.targetStaionId;
            yield return StartCoroutine(Cor_MoveToStation(_targetStationId));

            StationRewardInfo _rewardInfo = _info.reward;
            if (_rewardInfo == null || (_rewardInfo.goldAmount == 0 && _rewardInfo.gemAmount == 0))
                Debug.Log("無獎勵");
            else
                yield return StartCoroutine(Cor_GetReward(_rewardInfo, _targetStationId));

            if (_info.unlockRegionNum > -1)
                yield return StartCoroutine(Cor_CrossRegion(_info.unlockRegionNum));
        }

        if (callback != null)
            callback.Invoke();
    }

    private IEnumerator Cor_MoveToStation(int targetStationId)
    {
        Vector3 _targetPos = dict_stationIdToPos[targetStationId].localPosition;

        Debug.Log(string.Format("移動至 StaionID : {0}, pos : {1} >> {2}, 動畫開始...", targetStationId, baoLittleSister.transform.position, _targetPos));

        ScrollMapController.SetAutoFollow(baoLittleSister);
        Tween twn_moveAnime = baoLittleSister.transform
            .DOLocalMove(_targetPos, stationMovingDuration)
            .SetEase(stationMovingCurveType);

        yield return twn_moveAnime.WaitForCompletion();

        ScrollMapController.StopAutoFollow();

        Debug.Log(string.Format("移動至 StaionID : {0}, 動畫結束", targetStationId));
    }

    private IEnumerator Cor_GetReward(StationRewardInfo rewardInfo, int stationId)
    {
        Debug.Log(string.Format("取得獎勵 gold : {0}, gem : {1}, 動畫開始...", rewardInfo.goldAmount, rewardInfo.gemAmount));

        yield return new WaitForSeconds(1.5f);

        dict_stationIdToUnit[stationId].Discolor();

        Debug.Log(string.Format("取得獎勵 gold : {0}, gem : {1}, 動畫結束", rewardInfo.goldAmount, rewardInfo.gemAmount));
    }

    private IEnumerator Cor_CrossRegion(int unlockRegionNum)
    {
        Debug.Log("解鎖區域, 動畫開始...");

        UnlockRegionLock(unlockRegionNum);

        yield return new WaitForSeconds(1.5f);

        Debug.Log("解鎖區域, 動畫結束");
    }
}

[System.Serializable]
public class StationInfo
{
    public int stationId;
    public int pillCost;
    public StationRewardInfo rewardContent;
    public bool isStationUnlocked;
}

[System.Serializable]
public class StationRewardInfo
{
    public int goldAmount;
    public int gemAmount;
    public int itemId;
}

[System.Serializable]
public class StepPerformanceInfo
{
    public StationRewardInfo reward;
    public int targetStaionId;
    public int unlockRegionNum = -1;
    public bool isReachGoal;
}

[System.Serializable]
public class RegionLocker
{
    public int number;
    public RectTransform objectRect;
}