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
    [System.Serializable]
    public class TimeFlow
    {
        public ulong ClientSendTimeStamp { private set; get; }
        public ulong ServerReceiveTimeStamp { private set; get; }
        public ulong ClientReceiveTimeStamp { private set; get; }
        public ulong ServerTansmitTimeStamp { private set; get; }
        public bool IsFlowComplete { private set; get; }
        public string Log { private set; get; }
        public string NTPServerName { private set; get; }
        public string NTPServerAddress { private set; get; }

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
            NTPServerName = serverName;
            NTPServerAddress = serverAddress;

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

        public NTPServerEvaluation(string[] serverNameArr, int threshold, float filterRate)
        {
            SamplingThreshold = threshold;
            filterPercentage = filterRate;
            ntpServerNames = serverNameArr;
        }

        public string[] EvaluateNTPServer(List<TimeFlow> timeFlowRecords, bool printDebugLog = false)
        {
            if (ntpServerNames == null || ntpServerNames.Length <= 0)
                return null;

            EvaluationScoreTable = new Dictionary<string, float>();

            for (int i = 0; i < ntpServerNames.Length; i++)
            {
                string _ntpServer = ntpServerNames[i];

                TimeFlow[] _filterTimeFlows = timeFlowRecords
                    .Where(x => x.NTPServerName == _ntpServer)
                    .ToArray();

                if (_filterTimeFlows == null | _filterTimeFlows.Length <= 0)
                    continue;

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

            int _resultCount = Mathf.Clamp(Mathf.RoundToInt(EvaluationScoreTable.Count * filterPercentage), 1, EvaluationScoreTable.Count - 1);
            string[] _resultServer = new string[_resultCount];

            KeyValuePair<string, float>[] _sortEvaluation = EvaluationScoreTable.
                OrderByDescending(x => x.Value).
                ToArray();

            for (int i = 0; i < _resultServer.Length; i++)
            {
                _resultServer[i] = _sortEvaluation[i].Key;
            }

            if (printDebugLog)
            {
                string _log = "<color=yellow>---- Start Evaluation ----</color>\n";

                foreach (KeyValuePair<string, float> _eval in EvaluationScoreTable)
                {
                    _log += string.Format("[{0}] score : {1}\n", _eval.Key, _eval.Value);
                }

                _log += "---- Better Server ----\n";
                for (int i = 0; i < _resultServer.Length; i++)
                {
                    _log += string.Format("[{0}] {1}\n", i, _resultServer[i]);
                }

                Debug.Log(_log);
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

    public float freq = 3;
    [Range(0f, 1f)] public float filterRate = 0.4f;
    public bool isStop = false;
    public int evaluationThreshold = 10;

    [Header("LogType")]
    public bool printOffset;
    public bool printDelay;
    public bool printDetailTimeStamp;
    public bool printDetailDate;
    public bool printTargetServer;
    public bool printEvaluationResult;

    [Header("SocketSetting")]
    public AddressFamily addressFamily = AddressFamily.InterNetworkV6;
    public SocketType socketType = SocketType.Dgram;
    public ProtocolType protocolType = ProtocolType.Udp;

    private bool ntpGetting = false;
    private string[] currentServers;
    private Dictionary<string, IPEndPoint> ipEndPointTable;
    private List<TimeFlow> ntpTimeRecords = new List<TimeFlow>();

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
        currentServers = NTP_SERVER;

        StartCoroutine(Cor_EvaluationListen());

        while (!isStop)
        {
            bool _isInvalid = SetIPPoint(currentServers, out ipEndPointTable);
            if (!_isInvalid || ipEndPointTable == null || ipEndPointTable.Count <= 0)
            {
                ntpGetting = false;
                yield break;
            }

            GetNTPTime(ipEndPointTable);

            yield return new WaitForSeconds(freq);
        }

        isStop = false;
        ntpGetting = false;
    }

    private IEnumerator Cor_EvaluationListen()
    {
        while (currentServers.Length > 1)
        {
            NTPServerEvaluation _evaluation = new NTPServerEvaluation(currentServers, evaluationThreshold, filterRate);

            yield return new WaitUntil(() => ntpTimeRecords.Count >= _evaluation.SamplingThreshold);

            List<TimeFlow> _records = new List<TimeFlow>();
            _records.AddRange(ntpTimeRecords);

            string[] _filterServers = _evaluation.EvaluateNTPServer(_records, printEvaluationResult);

            if (_filterServers == null)
                continue;

            currentServers = _filterServers;
            ntpTimeRecords = new List<TimeFlow>();
        }

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
            byte[] ntpData = new byte[48];
            ntpData[0] = 0x1B;

            new Thread(() =>
            {
                TimeFlow _timeRecord = new TimeFlow(GetClientNowTimeStamp(), ipPoint.Key, ipPoint.Value.Address.ToString());

                Socket socket = new Socket(addressFamily, socketType, protocolType);
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
                _debugLog += string.Format("ServerName = {0}, ServerAddress = {1}\n", timeRecord.NTPServerName, timeRecord.NTPServerAddress);

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
