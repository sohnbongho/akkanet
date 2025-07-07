using Akka.Actor;
using System.Collections.Concurrent;

namespace CommunityServer.InfoRepository
{
    public class CharacterInfoRepository
    {
        private static readonly Lazy<CharacterInfoRepository> lazy = new Lazy<CharacterInfoRepository>(() => new CharacterInfoRepository());
        public static CharacterInfoRepository Instance { get { return lazy.Value; } }        

        // 캐릭터 정보들을 저장합니다.        
        public ConcurrentDictionary<ulong, IActorRef> CharacterInfos = new(); // <charSeq, SessionActorRef>
        public ConcurrentDictionary<ulong, ulong> UserSeqToCharSeqs = new(); // <userSeq, charSeq>

        private CharacterInfoRepository()
        {
            // 생성자를 private으로 선언하여 외부에서의 인스턴스화를 방지합니다.
        }
        public void Add(ulong userSeq, ulong charSeq, IActorRef actorRef)
        {
            CharacterInfos[charSeq] = actorRef;
            UserSeqToCharSeqs[userSeq] = charSeq;
        }
        public void Remove(ulong userSeq, ulong charSeq)
        {
            CharacterInfos.TryRemove(charSeq, out var _);
            UserSeqToCharSeqs.TryRemove(userSeq, out var _);
        }

    }
}
