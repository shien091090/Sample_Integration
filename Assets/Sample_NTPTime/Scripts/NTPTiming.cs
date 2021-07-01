using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System;
using UnityEngine;
using SNShien.Common.MathTools;

public class NTPTiming : MonoBehaviour
{
    private static NTPTiming _instance;
    public static NTPTiming Instance { get { return _instance; } }

    private string[] NTP_SERVER = new string[]
    {
        "TIME.google.com",
        "TIME1.google.com",
        "TIME2.google.com",
        "TIME3.google.com",
        "TIME4.google.com"
    };

    private static NTPAnalysisRecord s_ntpAnalysisRecord;
    private static DateTime ntpTimeOrigin = new DateTime(1900, 1, 1, 0, 0, 0, 0);
    [SerializeField] private int serverConnectRetryTimes = 5;
    [SerializeField] private int ntpOutlierRejectValue = 50; //ms
    [SerializeField] private ConnectState CurrentConnectState;

    [Header("LogType")]
    public bool printOffset;
    public bool printDelay;
    public bool printDetailTimeStamp;
    public bool printDetailDate;
    public bool printTargetServer;
    public bool printEvaluationResult;

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
        s_ntpAnalysisRecord = new NTPAnalysisRecord(NTP_SERVER);

        StartCoroutine(Cor_BuildConnectTarget(serverConnectRetryTimes));
    }

    public void GetNTPTime(float connectFreq, int evaluationThreshold, Action<ulong> callback)
    {
        if (CurrentConnectState == ConnectState.Broken)
        {
            StopAllCoroutines();
            Init();
            return;
        }
        else if (CurrentConnectState != ConnectState.StandBy)
            return;

        StartCoroutine(Cor_GetNTPTime(connectFreq, evaluationThreshold, callback));
    }

    private IEnumerator Cor_GetNTPTime(float connectFreq, int evaluationThreshold, Action<ulong> callback)
    {
        Coroutine _evaluationListen = StartCoroutine(Cor_EvaluationListen(evaluationThreshold));
        Coroutine _refreshNTPTime = StartCoroutine(Cor_RefreshNTPTime(connectFreq));

        yield return new WaitUntil(() => CurrentConnectState == ConnectState.Response);
        yield return _evaluationListen;
        yield return _refreshNTPTime;

        List<TimeFlow> _bestRecords = s_ntpAnalysisRecord.GetBestNtpTimeRecord;
        List<TimeFlow> _filterRecords = GetOutlierRejectResult(_bestRecords, ntpOutlierRejectValue);
        ulong _ntpTimeStamp = ReturnAnalysisNTPTime(_filterRecords);

        Init();
        callback.Invoke(_ntpTimeStamp);
    }

    private IEnumerator Cor_BuildConnectTarget(int retryTimes)
    {
        for (int i = 0; i < retryTimes; i++)
        {
            yield return new WaitUntil(() => Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork);

            Dictionary<string, IPEndPoint> _ipEndPointTable = new Dictionary<string, IPEndPoint>();
            AddressFamily _usedAddressFamily = AddressFamily.Unspecified;
            bool _isSuccess = SetIPPointAndCheckValid(s_ntpAnalysisRecord.CurrentServers, out _ipEndPointTable, out _usedAddressFamily);
            if (_isSuccess && _ipEndPointTable != null && _ipEndPointTable.Count > 0)
            {
                s_ntpAnalysisRecord.SetIPTarget(_ipEndPointTable, _usedAddressFamily);
                CurrentConnectState = ConnectState.StandBy;
                yield break;
            }
            else
                yield return new WaitForSeconds(1);
        }

        CurrentConnectState = ConnectState.Broken;
    }

    private IEnumerator Cor_EvaluationListen(int evaluationThreshold)
    {
        if (CurrentConnectState == ConnectState.Initialize)
            yield return new WaitUntil(() => CurrentConnectState == ConnectState.StandBy);

        s_ntpAnalysisRecord.CreateEvaluationMachine(evaluationThreshold);
        while (s_ntpAnalysisRecord.CurrentServers.Count > 0)
        {
            yield return new WaitUntil(() => s_ntpAnalysisRecord.IsCrossEvaluationThreshold());
            s_ntpAnalysisRecord.CutAndEvaluate(true);
        }

        CurrentConnectState = ConnectState.Response;

    }

    private IEnumerator Cor_RefreshNTPTime(float freq)
    {
        yield return new WaitUntil(() => s_ntpAnalysisRecord.IsReadyForEvaluation());

        CurrentConnectState = ConnectState.Getting;

        while (CurrentConnectState == ConnectState.Getting)
        {
            //GetNTPTime(s_ntpAnalysisRecord.IpEndPointTable, s_ntpAnalysisRecord.CurrentAddressFamily);

            yield return new WaitForSeconds(freq);
        }
    }

    private List<TimeFlow> GetOutlierRejectResult(List<TimeFlow> records, int rejectValue)
    {
        int _offsetAverage = SimpleAlgorithm.GetAverageAfterProcess(records.Count, () =>
        {
            int _sum = 0;
            for (int i = 0; i < records.Count; i++)
            {
                _sum += records[i].GetTimeOffset;
            }

            return _sum;
        });

        List<TimeFlow> _result = new List<TimeFlow>();
        for (int i = 0; i < records.Count; i++)
        {
            TimeFlow _record = records[i];

            if (Mathf.Abs(_record.GetTimeOffset - _offsetAverage) <= rejectValue)
                _result.Add(_record);
        }

        return _result;
    }

    private ulong ReturnAnalysisNTPTime(List<TimeFlow> records)
    {
        int _offsetAverage = SimpleAlgorithm.GetAverageAfterProcess(records.Count, () =>
        {
            int _sum = 0;
            for (int i = 0; i < records.Count; i++)
            {
                _sum += records[i].GetTimeOffset;
            }

            return _sum;
        });

        ulong _clientNowTimeStamp = GetClientNowTimeStamp();
        ulong _fixedTimeStamp = _clientNowTimeStamp + (ulong)_offsetAverage;

        Debug.Log(string.Format("NTP Date= {0}\n", GetDateDetailLog(_fixedTimeStamp)));
        Debug.Log(string.Format("Client Date= {0}\n", GetDateDetailLog(_clientNowTimeStamp)));

        return _fixedTimeStamp;
    }

    private bool SetIPPointAndCheckValid(List<string> servers, out Dictionary<string, IPEndPoint> ipEndPointTable, out AddressFamily usedAddressFamily)
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
            usedAddressFamily = AddressFamily.Unknown;
            ipEndPointTable = null;
            return false;
        }
        else
            usedAddressFamily = _useIpv4 ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6;

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

    private void GetNTPTime(Dictionary<string, IPEndPoint> _ipEndPointTable, AddressFamily _addressFamily)
    {
        Dictionary<string, IPEndPoint> _tempTable = new Dictionary<string, IPEndPoint>(_ipEndPointTable);

        foreach (KeyValuePair<string, IPEndPoint> ipPoint in _tempTable)
        {
            byte[] ntpData = new byte[48];
            ntpData[0] = 0x1B;

            new Thread(() =>
            {
                TimeFlow _timeRecord = new TimeFlow(
                    GetClientNowTimeStamp(),
                    ipPoint.Key,
                    ipPoint.Value.Address.ToString(),
                    Thread.CurrentThread.ManagedThreadId);

                Socket socket = new Socket(_addressFamily, SocketType.Dgram, ProtocolType.Udp);
                socket.SendTimeout = 2000;
                socket.ReceiveTimeout = 2000;

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

    private async Task<TimeFlow> AsyncGetNTPTime(string serverName, IPEndPoint ipInfo, AddressFamily _addressFamily)
    {
        byte[] ntpData = new byte[48];
        ntpData[0] = 0x1B;

        TimeFlow _timeRecord = new TimeFlow(
           GetClientNowTimeStamp(),
           serverName,
           ipInfo.Address.ToString(),
           Thread.CurrentThread.ManagedThreadId);

        Socket socket = new Socket(_addressFamily, SocketType.Dgram, ProtocolType.Udp);
        socket.SendTimeout = 2000;
        socket.ReceiveTimeout = 2000;

        try
        {
            socket.SendTo(ntpData, ipInfo);
            socket.Receive(ntpData);

            ulong _serverReceiveTimestamp = OctBitsPackToMilliseconds(ntpData, 40, 41, 42, 43, 44, 45, 46, 47);
            ulong _serverTransmitTimestamp = OctBitsPackToMilliseconds(ntpData, 32, 33, 34, 35, 36, 37, 38, 39);

            socket.Close();

            _timeRecord.SetReceiveTime(_serverReceiveTimestamp, _serverTransmitTimestamp, GetClientNowTimeStamp());
            return _timeRecord;
        }
        catch (Exception _exception)
        {
            socket.Close();

            _timeRecord.SetReceiveTime(0, 0, 0, string.Format("NTPClockGetError : {0}", _exception));
            return _timeRecord;
        }
    }

    private void RecordNTPRequestResult(TimeFlow timeRecord)
    {
        if (timeRecord == null)
            return;

        s_ntpAnalysisRecord.AddRecord(timeRecord);

        bool _isPrint = ( printDelay || printOffset || printDetailTimeStamp || printDetailDate || printTargetServer );
        if (_isPrint)
            PrintDebugLog(timeRecord);
    }

    private void PrintDebugLog(TimeFlow timeFlowData)
    {
        string _debugLog = string.Empty;

        if (printTargetServer)
            _debugLog += string.Format("ServerName = {0}, ServerAddress = {1}\n", timeFlowData.NTPServerName, timeFlowData.NTPServerAddress);

        if (printDelay)
            _debugLog += string.Format("Delay = {0} ms\n", timeFlowData.GetRoundTripDelay);

        if (printOffset)
            _debugLog += string.Format("Offset = {0} ms\n", timeFlowData.GetTimeOffset);

        if (printDetailTimeStamp)
        {
            _debugLog += string.Format("ClientSendTimeStamp = {0} ms\n", timeFlowData.ClientSendTimeStamp);
            _debugLog += string.Format("ServerReceiveTimeStamp = {0} ms\n", timeFlowData.ServerReceiveTimeStamp);
            _debugLog += string.Format("ClientReceiveTimeStamp = {0} ms\n", timeFlowData.ClientReceiveTimeStamp);
            _debugLog += string.Format("ServerTansmitTimeStamp = {0} ms\n", timeFlowData.ServerTansmitTimeStamp);
        }

        if (printDetailDate)
        {
            _debugLog += string.Format("ClientSendTime Date = {0}\n", GetDateDetailLog(timeFlowData.ClientSendTimeStamp));
            _debugLog += string.Format("ServerReceiveTime Date= {0}\n", GetDateDetailLog(timeFlowData.ServerReceiveTimeStamp));
            _debugLog += string.Format("ClientReceiveTime Date= {0}\n", GetDateDetailLog(timeFlowData.ClientReceiveTimeStamp));
            _debugLog += string.Format("ServerTansmitTime Date= {0}\n", GetDateDetailLog(timeFlowData.ServerTansmitTimeStamp));
        }

        if (!string.IsNullOrEmpty(timeFlowData.Log))
            _debugLog += string.Format("[Error Log] : {0}\n", timeFlowData.Log);

        if (!string.IsNullOrEmpty(_debugLog))
            Debug.Log(_debugLog);
    }

    public static ulong GetClientNowTimeStamp()
    {
        TimeSpan _span = DateTime.UtcNow - ntpTimeOrigin;
        return (ulong)_span.TotalMilliseconds;
    }

    public static ulong OctBitsPackToMilliseconds(byte[] datas, int int_4, int int_3, int int_2, int int_1, int fract_4, int fract_3, int fract_2, int fract_1)
    {
        ulong intPart = (ulong)datas[int_4] << 24 | (ulong)datas[int_3] << 16 | (ulong)datas[int_2] << 8 | (ulong)datas[int_1];
        ulong fractPart = (ulong)datas[fract_4] << 24 | (ulong)datas[fract_3] << 16 | (ulong)datas[fract_2] << 8 | (ulong)datas[fract_1];

        ulong milliseconds = ( intPart * 1000 ) + ( ( fractPart * 1000 ) / 0x100000000L );

        return milliseconds;
    }

    public static DateTime ConvertMilliSecondToDate(ulong ms)
    {
        return ntpTimeOrigin.Add(TimeSpan.FromMilliseconds(ms));
    }

    public static string GetDateDetailLog(DateTime date)
    {
        return string.Format("{0:0000}.{1:00}.{2:00} / {3:00}:{4:00}:{5:00}:{6:0000}...{7}", date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, date.Ticks);
    }

    public static string GetDateDetailLog(ulong ms)
    {
        DateTime _date = ConvertMilliSecondToDate(ms);
        return GetDateDetailLog(_date);
    }
}
