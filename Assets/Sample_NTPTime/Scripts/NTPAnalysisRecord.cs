using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;

public class NTPAnalysisRecord
{
    public NTPServerEvaluation EvaluationMachine { private set; get; }
    public List<string> TotalServers { private set; get; }
    public List<string> CurrentServers { private set; get; }
    public AddressFamily CurrentAddressFamily { private set; get; }
    public Dictionary<string, IPEndPoint> IpEndPointTable { private set; get; }
    public List<TimeFlow> EvaluatingTimeRecords { private set; get; }
    public List<TimeFlow> TotalNtpTimeRecords { private set; get; }

    private List<TimeFlow> _bestNtpTimeRecord;
    public List<TimeFlow> GetBestNtpTimeRecord
    {
        get
        {
            if (_bestNtpTimeRecord == null)
                return new List<TimeFlow>();

            return _bestNtpTimeRecord;
        }
    }

    public NTPAnalysisRecord(string[] servers)
    {
        TotalServers = new List<string>(servers);
        CurrentServers = new List<string>(servers);
        EvaluationMachine = null;
        CurrentAddressFamily = AddressFamily.Unknown;
        IpEndPointTable = new Dictionary<string, IPEndPoint>();
        EvaluatingTimeRecords = new List<TimeFlow>();
        TotalNtpTimeRecords = new List<TimeFlow>();
    }

    public void CreateEvaluationMachine(int evaluationThreshold)
    {
        EvaluationMachine = new NTPServerEvaluation(CurrentServers, evaluationThreshold);
    }

    public void SetIPTarget(Dictionary<string, IPEndPoint> _ipEndPointTable, AddressFamily _addressFamily)
    {
        IpEndPointTable = _ipEndPointTable;
        CurrentAddressFamily = _addressFamily;
    }

    public bool IsCrossEvaluationThreshold()
    {
        if (EvaluatingTimeRecords == null)
            return false;

        return EvaluatingTimeRecords.Count >= EvaluationMachine.SamplingThreshold;
    }

    public bool IsReadyForEvaluation()
    {
        return ( EvaluationMachine != null && IpEndPointTable != null );
    }

    //Queue<TimeFlow> timeFlowQueue = new Queue<TimeFlow>();
    public void AddRecord(TimeFlow record)
    {
        if (EvaluatingTimeRecords == null)
            EvaluatingTimeRecords = new List<TimeFlow>();

        //timeFlowQueue.Enqueue(record);
        if (record == null)
            return;
        else
            EvaluatingTimeRecords.Add(record);
    }

    public void CutAndEvaluate(bool showDebugLog = false)
    {
        UnityEngine.Debug.Log("[CutAndEvaluate] Thread ID : " + System.Threading.Thread.CurrentThread.ManagedThreadId);
        List<TimeFlow> _records = new List<TimeFlow>();
        //try
        //{
        //    _records.AddRange(EvaluatingTimeRecords);
        //}
        //catch (System.Exception errLog)
        //{
        //    UnityEngine.Debug.Log(errLog);
        //    return;
        //}
        _records.AddRange(EvaluatingTimeRecords);

        TotalNtpTimeRecords.AddRange(EvaluatingTimeRecords);
        EvaluatingTimeRecords = new List<TimeFlow>();

        string[] _removeServers = EvaluationMachine.EvaluateServerAndReturnFailedServers(_records, showDebugLog);

        if (_removeServers == null)
            return;

        for (int i = 0; i < _removeServers.Length; i++)
        {
            string _lastServerName = CurrentServers.Count == 1 ? CurrentServers[0] : string.Empty;

            if (CurrentServers.Remove(_removeServers[i]) && !string.IsNullOrEmpty(_lastServerName))
                ReachBestNTPTimeRecords(_lastServerName);

            IpEndPointTable.Remove(_removeServers[i]);
        }
    }

    private void ReachBestNTPTimeRecords(string bestServer)
    {
        _bestNtpTimeRecord = new List<TimeFlow>();

        if (TotalNtpTimeRecords == null || TotalNtpTimeRecords.Count <= 0)
            return;

        foreach (TimeFlow record in TotalNtpTimeRecords)
        {
            try
            {
                if (record.NTPServerName == bestServer)
                    _bestNtpTimeRecord.Add(record);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.Log(e);
            }

        }
        //_bestNtpTimeRecord = TotalNtpTimeRecords
        //    .Where(x => x.NTPServerName == bestServer)
        //    .ToList();
    }
}
