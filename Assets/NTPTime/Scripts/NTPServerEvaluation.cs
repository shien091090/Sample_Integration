using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NTPServerEvaluation
{
    private List<string> ntpServerNames;
    private int invalidScore;

    public Dictionary<string, float> EvaluationScoreTable { private set; get; }

    public int SamplingThreshold { private set; get; }

    public NTPServerEvaluation(List<string> serverNameArr, int threshold)
    {
        SamplingThreshold = threshold;
        ntpServerNames = serverNameArr;
    }

    public string[] EvaluateServerAndReturnFailedServers(List<TimeFlow> timeFlowRecords, bool printDebugLog = false)
    {
        if (ntpServerNames == null || ntpServerNames.Count <= 0)
            return null;

        List<TimeFlow> _records = new List<TimeFlow>();
        try
        {
            _records.AddRange(timeFlowRecords);
        }
        catch (System.Exception errLog)
        {
            Debug.Log(errLog);
            return null;
        }

        if (EvaluationScoreTable == null)
            EvaluationScoreTable = new Dictionary<string, float>();

        for (int i = 0; i < ntpServerNames.Count; i++)
        {
            string _ntpServer = ntpServerNames[i];

            List<TimeFlow> _filterTimeFlows = new List<TimeFlow>();
            for (int j = 0; j < _records.Count; j++)
            {
                if (_records[j] == null || string.IsNullOrEmpty(_records[j].NTPServerName))
                    continue;

                if (_records[j].NTPServerName == _ntpServer)
                    _filterTimeFlows.Add(_records[j]);
            }

            if (_filterTimeFlows == null | _filterTimeFlows.Count <= 0)
                continue;

            float _scoreAverage = 0;
            int _deno = 0;
            for (int j = 0; j < _filterTimeFlows.Count; j++)
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

            if (EvaluationScoreTable.ContainsKey(_ntpServer))
                EvaluationScoreTable[_ntpServer] += _scoreAverage;
            else
                EvaluationScoreTable[_ntpServer] = _scoreAverage;
        }

        int _removeCount = ntpServerNames.Count > 0 ? 1 : 0;
        string[] _removeServer = new string[_removeCount];

        KeyValuePair<string, float>[] _sortEvaluation = EvaluationScoreTable
            .OrderBy(x => x.Value)
            .ToArray();

        for (int i = 0; i < _removeServer.Length; i++)
        {
            string _remove = _sortEvaluation[i].Key;
            _removeServer[i] = _remove;
            EvaluationScoreTable.Remove(_remove);
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
