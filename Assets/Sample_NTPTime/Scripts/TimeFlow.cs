using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public int ThreadID { private set; get; }

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

    public TimeFlow(ulong timeStamp, string serverName, string serverAddress, int threadId)
    {
        ClientSendTimeStamp = timeStamp;
        NTPServerName = serverName;
        NTPServerAddress = serverAddress;
        ThreadID = threadId;

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
