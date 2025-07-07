using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityServer.Helper
{
    public class DummyUserInfo
    {
        public ulong UserSeq;
        public int BirthDay;
        public int Gender;
        public string Account { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Nickname { get; set; } = null!;
        public string GameSessionID { get; set; } = null!;
    };


    public sealed class DummyUserListHelper
    {
        private static readonly Lazy<DummyUserListHelper> lazy = new Lazy<DummyUserListHelper>(() => new DummyUserListHelper());
        public static DummyUserListHelper Instance { get { return lazy.Value; } }
        public const int MaxDummy = 20000;
        
        public ConcurrentDictionary<string, DummyUserInfo> DummyUserList = new ConcurrentDictionary<string, DummyUserInfo>();
        public DummyUserListHelper()
        {
            for(ulong i = 0; i < MaxDummy; i++)
            {
                var accountId = $"crazy{i}";
                DummyUserList[accountId] = new DummyUserInfo
                {
                    UserSeq = i,
                    Account = accountId,
                    BirthDay = 20000101,
                    Gender = ((int)i%2) +1,
                    Password = $"crazy",
                    Nickname = $"Nickname",
                    GameSessionID = $"{i}"
                };

            }
        }
    }
}
