using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCGLobby
{
	public class ResultData
	{
		public NotificationModel.BroadcastType broadcastType; //�s������
		public string messageFormat; //��r�榡
		public List<int> formatParams; //��X�Ѽ�
		public List<int> pushsParams; //�j���u����ܤ��e
		public List<int> blockParams; //�̽�����
	}
}
