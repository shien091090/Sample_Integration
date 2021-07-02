using System.Collections;
using System.Collections.Generic;
using SCGLobby.DataStruct;

namespace SCGLobby
{
    public class BroadcastInfo
    {
        public BroadcastChatInfo broadcastChatInfo;
        public BroadcastMarqueeInfo broadcastMarqueeInfo;
        public BroadcastPushsInfo broadcastPushsInfo;
    }

    public class BroadcastPushsInfo
    {
        public class PushsUnit : BroadcastMacro
        {
            public List<int> pushsContentInfo;
            public List<int> blockCondition;
            public NotificationModel.BroadcastRawData pushsData;
        }

        public List<PushsUnit> broadcastInfo;

        public BroadcastPushsInfo()
        {
            broadcastInfo = new List<PushsUnit>();
        }
    }

    public class BroadcastMarqueeInfo
    {
        public class MarqueeUnit
        {
            public string marqueeMessage;
            public List<int> blockCondition;
        }

        public NotificationModel.BroadcastRawData pushsData;
        public List<MarqueeUnit> broadcastInfo;

        public BroadcastMarqueeInfo()
        {
            broadcastInfo = new List<MarqueeUnit>();
        }
    }

    public class BroadcastChatInfo
    {
        public class ChatUnit
        {
            public string chatMessage;
            public List<int> blockCondition;
        }

        public NotificationModel.BroadcastRawData pushsData;
        public List<ChatUnit> broadcastInfo;

        public BroadcastChatInfo()
        {
            broadcastInfo = new List<ChatUnit>();
        }
    }
}