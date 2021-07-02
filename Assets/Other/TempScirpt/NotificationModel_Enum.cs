namespace SCGLobby
{
	public partial class NotificationModel : LobbyModel<NotificationModel>
	{

		public enum MessageContentType
		{
			CustomParam_Number, //�۩w�q�Ѽ�(�p�ƾ�)
			CustomParam_Tag, //�۩w�q�Ѽ�(��r����)
			NickName, //���a�ʺ�
			HallName, //�U�]�W��
			ItemGameName, //�d���C���W��
			ItemHallName, //�d���U�]�W��
			ItemStarCount, //�d���P��
			ItemCardType, //�d������
			GoldAmount, //Ĺ���ƶq
			GemAmount, //�_�ۼƶq
			ItemAmount, //�D��ƶq
            RewardName, //�����W��
            CurrentGameName, //��e�C���W��
            RealizeItemName //�I�����~�W��
        }

		public enum PushsContentType
		{
			GameBanner, //�C���ϥ�
            CaptionText, //���D��r
            MemberIcon, //���a�Y��
			MemberName, //���a�ʺ�
			WinType, //�j������
			JackpotWinType, //JP����
			CurrentHallName, //��e�U�]�W��
			ItemIcon, //�d���ϥ�
			ItemCardType, //�d������
			ItemStarCount, //�d���P��
			ItemHallName, //�d���U�]�W��
			GoldAmount, //Ĺ���ƶq
			GemAmount //�_�ۼƶq
		}

		public enum BlockConditionType
		{
			InLobby, //�j�U
			InGacha, //��J
			InEventWindow, //���ʤ���
			InScratchOffGame, //����
			Slot_Experience, //�Ѫ��_�����U
			Slot_Normal, //�Ѫ��_�@���U
			Slot_Master, //�Ѫ��_�����U
			Slot_GambleGod, //�Ѫ��_�ܴL�U
			Baccarat_Normal, //�L�a�ʮa_�@���U
			Baccarat_Master, //�L�a�ʮa_�����U
			Baccarat_GambleGod, //�L�a�ʮa_�ܴL�U
			LiveBaccarat_Normal, //�u�H�ʮa_�@���U
			LiveBaccarat_Master, //�u�H�ʮa_�����U
			LiveBaccarat_GambleGod, //�u�H�ʮa_�ܴL�U
			Sicbo_Normal, //���_�z�z��_�@���U
			Sicbo_Master, //���_�z�z��_�����U
			Sicbo_GambleGod, //���_�z�z��_�ܴL�U
			Roulette_Normal, //�ʭ����L_�@���U
			Roulette_Master, //�ʭ����L_�����U
			Roulette_GambleGod, //�ʭ����L_�ܴL�U
			Blackjack_Normal, //�L��21_�@���U
			Blackjack_Master, //�L��21_�����U
			Blackjack_GambleGod, //�L��21_�ܴL�U
			Mahjong_Experience, //�}�N�±N_�����U
			Mahjong_Normal, //�}�N�±N_�@���U
			Mahjong_Master, //�}�N�±N_�����U
			NiuNiu_Normal, //�m������_�@���U
			NiuNiu_Master, //�m������_�����U
			NiuNiu_GambleGod, //�m������_�ܴL�U
			Fish_Experience, //����_�����U
			Fish_Normal, //����_�@���U
			Fish_Master, //����_�����U
			AllGameHallSelectionPage //�Ҧ��C�����U���
		}

		public enum BroadcastType
		{
			Marquee, //�]���O
			ChatMsg, //��ѫǤ��}�W
			Pushs //�j�������u��
		}

		public enum ConditionType
		{
			GameType, //�C��
			HallType, //�U�]
			WinType, //Ĺ��
			Item, //�D��
			Chip, //�H��
			Gem, //�_��
			Other //��L
		}
	}
}
