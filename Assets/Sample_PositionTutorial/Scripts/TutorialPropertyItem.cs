using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPropertyItem : MonoBehaviour
{
    public float sldMinValue;
    public float sldMaxValue;

    [SerializeField] private Text txt_caption;
    [SerializeField] private Text txt_value;
    private Slider _sld;
    private Slider ValueSlider
    {
        get
        {
            if (_sld == null)
                _sld = this.gameObject.GetComponentInChildren<Slider>();

            return _sld;
        }
    }

    void Start()
    {
        Init();
    }

    private void Init()
    {
        AutoCaption();
        SetSliderSetting();
    }

    [ContextMenu("AutoCaption")]
    private void AutoCaption()
    {
        string _goName = this.gameObject.name;
        txt_caption.text = ConvertGoNameToCaption(_goName);
    }

    private string ConvertGoNameToCaption(string goName)
    {
        string _result = goName;
        _result = _result.Replace("Item_", string.Empty);

        return _result;
    }

    private void SetSliderSetting()
    {
        ValueSlider.onValueChanged.AddListener(AutoShowValue);
        ValueSlider.minValue = sldMinValue;
        ValueSlider.maxValue = sldMaxValue;
        ValueSlider.value = ( sldMinValue + sldMaxValue ) / 2;
    }

    private void AutoShowValue(float currentValue)
    {
        txt_value.text = ValueSlider.value.ToString("0.0");
    }
}
