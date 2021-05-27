using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public static class MyStopwatch
{
    public enum TimeUnit
    {
        Milliseconds,
        Ticks
    }

    private static string DefaultFormat = "MethodName : {0}, TimerTest = {1} {2}";

    public static void TimerTest(Action process, TimeUnit timeunit = TimeUnit.Milliseconds, string methodName = null)
    {
        Stopwatch _stopwatch = new Stopwatch();

        _stopwatch.Start();

        process.Invoke();

        _stopwatch.Stop();

        string _methodName = methodName != null ? methodName : string.IsNullOrEmpty(process.Method.Name) ? "N/A" : process.Method.Name;

        long _timeValue = 0;
        string _unitName = string.Empty;
        switch (timeunit)
        {
            case TimeUnit.Milliseconds:
                _timeValue = _stopwatch.ElapsedMilliseconds;
                _unitName = "ms";
                break;

            case TimeUnit.Ticks:
                _timeValue = _stopwatch.ElapsedTicks;
                _unitName = "ticks";
                break;
        }

        UnityEngine.Debug.Log(string.Format(DefaultFormat, _methodName, _timeValue, _unitName));
    }
}