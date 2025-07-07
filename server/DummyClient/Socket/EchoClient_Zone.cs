using Akka.Actor;
using Library.Data.Enums;
using Library.DTO;
using Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DummyClient.Socket
{
    public sealed partial class TelnetClient : UntypedActor
    {
        private ulong _defaultCharSeq = 12652791513328132096;
        /// <summary>
        /// Login 서버
        /// </summary>
        /// <param name="sender"></param>
        private void SendZoneEnterRequest(IActorRef sender)
        {
            var request = new MessageWrapper
            {
                ZoneEnterRequest = new ZoneEnterRequest
                {
                    //SessionGuid = "3643b9f5-878d-4b80-b2a0-376c297bb99e",
                    SessionGuid = "d4f7806b-81ce-4065-9a0f-8ceb95696ba8",
                    MapIndex = 1001,
                }
            };
            Tell(request);
        }
        
        private void SendKeepAlive(IActorRef sender)
        {
            var request = new MessageWrapper
            {
                KeepAliveRequest = new KeepAliveRequest
                {                    
                }
            };
            Tell(request);
        }

        /// <summary>
        /// 캐릭터 이동
        /// </summary>
        /// <param name="sender"></param>
        private void SendMoveRequest(IActorRef sender)        
        {
            var request = new MessageWrapper
            {
                MoveRequest = new MoveRequest
                {
                    Position = new Position { X = 35, Y = 2, Z= 35 },
                }
            };
            Tell(request);
        }

        /// <summary>
        /// 오브젝트 던지기
        /// </summary>
        /// <param name="sender"></param>        
        private void SendObjectUseRequest(IActorRef sender)
        {
            var request = new MessageWrapper
            {
                ObjectUseRequest = new ObjectUseRequest
                {
                    ObjectId = 1,
                }
            };
            Tell(request);
        }

        /// <summary>
        /// 오브젝트 사용 종료
        /// </summary>
        /// <param name="sender"></param>        
        private void SendObjectUseEndRequest(IActorRef sender)
        {
            var request = new MessageWrapper
            {
                ObjectUseEndRequest = new ObjectUseEndRequest
                {
                    ObjectId = 1,
                }
            };
            Tell(request);

        }

        /// <summary>
        /// 오브젝트 상태
        /// </summary>
        /// <param name="sender"></param>        
        private void SendObjectStateRequest(IActorRef sender)
        {
            var request = new MessageWrapper
            {
                ObjectStateRequest = new ObjectStateRequest
                {
                    ObjectId = 1,
                    State = (int)ObjectState.Open,
                }
            };
            Tell(request);
        }

        /// <summary>
        /// 오브젝트 상태
        /// </summary>
        /// <param name="sender"></param>                
        private void SendObjectOwnTypeRequest(IActorRef sender)
        {
            var request = new MessageWrapper
            {
                ObjectOwnTypeRequest = new ObjectOwnTypeRequest
                {
                    ObjectId = 1,
                    OwnType = (int)ObjectOwnType.Own,
                }
            };
            Tell(request);
        }

        /// <summary>
        /// 오브젝트 이동
        /// </summary>
        /// <param name="sender"></param>
        private void SendObjectTransformRequest(IActorRef sender)        
        {
            var request = new MessageWrapper
            {
                ObjectTransformRequest = new ObjectTransformRequest
                {
                    ObjectId = 1,
                    MoveType = (int)MoveType.Idle,
                    Position = new Position { X = 1, Y = 1, Z = 1},
                    Rotation = new Position { X = 2, Y = 2, Z = 2},
                    Velocity = new Position { X = 3, Y = 3, Z = 3},
                }
            };
            Tell(request);

        }

        /// <summary>
        /// 플레이어 액션
        /// </summary>
        /// <param name="sender"></param>
        private void SendPlayerActionRequest(IActorRef sender)        
        {
            var request = new MessageWrapper
            {
                PlayerActionRequest = new PlayerActionRequest
                {
                    ObjectId = 1,
                    ActionId = 1,
                    IsInAction = 1,
                }
            };
            Tell(request);
        }

        /// <summary>
        /// 볼액션
        /// </summary>
        /// <param name="sender"></param>
        private void SendPlayerBallActionRequest(IActorRef sender)
        {
            var request = new MessageWrapper
            {
                PlayerBallActionRequest = new PlayerBallActionRequest
                {
                    BallType = (int)BallActionType.Throw,
                    BallPosition = new Position { X=1,Y=1,Z=1},
                    Velocity = new Position { X=2,Y=2,Z=2},
                    Force = new Position { X=3,Y=3,Z=3},
                    HitPoint = new Position { X=4,Y=4,Z=4},
                }
            };
            Tell(request);
        }
                
        private void SendInvenItemRequest(IActorRef sender)
        {
            var request = new MessageWrapper
            {
                InvenItemRequest = new InvenItemRequest
                {
                }
            };
            Tell(request);
        }

        /// <summary>
        /// 알림 메시지
        /// </summary>
        /// <param name="sender"></param>
        private void SendSysMessageRequest(IActorRef sender)
        {
            var request = new MessageWrapper
            {
                SysMessageRequest = new SysMessageRequest
                {
                    Type = (int)SysMessageType.Warning,
                    CharSeq = _defaultCharSeq,
                    Message = "Alarm Alarm"
                }
            };
            Tell(request);
        }

        /// <summary>
        /// 방 생성
        /// </summary>
        /// <param name="sender"></param>        
        private void SendRoomCreateRequest(IActorRef sender)
        {
            var request = new MessageWrapper
            {
                RoomCreateRequest = new RoomCreateRequest
                {
                    GameMode = 1,

                    Name = "test방",                    
                    MaxUserCount = 8,                    

                    SongSn = 1001,
                    StageSn = 1001,
                }
            };
            Tell(request);

        }

        /// <summary>
        /// RoomList요청
        /// </summary>
        /// <param name="sender"></param>                
        private void SendRoomListRequest(IActorRef sender)
        {
            var request = new MessageWrapper
            {
                RoomListRequest = new RoomListRequest
                {                    
                }
            };
            Tell(request);
        }

        /// <summary>
        /// 방에 입장
        /// </summary>
        /// <param name="sender"></param>        
        private void SendRoomEnterRequest(IActorRef sender)
        {
            var request = new MessageWrapper
            {
                RoomEnterRequest = new RoomEnterRequest
                {
                    RoomSeq = 1,                    
                }
            };
            Tell(request);

        }

        /// <summary>
        /// 방에서 나오기
        /// </summary>
        /// <param name="sender"></param>        
        private void SendRoomLeaveRequest(IActorRef sender)
        {
            var request = new MessageWrapper
            {
                RoomLeaveRequest = new RoomLeaveRequest
                {                    
                }
            };
            Tell(request);
        }

        /// <summary>
        /// 방에 유저 정보 요청
        /// </summary>
        /// <param name="sender"></param>        
        private void SendRoomCharListRequest(IActorRef sender)
        {
            var request = new MessageWrapper
            {
                RoomCharListRequest = new RoomCharListRequest
                {
                }
            };
            Tell(request);
        }

        /// <summary>
        /// 아이템 판매
        /// </summary>
        /// <param name="sender"></param>        
        private void SendSellItemRequest(IActorRef sender)
        {
            var request = new MessageWrapper
            {
                SellItemRequest = new SellItemRequest
                {
                }
            };
            Tell(request);
        }

        /// <summary>
        /// 인벤토리 확장
        /// </summary>
        /// <param name="sender"></param>        
        private void SendItemInvenExpandRequest(IActorRef sender)
        {
            var request = new MessageWrapper
            {
                ItemInvenExpandRequest = new ItemInvenExpandRequest
                {
                    AddedInvenSize = 1,
                }
            };
            Tell(request);
        }

        /// <summary>
        /// 시즌 아이템 구매
        /// </summary>
        /// <param name="sender"></param>
        private void SendBuySeasonItemRequest(IActorRef sender)
        {
            var request = new MessageWrapper
            {
                BuySeasonItemRequest = new BuySeasonItemRequest
                {
                    GameModeType = 1,
                    ShopItemIdx = 1,
                }
            };
            Tell(request);
        }

        /// <summary>
        /// 시즌 아이템 구매
        /// </summary>
        /// <param name="sender"></param>
        
        private void SendEquipSeasonItemRequest(IActorRef sender)
        {
            var request = new MessageWrapper
            {
                EquipSeasonItemRequest = new EquipSeasonItemRequest
                {               
                    ItemSeq = 17327758530907738112,
                    IngameParts = "{}"
                }
            };
            Tell(request);
        }        
        
        private void SendUnEquipSeasonItemRequest(IActorRef sender)        
        {
            var request = new MessageWrapper
            {
                UnEquipSeasonItemRequest = new UnEquipSeasonItemRequest
                {
                    ItemEquipType = (int)InGameItemType.Bag,
                    IngameParts = "{}"
                }
            };
            Tell(request);
        }
        /// <summary>
        /// 눈 교환
        /// </summary>
        /// <param name="sender"></param>
        private void SendExchangeSnowForGoldRequest(IActorRef sender)
        {

        }

    }
}
