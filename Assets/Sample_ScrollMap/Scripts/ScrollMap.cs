using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollMap : ScrollRect
{
    [SerializeField] private RectTransform rect_fullMap;
    private Dictionary<int, float> dict_lockPos;

    protected override void OnEnable()
    {
        if (Application.isPlaying)
        {
            this.onValueChanged.AddListener(AutoLock);
        }

    }

    protected override void OnDisable()
    {
        if (Application.isPlaying)
            this.onValueChanged.RemoveAllListeners();
    }

    public void SetLockPos(params RectTransform[] lockObjects)
    {
        if (rect_fullMap == null || lockObjects == null || lockObjects.Length <= 0)
            return;

        RectTransform rect_scrollArea = this.GetComponent<RectTransform>();
        List<float> _lockPosList = new List<float>();
        for (int i = 0; i < lockObjects.Length; i++)
        {
            RectTransform _rect = lockObjects[i];

            float _scrollAreaLength = rect_fullMap.sizeDelta.y - _rect.sizeDelta.y;
            float _lockObjectPos = _rect.localPosition.y - rect_scrollArea.sizeDelta.y;
            float _lockPos = _lockObjectPos / _scrollAreaLength;
            _lockPosList.Add(Mathf.Clamp(_lockPos, 0, 1));
        }

        _lockPosList.Sort();

        dict_lockPos = new Dictionary<int, float>();
        for (int i = 0; i < _lockPosList.Count; i++)
        {
            dict_lockPos.Add(i, _lockPosList[i]);
        }
    }

    public void Unlock(int lockNum)
    {
        if (dict_lockPos != null && dict_lockPos.ContainsKey(lockNum))
            dict_lockPos.Remove(lockNum);
    }

    private void AutoLock(Vector2 pos)
    {
        float[] lockPosArray = dict_lockPos.Values.ToArray();

        for (int i = 0; i < lockPosArray.Length; i++)
        {
            float _lockPos = lockPosArray[i];

            if (verticalNormalizedPosition >= _lockPos)
            {
                verticalNormalizedPosition = _lockPos;
                break;
            }
        }
    }
}
