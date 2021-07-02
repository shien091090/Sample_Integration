using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using SCGCommon;
using SCGLobby.Common.ScenesManager;

namespace SCGLobby
{
    public partial class NotificationModel : LobbyModel<NotificationModel>
    {
        [System.Serializable]
        public class BroadcastRawData
        {
            //基本
            public string nickName; //玩家名稱
            public string memberId; //玩家ID

            //通用
            public string gameType; //遊戲名稱
            public int itemId; //道具ID
            public int hallId; //廳館ID
            public string scgGameId; //SCG遊戲ID
            public string realizeItemName; //兌獎物品名稱
            public int number; //計數器
            public string tagString; //標籤文字

            //跑馬燈&公開頻
            public string rewardName; //獎勵項目名稱
            public int itemAmount; //道具數量

            //大獎推播
            public string realizeItemId; //兌獎物品ID
            public string captionText; //標題文字
            public int goldAmount; //贏分數量
            public int gemAmount; //寶石數量
            public ScrollingData.WinScrollType winType; //大獎項目
            public JackpotModel.SlotJackpotLevel jackpotType; //JP獎項

            //public Cond_Other otherData;

            //public BroadcastRawData()
            //{
            //    otherData = new Cond_Other();
            //}
        }

        public class DebugMessage
        {
            public string titleColorHex;

            public DebugMessage(Color titleColor)
            {
                titleColorHex = string.Format("#{0}", ColorUtility.ToHtmlStringRGB(titleColor));
            }
        }

        public class ConditionTicket
        {
            public List<ConditionData> ConditionDatas { private set; get; }
            public BroadcastRawData rawDataPack;

            public ConditionTicket()
            {
                ConditionDatas = new List<ConditionData>();
                rawDataPack = new BroadcastRawData();
            }
        }

        //--------------------------------------------------------------------------------------------

        private const string DIRECTORY_FILE_NAME = "notification_condition.txt";
        public const string CLASS_NAMESPACE = "SCGLobby.";

        private NotificationDirectory notificationDirectory;
        private ConditionTicket conditionTicket;

        public static bool IsDirectoryValid = false;

        public delegate bool del_tryParse<T>(string str, out T data);

        //--------------------------------------------------------------------------------------------

        public void DirectoryInit()
        {
            notificationDirectory = new NotificationDirectory(ClientSettingManager.DATA_PATH + DIRECTORY_FILE_NAME);
            IsDirectoryValid = true;
        }

        public NotificationModel SetCondition(ConditionData _cond)
        {
            if (conditionTicket == null)
                conditionTicket = new ConditionTicket();

            LinkRawData(conditionTicket.rawDataPack, _cond);

            conditionTicket.ConditionDatas.Add(_cond);

            return this;
        }

        private void LinkRawData(BroadcastRawData rawDataPack, ConditionData data)
        {
            if (IsNullOrDefault(rawDataPack.nickName))
            {
                DataStruct.MemberInfo memberInfo = UserDataManager.profile.memberInfo;
                rawDataPack.nickName = memberInfo.nickName;
            }

            if (IsNullOrDefault(rawDataPack.memberId))
            {
                rawDataPack.memberId = InitModel.GetInstance().GetMemberId();
            }

            //寶石
            if (data.GetType() == typeof(Cond_Gem))
            {
                Cond_Gem _gemCond = (Cond_Gem)data;
                LinkProperty(ref rawDataPack.gemAmount, ref _gemCond.gemAmount);
            }
            //贏分
            else if (data.GetType() == typeof(Cond_WinType))
            {
                Cond_WinType _winTypeCond = (Cond_WinType)data;
                LinkProperty(ref rawDataPack.goldAmount, ref _winTypeCond.winValue);
                LinkProperty(ref rawDataPack.rewardName, ref _winTypeCond.rewardName);
            }
            //廳館
            else if (data.GetType() == typeof(Cond_HallType))
            {
                Cond_HallType _hallTypeCond = (Cond_HallType)data;
                LinkProperty(ref rawDataPack.hallId, ref _hallTypeCond.hallId);
            }
            //遊戲類型
            else if (data.GetType() == typeof(Cond_GameType))
            {
                Cond_GameType _gameTypeCond = (Cond_GameType)data;
                LinkProperty(ref rawDataPack.gameType, ref _gameTypeCond.gameType);
                LinkProperty(ref rawDataPack.scgGameId, ref _gameTypeCond.scgGameId);
            }
            //道具
            else if (data.GetType() == typeof(Cond_Item))
            {
                Cond_Item _itemCond = (Cond_Item)data;

                if (IsNullOrDefault(rawDataPack.itemAmount))
                    rawDataPack.itemAmount = 1;

                if (!IsNullOrDefault(rawDataPack.itemId))
                {
                    ItemInfo _item = BaseDataManager.itemCardRecords.GetItem(rawDataPack.itemId);

                    if (_item != null)
                    {
                        if (IsNullOrDefault(_itemCond.itemCardType))
                            _itemCond.itemCardType = _item.itemEffect.itemType;

                        if (IsNullOrDefault(_itemCond.itemHallType))
                            _itemCond.itemHallType = _item.hallId;
                    }
                }
            }
            //其他
            else if (data.GetType() == typeof(Cond_Other))
            {
                Cond_Other _otherCond = (Cond_Other)data;
                LinkProperty(ref rawDataPack.number, ref _otherCond.number);
                LinkProperty(ref rawDataPack.tagString, ref _otherCond.tag);
            }

        }

        private void LinkProperty<T>(ref T firstProp, ref T subProp)
        {
            bool _firstPorpIsValid = !IsNullOrDefault(firstProp);
            bool _subPropIsValid = !IsNullOrDefault(subProp);

            if (!_firstPorpIsValid && _subPropIsValid)
                firstProp = subProp;

            else if (!_subPropIsValid && _firstPorpIsValid)
                subProp = firstProp;
        }

        public void StartCompare(Action<BroadcastRawData> metaSettingAction, DebugMessage debugInfo = null)
        {
            if (conditionTicket == null)
                return;

            if (notificationDirectory == null)
                return;

            List<ConditionData> _conditionDatas = conditionTicket.ConditionDatas;
            BroadcastRawData _rawData = conditionTicket.rawDataPack;

            conditionTicket = new ConditionTicket();

            metaSettingAction.Invoke(_rawData);
            for (int i = 0; i < _conditionDatas.Count; i++)
            {
                LinkRawData(_rawData, _conditionDatas[i]);
            }

            List<ResultData> _resultList = notificationDirectory.ConditionCompare(_conditionDatas);

#if UNITY_EDITOR

            if (debugInfo != null) //Debug.Message
            {
                string _colorHex = string.IsNullOrEmpty(debugInfo.titleColorHex) ? ColorUtility.ToHtmlStringRGB(Color.white) : debugInfo.titleColorHex;
                string _conditionMessage = string.Format("<color={0}>[開始Notification條件比對]</color>", _colorHex);

                for (int i = 0; i < _conditionDatas.Count; i++)
                {
                    _conditionMessage += "\n";

                    Type _type = _conditionDatas[i].GetType();
                    string _className = _type.Name;
                    FieldInfo[] _fieldInfos = _type.GetFields();

                    _conditionMessage += string.Format("[條件 {1}] [{2}] ", _colorHex, i, _className);

                    for (int j = 0; j < _fieldInfos.Length; j++)
                    {
                        string _fieldName = _fieldInfos[j].Name;
                        object _fieldValue = _fieldInfos[j].GetValue(_conditionDatas[i]);
                        string _valueContent = _fieldValue == null ? "N/A" : _fieldValue.ToString();
                        _conditionMessage += string.Format("{0} : {1}", _fieldName, _valueContent);

                        if (j < _fieldInfos.Length - 1)
                            _conditionMessage += ", ";
                    }
                }

                Debug.Log(_conditionMessage);

                int _resultCount = (_resultList == null) ? 0 : _resultList.Count;
                string _resultMessage = string.Format("<color={0}>[條件篩選結果]</color> 符合條件的項目 = {1}", _colorHex, _resultCount);

                if (_resultList != null)
                {
                    int _marqueeCount = 0;
                    int _chatCount = 0;
                    int _pushCount = 0;

                    for (int i = 0; i < _resultList.Count; i++)
                    {
                        switch (_resultList[i].broadcastType)
                        {
                            case BroadcastType.Marquee:
                                _marqueeCount++;
                                break;

                            case BroadcastType.ChatMsg:
                                _chatCount++;
                                break;

                            case BroadcastType.Pushs:
                                _pushCount++;
                                break;
                        }
                    }

                    _resultMessage += string.Format("\n 跑馬燈 : {0}, 聊天室公開頻 : {1}, 大獎推播彈窗 : {2}", _marqueeCount, _chatCount, _pushCount);
                }

                Debug.Log(_resultMessage);
            }

#endif

            if (_resultList == null || _resultList.Count <= 0)
                return;

            BroadcastInfo _broadcastInfo = CreateBroadcastInfo(_resultList, _rawData);
            SendBroadcastInfo(_broadcastInfo);

        }

        private BroadcastInfo CreateBroadcastInfo(List<ResultData> resultList, BroadcastRawData rawData)
        {
            if (resultList == null || resultList.Count <= 0)
                return null;

            BroadcastInfo _broadcastInfo = new BroadcastInfo();

            for (int i = 0; i < resultList.Count; i++)
            {
                ResultData _data = resultList[i];

                switch (_data.broadcastType)
                {
                    case BroadcastType.Marquee:
                        if (_broadcastInfo.broadcastMarqueeInfo == null)
                            _broadcastInfo.broadcastMarqueeInfo = new BroadcastMarqueeInfo();

                        BroadcastMarqueeInfo.MarqueeUnit _marqueeUnit = new BroadcastMarqueeInfo.MarqueeUnit
                        {
                            marqueeMessage = AssembleBroadcastMessage(_data.messageFormat, _data.formatParams, rawData),
                            blockCondition = _data.blockParams
                        };

                        _broadcastInfo.broadcastMarqueeInfo.broadcastInfo.Add(_marqueeUnit);

                        break;

                    case BroadcastType.ChatMsg:
                        if (_broadcastInfo.broadcastChatInfo == null)
                            _broadcastInfo.broadcastChatInfo = new BroadcastChatInfo();

                        BroadcastChatInfo.ChatUnit _chatUnit = new BroadcastChatInfo.ChatUnit
                        {
                            chatMessage = AssembleBroadcastMessage(_data.messageFormat, _data.formatParams, rawData),
                            blockCondition = _data.blockParams
                        };

                        _broadcastInfo.broadcastChatInfo.broadcastInfo.Add(_chatUnit);

                        break;

                    case BroadcastType.Pushs:
                        if (_broadcastInfo.broadcastPushsInfo == null)
                            _broadcastInfo.broadcastPushsInfo = new BroadcastPushsInfo();

                        BroadcastPushsInfo.PushsUnit _pushsUnit = new BroadcastPushsInfo.PushsUnit
                        {
                            pushsContentInfo = _data.pushsParams,
                            blockCondition = _data.blockParams,
                            pushsData = rawData
                        };

                        _broadcastInfo.broadcastPushsInfo.broadcastInfo.Add(_pushsUnit);

                        continue;
                }
            }

            return _broadcastInfo;
        }

        private string AssembleBroadcastMessage(string format, List<int> paramTypeArr, BroadcastRawData rawData)
        {
            string _content = string.Empty;

            if (paramTypeArr == null || paramTypeArr.Count <= 0)
                return _content;

            if (string.IsNullOrEmpty(format))
                return _content;

            string pattern = @"{(.*?)}";
            MatchCollection matches = Regex.Matches(format, pattern);
            if (matches.Count != paramTypeArr.Count)
                return _content;

            string[] _body = new string[paramTypeArr.Count];

            for (int i = 0; i < _body.Length; i++)
            {
                _body[i] = GetMessageElement((MessageContentType)paramTypeArr[i], rawData);
            }

            _content = string.Format(format, _body);

            return _content;
        }

        private string GetMessageElement(MessageContentType paramType, BroadcastRawData rawData)
        {
            string _message = string.Empty;
            ItemInfo _item = null;

            switch (paramType)
            {
                case MessageContentType.NickName:
                    _message = VerifyRawData("{玩家名稱}", () =>
                    {
                        return rawData.nickName;

                    }, rawData.nickName);
                    return _message;

                case MessageContentType.HallName:
                    _message = VerifyRawData("{廳館名稱}", () =>
                    {
                        string _hallName = BaseDataManager.hallDataRecords.GetHallName(HallDataRecords.GameHallType.All, rawData.hallId);
                        return _hallName;

                    }, rawData.hallId);
                    return _message;

                case MessageContentType.ItemGameName:
                    _message = VerifyRawData("{卡片遊戲名稱}", () =>
                    {
                        _item = BaseDataManager.itemCardRecords.GetItem(rawData.itemId);
                        string _itemGameName = (_item.itemEffect.itemType == (int)Card_AllInOne.CardItemType.Multiplier) ?
                        string.Empty :
                        DataTableManager.Instance.GetText(_item.itemEffect.scgGameId[0], true);

                        return _itemGameName;

                    }, rawData.itemId);
                    return _message;

                case MessageContentType.ItemHallName:
                    _message = VerifyRawData("{卡片廳別}", () =>
                    {
                        _item = BaseDataManager.itemCardRecords.GetItem(rawData.itemId);
                        string _itemHallName = BaseDataManager.hallDataRecords.GetHallName(HallDataRecords.GameHallType.All, _item.hallId);

                        return _itemHallName;

                    }, rawData.itemId);
                    return _message;

                case MessageContentType.ItemStarCount:
                    _message = VerifyRawData("{卡片星數}", () =>
                    {
                        _item = BaseDataManager.itemCardRecords.GetItem(rawData.itemId);
                        string _startCount = _item.starLevel + DataTableManager.GetInstance().GetText("COMMON_CARDSTAR_TEXT");

                        return _startCount;

                    }, rawData.itemId);
                    return _message;

                case MessageContentType.ItemCardType:
                    _message = VerifyRawData("{卡片種類}", () =>
                    {
                        _item = BaseDataManager.itemCardRecords.GetItem(rawData.itemId);

                        return _item.itemName;

                    }, rawData.itemId);
                    return _message;

                case MessageContentType.GoldAmount:
                    _message = VerifyRawData("{贏分數量}", () =>
                    {
                        return rawData.goldAmount.ToString("N0");

                    }, rawData.goldAmount);
                    return _message;

                case MessageContentType.GemAmount:
                    _message = VerifyRawData("{寶石數量}", () =>
                    {
                        return rawData.gemAmount.ToString("N0");

                    }, rawData.gemAmount);
                    return _message;

                case MessageContentType.ItemAmount:
                    _message = VerifyRawData("{道具數量}", () =>
                    {
                        return rawData.itemAmount.ToString("N0");

                    }, rawData.itemAmount);
                    return _message;

                case MessageContentType.RewardName:
                    _message = VerifyRawData("{獎項名稱}", () =>
                    {
                        return rawData.rewardName;

                    }, rawData.rewardName);
                    return _message;

                case MessageContentType.CurrentGameName:
                    string _gameId = IsNullOrDefault(rawData.scgGameId)? GameMenuModel.GetInstance().currentSCGGameID : rawData.scgGameId;
                    _message = VerifyRawData("{當前遊戲名稱}", () =>
                     {
                         return BaseDataManager.gameInfoRecords.GetGameInfoByGameId(_gameId).gameName;

                     }, _gameId);
                    return _message;

                case MessageContentType.RealizeItemName:
                    _message = VerifyRawData("{兌獎物品名稱}", () =>
                    {
                        return rawData.realizeItemName;

                    }, rawData.realizeItemName);
                    return _message;

                case MessageContentType.CustomParam_Number:
                    _message = VerifyRawData("{自定義參數(計數器)}", () =>
                    {
                        return rawData.number.ToString();

                    }, rawData.number);
                    return _message;

                case MessageContentType.CustomParam_Tag:
                    _message = VerifyRawData("{自定義參數(文字標籤)}", () =>
                    {
                        return rawData.tagString.ToString();

                    }, rawData.tagString);
                    return _message;
            }

            return "{}";
        }

        private string VerifyRawData(string defaultBackMessage, Func<string> execution, params object[] datas)
        {
            if (datas != null && datas.Length > 0)
            {
                for (int i = 0; i < datas.Length; i++)
                {
                    if (IsNullOrDefault(datas[i]))
                        return defaultBackMessage;
                }
            }

            string _result = string.Empty;

            try
            {
                _result = execution.Invoke();
            }
            catch (Exception)
            {
                return defaultBackMessage;
            }

            return _result;
        }

        private void SendBroadcastInfo(BroadcastInfo info)
        {
            if (info == null)
                return;

            if (IsNullOrDefault(info, true))
                return;

            if (TutorialModel.Instance.isInTutorialGame())
                return;

            if (info != null)
                ConnectControlModel.Instance.Broadcast(info);
        }

        public static bool CheckBlockCondition(List<int> blockParams)
        {
            if (blockParams == null || blockParams.Count <= 0)
                return false;

            foreach (int param in blockParams)
            {
                List<string> _checkGameTypes = new List<string>();
                int _checkHallId = -1;
                bool _isInHallSelection = IsInHallSelection();

                //遊戲外判斷
                switch ((BlockConditionType)param)
                {
                    case BlockConditionType.InLobby:
                        if (ScenesManager.Instance.nowMainSceneName == "LobbyScene")
                            return true;
                        break;

                    case BlockConditionType.InGacha:
                        if (ViewsManager.Instance.IsViewOpen("GachaView"))
                            return true;
                        break;

                    case BlockConditionType.InEventWindow:
                        break;

                    case BlockConditionType.InScratchOffGame:
                        if (ViewsManager.Instance.IsViewOpen("ScratchCardView"))
                            return true;
                        break;

                    case BlockConditionType.AllGameHallSelectionPage:
                        if (_isInHallSelection)
                            return true;
                        break;
                }

                //遊戲內判斷
                if (!CommonModel.Instance.isInGame || _isInHallSelection)
                    continue;

                //遊戲類型
                switch ((BlockConditionType)param)
                {
                    case BlockConditionType.Slot_Experience:
                    case BlockConditionType.Slot_Normal:
                    case BlockConditionType.Slot_Master:
                    case BlockConditionType.Slot_GambleGod:
                        _checkGameTypes.Add(GameBridge.SLOT_TYPE);
                        break;
                    case BlockConditionType.Baccarat_Normal:
                    case BlockConditionType.Baccarat_Master:
                    case BlockConditionType.Baccarat_GambleGod:
                        _checkGameTypes.Add(GameBridge.BACCARAT_TYPE);
                        break;
                    case BlockConditionType.LiveBaccarat_Normal:
                    case BlockConditionType.LiveBaccarat_Master:
                    case BlockConditionType.LiveBaccarat_GambleGod:
                        _checkGameTypes.Add(GameBridge.LIVE_BACCARAT_TYPE);
                        break;
                    case BlockConditionType.Sicbo_Normal:
                    case BlockConditionType.Sicbo_Master:
                    case BlockConditionType.Sicbo_GambleGod:
                        _checkGameTypes.Add(GameBridge.SICBO_TYPE);
                        break;
                    case BlockConditionType.Roulette_Normal:
                    case BlockConditionType.Roulette_Master:
                    case BlockConditionType.Roulette_GambleGod:
                        _checkGameTypes.Add(GameBridge.ROULETTE_TYPE);
                        break;
                    case BlockConditionType.Blackjack_Normal:
                    case BlockConditionType.Blackjack_Master:
                    case BlockConditionType.Blackjack_GambleGod:
                        _checkGameTypes.Add(GameBridge.BLACKJACK_TYPE);
                        break;
                    case BlockConditionType.Mahjong_Experience:
                    case BlockConditionType.Mahjong_Normal:
                    case BlockConditionType.Mahjong_Master:
                        _checkGameTypes.Add(GameBridge.MAHJONG_TYPE);
                        break;
                    case BlockConditionType.NiuNiu_Normal:
                    case BlockConditionType.NiuNiu_Master:
                    case BlockConditionType.NiuNiu_GambleGod:
                        _checkGameTypes.Add(GameBridge.NIUNIU_TYPE);
                        break;
                    case BlockConditionType.Fish_Experience:
                    case BlockConditionType.Fish_Normal:
                    case BlockConditionType.Fish_Master:
                        _checkGameTypes.Add(GameBridge.FISH_TYPE);
                        _checkGameTypes.Add(GameBridge.YILEFISH_TYPE);
                        break;
                }

                //廳別
                switch ((BlockConditionType)param)
                {
                    case BlockConditionType.Slot_Experience:
                    case BlockConditionType.Mahjong_Experience:
                    case BlockConditionType.Fish_Experience:
                    case BlockConditionType.Baccarat_Normal:
                    case BlockConditionType.LiveBaccarat_Normal:
                    case BlockConditionType.Sicbo_Normal:
                    case BlockConditionType.Roulette_Normal:
                    case BlockConditionType.Blackjack_Normal:
                    case BlockConditionType.NiuNiu_Normal:
                        _checkHallId = 1;
                        break;
                    case BlockConditionType.Slot_Normal:
                    case BlockConditionType.Baccarat_Master:
                    case BlockConditionType.LiveBaccarat_Master:
                    case BlockConditionType.Sicbo_Master:
                    case BlockConditionType.Roulette_Master:
                    case BlockConditionType.Blackjack_Master:
                    case BlockConditionType.Mahjong_Normal:
                    case BlockConditionType.NiuNiu_Master:
                    case BlockConditionType.Fish_Normal:
                        _checkHallId = 2;
                        break;
                    case BlockConditionType.Slot_Master:
                    case BlockConditionType.Baccarat_GambleGod:
                    case BlockConditionType.LiveBaccarat_GambleGod:
                    case BlockConditionType.Sicbo_GambleGod:
                    case BlockConditionType.Roulette_GambleGod:
                    case BlockConditionType.Blackjack_GambleGod:
                    case BlockConditionType.Mahjong_Master:
                    case BlockConditionType.NiuNiu_GambleGod:
                    case BlockConditionType.Fish_Master:
                        _checkHallId = 3;
                        break;
                    case BlockConditionType.Slot_GambleGod:
                        _checkHallId = 4;
                        break;
                }

                if (IsInHallSelection())
                    continue;

                string _currentGameId = GameMenuModel.Instance.currentSCGGameID;
                GameInfo _currentGameInfo = BaseDataManager.gameInfoRecords.GetGameInfoByGameId(_currentGameId);

                if (_currentGameInfo == null)
                    continue;

                if (!_checkGameTypes.Contains(_currentGameInfo.gameTypeName))
                    continue;

                if (_checkHallId == HallModel.Instance.GetCurHallId())
                    return true;

            }

            return false;
        }

        public static bool IsLegacyBroadcast(JObject data)
        {
            BroadcastInfo info = data.ToObject<BroadcastInfo>();

            return IsNullOrDefault(info, true);
        }

        public static object ConvertData(string data, Type type, bool toList = false, string ignore = "")
        {
            object value = null;
            bool isIgnore = (!string.IsNullOrEmpty(ignore) && data == ignore);

            if (type.IsEnum && !toList)
            {
                if (string.IsNullOrEmpty(data))
                    value = Enum.Parse(type, "0");
                else
                {
                    try
                    {
                        value = Enum.Parse(type, data);
                    }
                    catch (Exception)
                    {
                        value = Enum.Parse(type, "0");
                    }
                }
            }

            else if (type == typeof(int))
                value = ConvertOuput<int>(toList, isIgnore, data, typeof(List<int>), TryParseData<int>(data, int.TryParse));

            else if (type == typeof(long))
                value = ConvertOuput<long>(toList, isIgnore, data, typeof(List<long>), TryParseData<long>(data, long.TryParse));

            else if (type == typeof(float))
                value = ConvertOuput<float>(toList, isIgnore, data, typeof(List<float>), TryParseData<float>(data, float.TryParse));

            else if (type == typeof(string))
                value = ConvertOuput<string>(toList, isIgnore, data, typeof(List<string>), data);

            else if (!isIgnore)
            {
                if (data.StartsWith("[") || data.StartsWith("{"))
                    value = JsonConvert.DeserializeObject(data, type);
            }

            return value;
        }

        private static object ConvertOuput<T>(bool toList, bool isIgnore, string data, Type type, object tryParseObj)
        {
            if (toList)
                return JsonConvert.DeserializeObject(data, type);
            else
                return isIgnore ? default(T) : tryParseObj;
        }

        private static object TryParseData<T>(string data, del_tryParse<T> func)
        {
            object _result = null;
            T _obj = Activator.CreateInstance<T>();

            if (!func(data, out _obj))
                _result = default(T);
            else
                _result = _obj;

            return _result;
        }

        public static bool IsNullOrDefault<T>(T refData, bool isClass = false)
        {
            int _loopTimes = 0;
            FieldInfo[] _fieldInfos = null;

            if (isClass)
            {
                _fieldInfos = typeof(T).GetFields();
                _loopTimes = _fieldInfos.Length;
            }
            else
                _loopTimes = 1;

            for (int i = 0; i < _loopTimes; i++)
            {
                object _value = isClass ? _fieldInfos[i].GetValue(refData) : refData;

                if (_value != null)
                {
                    Type _fieldType = isClass ? _fieldInfos[i].FieldType : refData.GetType();

                    if (_fieldType.IsEnum)
                    {
                        if (!isClass)
                            return false;
                        else
                            continue;
                    }

                    else if (_fieldType.IsArray)
                    {
                        Array _arr = (Array)_value;
                        if (_arr.Length <= 0)
                            continue;
                    }

                    else if (typeof(ICollection).IsAssignableFrom(_fieldType))
                    {
                        ICollection _col = (ICollection)_value;
                        if (_col.Count <= 0)
                            continue;
                    }

                    else if (_fieldType == typeof(int) && _value.Equals(default(int)))
                        continue;

                    else if (_fieldType == typeof(long) && _value.Equals(default(long)))
                        continue;

                    else if (_fieldType == typeof(float) && _value.Equals(default(float)))
                        continue;

                    else if (_fieldType == typeof(string) && (_value.Equals(default(string)) || _value.Equals("")))
                        continue;

                    else
                        return false;

                }
            }

            return true;
        }

        private static bool IsInHallSelection()
        {
            if (HallModel.Instance.GetCurHallId() == -1)
                return false;

            string _currentGameId = GameMenuModel.Instance.currentSCGGameID;
            GameInfo _currentGameInfo = BaseDataManager.gameInfoRecords.GetGameInfoByGameId(_currentGameId);

            if (_currentGameInfo == null)
            {
                if (ScenesManager.Instance.nowMainSceneName == "HallScene")
                    return true;
                else
                    return false;
            }

            if ((_currentGameInfo.gameTypeName == GameBridge.SLOT_TYPE || _currentGameInfo.gameTypeName == GameBridge.FISH_TYPE)
                && ScenesManager.Instance.nowMainSceneName == "HallScene")
                return true;

            else if (_currentGameInfo.gameTypeName != GameBridge.SLOT_TYPE
                && ((ScenesManager.Instance.nowMainSceneName == "LobbyScene" && ScenesManager.Instance.preMainSceneName == "HallScene")
                || (ScenesManager.Instance.nowMainSceneName == "GameScene" && ScenesManager.Instance.preMainSceneName == "HallScene")))
                return true;

            return false;
        }
    }

}
