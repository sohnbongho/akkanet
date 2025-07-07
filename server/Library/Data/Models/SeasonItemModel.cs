using Library.DTO;
using Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Data.Models
{
    //// 장비 아이템(일반)
    public class UniqueItem
    {
        public BaseItem BaseItem { get; set; } = null!;
        public ulong CharSeq { get; set; }
        public MainItemType ItemType { get; set; }
        public int Favorites { get; set; }
        public string Color { get; set; } = string.Empty;
        public string AbilityValue { get; set; } = string.Empty; // 능력치
        public DateTime ExpirationDate { get; set; }// 능력치
        public int Equipped { get; set; }
    }

    // 물약,포션 아이템(겹쳐지는 아이템)
    public class OverlapItem
    {
        public BaseItem BaseItem { get; set; } = null!;
        public ulong CharSeq { get; set; }
        public MainItemType ItemType { get; set; } 
        public int Favorites { get; set; }
        public string AbilityValue { get; set; } = string.Empty;       // 능력치	
        public DateTime ExpirationDate { get; set; }
    }

    /// 장비 아이템(시즌 아이템)
    public class SeasonUniqueItem
    {
        public BaseItem BaseItem { get; set; } = null!;
        public ulong CharSeq { get; set; }
        public GameModeType GameModeType { get; set; }
        public int Favorites { get; set; }
        public int Equipped { get; set; }
        public long DbStoredAmount { get; set; } // DB에 가지고 있는 값 보관중인 수량
        public long AddedStoredAmount { get; set; } // 메모리 버퍼에 가지고 있는 보관중인 수량
        public long SubStoredAmount { get; set; } // 메모리 버퍼에 제거된
        public long TotalStoredAmount => (DbStoredAmount + AddedStoredAmount - SubStoredAmount);
    }


    // 물약,포션 아이템(겹쳐지는 아이템)
    public class SeasonOverlapItem
    {
        public BaseItem BaseItem { get; set; } = null!;
        public ulong charSeq { get; set; }
        public GameModeType GameModeType { get; set; }
        public int Favorites { get; set; }
    }
    
}
