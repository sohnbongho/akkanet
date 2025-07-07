using Library.DTO;
using Library.Helper;
using Library.Logger;
using LobbyServer.Helper;
using log4net;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace LobbyServer.Service.Shop;

public class OneStoreShopService
{
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    private class ReceiptResult
    {
        [JsonProperty("code")]
        public string Code { get; set; } = string.Empty;

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

    }


    private class ReceiptResponse
    {
        [JsonProperty("result")]
        public ReceiptResult Result { get; set; } = new ReceiptResult();
    }

    public async Task<(ErrorCode, List<string>)> VerifyReceiptAsync(HttpClient httpClient, string hostName, string packageName, string productId, string purchaseToken, bool isGlobal)
    {
        var resContent = string.Empty;
        try
        {
            var tokenManager = OneStoreAccessTokenManager.Instance;
            var accessToken = await tokenManager.FetchAccessTokenAsync(httpClient, hostName, isGlobal);
            if (string.IsNullOrEmpty(accessToken))
            {
                return (ErrorCode.NotFondAccessToken, new List<string>());
            }

            var marketCode = isGlobal ? ConstInfo.OneStoreGlobalMarketCode : ConstInfo.OneStoreMarketCode;
            var fullHostName = OneStoreUrlCollection.ConsumePurchase(hostName, packageName, productId, purchaseToken);
            var request = new HttpRequestMessage(HttpMethod.Post, fullHostName);

            // 요청 본문 설정
            var requestData = new
            {
                packageName = packageName,
                productId = productId,
                purchaseToken = purchaseToken,
                grant_type = "client_credentials"
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
            request.Content = jsonContent;

            // ✅ 요청 헤더 추가
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Headers.Add("Authorization", accessToken);
            request.Headers.Add("x-market-code", marketCode); // ✅ 마켓 코드 추가            

            // HTTP 응답 처리
            var response = await httpClient.SendAsync(request);

            resContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode == false)
            {
                _logger.DebugEx(() => $"VerifyReceiptAsync 400 ERROR resContent:{resContent} packageName:{packageName} productId:{productId} purchaseToken:{purchaseToken}");
                return (ErrorCode.InvalildBuyItem, new List<string>());
            }
            response.EnsureSuccessStatusCode(); // 오류 시 예외 발생

            var contentResponse = JsonConvert.DeserializeObject<ReceiptResponse>(resContent);
            if (contentResponse == null)
            {
                _logger.DebugEx(() => $"VerifyReceiptAsync (contentResponse is null) packageName:{packageName} productId:{productId} purchaseToken:{purchaseToken}");
                return (ErrorCode.InvalildBuyItem, new List<string>());
            }

            if (contentResponse.Result.Code.Equals("Success"))
            {
                //_logger.DebugEx(() => $"Success VerifyReceiptAsync resContent:{resContent} packageName:{packageName} productId:{productId} purchaseToken:{purchaseToken}");

                // 구매 성공
                return (ErrorCode.Succeed, new List<string> { productId });
            }
            _logger.DebugEx(() => $"failed VerifyReceiptAsync ResCode:{contentResponse.Result.Code} resContent:{resContent} packageName:{packageName} productId:{productId} purchaseToken:{purchaseToken}");
            return (ErrorCode.InvalildBuyItem, new List<string>());

        }
        catch (Exception ex)
        {
            _logger.DebugEx($"Exception VerifyReceiptAsync resContent:{resContent} packageName:{packageName} productId:{productId} purchaseToken:{purchaseToken}", ex);
            return (ErrorCode.InvalildBuyItem, new List<string>());
        }

    }
}
