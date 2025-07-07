using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Helper;

public static class DateTimeHelper
{
    /// <summary>
    /// UtcNow 또는 Now를 사용할지 제어.
    /// </summary>
    //public static bool UseUtc { get; set; } = true;

    /// <summary>
    /// 현재 시간을 반환 (Utc 또는 Local).
    /// </summary>
    //public static DateTime Now => UseUtc ? DateTime.UtcNow : DateTime.Now;
    public static DateTime Now => DateTime.UtcNow;


    /// <summary>
    /// 특정 날짜가 현재 시간보다 이전인지 확인.
    /// </summary>
    public static bool IsPast(DateTime dateTime)
    {
        return dateTime < Now;
    }
}
