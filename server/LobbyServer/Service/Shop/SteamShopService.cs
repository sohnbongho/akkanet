
using Library.DTO;
using Library.Logger;
using log4net;
using Newtonsoft.Json;
using System.Reflection;

namespace LobbyServer.Service.Shop;

public class SteamShopService
{
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    private static readonly string SteamApiKey = "39B772E34B223997E9C446CF490E15F0"; // 여기에 API 키 입력
    private static readonly string SteamAppId = "3579110"; // 여기에 앱 ID 입력        

    /// <summary>
    /// Transaction (응답)초기화
    /// </summary>
    public class InitTransactionParams
    {
        [JsonProperty("orderid")]
        public ulong Orderid;

        [JsonProperty("transid")]
        public ulong Transid;
    }
    public class InitTransactionResult
    {
        [JsonProperty("result")]
        public string Result = string.Empty;

        [JsonProperty("params")]
        public InitTransactionParams Params { get; set; } = new InitTransactionParams();

    }
    public class InitTransactionResponse
    {
        [JsonProperty("response")]
        public InitTransactionResult Result { get; set; } = new InitTransactionResult();
    }

    public static async Task<(ErrorCode, InitTransactionResponse)> InitTransactionAsync(HttpClient httpClient, ulong orderId, SteamInitTransactionRequest request)
    {
        var isSandBox = request.IsSandBox;
        var steamApiUrl = isSandBox ? "https://partner.steam-api.com/ISteamMicroTxnSandbox/InitTxn/v3/"
            : "https://partner.steam-api.com/ISteamMicroTxn/InitTxn/v3/";
        var responseBody = string.Empty;

        try
        {
            var requestData = new Dictionary<string, string>
            {
                { "key", SteamApiKey },
                { "appid", SteamAppId },
                { "orderid", orderId.ToString() }, // 고유 주문번호
                { "steamid", request.UserSteamId }, // 구매자의 Steam ID
                { "itemcount", request.ItemCount.ToString() }, // 구매할 아이템 개수
                { "language", request.Language }, // 언어 설정 (한국어: "ko")
                { "currency", request.Currency }, // 결제 통화 (예: "USD", "KRW")
                { "itemid[0]", request.ItemId.ToString() }, // 아이템 ID (스토어에서 관리)
                { "qty[0]", request.Qty.ToString() }, // 아이템 수량
                { "amount[0]", request.Amount }, // 가격 (소수점 없이, 100원이면 "100")
                { "description[0]", request.Description }, // 아이템 설명
                { "user[0]", request.UserSteamId } // 구매자 Steam ID (중복 필요)
            };

            var requestContent = new FormUrlEncodedContent(requestData);


            HttpResponseMessage response = await httpClient.PostAsync(steamApiUrl, requestContent);
            responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.DebugEx(() => $"Steam API error StatusCode:{response.StatusCode}, response:{responseBody}");
                return (ErrorCode.InvalildBuyItem, new InitTransactionResponse());
            }
            response.EnsureSuccessStatusCode();

            _logger.DebugEx(() => $"Steam API Response: {responseBody}");

            var jsonResponse = JsonConvert.DeserializeObject<InitTransactionResponse>(responseBody);
            if (jsonResponse != null && jsonResponse.Result.Result.Equals("OK"))
            {
                _logger.DebugEx(() => "✅ Successd Transaction");
                return (ErrorCode.Succeed, jsonResponse);
            }
            else
            {
                _logger.DebugEx(() => "failed init transaction!");
                return (ErrorCode.InvalildBuyItem, new InitTransactionResponse());
            }
        }
        catch (Exception ex)
        {
            _logger.DebugEx(() => $"Exception init transaction responseBody:{responseBody} msg:{ex.Message}");
            return (ErrorCode.InvalildBuyItem, new InitTransactionResponse());
        }
    }

    /// <summary>
    /// 스팀 아이템 구매 확정
    /// </summary>
    public class FinalizeTransactionParams
    {
        [JsonProperty("orderid")]
        public ulong Orderid;

        [JsonProperty("transid")]
        public ulong Transid;
    }
    public class FinalizeTransactionResult
    {
        [JsonProperty("result")]
        public string Result = string.Empty;

        [JsonProperty("params")]
        public FinalizeTransactionParams Params { get; set; } = new FinalizeTransactionParams();
    }
    public class FinalizeTransactionResponse
    {
        [JsonProperty("response")]
        public FinalizeTransactionResult Result { get; set; } = new FinalizeTransactionResult();
    }

    public static async Task<ErrorCode> FinalizeTransactionAsync(HttpClient httpClient, bool isSandBox, string orderid)
    {
        string finalizeTxnUrl = isSandBox ? "https://partner.steam-api.com/ISteamMicroTxnSandbox/FinalizeTxn/v2/"
            : "https://partner.steam-api.com/ISteamMicroTxn/FinalizeTxn/v2/";
        var requestData = new Dictionary<string, string>
            {
                { "key", SteamApiKey },
                { "appid", SteamAppId },
                { "orderid", orderid } // 트랜잭션 ID
            };

        var requestContent = new FormUrlEncodedContent(requestData);
        var responseBody = string.Empty;
        try
        {
            var response = await httpClient.PostAsync(finalizeTxnUrl, requestContent);

            responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.DebugEx(() => $"Steam API error StatusCode:{response.StatusCode}, response:{responseBody}");
                return ErrorCode.InvalildBuyItem;
            }
            response.EnsureSuccessStatusCode();

            _logger.DebugEx(() => $"Steam API Response: {responseBody}");

            var jsonResponse = JsonConvert.DeserializeObject<FinalizeTransactionResponse>(responseBody);
            if (jsonResponse != null && jsonResponse.Result.Result.Equals("OK"))
            {
                _logger.DebugEx(() => "✅ 트랜잭션 최종 승인 성공!");
                return ErrorCode.Succeed;
            }

            _logger.DebugEx(() => $"❌ fail transaction :{responseBody}");
            return ErrorCode.InvalildBuyItem;
        }
        catch (Exception ex)
        {
            _logger.DebugEx(() => $"⚠️ Exception transaction responseBody:{responseBody} message:{ex.Message}");
            return ErrorCode.InvalildBuyItem;
        }
    }

}
