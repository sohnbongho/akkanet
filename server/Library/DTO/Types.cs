namespace Library.DTO
{

    public enum DbConnectionType : int
    {
        None = 0,

        System = 1,
        Design = 2,
        Game = 3,
    }

    /// <summary>
    /// 로그인 방식
    /// </summary>
    public enum LoginType : int
    {
        None = 0,

        Google = 1,         // 구글
        FaceBook = 2,       // 페이스 북
        Apple = 3,          // 애플
        CrazyDiamond = 4,   // 자체 로그인
    }

    //1:로그인 서버, 2: 로비 서버, 3: 커뮤니티, 4: 존 서버, 5: 룸 서버, 6:채트
    public enum ServerType
    {
        None = 0,
        Login = 1,
        Lobby = 2,
        Community = 3,
        Zone = 4,
        Room = 5,
        Chat = 6,
        WebSocketCommunity = 7,

        Max = WebSocketCommunity, // 가장 마지막 보다 +1
    }

    /// <summary>
    /// 아이템 정보
    /// 기획데이터 참조 main_type
    /// </summary>
    public enum MainItemType
    {
        None = 0,

        Character = 2,        // 캐릭터 관련                        
        Clothing = 3,        // 의상 관련 아이템
        Accessory = 4,      // 액세서리 관련 아이템

        BackGround = 8,     // 배경
        BackGroundProp = 9,           // 배경 스티커

        Currency = 10,      // 제화
        SoftCat = 11,      // 말랑 캣
        DiggingWarrior = 12,      // 삽투사

        Package = 13,      // 세일 카테고리
        ShowRoom = 14,      // 세일 카테고리
    }

    /// <summary>
    /// 재화 종료
    /// </summary>
    public enum CurrencyType
    {
        None = 0,
        Gold = 1,      // 게임머니        
        Diamond = 2,    // 다이아몬드
        Clover = 3,    // 클로버

        Bell = 4,    // 방울        
        PremiumGold = 5,        // 유료 골드
        PremiumDiamond = 6,     // 유료 다이아몬드
        ColorTube = 7,     // 컬러튜드

        Max = ColorTube + 1, // 무저건 마지막보다 작다
    }

    /// <summary>
    /// 구매 타입
    /// </summary>
    public enum PurchaseType
    {
        None = 0,
        Normal = 1,      // 일반 구매
        Gift = 2,    // 선물하기        
    }

    /// <summary>
    /// 세일 타입
    /// </summary>
    public enum SaleCategoryType
    {
        None = 0,
        Season = 1,      // 시즌
        REtro = 2,    // 레트로
    }

    /// <summary>
    /// 게임 모드 타입
    /// </summary>
    public enum GameModeType : int
    {
        None = 0,

        DiggingWarrior = 1,     // 삽투사 모드(눈파기)
        BubblePop = 2,          // 버블 팝
        CutShroom = 3,          // 컷쉬룸
        Cleaning = 4,           // 청소 반장
        SoftCat = 5,            // 말랑 캣
        Jump = 6,

        Max = Jump + 1, // 항상 가장 마지막
        Mode8 = 8,
        Mode9 = 9,
        Mode10 = 10,
        Mode11 = 11,
        Mode12 = 12,
        Mode13 = 13,
        Mode14 = 14,
        Mode15 = 15,
        Mode16 = 16,
        Mode17 = 17,
        Mode18 = 18,
        Mode19 = 19,
        Mode20 = 20,
        Mode31 = 31,
        Mode32 = 32,
        Mode33 = 33,
        Mode34 = 34,
        Mode35 = 35,
        Mode36 = 36,
        Mode37 = 37,
        Mode38 = 38,
        Mode39 = 39,
        Mode40 = 40,
    }

    /// <summary>
    /// 게임모드 경험치
    /// </summary>
    public enum GameModeExpType : int
    {
        None = 0,

        DiggingWarrior = 1,     // 삽투사 모드(눈파기)
        BubblePop = 2,          // 버블 팝
        CutShroom = 3,          // 컷쉬룸
        Cleaning = 4,           // 청소 반장        
        SoftCat = 5,           // 말랑 캣
        Jump = 6,               // 점프맵

        Max = Jump + 1, // 항상 가장 마지막
    }

    /// <summary>
    /// 게임 모드별 맵 번호
    /// </summary>
    public enum GameModeMapIndex : int
    {
        DiggingWarrior = 1001,     // 삽투사 모드(눈파기)
        BubblePop = 1002,          // 버블 팝
        CutShroom = 1003,          // 컷쉬룸
        Cleaning = 1004,           // 청소 반장
        SoftCat = 1005,            // 말랑 캣
    }

    /// <summary>
    /// 게임 모드 상태
    /// </summary>
    public enum ZoneObjectType
    {
        None = 0,

        Level1 = 1,
        Level2 = 2,
        Level3 = 3,
        Level4 = 4,
        Level5 = 5,
    }


    /// <summary>
    /// 시즌 Item장착 타입
    /// </summary>
    public enum InGameItemType : int
    {
        None = 0,

        Bag = 1001, // 가방
        Equipment = 1002, // 장비
        Vehicle = 1003, // 탈것

        Shelter = 2001, // 1=쉼터
        SnackOrToy = 2002, // 2=간식/장난감
        Transport = 2003, // 3=이동기        
        SpeedBuff = 401, // 4=버프 (일회성)
        Badge = 701, // 5=뱃지 (일회성)
    }

    /// <summary>
    /// 주요 아이템 번호
    /// </summary>
    public enum ItemIndex : int
    {
        None = 0,

        Gold = 1,
        Diamond = 2,
        Clover = 3,
        Bell = 4,

        PremiumGold = 5,        // 유료 골드
        PremiumDiamond = 6,     // 유료 다이아몬드
        ColorTube = 7,     // 컬러튜드
    }

    /// <summary>
    /// 게임 종료 종류
    /// </summary>
    public enum GameEndingType : int
    {
        None = 0,

        TimeOver = 1,               // 게임 종료 시간 만료
        OutHunter = 2,              // 술레가 접속 종료
        UnlockAllNotch = 3,         // 노치 전체 해제
        LackOfUsers = 4,              // 유저 수 부족
    }

    /// <summary>
    /// 승리자 타입
    /// </summary>
    public enum WinnerType : int
    {
        None = 0,

        Normal = 1, // 일반 유저 
        Hunter = 2, // 승리자 타입
        Nobody = 3, // 무승부
    }

    /// <summary>
    /// 게임 모드 상태
    /// </summary>
    public enum GameModeState
    {
        None = 0,

        Ready = 1,
        Start = 2,
    }

    /// <summary>
    /// 부활 타입
    /// </summary>
    public enum ResurrectionType
    {
        None = 0,

        SelfAdvertising = 1,        // 광고에 의한 부활
        SelfClover = 2,             // 클로버 사용에 의한 부활
        OtherUser = 3,              // 다른 유저에 의한 부활

        GameEnd = 4,              // 게임 종료
        SelfBot = 5,                // Bot부활
        SelfTimeOver = 6,                // [스팀] 시간 초과에 의한 부활
    }

    /// <summary>
    /// 푸시 알림 타입
    /// </summary>
    public enum PushAlarmType
    {
        None = 0,

        Follow = 1,  // 팔로우 추가
        Present = 2,  // 선물하기
    }
    public static class PushAlarmKey
    {
        public static readonly string Title = "title_loc_key"; // 푸사 알림의 type (PushAlarmType)
        public static readonly string BodyKey = "body_loc_key"; // 푸사 알림의 type (PushAlarmType)
        public static readonly string BodyArgs = "body_loc_args"; // 푸사 알림의 type (PushAlarmType)
        public static class Follow
        {
            public static readonly string Title = "follow_title";
            public static readonly string BodyKey = "follow_body";

        }
        public class Present
        {
            public static readonly string Title = "present_title";
            public static readonly string BodyKey = "present_body";
        }
    }


    /// <summary>
    /// 말랑 모드 고양이 종류
    /// </summary>
    public enum SoftCatType
    {
        None = 0,

        Basic = 1, // 일반 캣
        Buff = 2, // 버프
        Random = 3, // 랜덤박스
    }

    public enum RewardType
    {
        None = 0,
        GameEnd = 1, // 게임 종료
        Advertising = 2, // 광고 확인
        Clover = 3, // 클로버 지급 지급의 의한 보상

    }

    /// <summary>
    /// 봇 액터 종류
    /// </summary>
    public enum BotActorType
    {
        None = 0,

        OnlyMove = 1,
        Cleaning = 2, // 청소 반장 모드 Actor
    }

    /// <summary>
    /// 페스타 종류
    /// </summary>
    public enum FestaType
    {
        None = 0,
        Gold = 1,      // 게임머니        
        Diamond = 2,    // 다이아몬드        
    }

    /// <summary>
    /// 
    /// </summary>
    public enum PlayType
    {
        None = 0,
        GameStart = 1,  // 게임 시작
        GameEnd = 2,    // 게임 종료
    }

    /// <summary>
    /// 장비 종류
    /// </summary>
    public enum UserDeviceType
    {
        None = 0,

        Android = 1,
        iOS = 2,
        PC = 3,

    }

    /// <summary>
    /// 유저 밴 타입
    /// </summary>
    public enum UserBanType : int
    {
        None = 0,

        PlayBan = 1, // 유저 밴
        ArticleComment = 2, // 게시글 댓글 밴
    }


    /// <summary>
    /// 패키지 종류
    /// </summary>
    public enum PackageCategory
    {
        None = 0,

        Clothing = 1, // 의상
        Currency = 2, // 재화
        Etc = 3, // 기타
    }

    /// <summary>
    /// 아이템 획득 
    /// </summary>
    public enum ItemGainReason
    {
        None = 0,

        Purchase = 1, // 구매
        GamePlay = 2, // 게임 플레이
        Mail = 3, // 메일 확인
        GrowClover = 4, // 쑥쑥 클로버 이벤트        
        DailyStamp = 5, // 데일리 스탬프
        SnowToExchange = 6, // 삽투사 눈

        MoneyToBell = 7,
        DiamondToBell = 8,

        PurchaseMulti = 9, // 여러개 동시 구매
        PurchasePackage = 10, // 패키지 아이템 구매
        Recepit = 11, // 영수증 타입
        ArriveObject = 12, // 오브젝트 목적지 도달
        BuyGameModeItem = 13, // 게임 모드 아이템 구매

        InitCharacter = 14, // 캐릭터 생성시, 첫 지급
        BuySeasonItem = 15,
        BonusReward = 16,  // 보상
        Advertising = 17,  // 광고
        SessionClose = 18,  // 세션 종료

        ResurrectionSelf = 19,  // 자신 부활
        GameComplete = 20,  // 게임 완료
        Gift = 21,  // 게임 완료
        Dye = 22,  // 염색

        MindAnalyzer = 23, // 심리 분석
        GrowCloverLeaf = 24, // 클로버
    }

    /// <summary>
    /// 콘텐츠 활성, 비활성
    /// </summary>
    public enum FeatureControlType : int
    {
        None = 0,

        StyleShop = 1,  // 스타일샵
        Space = 2,  // 스페이스
        DailyStamp = 3,  // 데일리 스탬프
        Messenger = 4,  // 메신저

        DiggingWarrior = 5,  // 삽투사
        Cleaning = 6,       // 청소반장
        BubblePop = 7,       // 버플팝
        CutShroom = 8,      // 컨슈룸

        PhotoBooth = 9,     // 포트 부스
    }

    /// <summary>
    /// 영수증 타입
    /// </summary>
    public enum ReceiptType : int
    {
        None = 0,

        Google = 1,  // 구글
        Apple = 2,  // 애플
        OneStore = 3,  // 원스토어
        Steam = 4,  // 스팀
    }
    
    public enum NoticeBoardType : int
    {
        None = 0,

        All = 1,  // 전체 요청
        Event = 2,  // 이벤트
        Update = 3,  // 업데이트        
        System = 4,  // 시스템        
    }

}


