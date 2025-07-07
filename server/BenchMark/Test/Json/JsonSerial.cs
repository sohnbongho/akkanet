using Library.Data.Models;
using Library.DTO;
using Library.Repository.Log;
using Newtonsoft.Json;


namespace BenchMark.Test.Json;

public class JsonSerial
{
    public void Test()
    {
        var prevCurrencys = new List<CurrencyItem> {
            new CurrencyItem
            {
                CurrencyType = CurrencyType.Gold,
                Amount = 1,
            }
        };
        var currencys = new List<CurrencyItem> {
            new CurrencyItem
            {
                CurrencyType = CurrencyType.Gold,
                Amount = 2,
            }
        };
        //List< CurrencyItem > 

        var purchaseLog = new PurchaseLog
        {
            CurrencyLog = CurrencyLog.Of(prevCurrencys, currencys),
            ItemPrice = 1,
            PurchaseType = PurchaseType.Gift,
        };
        var jsonData = JsonConvert.SerializeObject(purchaseLog);

        Console.WriteLine($"jsonData :{jsonData}");

    }
}
