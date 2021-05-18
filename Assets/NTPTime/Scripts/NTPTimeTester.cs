using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class NTPTimeTester : MonoBehaviour
{
    private string[] NTP_SERVER =
    {
        "TIME.google.com",
        "TIME1.google.com",
        "TIME2.google.com",
        "TIME3.google.com",
        "TIME4.google.com"
    };

    List<IPEndPoint> ipEndPoint_List = new List<IPEndPoint>();

    public void BTN_NTPTimeTest()
    {
        for (int i = 0; i < NTP_SERVER.Length; i++)
        {
            try
            {
                var addresses = Dns.GetHostEntry(NTP_SERVER[i]).AddressList;
                ipEndPoint_List.Add(new IPEndPoint(addresses[0], 123));
            }
            catch (Exception _exception)
            {
                UnityEngine.Debug.Log(_exception);
            }
        }

        GetNTPClock();

    }

    private void GetNTPClock()
    {
        if (ipEndPoint_List.Count == 0)
            return;

        byte[] ntpData = new byte[48];
        ntpData[0] = 0x1B; //LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)

        new Thread(() =>
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SendTimeout = 5000;
            socket.ReceiveTimeout = 5000;

            try
            {
                socket.SendTo(ntpData, ipEndPoint_List[0]);
                socket.Receive(ntpData);

                ReceiveNTPData(ntpData);

                socket.Close();
            }
            catch (Exception _exception)
            {
                socket.Close();
                UnityEngine.Debug.Log("NTPClockGetError : " + _exception);
            }
        }).Start();
    }

    private void ReceiveNTPData(byte[] ntpData)
    {
        ulong _serverSendTime = OctBitsPackToMilliseconds(ntpData, 40, 41, 42, 43, 44, 45, 46, 47);
        DateTime _serverSendDate = new DateTime(1900, 1, 1, 0, 0, 0, 0).Add(TimeSpan.FromMilliseconds(_serverSendTime));

        ulong _clientReceiveTime = OctBitsPackToMilliseconds(ntpData, 32, 33, 34, 35, 36, 37, 38, 39);
        DateTime _clientReceiveDate = new DateTime(1900, 1, 1, 0, 0, 0, 0).Add(TimeSpan.FromMilliseconds(_serverSendTime));

        ulong _clientLocalTime = OctBitsPackToMilliseconds(ntpData, 16, 17, 18, 19, 20, 21, 22, 23);
        DateTime _clientLocalDate = new DateTime(1900, 1, 1, 0, 0, 0, 0).Add(TimeSpan.FromMilliseconds(_serverSendTime));

        ulong _delay = (ulong)ntpData[4] << 24 | (ulong)ntpData[5] << 16 | (ulong)ntpData[6] << 8 | (ulong)ntpData[7];

        Debug.Log("Delay : " + _delay);
        Debug.Log("ServerSendDate : " + GetDateDetailLog(_serverSendDate));
        Debug.Log("ClientReceiveDate : " + GetDateDetailLog(_clientReceiveDate));
        Debug.Log("ClientLocalDate : " + GetDateDetailLog(_clientLocalDate));
        Debug.Log("UTCNow : " + GetDateDetailLog(DateTime.UtcNow));

    }

    private ulong OctBitsPackToMilliseconds(byte[] datas, int int_4, int int_3, int int_2, int int_1, int fract_4, int fract_3, int fract_2, int fract_1)
    {
        ulong intPart = (ulong)datas[int_4] << 24 | (ulong)datas[int_3] << 16 | (ulong)datas[int_2] << 8 | (ulong)datas[int_1];
        ulong fractPart = (ulong)datas[fract_4] << 24 | (ulong)datas[fract_3] << 16 | (ulong)datas[fract_2] << 8 | (ulong)datas[fract_1];

        ulong milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

        return milliseconds;
    }

    private string GetDateDetailLog(DateTime date)
    {
        return string.Format("{0:0000}.{1:00}.{2:00} / {3:00}:{4:00}:{5:00}:{6:0000}...{7}", date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, date.Ticks);
    }
}
