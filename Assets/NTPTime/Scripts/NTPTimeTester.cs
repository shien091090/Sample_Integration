using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class NTPTimeTester : MonoBehaviour
{
    public class TimeFlow
    {
        private ulong clientSendTimeStamp;
        private ulong serverTimeStamp;
        private ulong clientReceiveTimeStamp;

        public bool IsFlowComplete
        {
            get
            {
                return ( clientSendTimeStamp > 0 && serverTimeStamp > 0 && clientReceiveTimeStamp > 0 );
            }
        }

        public TimeFlow(ulong timeStamp)
        {
            clientSendTimeStamp = timeStamp;
        }
    }

    private string[] NTP_SERVER =
    {
        "TIME.google.com",
        "TIME1.google.com",
        "TIME2.google.com",
        "TIME3.google.com",
        "TIME4.google.com"
    };

    public int freq;
    public bool isStop = false;
    private List<IPEndPoint> ipEndPoint_List;
    private List<TimeFlow> ntpTimeRecord;
    private bool ntpGetting = false;

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
        TimeSpan _span = DateTime.UtcNow - new DateTime(1900, 1, 1, 0, 0, 0, 0);
        TimeFlow _timeRecord = new TimeFlow((ulong)_span.TotalMilliseconds);

        byte[] ntpData = new byte[48];
        ntpData[0] = 0x1B;

        new Thread(() =>
        {
            var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            socket.SendTimeout = 5000;
            socket.ReceiveTimeout = 5000;

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
                Debug.Log("NTPClockGetError : " + _exception);
            }
        }).Start();

    }

    private void ReceiveNTPData(byte[] ntpData, TimeFlow timeRecord)
    {
        ulong _serverSendTime = OctBitsPackToMilliseconds(ntpData, 40, 41, 42, 43, 44, 45, 46, 47);
        DateTime _serverSendDate = new DateTime(1900, 1, 1, 0, 0, 0, 0).Add(TimeSpan.FromMilliseconds(_serverSendTime));

        ulong _clientReceiveTime = OctBitsPackToMilliseconds(ntpData, 32, 33, 34, 35, 36, 37, 38, 39);
        DateTime _clientReceiveDate = new DateTime(1900, 1, 1, 0, 0, 0, 0).Add(TimeSpan.FromMilliseconds(_serverSendTime));

        ulong _clientLocalTime = OctBitsPackToMilliseconds(ntpData, 16, 17, 18, 19, 20, 21, 22, 23);
        DateTime _clientLocalDate = new DateTime(1900, 1, 1, 0, 0, 0, 0).Add(TimeSpan.FromMilliseconds(_serverSendTime));

        ulong _delay = (ulong)ntpData[4] << 24 | (ulong)ntpData[5] << 16 | (ulong)ntpData[6] << 8 | (ulong)ntpData[7];

        //Debug.Log(string.Format("<color=yellow>Delay : {0}</color>", _delay));
        Debug.Log(GetDateDetailLog(_serverSendDate) + "(ServerSendDate)");
        Debug.Log(GetDateDetailLog(_clientReceiveDate) + "(ClientReceiveDate)");
        Debug.Log(GetDateDetailLog(_clientLocalDate) + "(ClientLocalDate)");
        Debug.Log("-------------------");

    }

    private ulong OctBitsPackToMilliseconds(byte[] datas, int int_4, int int_3, int int_2, int int_1, int fract_4, int fract_3, int fract_2, int fract_1)
    {
        ulong intPart = (ulong)datas[int_4] << 24 | (ulong)datas[int_3] << 16 | (ulong)datas[int_2] << 8 | (ulong)datas[int_1];
        ulong fractPart = (ulong)datas[fract_4] << 24 | (ulong)datas[fract_3] << 16 | (ulong)datas[fract_2] << 8 | (ulong)datas[fract_1];

        ulong milliseconds = ( intPart * 1000 ) + ( ( fractPart * 1000 ) / 0x100000000L );

        return milliseconds;
    }

    private string GetDateDetailLog(DateTime date)
    {
        return string.Format("{0:0000}.{1:00}.{2:00} / {3:00}:{4:00}:{5:00}:{6:0000}...{7}", date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, date.Ticks);
    }
}
