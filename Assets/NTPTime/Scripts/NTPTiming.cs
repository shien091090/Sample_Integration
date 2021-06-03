using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using UnityEngine;

public class NTPTiming : MonoBehaviour
{
    public enum TimeFlowState
    {
        NotCompleted,
        Valid,
        Invalid
    }

    public enum ConnectState
    {
        Initialize,
        Broken,
        StandBy,
        Getting
    }

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
        private List<string> ntpServerNames;
        private int invalidScore = 1;
        private float filterPercentage = 0.4f;

        public Dictionary<string, float> EvaluationScoreTable { private set; get; }

        public int SamplingThreshold { private set; get; }

        public NTPServerEvaluation(List<string> serverNameArr, int threshold, float filterRate)
        {
            SamplingThreshold = threshold;
            filterPercentage = filterRate;
            ntpServerNames = serverNameArr;
        }

        public string[] EvaluateNTPServer(List<TimeFlow> timeFlowRecords, bool printDebugLog = false)
        {
            if (ntpServerNames == null || ntpServerNames.Count <= 0)
                return null;

            List<TimeFlow> _records = new List<TimeFlow>();
            _records.AddRange(timeFlowRecords);

            EvaluationScoreTable = new Dictionary<string, float>();

            for (int i = 0; i < ntpServerNames.Count; i++)
            {
                string _ntpServer = ntpServerNames[i];

                TimeFlow[] _filterTimeFlows = _records
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


            int _removeCount = Mathf.Clamp(Mathf.RoundToInt(EvaluationScoreTable.Count - EvaluationScoreTable.Count * filterPercentage), 0, EvaluationScoreTable.Count - 1);
            string[] _removeServer = new string[_removeCount];

            KeyValuePair<string, float>[] _sortEvaluation = EvaluationScoreTable.
                OrderBy(x => x.Value).
                ToArray();

            for (int i = 0; i < _removeServer.Length; i++)
            {
                _removeServer[i] = _sortEvaluation[i].Key;
            }

            if (printDebugLog)
            {
                string _log = "<color=yellow>---- Start Evaluation ----</color>\n";

                foreach (KeyValuePair<string, float> _eval in EvaluationScoreTable)
                {
                    _log += string.Format("[{0}] score : {1}\n", _eval.Key, _eval.Value);
                }

                _log += "---- Remove Server ----\n";
                for (int i = 0; i < _removeServer.Length; i++)
                {
                    _log += string.Format("[{0}] {1}\n", i, _removeServer[i]);
                }

                Debug.Log(_log);
            }

            return _removeServer;
        }

        private int ScoreAlgorithm(int delayValue)
        {
            return Mathf.RoundToInt(5000f / Mathf.Pow(delayValue, 1.55f));
        }
    }

    //-----------------------------------------------------------------------

    private static NTPTiming _instance;
    public static NTPTiming Instance { get { return _instance; } }

    private List<string> NTP_SERVER = new List<string>()
    {
        "TIME.google.com",
        "TIME1.google.com",
        "TIME2.google.com",
        "TIME3.google.com",
        "TIME4.google.com"
    };

    private readonly DateTime ntpTimeOrigin = new DateTime(1900, 1, 1, 0, 0, 0, 0);
    private int serverConnectRetryTimes = 5;
    //public float freq = 3;
    //[Range(0f, 1f)] public float filterRate = 0.4f;
    //public int evaluationThreshold = 10;

    [Header("LogType")]
    public bool printOffset;
    public bool printDelay;
    public bool printDetailTimeStamp;
    public bool printDetailDate;
    public bool printTargetServer;
    public bool printEvaluationResult;

    public static ConnectState CurrentConnectState { private set; get; }
    private static NTPServerEvaluation evaluationMachine;
    private List<string> currentServers;
    private AddressFamily currentAddressFamily = AddressFamily.Unknown;
    private Dictionary<string, IPEndPoint> ipEndPointTable;
    private List<TimeFlow> ntpTimeRecords;

    //-----------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null)
            _instance = this;

        Init();
    }

    private void Init()
    {
        CurrentConnectState = ConnectState.Initialize;

        evaluationMachine = null;
        currentAddressFamily = AddressFamily.Unknown;
        ipEndPointTable = new Dictionary<string, IPEndPoint>();
        ntpTimeRecords = new List<TimeFlow>();

        currentServers = NTP_SERVER;
        StartCoroutine(Cor_BuildConnectTarget(serverConnectRetryTimes));
    }

    public void GetNTPTime(int connectFreq, float filterRate, int evaluationThreshold, Action<long> callback)
    {
        if (CurrentConnectState == ConnectState.Broken)
        {
            StopAllCoroutines();
            Init();
        }
        else if (CurrentConnectState == ConnectState.Initialize || CurrentConnectState == ConnectState.Getting)
            return;

        StartCoroutine(Cor_EvaluationListen(filterRate, evaluationThreshold));
        StartCoroutine(Cor_RefreshNTPTime(connectFreq));

    }

    private IEnumerator Cor_BuildConnectTarget(int retryTimes)
    {
        for (int i = 0; i < retryTimes; i++)
        {
            yield return new WaitUntil(() => Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork);

            bool _isSuccess = SetIPPoint(currentServers, out ipEndPointTable);
            if (_isSuccess && ipEndPointTable != null && ipEndPointTable.Count > 0)
            {
                CurrentConnectState = ConnectState.StandBy;
                break;
            }
            else
                yield return new WaitForSeconds(1);
        }

        CurrentConnectState = ConnectState.Broken;
    }

    public IEnumerator Cor_RefreshNTPTime(float freq)
    {
        yield return new WaitUntil(() => evaluationMachine != null && ipEndPointTable != null);

        CurrentConnectState = ConnectState.Getting;

        while (CurrentConnectState == ConnectState.Getting)
        {
            GetNTPTime(ipEndPointTable);

            yield return new WaitForSeconds(freq);
        }
    }

    private IEnumerator Cor_EvaluationListen(float filterRate, int evaluationThreshold)
    {
        if (CurrentConnectState == ConnectState.Initialize)
            yield return new WaitUntil(() => CurrentConnectState == ConnectState.StandBy);

        evaluationMachine = new NTPServerEvaluation(currentServers, evaluationThreshold, filterRate);
        while (currentServers.Count > 1)
        {
            ntpTimeRecords = new List<TimeFlow>();

            yield return new WaitUntil(() =>
            ntpTimeRecords != null &&
            ntpTimeRecords.Count >= evaluationMachine.SamplingThreshold);

            List<TimeFlow> _records = new List<TimeFlow>();
            _records.AddRange(ntpTimeRecords);

            string[] _removeServers = evaluationMachine.EvaluateNTPServer(_records, printEvaluationResult);

            if (_removeServers == null)
                continue;

            for (int i = 0; i < _removeServers.Length; i++)
            {
                currentServers.Remove(_removeServers[i]);
                ipEndPointTable.Remove(_removeServers[i]);
            }
        }

    }

    private bool SetIPPoint(List<string> servers, out Dictionary<string, IPEndPoint> ipEndPointTable)
    {
        Dictionary<string, IPEndPoint> _ipEndPointTable = new Dictionary<string, IPEndPoint>();

        Dictionary<string, IPAddress> _ipv4AddressTable = new Dictionary<string, IPAddress>();
        Dictionary<string, IPAddress> _ipv6AddressTable = new Dictionary<string, IPAddress>();

        for (int i = 0; i < servers.Count; i++)
        {
            try
            {
                string _serverName = servers[i];

                IPAddress[] _ipv4Address = GetServerAddress(_serverName, AddressFamily.InterNetwork);
                IPAddress[] _ipv6Address = GetServerAddress(_serverName, AddressFamily.InterNetworkV6);

                if (_ipv4Address != null)
                    _ipv4AddressTable.Add(_serverName, _ipv4Address[0]);

                if (_ipv6Address != null)
                    _ipv6AddressTable.Add(_serverName, _ipv6Address[0]);

            }
            catch (Exception _exception)
            {
                Debug.Log(_exception);
            }
        }

        bool _useIpv4 = _ipv4AddressTable.Count >= _ipv6AddressTable.Count;
        Dictionary<string, IPAddress> _targetAddressTable = _useIpv4 ? _ipv4AddressTable : _ipv6AddressTable;

        if (_targetAddressTable.Count <= 0)
        {
            currentAddressFamily = AddressFamily.Unknown;
            ipEndPointTable = null;
            return false;
        }
        else
            currentAddressFamily = _useIpv4 ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6;

        foreach (KeyValuePair<string, IPAddress> ipInfo in _targetAddressTable)
        {
            IPEndPoint _iPEndPoint = new IPEndPoint(ipInfo.Value, 123);
            _ipEndPointTable.Add(ipInfo.Key, _iPEndPoint);
        }

        ipEndPointTable = _ipEndPointTable;
        return true;
    }

    private IPAddress[] GetServerAddress(string serverName, AddressFamily addressFamily)
    {
        IPAddress[] _addresses = Dns.GetHostEntry(serverName).AddressList;

        IPAddress[] _filterAddresses = _addresses
            .Where(x => x.AddressFamily == addressFamily)
            .ToArray();

        if (_filterAddresses == null || _filterAddresses.Length <= 0)
            return null;
        else
            return _filterAddresses;
    }

    private void GetNTPTime(Dictionary<string, IPEndPoint> _ipEndPointTable)
    {
        Dictionary<string, IPEndPoint> _tempTable = new Dictionary<string, IPEndPoint>(_ipEndPointTable);

        foreach (KeyValuePair<string, IPEndPoint> ipPoint in _tempTable)
        {
            byte[] ntpData = new byte[48];
            ntpData[0] = 0x1B;

            new Thread(() =>
            {
                TimeFlow _timeRecord = new TimeFlow(GetClientNowTimeStamp(), ipPoint.Key, ipPoint.Value.Address.ToString());

                Socket socket = new Socket(currentAddressFamily, SocketType.Dgram, ProtocolType.Udp);
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
