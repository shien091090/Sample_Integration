using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;

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
        public string ntpServerName { private set; get; }
        public string ntpServerAddress { private set; get; }

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

                    return (int)( ( t1 - t0 ) + ( t2 - t3 ) ) / 2;
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

                    return (int)( ( t3 - t0 ) - ( t2 - t1 ) );
                }
            }
        }

        public TimeFlow(ulong timeStamp, string serverName, string serverAddress)
        {
            ClientSendTimeStamp = timeStamp;
            ntpServerName = serverName;
            ntpServerAddress = serverAddress;

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

    public class NTPServerEvaluation
    {
        private string[] ntpServerNames;
        private int invalidScore = 1;
        private float filterPercentage = 0.4f;

        public Dictionary<string, float> EvaluationScoreTable { private set; get; }

        public int SamplingThreshold { private set; get; }

        public NTPServerEvaluation(string[] serverNameArr, int threshold)
        {
            SamplingThreshold = threshold;
            ntpServerNames = serverNameArr;
        }

        public string[] EvaluateNTPServer(List<TimeFlow> timeFlowRecords)
        {
            if (ntpServerNames == null || ntpServerNames.Length <= SamplingThreshold)
                return null;

            EvaluationScoreTable = new Dictionary<string, float>();

            for (int i = 0; i < ntpServerNames.Length; i++)
            {
                string _ntpServer = ntpServerNames[i];

                TimeFlow[] _filterTimeFlows = timeFlowRecords
                    .Where(x => x.ntpServerName == _ntpServer)
                    .ToArray();

                float _scoreAverage = 0;
                int _deno = 0;
                for (int j = 0; j < _filterTimeFlows.Length; j++)
                {
                    int _score = 0;

                    switch (_filterTimeFlows[j].GetState)
                    {
                        case TimeFlowState.NotCompleted:
                            continue;

                        case TimeFlowState.Valid:
                            _score = ScoreAlgorithm(_filterTimeFlows[j].GetRoundTripDelay);
                            break;

                        case TimeFlowState.Invalid:
                            _score = invalidScore;
                            break;
                    }

                    _scoreAverage += _score;
                    _deno++;
                }
                _scoreAverage /= _deno;

                EvaluationScoreTable[_ntpServer] = _scoreAverage;
            }

            int _resultCount = Mathf.RoundToInt(EvaluationScoreTable.Count * filterPercentage);
            string[] _resultServer = new string[_resultCount];

            KeyValuePair<string, float>[] _sortEvaluation = EvaluationScoreTable.
                OrderByDescending(x => x.Value).
                ToArray();

            for (int i = 0; i < _resultServer.Length; i++)
            {
                _resultServer[i] = _sortEvaluation[i].Key;
            }

            return _resultServer;
        }

        private int ScoreAlgorithm(int delayValue)
        {
            return Mathf.RoundToInt(5000f / Mathf.Pow(delayValue, 1.55f));
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
    public bool printTargetServer;

    [Header("SocketSetting")]
    public AddressFamily addressFamily = AddressFamily.InterNetworkV6;
    public SocketType socketType = SocketType.Dgram;
    public ProtocolType protocolType = ProtocolType.Udp;

    private bool ntpGetting = false;
    private Dictionary<string, IPEndPoint> ipEndPointTable;
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

        bool _isInvalid = SetIPPoint(NTP_SERVER, out ipEndPointTable);
        if (!_isInvalid || ipEndPointTable == null || ipEndPointTable.Count <= 0)
        {
            ntpGetting = false;
            yield break;
        }

        while (!isStop)
        {
            GetNTPTime(ipEndPointTable);

            yield return new WaitForSeconds(freq);
        }

        isStop = false;
        ntpGetting = false;
    }

    private bool SetIPPoint(string[] servers, out Dictionary<string, IPEndPoint> ipEndPointTable)
    {
        Dictionary<string, IPEndPoint> _ipEndPointTable = new Dictionary<string, IPEndPoint>();

        for (int i = 0; i < servers.Length; i++)
        {
            try
            {
                IPAddress[] addresses = Dns.GetHostEntry(servers[i]).AddressList;

                IPAddress[] _filterAddresses = addresses
                    .Where(x => x.AddressFamily == addressFamily)
                    .ToArray();

                IPEndPoint _iPEndPoint = new IPEndPoint(_filterAddresses[0], 123);
                _ipEndPointTable.Add(servers[i], _iPEndPoint);
            }
            catch (Exception _exception)
            {
                Debug.Log(_exception);
            }
        }

        if (_ipEndPointTable.Count <= 0)
        {
            ipEndPointTable = null;
            return false;
        }


        ipEndPointTable = _ipEndPointTable;
        return true;
    }

    private void GetNTPTime(Dictionary<string, IPEndPoint> _ipEndPointTable)
    {
        foreach (KeyValuePair<string, IPEndPoint> ipPoint in _ipEndPointTable)
        {
            TimeFlow _timeRecord = new TimeFlow(GetClientNowTimeStamp(), ipPoint.Key, ipPoint.Value.Address.ToString());

            byte[] ntpData = new byte[48];
            ntpData[0] = 0x1B;

            new Thread(() =>
            {
                var socket = new Socket(addressFamily, socketType, protocolType);
                socket.SendTimeout = 5000;
                socket.ReceiveTimeout = 5000;

                try
                {
                    socket.SendTo(ntpData, ipPoint.Value);
                    socket.Receive(ntpData);

                    ulong _serverReceiveTimestamp = OctBitsPackToMilliseconds(ntpData, 40, 41, 42, 43, 44, 45, 46, 47);
                    ulong _serverTransmitTimestamp = OctBitsPackToMilliseconds(ntpData, 32, 33, 34, 35, 36, 37, 38, 39);

                    RecordNTPRequestResult(_timeRecord.SetReceiveTime(_serverReceiveTimestamp, _serverTransmitTimestamp, GetClientNowTimeStamp()));

                    socket.Close();
                }
                catch (Exception _exception)
                {
                    socket.Close();

                    RecordNTPRequestResult(_timeRecord.SetReceiveTime(0, 0, 0, string.Format("NTPClockGetError : {0}", _exception)));
                }

            }).Start();
        }
    }

    private void RecordNTPRequestResult(TimeFlow timeRecord)
    {
        if (ntpTimeRecords == null)
            ntpTimeRecords = new List<TimeFlow>();

        ntpTimeRecords.Add(timeRecord);

        bool _isPrint = ( printDelay || printOffset || printDetailTimeStamp || printDetailDate || printTargetServer );

        if (_isPrint)
        {
            string _debugLog = string.Empty;

            if (printTargetServer)
                _debugLog += string.Format("ServerName = {0}, ServerAddress = {1}\n", timeRecord.ntpServerName, timeRecord.ntpServerAddress);

            if (printDelay)
                _debugLog += string.Format("Delay = {0} ms\n", timeRecord.GetRoundTripDelay);

            if (printOffset)
                _debugLog += string.Format("Offset = {0} ms\n", timeRecord.GetTimeOffset);

            if (printDetailTimeStamp)
            {
                _debugLog += string.Format("ClientSendTimeStamp = {0} ms\n", timeRecord.ClientSendTimeStamp);
                _debugLog += string.Format("ServerReceiveTimeStamp = {0} ms\n", timeRecord.ServerReceiveTimeStamp);
                _debugLog += string.Format("ClientReceiveTimeStamp = {0} ms\n", timeRecord.ClientReceiveTimeStamp);
                _debugLog += string.Format("ServerTansmitTimeStamp = {0} ms\n", timeRecord.ServerTansmitTimeStamp);
            }

            if (printDetailDate)
            {
                _debugLog += string.Format("ClientSendTime Date = {0}\n", GetDateDetailLog(ConvertMilliSecondToDate(timeRecord.ClientSendTimeStamp)));
                _debugLog += string.Format("ServerReceiveTime Date= {0}\n", GetDateDetailLog(ConvertMilliSecondToDate(timeRecord.ServerReceiveTimeStamp)));
                _debugLog += string.Format("ClientReceiveTime Date= {0}\n", GetDateDetailLog(ConvertMilliSecondToDate(timeRecord.ClientReceiveTimeStamp)));
                _debugLog += string.Format("ServerTansmitTime Date= {0}\n", GetDateDetailLog(ConvertMilliSecondToDate(timeRecord.ServerTansmitTimeStamp)));
            }

            if (!string.IsNullOrEmpty(timeRecord.Log))
                _debugLog += string.Format("[Error Log] : {0}\n", timeRecord.Log);

            if (!string.IsNullOrEmpty(_debugLog))
                Debug.Log(_debugLog);
        }
    }

    private ulong GetClientNowTimeStamp()
    {
        TimeSpan _span = DateTime.UtcNow - ntpTimeOrigin;
        return (ulong)_span.TotalMilliseconds;
    }

    private ulong OctBitsPackToMilliseconds(byte[] datas, int int_4, int int_3, int int_2, int int_1, int fract_4, int fract_3, int fract_2, int fract_1)
    {
        ulong intPart = (ulong)datas[int_4] << 24 | (ulong)datas[int_3] << 16 | (ulong)datas[int_2] << 8 | (ulong)datas[int_1];
        ulong fractPart = (ulong)datas[fract_4] << 24 | (ulong)datas[fract_3] << 16 | (ulong)datas[fract_2] << 8 | (ulong)datas[fract_1];

        ulong milliseconds = ( intPart * 1000 ) + ( ( fractPart * 1000 ) / 0x100000000L );

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
