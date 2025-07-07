using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Data.Enums;

public enum SessionClosedType : int
{
    None = 0,

    ClientClosed = 1,                 // 클라이언트 소켓 연결 끈김
    KeepAliveTimeOut = 2,       // 타임 아웃에 의한 강제 연결 종료
    DuplicatedLogin = 3,       // 중복 로그인에 따른 로그아웃
    FailOtherKick = 4,       // 다른 유저 킥시키고 들어가는 부분 실패
    TooFastPacketRequest = 5,       // 너무 빠른 패킷 요청
}
