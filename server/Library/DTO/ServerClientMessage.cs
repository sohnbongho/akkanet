using Messages;

namespace Library.DTO
{
    public class BaseItemInfo
    {
        public int ItemId { get; set; }
        public string ItemSeq { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public BaseItemInfo Clone()
        {
            return new BaseItemInfo { ItemId = ItemId, ItemSeq = ItemSeq, ItemCount = ItemCount };
        }
    }

    public class PurchaseItemInfo
    {
        public MainItemType ItemType { get; set; }
        public int ItemId { get; set; }
        public int ColorId { get; set; }
    }

    /// <summary>
    /// 구매했을때 아이템 정보
    /// </summary>
    public class BuyPurchaseItemInfo
    {
        public MainItemType ItemType { get; set; }
        public int ItemId { get; set; }
        public int ColorId { get; set; }
        public CurrencyType CurrencyType { get; set; } = CurrencyType.None;
        public string ItemPrice { get; set; } = string.Empty;
    }

    // 지급하는 아이템들
    public class RewardedItem
    {
        public MainItemType ItemType { get; set; }
        public int ItemId { get; set; }
        public int ColorId { get; set; }
    }


    public class CurrencyItemInfo
    {
        public string TotalMoney { get; set; } = string.Empty;
        public string TotalDiamond { get; set; } = string.Empty;
        public string TotalClover { get; set; } = string.Empty;
        public string TotalBell { get; set; } = string.Empty;
        public string TotalColorTube { get; set; } = string.Empty;

        public string TotalPremiumMoney { get; set; } = string.Empty;
        public string TotalPremiumDiamond { get; set; } = string.Empty;

        public CurrencyItemInfo Clone()
        {
            return new CurrencyItemInfo
            {
                TotalMoney = TotalMoney,
                TotalDiamond = TotalDiamond,
                TotalClover = TotalClover,
                TotalBell = TotalBell,
                TotalColorTube = TotalColorTube,
                TotalPremiumMoney = TotalPremiumMoney,
                TotalPremiumDiamond = TotalPremiumDiamond,
            };
        }
    }

    /// <summary>
    /// 기본 캐릭터 생성과 정보 갱신
    /// </summary>    
    public class CreatedAndUpdatedCharacterRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public int Gender { get; set; }

        public int BirthYear { get; set; }
        public int BirthMonth { get; set; }
        public int BirthDay { get; set; }

        public BodyPartsInfo BodyPartsInfo { get; set; } = new BodyPartsInfo();
        public List<RewardedItem> RewardedItems { get; set; } = new List<RewardedItem>(); // 기본 지급하는 아이템
        public bool MarketingAgreed { get; set; } = false;  // 마케팅 동의
    }
    public class CreatedAndUpdatedCharacterResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
        public string UserSeq { get; set; } = string.Empty;
        public string CharSeq { get; set; } = string.Empty;
    }

    /// <summary>
    /// 다른 유저들 아이템 구매
    /// </summary>
    public class ShopPurchaseAppearanceRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public List<PurchaseItemInfo> PurchaseItems { get; set; } = new List<PurchaseItemInfo>();
    }

    public class ShopPurchaseAppearanceResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }

        // 구매 성공한 아이템들
        public List<InventoryItem> Items { get; set; } = new List<InventoryItem>();

        public string Money { get; set; } = string.Empty;
        public string Diamond { get; set; } = string.Empty;
        public string Clover { get; set; } = string.Empty;
        public string Bell { get; set; } = string.Empty;
        public string ColorTube { get; set; } = string.Empty;

        public string PremiumMoney { get; set; } = string.Empty;
        public string PremiumDiamond { get; set; } = string.Empty;
    }
    /// <summary>
    /// 패키지 아이템 구매
    /// </summary>
    public class ShopPurchasePackageItemRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public int ItemId { get; set; }
    }

    public class ShopPurchasePackageItemResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }

        //지급된 아이템들
        public List<InventoryItem> InvenItems { get; set; } = new List<InventoryItem>();
        public CurrencyItemInfo CurrencyItemInfo { get; set; } = new CurrencyItemInfo();
    }

    /// <summary>
    /// 여러 아이템 입히기 
    /// </summary>
    public class EquipMultipleItemsRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public List<BaseItemInfo> WearItems { get; set; } = new List<BaseItemInfo>();
        public BodyPartsInfo BodyPartsInfo { get; set; } = new BodyPartsInfo();
        public short Equipped { get; set; } = 1;

        // BackGround이미지 업데이트 여부
        public bool UpdatedBackgroundImage { get; set; } = false;
        public string BackGroundImageUrl { get; set; } = string.Empty;
    }
    public class EquipMultipleItemsResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
    }

    /// <summary>
    /// 대상의 배경 정보를 얻어온다.
    /// </summary>
    public class BackgroundTargetInventoryRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public string TargetCharSeq { get; set; } = string.Empty;
        public short Equipped { get; set; } = 1;
    }
    public class BackgroundTargetInventoryResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
        public List<InventoryItem> Items { get; set; } = new List<InventoryItem>();
        public string BackGroundImageUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// 쇼륨 인벤토리 정보
    /// </summary>
    public class ShowRoomInventoryRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public string TargetCharSeq { get; set; } = string.Empty;
        public short Equipped { get; set; } = 1;
    }
    public class ShowRoomInventoryResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
        public List<InventoryItem> Items { get; set; } = new List<InventoryItem>();
    }

    /// <summary>
    /// 채팅 메시지 읽기
    /// </summary>
    public class ChatRoomAddedReadRequest
    {
        public string SessionGuid { get; set; } = string.Empty;

        public string RoomId { get; set; } = string.Empty;
        public List<string> ChatIds { get; set; } = new List<string>();
    }

    public class ChatRoomAddedReadResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
    }

    /// <summary>
    /// 채팅 룸 리스트 조회
    /// </summary>
    public class ChatRoomListRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
    }

    public class ChatRoomListResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }

        public List<ChatRoomInfo> Rooms { get; set; } = new List<ChatRoomInfo>();
    }

    /// <summary>
    /// 채팅 가져오기
    /// </summary>
    public class ChatRoomChatsRequest
    {
        public string SessionGuid { get; set; } = string.Empty;

        public string RoomId { get; set; } = string.Empty;
        public int Offset { get; set; }
        public int Limit { get; set; }
    }

    public class ChatRoomChatsResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }

        public string RoomId { get; set; } = string.Empty;
        public int Offset { get; set; }
        public int Limit { get; set; }
        public List<ChatMessageInfo> Chats { get; set; } = new List<ChatMessageInfo>();
    }

    /// <summary>
    /// 알림 읽기
    /// </summary>
    public class MarkedAlertMessageRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public List<string> MarkedAlertSeqs { get; set; } = new List<string>();
    }
    public class MarkedAlertMessageResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
        public List<string> MarkedAlertSeqs { get; set; } = new List<string>();
    }

    /// <summary>
    /// 프로필 읽기
    /// </summary>
    public class ProfileFetchRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public string TargetUserSeq { get; set; } = string.Empty;
    }

    public class ProfileFetchResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
        public string TargetUserSeq { get; set; } = string.Empty;
        public string TargetCharSeq { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;
        public string UserHandle { get; set; } = string.Empty;
        public string ProfileJsonData { get; set; } = string.Empty;

        public int FollowCount { get; set; } // 팔로우(내가 따라 다니는 사람) 유저들 수
        public int FollowerCount { get; set; } // 팔로워(나를 따라다니는 사람) 유저들 수
        public string ArticleCount { get; set; } = string.Empty; // 게시물 수
    }

    /// <summary>
    /// 프로필 수정
    /// </summary>
    public class ProfileUpdateRequest
    {
        public string SessionGuid { get; set; } = string.Empty;

        public bool UpdatedUrl { get; set; } = false;
        public string ImageUrl { get; set; } = string.Empty;

        public bool UpdatedName { get; set; } = false;
        public string NickName { get; set; } = string.Empty;

        public bool UpdatedUserHandle { get; set; } = false;
        public string UserHandle { get; set; } = string.Empty;

        public bool UpdatedJson { get; set; } = false;
        public string ProfileJsonData { get; set; } = string.Empty;

    }
    public class ProfileUpdateResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
    }

    /// <summary>
    /// 검색
    /// </summary>
    public class SearchedUserInfo
    {
        public string UserSeq { get; set; } = string.Empty;
        public string CharSeq { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string UserHandle { get; set; } = string.Empty;
    }



    /// <summary>
    /// 검색 - 계정
    /// </summary>
    public class SearchAccountRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public string KeyWord { get; set; } = string.Empty;

    }

    public class SearchAccountResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }

        public List<SearchedUserInfo> Users { get; set; } = new List<SearchedUserInfo>();
    }

    /// <summary>
    /// 검색 - 태그들
    /// </summary>
    public class SearchedTagInfo
    {
        public string Id { get; set; } = string.Empty;
        public string TagName { get; set; } = string.Empty;
        public int Count { get; set; }

    }
    public class SearchTagsRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public string KeyWord { get; set; } = string.Empty;

    }

    public class SearchTagsResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }

        public List<SearchedTagInfo> Tags { get; set; } = new List<SearchedTagInfo>();
    }

    /// <summary>
    /// 태그 - 게시글들
    /// </summary>
    public class SearchTagArticlesRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public string TagName { get; set; } = string.Empty;

    }

    public class SearchTagArticlesResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }

        public List<ArticleInfo> Articles { get; set; } = new List<ArticleInfo>();
    }

    /// <summary>
    /// 계정 - 탈퇴
    /// </summary>
    public class AccountDeactivateRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public List<int> ReasonIds { get; set; } = new List<int>(); // 사유 코드들

    }

    public class AccountDeactivateResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }

    }

    /// <summary>
    /// 푸시 알림 설정 업데이트
    /// </summary>
    public class PushAlaramSetupUpdatedRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public bool MaketAgreed { get; set; } // 마케팅 동의
        public bool QuiteMode { get; set; } // 매너 모드
        public DateTime StartQuiteDate { get; set; } // 매너 모드 - 시작
        public DateTime EndQuiteDate { get; set; } // 매너 모드 - 종료

        public bool NewFollow { get; set; } = true; // 팔로우 추가시
        public bool Present { get; set; } = true; // 선물하기 추가알림

    }

    public class PushAlaramSetupUpdatedResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }

    }

    /// <summary>
    /// 푸시 알림 정보
    /// </summary>
    public class PushAlaramSetupFetchedRequest
    {
        public string SessionGuid { get; set; } = string.Empty;

    }

    public class PushAlaramSetupFetchedResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
        public bool MaketAgreed { get; set; } // 마케팅 동의
        public bool QuiteMode { get; set; } // 매너 모드
        public DateTime StartQuiteDate { get; set; } // 매너 모드 - 시작
        public DateTime EndQuiteDate { get; set; } // 매너 모드 - 종료

        public bool NewFollow { get; set; } = true; // 팔로우 추가시
        public bool Present { get; set; } = true; // 선물하기 추가알림

    }

    /// <summary>
    /// 벨 아이템 교환
    /// </summary>
    public class BellExchangeMoneyToBellRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public string Money { get; set; } = string.Empty; // 골드
    }

    public class BellExchangeMoneyToBellResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
        public string TotalBell { get; set; } = string.Empty;
        public string TotalMoney { get; set; } = string.Empty;

    }

    /// <summary>
    /// 벨 아이템 교환
    /// </summary>
    public class BellExchangeDiamondToBellRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public string Diamond { get; set; } = string.Empty; // 방울 수
    }

    public class BellExchangeDiamondToBellResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
        public string TotalBell { get; set; } = string.Empty;
        public string TotalDiamond { get; set; } = string.Empty;

    }

    /// <summary>
    /// 게시글 댓글 요청
    /// </summary>
    public class ArticleCommentsRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public string ArticleId { get; set; } = string.Empty;

    }

    public class ArticleCommentsResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
        public List<ArticleCommentInfo> Comments { get; set; } = new List<ArticleCommentInfo>();
    }

    /// <summary>
    /// 데일리 스탬 조회
    /// </summary>
    public class DailyStampCountRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public short Year { get; set; }
        public short Month { get; set; }

    }

    public class DailyStampCountResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
        public short StampCount { get; set; }
        public DateTime AcquiredDate { get; set; } // 스탬프 마지막 획득날짜 

    }

    /// <summary>
    /// 데일리 스탬 획득
    /// </summary>
    public class DailyStampAcquiredRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public short Year { get; set; }
        public short Month { get; set; }
        public short StampCount { get; set; }

        public int UsedDiamond { get; set; }
        public int UsedClover { get; set; }

    }

    public class DailyStampAcquiredResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
        public short StampCount { get; set; }
        public BaseItemInfo AwaredItemInfo { get; set; } = new BaseItemInfo();
        public CurrencyItemInfo TotalCurrency { get; set; } = new CurrencyItemInfo();


    }

    /// <summary>
    /// 아이템 정보 요청
    /// </summary>
    public class InventoryItemDetailRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public int ItemId { get; set; }
        public string ItemSeq { get; set; } = string.Empty;



    }

    public class InventoryItemDetailResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
        public int ItemId { get; set; }
        public string ItemSeq { get; set; } = string.Empty;

        public InventoryItem InventoryItem { get; set; } = new InventoryItem();
    }

    /// <summary>
    /// 골드, 다이아 페스타
    /// </summary>
    public class FestaInfo
    {
        public FestaType FestaType { get; set; } = FestaType.None;

        public string AccumulatedPoint { get; set; } = string.Empty; // 누적 포인트
        public int AcquireStep { get; set; }
    }

    /// <summary>
    /// 골드, 다이아 페스타 정보
    /// </summary>
    public class GoldDiaFestaInfoRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
    }

    public class GoldDiaFestaInfoResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
        public List<FestaInfo> FestaInfos { get; set; } = new List<FestaInfo>();


    }

    /// <summary>
    /// 골드, 다이아 페스타 포인트 사용
    /// </summary>
    public class GoldDiaFestaRewardRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public FestaType FestaType { get; set; } = FestaType.None;
        public int Step { get; set; }
    }

    public class GoldDiaFestaRewardResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
        public FestaInfo FestaInfo { get; set; } = new FestaInfo();

        public BaseItemInfo BaseItemInfo { get; set; } = new BaseItemInfo();
        public CurrencyItemInfo CurrencyItemInfo { get; set; } = new CurrencyItemInfo();
    }

    /// <summary>
    /// 채팅에서 안읽은 메시지가 있는지 확인
    /// </summary>
    public class ChatCheckeHasUnreadMessageRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
    }

    public class ChatCheckeHasUnreadMessageResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
        public bool HasUnreadMessage { get; set; } = false;

    }

    /// <summary>
    /// 공지사항
    /// </summary>
    public class NoticeData
    {
        public ulong Id { get; set; }                 // 공지사항의 고유 ID
        public string Title { get; set; } = string.Empty;            // 공지 제목
        public string Content { get; set; } = string.Empty;          // 공지 내용        
        public DateTime? ExpiryDate { get; set; } = null;
    }

    /// <summary>
    /// 공지사항(지속) 가져오기
    /// </summary>
    public class NoticePersistentRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
    }

    public class NoticePersistentResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
        public List<NoticeData> NoticeDatas { get; set; } = new List<NoticeData>();

    }

    /// <summary>
    /// 쑥쑥 클로버 정보 요청
    /// </summary>

    public class GrowCloverLeafInfoRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
    }

    public class GrowCloverLeafInfoResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }

        public int TodayFreeCloverLeaf { get; set; } // 오늘 획득한 공짜 클로버잎 수
        public int TodayAdCloverLeaf { get; set; } // 오늘 획득한 광고 클로버잎 수
        public int TotalCloverLeaf { get; set; } // 총 클로버잎 수
        public string TotalClover { get; set; } = string.Empty;


    }

    /// <summary>
    /// 쑥쑥 클로버잎 추가 요청
    /// </summary>

    public class GrowCloverLeafAddRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public bool IsAd { get; set; } // 광고 유무
    }

    public class GrowCloverLeafAddResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }

        public int TodayFreeCloverLeaf { get; set; } // 오늘 획득한 공짜 클로버잎 수
        public int TodayAdCloverLeaf { get; set; } // 오늘 획득한 광고 클로버잎 수
        public int TotalCloverLeaf { get; set; } // 총 클로버잎 수
    }

    /// <summary>
    /// 클로버잎 => 클로버로 변환
    /// </summary>

    public class GrowCloverLeafExchangeRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
    }

    public class GrowCloverLeafExchangeResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }

        public int TodayFreeCloverLeaf { get; set; } // 오늘 획득한 공짜 클로버잎 수
        public int TodayAdCloverLeaf { get; set; } // 오늘 획득한 광고 클로버잎 수
        public int TotalCloverLeaf { get; set; } // 총 클로버잎 수
        public string TotalClover { get; set; } = string.Empty;
    }

    /// <summary>
    /// 기능 블럭 여부
    /// </summary>
    public class FeatureInfoRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
    }

    public class FeatureInfoResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }

        public List<FeatureControlType> BlockedFeatures { get; set; } = new List<FeatureControlType>();
    }

    /// <summary>
    /// 긴급 점검
    /// </summary>
    public class EmergencyNoticeRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
    }

    public class EmergencyNoticeResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }

        public List<DateTime> EmergencyNoticeDates { get; set; } = new List<DateTime>();
    }


    public class InAppItemInfo
    {
        public int ItemId { get; set; }
        public string TotalAmount { get; set; } = string.Empty;
        public string AddedAmount { get; set; } = string.Empty;
    }

    /// <summary>
    /// 영수증 확인
    /// </summary>
    public class VerifyReceiptRequest
    {
        public string SessionGuid { get; set; } = string.Empty;

        public string OrderId { get; set; } = string.Empty; // 주문ID(OrderId)	
        public string ProductId { get; set; } = string.Empty;           // 제품 ID 
        public string ApplicationName { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public string GooglePurchaseToken { get; set; } = string.Empty;  // 구글에서 생성한 토큰	

        public string CurrencyCode { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;

    }
    public class VerifyReceiptResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }

        public List<InAppItemInfo> InAppItems { get; set; } = new List<InAppItemInfo>();
    }

    /// <summary>
    /// 애플 영수증 확인
    /// </summary>
    public class VerifyAppleReceiptRequest
    {
        public string SessionGuid { get; set; } = string.Empty;

        public string TrsactionId { get; set; } = string.Empty;
        public string ReceiptData { get; set; } = string.Empty;
        public UserDeviceType UserDeviceType { get; set; } = UserDeviceType.None;
        public string CurrencyCode { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;

    }

    public class VerifyAppleReceiptResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }

        public List<InAppItemInfo> InAppItems { get; set; } = new List<InAppItemInfo>();
        public int StatusCode { get; set; }
    }

    // Apple 영수증 응답 모델
    public class VerifyTransactionResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public TransactionData[] SignedTransactions { get; set; } = Array.Empty<TransactionData>();
    }

    public class TransactionData
    {
        public string OriginalTransactionId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string PurchaseDate { get; set; } = string.Empty;
        public string ExpirationDate { get; set; } = string.Empty;
    }

    /// <summary>
    /// 게임모드 경험치들
    /// </summary>
    public class GameModeExpsRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
    }

    public class GameModeExpsResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }

        public List<GameModeExpInfo> GameModeInfos { get; set; } = new List<GameModeExpInfo>();
    }

    public class GameModeExpInfo
    {
        public GameModeType GameModeType { get; set; } = GameModeType.None;
        public int Level { get; set; } = 0;
        public long Exp { get; set; } = 0;
    }

    /// <summary>
    /// 구글 Token 새로 고침
    /// </summary>
    public class RefreshGoogleTokenRequest
    {
        public UserDeviceType UserDeviceType { get; set; } = UserDeviceType.None;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshGoogleTokenResponse
    {
        public ErrorCode ErrorCode { get; set; }

        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// 애플 Token 새로 고침
    /// </summary>
    public class RefreshAppleTokenRequest
    {
        public UserDeviceType UserDeviceType { get; set; } = UserDeviceType.None;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshAppleTokenResponse
    {
        public ErrorCode ErrorCode { get; set; }

        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// Apple Token 얻기
    /// </summary>
    public class GetGoogleTokenRequest
    {
        public string AuthorizationCode { get; set; } = string.Empty;
    }

    public class GetGoogleTokenResponse
    {
        public ErrorCode ErrorCode { get; set; }

        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// 페이스북 Token 새로 고침
    /// </summary>
    public class RefreshFacebookTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshFacebookTokenResponse
    {
        public ErrorCode ErrorCode { get; set; }

        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// 페이스북 Token 얻기
    /// </summary>
    public class GetFacebookTokenRequest
    {
        public string AuthorizationCode { get; set; } = string.Empty;
    }

    public class GetFacebookTokenResponse
    {
        public ErrorCode ErrorCode { get; set; }

        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// Apple Token 얻기
    /// </summary>
    public class GetAppleTokenRequest
    {
        public UserDeviceType UserDeviceType { get; set; } = UserDeviceType.None;
        public string Id { get; set; } = string.Empty;
    }

    public class GetAppleTokenResponse
    {
        public ErrorCode ErrorCode { get; set; }

        public string Content { get; set; } = string.Empty;
    }


    /// <summary>
    /// 심리테스트 - 클로버 사용
    /// </summary>

    public class MindAnalyzerUseCloverRequest
    {
        public string SessionGuid { get; set; } = string.Empty;

    }

    public class MindAnalyzerUseCloverResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
        public long TotalClover { get; set; }

    }

    /// <summary>
    /// 원스토어 - consumePurchase (구매상품 소비)
    /// </summary>
    public class OneStoreConsumePurchaseRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty; // API를 호출하는 앱의 패키지 네임 (Data Size : 128)
        public string ProductId { get; set; } = string.Empty; // 상품 ID (Data Size : 150)
        public string PurchaseToken { get; set; } = string.Empty; // 구매 토큰 (Data Size : 20)
        public bool IsSandBox { get; set; } = false; // SandBox
        public bool IsGlobal { get; set; } = false; // // false(MKT_ONE:원스토어(기본)), true(MKT_GLB:원스토어글로벌)
        public string CurrencyCode { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
    }
    public class OneStoreConsumePurchaseResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
        public List<InAppItemInfo> InAppItems { get; set; } = new List<InAppItemInfo>();
    }


    /// <summary>
    /// 스팀 - 상품 트랜잭션 초기화
    /// </summary>
    public class SteamInitTransactionRequest
    {
        public string SessionGuid { get; set; } = string.Empty;

        public bool IsSandBox { get; set; } = true;

        public string UserSteamId { get; set; } = string.Empty; // steamid: Steam ID of user making purchase.
        public uint ItemCount { get; set; } // itemcount: Number of items in cart.
        public string Language { get; set; } = "ko";  // 언어: ISO 639-1 language code of the item descriptions        
        public string Currency { get; set; } = "KRW";  // currency: ISO 4217 currency code. See Supported Currencies for proper format of each currency.
        public uint ItemId { get; set; } // itemid[0]: 3rd party ID for item.
        public short Qty { get; set; } // qty[0]: Quantity of this item.
        public string Amount { get; set; } = string.Empty; // amount[0]: Total cost (in cents) of item(s) to be charged at this time
        public string Description { get; set; } = string.Empty; // description[0]: Description of item. Maximum length of 128 characters.
        public string CurrencyCode { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;

    }
    public class SteamInitTransactionResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
        public string Orderid { get; set; } = string.Empty;
        public string Transid { get; set; } = string.Empty;
    }

    /// <summary>
    /// 스팀 - 영수증 확정하고 아이템 지급
    /// </summary>
    public class StreamFinalizeTransactionRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public bool IsSandBox { get; set; } = true;
        public string Orderid { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
    }

    public class SteamFinalizeTransactionResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
        public List<InAppItemInfo> InAppItems { get; set; } = new List<InAppItemInfo>();
    }

    /// <summary>
    /// 
    /// </summary>
    public class SeasonScheduleInfo
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;

        public DateTime ResetDate { get; set; }
    }
    public class SeasonScheduleRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
    }

    public class SeasonScheduleResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
        public List<SeasonScheduleInfo> Infos { get; set; } = new List<SeasonScheduleInfo>();

    }

    /// <summary>
    /// 공지 게시판 가져오기
    /// </summary>
    public class NoticeBoardData
    {
        public ulong Id { get; set; }                 // 공지사항의 고유 ID
        public ulong NoticeSeq { get; set; }                 // 공지사항의 고유 ID        
        public string Title { get; set; } = string.Empty;            // 공지 제목
        public string Content { get; set; } = string.Empty;          // 공지 내용        
        public DateTime? ExpiryDate { get; set; } = null!;
    }
    public class NoticeBoardRequest
    {
        public string SessionGuid { get; set; } = string.Empty;
        public NoticeBoardType NoticeBoardType { get; set; } = NoticeBoardType.None;
        public string LanguageCode { get; set; } = "kor"; // LanguageCode 참고
    }

    public class NoticeBoardResponse
    {
        public string SessionGuid { get; set; } = string.Empty;
        public ErrorCode ErrorCode { get; set; }
        public NoticeBoardType NoticeBoardType { get; set; } = NoticeBoardType.None;
        public string LanguageCode { get; set; } = "kor"; // LanguageCode 참고
        public List<NoticeBoardData> NoticeDatas { get; set; } = new List<NoticeBoardData>();

    }
}
