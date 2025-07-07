namespace Library.DTO
{
    public enum ErrorCode : int
    {
        Succeed = 0,

        //////////////////// DB관련
        DbInsertedError = 101,    // DB Insert 실패
        DbUpdatedError = 102,     // DB Update 실패
        DbSelectedError = 103,     // DB Select 실패
        DbDeletedError = 104,     // 현재 해당 table Lock상태입니다.
        DbContentsLock = 105,     // 현재 해당 콘텐츠 Lock상태입니다.
        DbTableLock = 106,     // 현재 해당 table Lock상태입니다.
        DbInitializedError = 107,     // db초기화 오류

        ////////////////////  로비 서버 관련
        NotEnteredLobby = 1001,    // 로비에 입장하지 않았습니다.        
        NotFoundArticle = 1002,    // 게시글을 찾을수 없음
        NotAuthorityArticle = 1003,    // 게시글 권한 없음
        AlreadyReportedArticle = 1004, // 이미 신고된 글
        OverReportedArticleHour = 1005, // 신고한 글 초과
        OverReportedArticleDay = 1006, // 신고한 글 초과
        OverReportedArticleWeek = 1007, // 신고한 글 초과
        NotFoundArticleComment = 1008,    // 댓글을 찾을수 없습니다.
        ExceptionGoogleApi = 1009,    // 구글 API Exception
        BanArticleComment = 1150,    // 댓글 다는것에 대한 밴
        NoChangedArticle = 1151,    // 글 변경 없음

        OverFreeCloveLeaf = 1152,    // 프리 쑥쑥 이벤트 획득 실패
        OverAdCloveLeaf = 1153,    // 프리 쑥쑥 이벤트 획득 실패
        OverTotalCloveLeaf = 1154,    // 프리 쑥쑥 이벤트 획득 실패
        NotEnoughCloveLeaf = 1155,    // 클로버잎 부족
        
        NotFondAccessToken = 1157,    // 토큰 생성 실패

        //////////////////// 캐릭터 생성
        DuplicatedLogin = 1010,
        NotFoundAccount = 1011,
        NotFoundUserSeq = 1012, // UserSeq를 찾을수 없습니다.
        InvalidPass = 1013,
        InvalidLoginToken = 1014,
        NotFoundUserBySeesionGuid = 1015,
        NotFoundCharBySeesionGuid = 1016,
        AlreadyReactivateAccount = 1017, // 이미 비활성된 계정
        ReactivateAccountOnLogin = 1018, // 비활성된 계정으로 로그인 시도하여 계정 활성화 됨(계정이 복원되어, 다시 로그인 시도 필요)
        DeactivateAccountOnLogin = 1019, // 비활성된 계정으로 로그인 시도 (계정 복원 안됨)
        HasDeactivateAccount = 1020, // 비활성된 계정이 있음

        DuplicatedAccount = 1021, // 중복된 계정
        DuplicatedUserSeq = 1022, // 중복된 UserSeq
        DuplicatedNickname = 1023, // 중복된 닉네임

        HasCharacter = 1024, // 이미 캐릭터를 가지고 있습니다.        
        NotFoundCharacter = 1025, // 캐릭터를 찾을수 없습니다.                
        AbusiveNickName = 1026, // 불건전한 닉네임               

        OverFollowCount = 1027, // 최대 팔로워 수 초과
        OverFollowTodayCount = 1028, // 최대 팔로워 수 초과
        AlreadyFollowUser = 1029, // 이미 추가한 팔로우
        CannotAddSelf = 1030, // 나 자신은 추가할수 없음
        BanUser = 1031, // 밴된 유저
        BanDevice = 1032, // 밴한 Device
        InvalidRefreshToken = 1033, // 토큰 생성 실패

        //////////////////// Zone
        NotFoundZoneIndex = 1101, // 존을 찾을수 없습니다.

        AlreadyEnteredZone = 1102, // 이미 존에 입장해 있습니다.
        NotEnteredZone = 1103, // 존에 입장하지 않은 상태입니다.

        NotFoundObject = 1104, // 없는 오브젝트
        AlreadyHasObject = 1105, // 누군가 소유하고 있는 있는 오브젝트        
        DifferntOwnerObject = 1106, // 소유자 다른 오브젝트 시도
        NotAcquireObject = 1134, // 오브젝트를 소유할수 없음
        NeedToWait = 1135, // 좀더 기다려야 합니다.

        //////////////////// Room
        AlreadyCreatedRoom = 1107,            // 소유자 다른 오브젝트 시도
        AlreadyJoinedRoom = 1108,            // 이미 룸에 참여중입니다.
        NotFoundRoom = 1109,            //룸을 찾을수 없습니다.
        OverCharacterRoom = 1110,            // 입장 제한 수를 초과하였습니다.
        NotEnteredRoom = 1111,            // 방에 입장한 상태가 아닙니다.
        NotFoundUserRoom = 1112,            // 방에 유저가 없습니다.
        InvalidPassRoom = 1113,            // 비밀번호가 잘못되었습니다.
        NotMasterAuthorityRoom = 1114,            // 방장이 아닙니다.
        InvalidCharSeqRoom = 1115,            // 잘못된 유저seq 시도
        InvalidTeamRoom = 1116,            // 잘못된 팀
        InvalidGameMode = 1117,            // 잘못된 게임 모드
        NotFoundDifficultyType = 1118,      // 난이도를 찾을수 없습니다.
        NotFoundKeyNoteType = 1119,      // 키노트 타입을 찾을수 없음        
        StartedGameMode = 1120,      // 이미 4모드 시작되었음
        DuplicatedRewareded = 1121,      // 중복 보상
        InvalidGameGuid = 1122,      // 잘못된 게임 GUID
        NotStartedGameMode = 1123,      // 게임을 시작할수 없습니다.
        InvalidCurrencyAmount = 1124,      // 잘못된 제화 수
        NotReadyGameMode = 1125,
        NotEqualObjType = 1126,         // 맞지 않는 오브젝트 타입입니다.

        // ChatRoom
        OverMaxChatRoom = 1131,      // 최대 채팅룸을 초과
        NotFoundChatRoom = 1132,            //룸을 찾을수 없습니다.
        InvalidRoomName = 1133,            // 룸 이름이 적절하지 않습니다.

        // Item
        NotFoundItem = 2001,            // 없는 아이템
        OverMaxInvenSize = 2002,             // 최대 인벤 사이즈 초과
        ZeroSelledSize = 2003,             // 아이템을 0개 판매 시도
        NotEnoughPrice = 2004,             // 제화가 부족합니다.
        FailUpdatedItem = 2005,             // 아이템 업데이트 실패
        AlreadyEquippedItemType = 2006,             // 장착 중인 아이템 부위 다시 장착 시도
        AlreadyUnEquippedItemType = 2007,             // 장착 중인 아이템 부위 미 장착
        NotEnoughSnowPoint = 2008,             // 눈 포인트가 부족합니다.
        InvalidPurchaseItem = 2009,             // 아이템 구매 조건에 맞지 않습니다.
        HasSeasonItem = 2010,             // 이미 가지고 있는 시즌아이템입니다.
        CannotAccumulateSnow = 2011,             // 눈 누적할수 없음
        NotFoundReceipt = 2012,                 // 영수증을 찾을수 없음
        NonSaleItems = 2013,                    // 팔지 않는 상품
        OverHasItem = 2014,                    // 최대 아이템 수 초과
        CannotDye = 2015,                    // 염색할수 없는 아이템이다
        NotFoundColor = 2016,                    // 색을 찾을수 없습니다.
        NotFoundCurrencyType = 2017,            // 없는 CurrencyType
        AlreadyTakedOrderId = 2018,            // 이미 가져간 제품
        NotFoundItemType = 2019,            // 없는 아이템
        ImpossibleExpandItem = 2020,            // 연장 할수 없는 아이템
        ImpossibleColorItem = 2021,            // 염색할수 없는 아이템
        AccessDeniedStore = 2022,            // 이용할수 없는 상점
        CannotEquippedItem = 2023,            // 장착 할수 없는 아이템
        CannotExtenedItem = 2024,            // 기간 연장을 할수 없는 아이템
        InvalildBuyItem = 2025,            // 살수없는 아이템

        NotFoundGameModeExpType = 2026,            // 게임모드경험치 찾을수 없음
        MaxLevelReached = 2027,            // 최대 게임 레벨 달성

        FailFetchedItem = 2028,             // 아이템 가져오기 실패
        FailOpenBox = 2029,             // 박스 열기 실패
        ItemBoxNotYetOpenable = 2030,       // 아직 열수 없는 아이템 박스
        //
        CannotReadyRoomHost = 2101,             // 방장은 레디할수 없습니다.
        AlreadyReadyRoom = 2102,             // 이미 준비한 유저입니다.

        NotAuthorityRoom = 2103,             // 룸에 대한 권한이 없습니다.
        AlreadyGameStartRoom = 2104,             // 이미 게임 시작한 방입니다.
        NotGameStartRoom = 2105,             // 게임 시작한 방이 아닙니다/
        OverUserCountRoom = 2106,             // 방에 유저수가 많습니다.
        DuplicatedPosition = 2107,             // 유저 위치가 겹침
        NotReadyAllUsers = 2108,             // 유저들이 레디하지 않음
        OverUserCountZone = 2109,             // 존에 유저수가 많습니다.

        // 선물하기 업데이트 
        FailUpdatedAlert = 2110,             // 알림 업데이트 실패

        // 메일 관련
        NotFoundMail = 2120,             // 메일 찾기 실패
        AlreadyReceivedMail = 2121,            // 이미 받은 메일
        OverMaxMailSize = 2122,             // 최대 인벤 사이즈 초과

        NotFoundInven = 2130,             // 인벤토리

        NotFoundGameMode = 2140,             // 찾을수 없는 게임모드

        // 좋아요
        UnchangedLikeStatus = 2141,          // 좋아요 상태가 동일
        
        BlockGameMode = 2142,          // 게임 모드 블럭

        // 프로필
        NotAllowedUpdatedUserToken = 2151,          // 유저 토큰 업데이트 기간이 아님
        AlreadyUserToken = 2152,             // User Token이 겹침

        AliveCharacter = 2153,              // 캐릭터가 아직 살아있음
        ImpossibleAction = 2154,              // 할수 없는 액션
        NotFoundNotchId = 2155,            // 없는 노치
        NotStartGameModeOfNoNotches = 2156,            // 노치가 없어서 게임을 시작할수 없음
        InvalildNotchBuildOfGameMode = 2157,            // 노치를 생성할수 없음
        InvalildNotchBuildUser = 2158,            // 술레만 노치를 설치할수 있다.
        NotStartGameModeOfUserCount = 2159,            // 유저 수가 부족해서 시작할수 없다.
        NotFoundHunter = 2160,            // 술레가 없습니다        

        AlreadyReward = 2161, // 이미 광고로 보상을 받음        
        NotFoundReward = 2162, // 보상을 찾을수 없음
        NotGameGuid = 2163, // 보상을 찾을수 없음        

        NotAcquiredDailyStamp = 2164, // 스탬프를 획득할수 없음
        UnKnownError = 2165,          // 알수 없는 오류
        AlreadyAcquiredStampToday = 2166,          // 이미 스탬프를 획득했음

        AlreadyUnlockNotch = 2167,            // 이미 unlock한 노치
        NotEnoughFestaPoint = 2168,            // 포인트가 부족합니다.
        NotFoundFestaPoint = 2169,            // 페스타 정보 찾을수 없음
        InvalidFestaStep = 2170,            // 페스타 스탭이 잘못됨
        InvalidStampDateTime = 2171,            // 스탬프 기간이 잘못되었음

        /// GMTool 에러 메시지
        InvalidGiveItem = 3001,

        // 유저 킥을 위한 대기
        UserWait = 4001,
    }

}


