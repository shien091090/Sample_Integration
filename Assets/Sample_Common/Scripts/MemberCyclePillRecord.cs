using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemberCyclePillRecord
{
    public int cycleNum;
    public int accumulativeScore;
    public int freePillPool;
    public int accumulationPillPool;
    public int pillCount;

    public void AddPill(PillType pillType, int count)
    {
        switch (pillType)
        {
            case PillType.FreePill:
                freePillPool += count;
                break;

            case PillType.AccumulationPill:
                accumulationPillPool += count;
                break;
        }

        pillCount += count;
    }

    public void SubtractPill(int count)
    {
        if (count > pillCount || count < 0)
            Debug.Log("[ERROR] 仙丹減去數量錯誤");

        pillCount = Mathf.Clamp(pillCount - count, 0, PillManager.DAY_PILL_LIMIT);
    }

    public MemberCyclePillRecord(int _cycleNum)
    {
        cycleNum = _cycleNum;
    }
}