using UnityEngine;
using UnityEngine.UI;

public class PositionDisplayer : MonoBehaviour
{
    [SerializeField] private RectTransform pivotObjPos;
    [SerializeField] private RectTransform parentObj;

    private Text _txt;
    private Text Txt
    {
        get
        {
            if (_txt == null)
                _txt = this.gameObject.GetComponentInChildren<Text>();

            return _txt;
        }
    }

    private RectTransform _ownRect;
    private RectTransform OwnRect
    {
        get
        {
            if (_ownRect == null)
                _ownRect = this.gameObject.GetComponent<RectTransform>();

            return _ownRect;
        }
    }

    public bool displaying = true;

    void Update()
    {
        if (displaying)
        {
            Txt.text = string.Format("({0}, {1})", OwnRect.localPosition.x.ToString("0.0"), OwnRect.localPosition.y.ToString("0.0"));
            SetPivotObjPos();
        }

    }

    private void SetPivotObjPos()
    {
        Vector3 _newPos = OwnRect.localPosition;

        if (parentObj != null)
            _newPos += parentObj.localPosition;

        pivotObjPos.localPosition = _newPos;
    }

}
