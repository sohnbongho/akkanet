using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Library.Helper
{
    //Snowflake Algorithm(Twitter에서 사용):
    //이 알고리즘은 유니크한 ID를 생성하기 위해 시간, 데이터 센터 ID, 기계 ID, 시퀀스 번호 등을 조합합니다.
    //다중 서버 환경에서도 안전하게 ID를 생성할 수 있습니다.

    //var epoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    //var workerId = 5L; = 2^10 => 0 ~ 1023 까지 사용가능
    //var generator = new SnowflakeIdGenerator(workerId, epoch);

    //var id = generator.NextId();
    //Console.WriteLine($"Generated ID: {id}");

    //최대 지속 가능 시간(밀리초): (2 ^ 41 − 1)
    //최대 지속 가능 시간(년): 최대 지속 가능 시간(밀리초) / (1000 * 60 * 60 * 24 * 365.25)
    //이 계산을 통해 얻은 결과는 약 69.7년입니다.
    //따라서, DateTime.UtcNow.Ticks / 10000을 사용하면
    //Snowflake ID 생성기는 약 69.7년 동안 유니크한 ID를 생성

    public class SnowflakeIdGenerator
    {

    }


}
