using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NTPTimeTester : MonoBehaviour
{
    [Header("請求頻率")]
    public float connectFreq;

    [Header("NTP Server評分前觀察次數")]
    public int evaluationThreshold;

    public Text txt_ntpLabel;

    public void BTN_NTPTest()
    {
        NTPTiming.Instance.GetNTPTime(connectFreq, evaluationThreshold,
            (timeStamp) =>
            {
                string _result = string.Format("NTP Time Stamp : {0}\nDate : {1}", timeStamp, NTPTiming.GetDateDetailLog(timeStamp));
                txt_ntpLabel.text = _result;
            });

        txt_ntpLabel.text = "取得NTP中...";
    }


}
