namespace SCGLobby
{
	public partial class NotificationModel : LobbyModel<NotificationModel>
	{

		public enum MessageContentType
		{
			CustomParam_Number, //自定義參數(計數器)
			CustomParam_Tag, //自定義參數(文字標籤)
			NickName, //玩家暱稱
			HallName, //廳館名稱
			ItemGameName, //卡片遊戲名稱
			ItemHallName, //卡片廳館名稱
			ItemStarCount, //卡片星數
			ItemCardType, //卡片種類
			GoldAmount, //贏分數量
			GemAmount, //寶石數量
			ItemAmount, //道具數量
            RewardName, //獎項名稱
            CurrentGameName, //當前遊戲名稱
            RealizeItemName //兌獎物品名稱
        }

		public enum PushsContentType
		{
			GameBanner, //遊戲圖示
            CaptionText, //標題文字
            MemberIcon, //玩家頭像
			MemberName, //玩家暱稱
			WinType, //大獎項目
			JackpotWinType, //JP獎項
			CurrentHallName, //當前廳館名稱
			ItemIcon, //卡片圖示
			ItemCardType, //卡片種類
			ItemStarCount, //卡片星數
			ItemHallName, //卡片廳館名稱
			GoldAmount, //贏分數量
			GemAmount //寶石數量
		}

		public enum BlockConditionType
		{
			InLobby, //大廳
			InGacha, //扭蛋
			InEventWindow, //活動介面
			InScratchOffGame, //刮刮樂
			Slot_Experience, //老虎機_體驗廳
			Slot_Normal, //老虎機_一般廳
			Slot_Master, //老虎機_高手廳
			Slot_GambleGod, //老虎機_至尊廳
			Baccarat_Normal, //尊榮百家_一般廳
			Baccarat_Master, //尊榮百家_高手廳
			Baccarat_GambleGod, //尊榮百家_至尊廳
			LiveBaccarat_Normal, //真人百家_一般廳
			LiveBaccarat_Master, //真人百家_高手廳
			LiveBaccarat_GambleGod, //真人百家_至尊廳
			Sicbo_Normal, //骰寶爆爆樂_一般廳
			Sicbo_Master, //骰寶爆爆樂_高手廳
			Sicbo_GambleGod, //骰寶爆爆樂_至尊廳
			Roulette_Normal, //百倍輪盤_一般廳
			Roulette_Master, //百倍輪盤_高手廳
			Roulette_GambleGod, //百倍輪盤_至尊廳
			Blackjack_Normal, //尊爵21_一般廳
			Blackjack_Master, //尊爵21_高手廳
			Blackjack_GambleGod, //尊爵21_至尊廳
			Mahjong_Experience, //咪吉麻將_體驗廳
			Mahjong_Normal, //咪吉麻將_一般廳
			Mahjong_Master, //咪吉麻將_高手廳
			NiuNiu_Normal, //搶莊牛牛_一般廳
			NiuNiu_Master, //搶莊牛牛_高手廳
			NiuNiu_GambleGod, //搶莊牛牛_至尊廳
			Fish_Experience, //魚機_體驗廳
			Fish_Normal, //魚機_一般廳
			Fish_Master, //魚機_高手廳
			AllGameHallSelectionPage //所有遊戲選廳選桌
		}

		public enum BroadcastType
		{
			Marquee, //跑馬燈
			ChatMsg, //聊天室公開頻
			Pushs //大獎推播彈窗
		}

		public enum ConditionType
		{
			GameType, //遊戲
			HallType, //廳館
			WinType, //贏分
			Item, //道具
			Chip, //碎片
			Gem, //寶石
			Other //其他
		}
	}
}
