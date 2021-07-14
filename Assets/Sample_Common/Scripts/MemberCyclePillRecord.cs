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

    public MemberCyclePillRecord(int _cycleNum)
    {
        cycleNum = _cycleNum;
    }
}