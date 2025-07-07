using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Library.Helper
{
    //SHA-256 해시 알고리즘을 사용할 때, 생성되는 해시 문자열의 길이는 항상 일정합니다.
    //SHA-256은 256비트 길이의 해시 값을 생성하며, 이는 16진수로 표현할 때 64자리 길이의 문자열이 됩니다.
    //각 바이트는 16진수로 표현될 때 2개의 문자를 사용합니다 (0-255 범위의 값은 16진수로 00부터 FF까지). 
    //따라서 256비트는 256 / 8 = 32바이트이고, 이것을 16진수 문자열로 표현하면 32 * 2 = 64자리가 됩니다.
    //결론적으로, SHA-256 해시 알고리즘으로 생성된 hashedString은 항상 64자리 길이를 가지며, 
    //이보다 길어지거나 짧아지는 일은 없습니다.
    public static class HashHelper
    {        

        /// <summary>
        /// 무조건 64글자
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static string ComputeSha256Hash(string rawData)
        {
            // SHA256 인스턴스 생성
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // 입력 문자열을 바이트 배열로 변환
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // 바이트 배열을 16진수 문자열로 변환
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }


}
