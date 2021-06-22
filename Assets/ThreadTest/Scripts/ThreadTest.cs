using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using SNShien.Common.DataTools;

public class ThreadTest : MonoBehaviour
{
    [System.Serializable]
    public class ThreadInfo
    {
        public string threadName;
        public int threadID;

        public ThreadInfo(string name, int id)
        {
            threadName = name;
            threadID = id;
        }
    }

    public List<ThreadInfo> threadInfoList;
    public int threadCount;

    void Start()
    {
        threadInfoList = new List<ThreadInfo>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            threadInfoList.Clear();
        }
    }

    public void BTN_Test()
    {
        StartCoroutine(Cor_ClearList());

        for (int i = 0; i < threadCount; i++)
        {
            int _dice = Random.Range(100, 5000);

            Thread _thread = new Thread(AddNumberIntoList);
            _thread.Start(_dice);
        }
    }

    private IEnumerator Cor_ClearList()
    {
        for (int i = 0; i < 3; i++)
        {
            threadInfoList = new List<ThreadInfo>();

            yield return new WaitForSeconds(0.8f);
        }
    }

    private void AddNumberIntoList(object sleepTime)
    {
        string _name = Thread.CurrentThread.ThreadState.ToString();
        int _id = Thread.CurrentThread.ManagedThreadId;
        ThreadInfo _info = new ThreadInfo(_name, _id);

        Thread.Sleep((int)sleepTime);

        threadInfoList.Add(_info);
    }
}
