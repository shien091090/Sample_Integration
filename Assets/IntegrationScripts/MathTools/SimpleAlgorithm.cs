using System.Collections;
using System.Collections.Generic;
using System;

namespace SNShien.Common.MathTools
{
    public static class SimpleAlgorithm
    {
        public static int GetAverageAfterProcess(int deno, Func<int> sumProcess)
        {
            if (sumProcess == null || deno == 0)
                return 0;

            int _sum = sumProcess.Invoke();
            int _result = UnityEngine.Mathf.Clamp(_sum / deno, int.MinValue, int.MaxValue);

            return _result;
        }

    }
}


