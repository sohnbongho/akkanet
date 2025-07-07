using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Data.Enums
{
    /// <summary>
    /// 행동
    /// </summary>
    public enum MoveType : int
    {
        Idle = 0,                   // 0 : 정지중
        StartedMove = 1,           // 1 : 이동시작
        Moving = 2,                 // 2 : 이동중
        StopedMove = 3,             // 3 : 이동멈춤
        StopedJump = 4,             // 4 : 점프멈춤
        Jumping = 5,               // 5 : 점프 중
        StartedJump = 6,            // 6 : 점프시작
        DirectMove = 100,           // 100 : 즉시 이동
        Reset = 101,                // 101 : 위치 초기화
    }

    /// <summary>
    /// 볼 액션 타입
    /// </summary>
    public enum BallActionType : int
    {
        None = 0,

        Throw = 1,      // 던지기
        Kick = 2,       // 발로 차기
        Coin = 3,       // 동전 던지기
        PutDown = 4,    // 내려놓기
    }

    /// <summary>
    /// 오브젝트 관련
    /// </summary>
    public enum ObjectState : int
    {
        None = 0,
        Close = 1,
        Open = 2,
    }

    public enum ObjectOwnType : int
    {
        None = 0,
        Own = 1,
        Update = 2,
        FixedUpdate = 3,
        NavDestination = 4,
    }

    /// <summary>
    /// System 메시지
    /// </summary>
    public enum SysMessageType : int
    {
        None = 0,
        Warning = 1,        // 경고
        Alarm = 2,          // 알림
        World = 3,          // 월드
    }

    public enum RequiredGrade : int
    {
        None = 0,
        A = 1,
        B = 2,
        C = 3,
        D = 4,
        E = 5,
        F = 6,
    }

    /// <summary>
    /// 팀
    /// </summary>
    public enum TeamType : int
    {
        None = 0,
        Red = 1,
        Blue = 2,
    }

    /// <summary>
    /// 룸 권한
    /// </summary>
    public enum RoomAuthority : int
    {
        Normal = 0,     // 일반 유저
        Admin = 1,      // 방장   
    }

    /// <summary>
    /// 난이도
    /// </summary>
    public enum DifficultyType : int
    {
        None        = 0,   
       
        Easy        = 1,   
        Normal      = 2,
        Hard        = 3,
        Extreme     = 4,
    }

    /// <summary>
    /// KeyNote타입
    /// </summary>
    public enum KeyNoteType : int
    {
        None = 0,

        Single = 1,     // 싱글탭
        Double = 2,     // 더블탭
        Drag = 3,      // 드래그
    }

}
