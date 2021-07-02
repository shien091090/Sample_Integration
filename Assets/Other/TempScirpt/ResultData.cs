using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCGLobby
{
	public class ResultData
	{
		public NotificationModel.BroadcastType broadcastType; //廣播類型
		public string messageFormat; //文字格式
		public List<int> formatParams; //輸出參數
		public List<int> pushsParams; //大獎彈窗顯示內容
		public List<int> blockParams; //屏蔽條件
	}
}
