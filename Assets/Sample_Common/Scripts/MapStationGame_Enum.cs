using System;

public enum BetHall
{
    一般廳,
    高手廳,
    至尊廳
}

public enum ActivityState
{
    已結束,
    活動中,
    保留中
}

[Flags]
public enum PillType
{
    FreePill,
    AccumulationPill
}

public enum VIPLevel
{
    銀卡,
    金卡,
    白金卡,
    鑽石卡
}