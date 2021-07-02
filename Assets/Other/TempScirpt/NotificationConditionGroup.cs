using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCGLobby
{
	[System.Serializable]
	public class Cond_GameType : ConditionData
	{
		public string gameType; //遊戲類型
		public string scgGameId; //包含遊戲ID
	}

	[System.Serializable]
	public class Cond_HallType : ConditionData
	{
		public int hallId; //廳別
	}

	[System.Serializable]
	public class Cond_WinType : ConditionData
	{
        public int ranking; //名次
        public string rewardName; //獎項名稱
        public int winValue; //贏分值
    }

	[System.Serializable]
	public class Cond_Item : ConditionData
	{
		public int itemCardType; //卡片種類
		public int itemHallType; //卡片廳館
	}

	[System.Serializable]
	public class Cond_Gem : ConditionData
	{
		public int gemAmount; //獲得寶石
	}

	[System.Serializable]
	public class Cond_Other : ConditionData
	{
		public int number; //計數器
		public string tag; //文字標籤
	}
}
