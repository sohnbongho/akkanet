using Library.Data.Models;
using Library.DTO;

namespace Library.Repository.Log;

public class CurrencyLog
{
    public static CurrencyLog Of(List<CurrencyItem> prevCurrencys, List<CurrencyItem> currencys)
    {
        var totalMoney = currencys.FirstOrDefault(x => x.CurrencyType == CurrencyType.Gold)?.Amount ?? 0;
        var totalDiamond = currencys.FirstOrDefault(x => x.CurrencyType == CurrencyType.Diamond)?.Amount ?? 0;
        var totalClover = currencys.FirstOrDefault(x => x.CurrencyType == CurrencyType.Clover)?.Amount ?? 0;
        var totalBell = currencys.FirstOrDefault(x => x.CurrencyType == CurrencyType.Bell)?.Amount ?? 0;
        var totalColorTube = currencys.FirstOrDefault(x => x.CurrencyType == CurrencyType.ColorTube)?.Amount ?? 0;
        var totalPremiumMoney = currencys.FirstOrDefault(x => x.CurrencyType == CurrencyType.PremiumGold)?.Amount ?? 0;
        var totalPremiumDiamond = currencys.FirstOrDefault(x => x.CurrencyType == CurrencyType.PremiumDiamond)?.Amount ?? 0;

        var prevTotalMoney = prevCurrencys.FirstOrDefault(x => x.CurrencyType == CurrencyType.Gold)?.Amount ?? 0;
        var prevTotalDiamond = prevCurrencys.FirstOrDefault(x => x.CurrencyType == CurrencyType.Diamond)?.Amount ?? 0;
        var prevTotalClover = prevCurrencys.FirstOrDefault(x => x.CurrencyType == CurrencyType.Clover)?.Amount ?? 0;
        var prevTotalBell = prevCurrencys.FirstOrDefault(x => x.CurrencyType == CurrencyType.Bell)?.Amount ?? 0;
        var prevTotalColorTube = prevCurrencys.FirstOrDefault(x => x.CurrencyType == CurrencyType.ColorTube)?.Amount ?? 0;
        var prevTotalPremiumMoney = prevCurrencys.FirstOrDefault(x => x.CurrencyType == CurrencyType.PremiumGold)?.Amount ?? 0;
        var prevTotalPremiumDiamond = prevCurrencys.FirstOrDefault(x => x.CurrencyType == CurrencyType.PremiumDiamond)?.Amount ?? 0;

        var currencyLog = new CurrencyLog
        {
            PrevMoney = prevTotalMoney,
            Money = totalMoney,

            PrevDiamond = prevTotalDiamond,
            Diamond = totalDiamond,

            PrevClover = prevTotalClover,
            Clover = totalClover,

            PrevBell = prevTotalBell,
            Bell = totalBell,

            PrevColorTube = prevTotalColorTube,
            ColorTube = totalColorTube,

            PrevPremiumMoney = prevTotalPremiumMoney,
            PremiumMoney = totalPremiumMoney,

            PrevPremiumDiamond = prevTotalPremiumDiamond,
            PremiumDiamond = totalPremiumDiamond,
        };
        return currencyLog;

    }

    public long PrevMoney { get; set; }
    public long Money { get; set; }
    public long PrevDiamond { get; set; }
    public long Diamond { get; set; }

    public long PrevClover { get; set; }
    public long Clover { get; set; }

    public long PrevBell { get; set; }
    public long Bell { get; set; }

    public long PrevColorTube { get; set; }
    public long ColorTube { get; set; }

    public long PrevPremiumMoney { get; set; }
    public long PremiumMoney { get; set; }
    public long PrevPremiumDiamond { get; set; }
    public long PremiumDiamond { get; set; }
}

public interface ICurrencyLog
{
    CurrencyLog CurrencyLog { get; set; }
}

public class PurchaseLog : ICurrencyLog
{
    public long ItemPrice { get; set; }
    public PurchaseType PurchaseType { get; set; }
    public CurrencyLog CurrencyLog { get; set; } = null!;
}

public class MailLog 
{
    public int ItemId { get; set; }
    public int ItemAmount { get; set; }    
}

public class DailyStampLog : ICurrencyLog
{
    public int ItemSn { get; set; }
    public long Amount { get; set; }
    public int StampCount { get; set; }
    public int PayDiamond { get; set; }
    public int PayClover { get; set; }

    public CurrencyLog CurrencyLog { get; set; } = null!;
}

public class AddSnowLog
{
    public long DecreaseSnowPoint { get; set; }
    public long AddedMoney { get; set; }
    public long TotalMoney { get; set; }
}

public class AddGrowCloverLog
{
    public int ItemId { get; set; }
    public int AddedCloverLeafToClover { get; set; }
    
}

public class MoneyToBellLog : ICurrencyLog
{
    public long AddedBell { get; set; }
    public long DecreaseMoney { get; set; }
    public CurrencyLog CurrencyLog { get; set; } = null!;

}

public class DiamondToBellLog : ICurrencyLog
{
    public long AddedBell { get; set; }
    public long DecreaseDiamond { get; set; }
    public CurrencyLog CurrencyLog { get; set; } = null!;

}