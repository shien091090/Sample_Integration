using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StationUnit : MonoBehaviour
{
    public enum RewardPanelType
    {
        None,
        Gold,
        Gem
    }

    [SerializeField] private Text txt_stationId;
    [SerializeField] private Text txt_goldAmount;
    [SerializeField] private Text txt_gemAmount;
    [SerializeField] private GameObject go_rewardPanel;
    [SerializeField] private Image[] group_discolorImg;

    public Color unlockColor;
    private List<Color> originColors;

    public int StationID { private set; get; }

    public void Init(StationInfo info)
    {
        StationID = info.stationId;
        txt_stationId.text = StationID.ToString("00");

        if (info.rewardContent.goldAmount > 0)
            SetRewardPanel(RewardPanelType.Gold, info.rewardContent.goldAmount);
        else if (info.rewardContent.gemAmount > 0)
            SetRewardPanel(RewardPanelType.Gem, info.rewardContent.gemAmount);
        else
            SetRewardPanel(RewardPanelType.None);

        Discolor(info.isStationUnlocked);
    }

    private void SetRewardPanel(RewardPanelType panelType, int amount = 0)
    {
        go_rewardPanel.SetActive(panelType != RewardPanelType.None);

        txt_goldAmount.GetComponent<Transform>().parent.gameObject.SetActive(panelType == RewardPanelType.Gold);
        txt_gemAmount.GetComponent<Transform>().parent.gameObject.SetActive(panelType == RewardPanelType.Gem);

        switch (panelType)
        {
            case RewardPanelType.Gold:
                txt_goldAmount.text = amount.ToString();
                break;

            case RewardPanelType.Gem:
                txt_gemAmount.text = amount.ToString();
                break;
        }
    }

    public void Discolor(bool isUnlockColor = true)
    {
        if (group_discolorImg == null)
            return;

        if (isUnlockColor)
        {
            for (int i = 0; i < group_discolorImg.Length; i++)
            {
                Image _img = group_discolorImg[i];

                if (originColors == null)
                    originColors = new List<Color>();

                if (originColors.Count < i + 1)
                    originColors.Add(_img.color);

                _img.color = unlockColor;
            }
        }
        else
        {
            if (originColors == null)
                return;

            for (int i = 0; i < group_discolorImg.Length; i++)
            {
                Image _img = group_discolorImg[i];
                _img.color = originColors[i];
            }
        }
    }
}
