using System;
using UnityEngine;

public class TimeStampConversion : MonoBehaviour
{
    [Header("在這邊輸入日期")]
    public string inputDateText;
    [Header("輸出TimeStamp(勿填)")]
    [SerializeField] private string outputTimeStamp;
    
    [Space(10)]

    [Header("在這邊輸入TimeStamp")]
    public long inputTimeStamp;
    [Header("輸出日期(勿填)")]
    [SerializeField] private string outputDateText;

    [ContextMenu("日期字串轉TimeStamp")]
    public void Editor_ConvertDateToTimeStamp()
    {
        DateTime _date = ConvertDateText(inputDateText);
        if(_date == default)
        {
            outputTimeStamp = 0.ToString();
            Debug.Log("日期字串轉TimeStamp失敗, 請檢查inputDateText");
            return;
        }

        long _timeStamp = ConvertDateToTimeStamp(_date);
        outputTimeStamp = _timeStamp.ToString();
    }

    [ContextMenu("TimeStamp轉日期字串")]
    public void Editor_ConvertTimeStampToDate()
    {
        DateTime _date = ConvertTimeStampToDate(inputTimeStamp);
        if (_date == default)
        {
            outputDateText = string.Empty;
            Debug.Log("TimeStamp轉日期字串失敗, 請檢查inputTimeStamp");
            return;
        }

        outputDateText = _date.ToString();
    }

    private DateTime ConvertDateText(string dateText)
    {
        if (string.IsNullOrEmpty(dateText))
            return default;

        DateTime _date = new DateTime();
        if (DateTime.TryParse(dateText, out _date))
            return _date;
        else
            return default;
    }

    private DateTime ConvertTimeStampToDate(long timeStamp)
    {
        try
        {
            DateTime _startDate = new DateTime(1970, 1, 1);
            return _startDate
                .AddHours(8)
                .AddMilliseconds(timeStamp);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return default;
        }

    }

    private long ConvertDateToTimeStamp(DateTime date)
    {
        try
        {
            DateTime _startDate = new DateTime(1970, 1, 1);

            return (long)(date - _startDate.AddHours(8)).TotalMilliseconds;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return default;
        }
    }
}
