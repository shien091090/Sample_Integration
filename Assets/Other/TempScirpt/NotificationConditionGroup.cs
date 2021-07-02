using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCGLobby
{
	[System.Serializable]
	public class Cond_GameType : ConditionData
	{
		public string gameType; //�C������
		public string scgGameId; //�]�t�C��ID
	}

	[System.Serializable]
	public class Cond_HallType : ConditionData
	{
		public int hallId; //�U�O
	}

	[System.Serializable]
	public class Cond_WinType : ConditionData
	{
        public int ranking; //�W��
        public string rewardName; //�����W��
        public int winValue; //Ĺ����
    }

	[System.Serializable]
	public class Cond_Item : ConditionData
	{
		public int itemCardType; //�d������
		public int itemHallType; //�d���U�]
	}

	[System.Serializable]
	public class Cond_Gem : ConditionData
	{
		public int gemAmount; //��o�_��
	}

	[System.Serializable]
	public class Cond_Other : ConditionData
	{
		public int number; //�p�ƾ�
		public string tag; //��r����
	}
}
