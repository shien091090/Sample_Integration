using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public enum TimeFlowState
{
    NotCompleted,
    Valid,
    Invalid
}

public class NTPTimeTester : MonoBehaviour
{
    public class TimeFlow
    {
        public ulong ClientSendTimeStamp { private set; get; }
        public ulong ServerReceiveTimeStamp { private set; get; }
        public ulong ClientReceiveTimeStamp { private set; get; }
        public ulong ServerTansmitTimeStamp { private set; get; }
        public bool IsFlowComplete { private set; get; }
        public string Log { private set; get; }

        public TimeFlowState GetState
        {
            get
            {
                if (!IsFlowComplete)
                    return TimeFlowState.NotCompleted;
                else
                {
                    if (ServerTansmitTimeStamp == 0)
                        return TimeFlowState.Invalid;
                    else
                        return TimeFlowState.Valid;
                }
            }
        }

        public int GetTimeOffset
        {
            get
            {
                if (GetState != TimeFlowState.Valid)
                    return -1;
                else
                {
                    ulong t0 = ClientSendTimeStamp;
                    ulong t1 = ServerReceiveTimeStamp;
                    ulong t2 = ServerTansmitTimeStamp;
                    ulong t3 = ClientReceiveTimeStamp;

                    return (int)((t1 - t0) + (t2 - t3)) / 2;
                }
            }
        }

        public int GetRoundTripDelay
        {
            get
            {
                if (GetState != TimeFlowState.Valid)
                    return -1;
                else
                {
                    ulong t0 = ClientSendTimeStamp;
                    ulong t1 = ServerReceiveTimeStamp;
                    ulong t2 = ServerTansmitTimeStamp;
                    ulong t3 = ClientReceiveTimeStamp;

                    return (int)((t3 - t0) - (t2 - t1));
                }
            }
        }

        public TimeFlow(ulong timeStamp)
        {
            ClientSendTimeStamp = timeStamp;

            IsFlowComplete = false;
        }

        public TimeFlow SetReceiveTime(ulong _serverReceive, ulong _serverTansmit, ulong _clientNow, string _log = null)
        {
            ServerReceiveTimeStamp = _serverReceive;
            ServerTansmitTimeStamp = _serverTansmit;
            ClientReceiveTimeStamp = _clientNow;
            Log = _log;

            IsFlowComplete = true;

            return this;
        }
    }

    //-----------------------------------------------------------------------

    private string[] NTP_SERVER =
    {
        "TIME.google.com",
        "TIME1.google.com",
        "TIME2.google.com",
        "TIME3.google.com",
        "TIME4.google.com"
    };

    private readonly DateTime ntpTimeOrigin = new DateTime(1900, 1, 1, 0, 0, 0, 0);

    public int freq = 3;
    public bool isStop = false;

    [Header("LogType")]
    public bool printOffset;
    public bool printDelay;
    public bool printDetailTimeStamp;
    public bool printDetailDate;

    [Header("SocketSetting")]
    public AddressFamily addressFamily = AddressFamily.InterNetworkV6;
    public SocketType socketType = SocketType.Dgram;
    public ProtocolType protocolType = ProtocolType.Udp;

    private bool ntpGetting = false;
    private List<IPEndPoint> ipEndPoint_List;
    private List<TimeFlow> ntpTimeRecords;

    //-----------------------------------------------------------------------

    public void BTN_NTPTest()
    {
        StartCoroutine(Cor_GetNTPTime());
    }

    private IEnumerator Cor_GetNTPTime()
    {
        if (ntpGetting)
            yield break;

        ntpGetting = true;

        bool _isInvalid = SetIPPoint(NTP_SERVER, out ipEndPoint_List);
        if (!_isInvalid || ipEndPoint_List == null || ipEndPoint_List.Count <= 0)
        {
            ntpGetting = false;
            yield break;
        }

        while (!isStop)
        {
            GetNTPTime(ipEndPoint_List[0]);

            yield return new WaitForSeconds(freq);
        }

        isStop = false;
        ntpGetting = false;
    }

    private bool SetIPPoint(string[] servers, out List<IPEndPoint> ipEndList)
    {
        List<IPEndPoint> _ipEndList = new List<IPEndPoint>();

        for (int i = 0; i < servers.Length; i++)
        {
            try
            {
                IPAddress[] addresses = Dns.GetHostEntry(servers[i]).AddressList;
                _ipEndList.Add(new IPEndPoint(addresses[0], 123));
            }
            catch (Exception _exception)
            {
                Debug.Log(_exception);
                ipEndList = null;
                return false;
            }
        }

        ipEndList = _ipEndList;
        return true;
    }

    private void GetNTPTime(IPEndPoint ip)
    {
        TimeFlow _timeRecord = new TimeFlow(GetClientNowTimeStamp());

        byte[] ntpData = new byte[48];
        ntpData[0] = 0x1B;

        new Thread(() =>
        {
            var socket = new Socket(addressFamily, socketType, protocolType);
            socket.SendTimeout = 50000;
            socket.ReceiveTimeout = 50000;

            try
            {
                socket.SendTo(ntpData, ipEndPoint_List[0]);
                socket.Receive(ntpData);

                ReceiveNTPData(ntpData, _timeRecord);

                socket.Close();
            }
            catch (Exception _exception)
            {
                socket.Close();

                RecordNTPRequestResult(_timeRecord.SetReceiveTime(0, 0, 0, string.Format("NTPClockGetError : {0}", _exception)));
            }

        }).Start();

    }

    private void RecordNTPRequestResult(TimeFlow timeRecord)
    {
        if (ntpTimeRecords == null)
            ntpTimeRecords = new List<TimeFlow>();

        ntpTimeRecords.Add(timeRecord);

        bool _isPrint = (printDelay || printOffset || printDetailTimeStamp || printDetailDate);

        if (_isPrint)
        {
            int _index = ntpTimeRecords.Count - 1;

            if (printDelay)
                Debug.Log(string.Format("[{0}] Delay = {1} ms", _index, timeRecord.GetRoundTripDelay));

            if (printOffset)
                Debug.Log(string.Format("[{0}] Offset = {1} ms", _index, timeRecord.GetTimeOffset));

            if (printDetailTimeStamp)
            {
                Debug.Log(string.Format("[{0}] ClientSendTimeStamp = {1} ms", _index, timeRecord.ClientSendTimeStamp));
                Debug.Log(string.Format("[{0}] ServerReceiveTimeStamp = {1} ms", _index, timeRecord.ServerReceiveTimeStamp));
                Debug.Log(string.Format("[{0}] ClientReceiveTimeStamp = {1} ms", _index, timeRecord.ClientReceiveTimeStamp));
                Debug.Log(string.Format("[{0}] ServerTansmitTimeStamp = {1} ms", _index, timeRecord.ServerTansmitTimeStamp));

            }

            if (printDetailDate)
            {
                Debug.Log(string.Format("[{0}] ClientSendTime Date = {1}", _index, GetDateDetailLog(ConvertMilliSecondToDate(timeRecord.ClientSendTimeStamp))));
                Debug.Log(string.Format("[{0}] ServerReceiveTime Date= {1}", _index, GetDateDetailLog(ConvertMilliSecondToDate(timeRecord.ServerReceiveTimeStamp))));
                Debug.Log(string.Format("[{0}] ClientReceiveTime Date= {1}", _index, GetDateDetailLog(ConvertMilliSecondToDate(timeRecord.ClientReceiveTimeStamp))));
                Debug.Log(string.Format("[{0}] ServerTansmitTime Date= {1}", _index, GetDateDetailLog(ConvertMilliSecondToDate(timeRecord.ServerTansmitTimeStamp))));
            }

            Debug.Log("==========================================");
        }
    }

    private ulong GetClientNowTimeStamp()
    {
        TimeSpan _span = DateTime.UtcNow - ntpTimeOrigin;
        return (ulong)_span.TotalMilliseconds;
    }

    private void ReceiveNTPData(byte[] ntpData, TimeFlow timeRecord)
    {
        ulong _serverReceiveTimestamp = OctBitsPackToMilliseconds(ntpData, 40, 41, 42, 43, 44, 45, 46, 47);
        ulong _serverTransmitTimestamp = OctBitsPackToMilliseconds(ntpData, 32, 33, 34, 35, 36, 37, 38, 39);

        RecordNTPRequestResult(timeRecord.SetReceiveTime(_serverReceiveTimestamp, _serverTransmitTimestamp, GetClientNowTimeStamp()));

    }

    private ulong OctBitsPackToMilliseconds(byte[] datas, int int_4, int int_3, int int_2, int int_1, int fract_4, int fract_3, int fract_2, int fract_1)
    {
        ulong intPart = (ulong)datas[int_4] << 24 | (ulong)datas[int_3] << 16 | (ulong)datas[int_2] << 8 | (ulong)datas[int_1];
        ulong fractPart = (ulong)datas[fract_4] << 24 | (ulong)datas[fract_3] << 16 | (ulong)datas[fract_2] << 8 | (ulong)datas[fract_1];

        ulong milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

        return milliseconds;
    }

    private DateTime ConvertMilliSecondToDate(ulong ms)
    {
        return ntpTimeOrigin.Add(TimeSpan.FromMilliseconds(ms));
    }

    private string GetDateDetailLog(DateTime date)
    {
        return string.Format("{0:0000}.{1:00}.{2:00} / {3:00}:{4:00}:{5:00}:{6:0000}...{7}", date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, date.Ticks);
    }
}
