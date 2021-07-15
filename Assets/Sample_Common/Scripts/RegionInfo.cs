using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RegionInfo
{
    public int pillCost;
    public int bottom;
    public int upper;

    public RegionInfo(int bot, int upp, int pillCost)
    {
        if (upp <= bot)
        {
            Debug.Log("[ERROR] RegionRange範圍設定錯誤");
            return;
        }

        bottom = bot;
        upper = upp;
    }

    public int GetStationCount()
    {
        int _result = upper - bottom;
        _result = Mathf.Clamp(_result, 0, int.MaxValue);

        return _result;
    }
}