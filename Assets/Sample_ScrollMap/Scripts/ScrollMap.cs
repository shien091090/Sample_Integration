using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ScrollMap : ScrollRect
{
    [SerializeField] private RectTransform rect_fullMap;
    [SerializeField] private ScrollRect backgroundSrollMap;
    private Dictionary<int, float> dict_lockPos;
    public RectTransform followingTarget;
    public bool isAutoFollow;

    public bool CanDrag
    {
        set
        {
            vertical = value;
        }
    }

    protected override void OnEnable()
    {
        if (Application.isPlaying)
        {
            this.onValueChanged.AddListener(PulleyBackgrounMapAndAutoLock);
        }

    }

    protected override void OnDisable()
    {
        if (Application.isPlaying)
            this.onValueChanged.RemoveAllListeners();
    }

    void Update()
    {
        if (isAutoFollow)
            FollowTarget();
    }

    private void FollowTarget()
    {
        if (followingTarget == null)
            StopAutoFollow();

        float _targetPosY = followingTarget.localPosition.y;
        FocusPos(_targetPosY);
    }

    public void SetLockPos(params float[] lockerPosYArr)
    {
        if (rect_fullMap == null || lockerPosYArr == null || lockerPosYArr.Length <= 0)
            return;

        RectTransform rect_scrollArea = this.GetComponent<RectTransform>();
        List<float> _lockPosList = new List<float>();
        for (int i = 0; i < lockerPosYArr.Length; i++)
        {
            float _scrollAreaLength = rect_fullMap.sizeDelta.y - rect_scrollArea.sizeDelta.y;
            float _lockObjectPos = lockerPosYArr[i] - rect_scrollArea.sizeDelta.y;
            float _lockPos = _lockObjectPos / _scrollAreaLength;
            _lockPosList.Add(Mathf.Clamp(_lockPos, 0, 1));
        }

        dict_lockPos = _lockPosList
            .Select((pos, index) => new { pos, index })
            .ToDictionary(x => x.index, x => x.pos);
    }

    public void UnlockScrollTenon(int lockNum)
    {
        if (dict_lockPos != null && dict_lockPos.ContainsKey(lockNum))
            dict_lockPos.Remove(lockNum);
    }

    public void FocusPos(float posY)
    {
        RectTransform rect_scrollArea = this.GetComponent<RectTransform>();
        float _scrollAreaLength = rect_fullMap.sizeDelta.y - rect_scrollArea.sizeDelta.y;
        float _targetPos = posY - (rect_scrollArea.sizeDelta.y / 2);
        float _resultValue = Mathf.Clamp(_targetPos / _scrollAreaLength, 0, 1);

        verticalNormalizedPosition = _resultValue;
    }

    public void SetAutoFollow(RectTransform target)
    {
        if (target == null)
            return;

        followingTarget = target;
        isAutoFollow = true;
    }

    public void StopAutoFollow()
    {
        if (followingTarget != null)
            followingTarget = null;

        isAutoFollow = false;
    }

    private void PulleyBackgrounMapAndAutoLock(Vector2 pos)
    {
        backgroundSrollMap.normalizedPosition = pos;

        if (dict_lockPos == null)
            return;

        float[] lockPosArray = dict_lockPos.Values.ToArray();
        for (int i = 0; i < lockPosArray.Length; i++)
        {
            float _lockPos = lockPosArray[i];

            if (verticalNormalizedPosition >= _lockPos)
            {
                verticalNormalizedPosition = _lockPos;
                backgroundSrollMap.verticalNormalizedPosition = _lockPos;
                break;
            }
        }
    }
}
