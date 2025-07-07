using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Data.Enums;

/// <summary>
/// tbl_user_log 관련 Type정보
/// </summary>
public enum DbUserLogType : int
{
    None = 0,

    DeletedCharacter = 1, // 캐릭터 삭제
}

/// <summary>
/// 0 ~ 15까지 있는 데이터 베이스 중 어디에 저장할지
/// </summary>
public enum DataBaseId
{
    ServerStatus = 0,
    Session = 1,
    Member = 2,
    Character = 3,

    Rank = 4, // 유저 랭크 정보
    Room = 5, // 룸정보
    CacheList = 6, // 캐시 리스트 정보            

    DBLock = 10,


}
