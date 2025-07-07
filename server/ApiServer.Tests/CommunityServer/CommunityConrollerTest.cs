
using Akka.TestKit.NUnit;

namespace ApiServer.Tests.CommunityServer
{
    [TestFixture]
    public partial class CommunityConrollerTest : TestKit
    {
        private const int _maxAccount = 3;
        private static string[] _defaultAccountId = new string[_maxAccount];
        private static string[] _defaultNickName = new string[_maxAccount];
        private static string[] _defaultSessoinGuid = new string[_maxAccount];
        private static string[] _defaultUserSeq = new string[_maxAccount];
        private static string[] _defaultCharSeq = new string[_maxAccount];        

        [OneTimeSetUp]
        public static void InitialSetup()
        {
            
        }
    }
}
