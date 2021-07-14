using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PillManager
{
    public const int SCORE_LIMIT = 999999;
    public const int DAY_PILL_LIMIT = 100;

    public Dictionary<int, MemberCyclePillRecord> dict_pillRecord; //Key : CycleNum

    public Dictionary<BetHall, AccumulationPillThresHold> dict_accumulationSetting;
    public Dictionary<VIPLevel, int> dict_vipFreePillSetting;
    public List<int> scoreThresHoldIndividualSetting; //個別的積分門檻 ex : { 50, 60, 70, ... }
    public List<int> scoreThresHoldAccumulationSetting; //累計的積分門檻 ex : { 50, 110, 180, ...}
    public bool isActivityTimeOut;

    public static PillManager InitPillManagerSetting(List<AccumulationPillThresHold> accumulationSetting, List<int> scoreThresHoldSetting, List<VipFreePillSetting> vipFreePillSetting)
    {
        if (accumulationSetting == null)
            return null;

        if (scoreThresHoldSetting == null)
            return null;

        PillManager _resultRecord = new PillManager();
        _resultRecord.dict_accumulationSetting = accumulationSetting.ToDictionary(accumulation => accumulation.betType);
        _resultRecord.dict_vipFreePillSetting = vipFreePillSetting.ToDictionary(freePill => freePill.vipType, freePill => freePill.freePillCount);
        _resultRecord.scoreThresHoldIndividualSetting = scoreThresHoldSetting;
        _resultRecord.scoreThresHoldAccumulationSetting = ParseAccumulationFromIndividualList(scoreThresHoldSetting);
        _resultRecord.isActivityTimeOut = false;

        return _resultRecord;
    }

    private static List<int> ParseAccumulationFromIndividualList(List<int> IndividualList)
    {
        List<int> _accumulationList = new List<int>();
        for (int i = 0; i < IndividualList.Count; i++)
        {
            int _acc = 0;
            if (_accumulationList.Count > 0)
                _acc += _accumulationList[_accumulationList.Count - 1];

            _accumulationList.Add(_acc + IndividualList[i]);
        }

        return _accumulationList;
    }

    public void UpdateDay(MapStationActivityState activityState)
    {
        isActivityTimeOut = activityState.currentState != ActivityState.活動中;

        int _cycleNum = activityState.currentCycleNum;
        if (dict_pillRecord == null || dict_pillRecord.ContainsKey(_cycleNum))
            return;

        MemberCyclePillRecord _record = new MemberCyclePillRecord(_cycleNum);
        dict_pillRecord.Add(_cycleNum, _record);

        int _leakScore = GetLeakScore(_cycleNum - 1, scoreThresHoldAccumulationSetting);
        if (_leakScore > 0)
            AddScoreByBet(BetHall.至尊廳, _leakScore, _cycleNum);
    }

    public int GetTotalPillCount()
    {
        if (dict_pillRecord == null)
            return 0;

        int _totalPillCount = 0;
        foreach (KeyValuePair<int, MemberCyclePillRecord> pillRecord in dict_pillRecord)
        {
            int _freePillCount = pillRecord.Value.freePillPool;
            int _accumulationPillCount = pillRecord.Value.accumulationPillPool;

            _totalPillCount += _freePillCount;
            _totalPillCount += _accumulationPillCount;
        }

        return _totalPillCount;
    }

    public int GetPillCountByCycleNum(int cycleNum, PillType pillType)
    {
        if (!dict_pillRecord.ContainsKey(cycleNum))
        {
            MemberCyclePillRecord _pillRecord = new MemberCyclePillRecord(cycleNum);
            dict_pillRecord.Add(cycleNum, _pillRecord);
        }

        MemberCyclePillRecord _targetPillRecord = dict_pillRecord[cycleNum];

        int _resultCount = 0;
        if ((pillType & PillType.FreePill) == PillType.FreePill)
            _resultCount += _targetPillRecord.freePillPool;

        if ((pillType & PillType.AccumulationPill) == PillType.AccumulationPill)
            _resultCount += _targetPillRecord.accumulationPillPool;

        return _resultCount;
    }

    public bool CheckAddScoreIsValid(BetHall betType, int todayAccumulationPill)
    {
        if (!dict_accumulationSetting.ContainsKey(betType))
            return false;

        AccumulationPillThresHold _thresHoldInfo = dict_accumulationSetting[betType];

        if (todayAccumulationPill >= _thresHoldInfo.bottom && todayAccumulationPill <= _thresHoldInfo.upper)
            return true;
        else
            return false;
    }

    public void AddScoreByBet(BetHall betType, int score, int cycleNum)
    {
        int _todayAccumulationPill = GetPillCountByCycleNum(cycleNum, PillType.AccumulationPill);
        bool _isValidScore = CheckAddScoreIsValid(betType, _todayAccumulationPill);

        if (!_isValidScore)
            return;

        MemberCyclePillRecord _todayRecord = dict_pillRecord[cycleNum];
        int _nowScore = _todayRecord.accumulativeScore;
        int _newScore = _nowScore + score;
        _todayRecord.accumulativeScore = _newScore;

        int _increasePill = GetAccumulationPillIncreaseCount(_nowScore, _newScore, scoreThresHoldAccumulationSetting);
        bool _isPillCountMax = IsAccumulationPillCountMax(_todayRecord.accumulationPillPool);

        if (_increasePill > 0 && !_isPillCountMax)
            _todayRecord.AddPill(PillType.AccumulationPill, _increasePill);
    }

    private int GetAccumulationPillIncreaseCount(int beforeScore, int afterScore, List<int> thresHoldSetting)
    {
        int _beforePillNumber = GetAccumulationPillNumber(beforeScore, thresHoldSetting);
        int _afterPillNumber = GetAccumulationPillNumber(afterScore, thresHoldSetting);

        return _afterPillNumber - _beforePillNumber;
    }

    private bool IsAccumulationPillCountMax(int currentPillCount)
    {
        return currentPillCount >= DAY_PILL_LIMIT;
    }

    private int GetCurrentScore(int accumulativeScore, List<int> thresHoldSetting)
    {
        for (int i = 0; i < thresHoldSetting.Count; i++)
        {
            int _startScore = thresHoldSetting[i];
            if (accumulativeScore >= _startScore)
                return accumulativeScore - _startScore;
        }

        return accumulativeScore;
    }

    private int GetCurrentScoreThresHold(int accumulativeScore, List<int> thresHoldSetting)
    {
        for (int i = 0; i < thresHoldSetting.Count; i++)
        {
            int _startScore = thresHoldSetting[i];
            if (accumulativeScore < _startScore)
                return _startScore;
        }

        return SCORE_LIMIT;
    }

    private int GetAccumulationPillNumber(int accumulativeScore, List<int> thresHoldSetting)
    {
        for (int i = 0; i < thresHoldSetting.Count; i++)
        {
            int _startScore = thresHoldSetting[i];
            if (accumulativeScore < _startScore)
                return i;
        }

        return thresHoldSetting.Count - 1;
    }

    private int GetLeakScore(int targetDayCycleNum, List<int> thresHoldSetting)
    {
        if (dict_pillRecord == null || !dict_pillRecord.ContainsKey(targetDayCycleNum))
            return 0;

        int _maxThresHold = thresHoldSetting[thresHoldSetting.Count - 1];
        int _score = dict_pillRecord[targetDayCycleNum].accumulativeScore;
        int _leakScore = Mathf.Clamp(_score - _maxThresHold, 0, SCORE_LIMIT);

        return _leakScore;
    }

    public void AddFreePillByLogin(VIPLevel vip, int cycleNum)
    {
        if (!dict_vipFreePillSetting.ContainsKey(vip))
            return;

        MemberCyclePillRecord _todayRecord = dict_pillRecord[cycleNum];

        int _freePillLimit = dict_vipFreePillSetting[vip];
        int _todayFreePillCount = _todayRecord.freePillPool;
        int _increasePill = _freePillLimit - _todayFreePillCount;

        if (_increasePill > 0)
            _todayRecord.AddPill(PillType.FreePill, _increasePill);
    }
}
