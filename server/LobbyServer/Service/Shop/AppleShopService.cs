using Library.DTO;
using Library.Helper;
using Library.Logger;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace LobbyServer.Service.Shop;

public class AppleShopService
{
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    private readonly HttpClient _httpClient;
    public AppleShopService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    // Apple 영수증 응답을 위한 클래스
    private class AppleReceiptResponse
    {
        public int status { get; set; }
        public Receipt receipt { get; set; } = new Receipt();
        public string environment { get; set; } = string.Empty;
    }

    // 영수증 정보 클래스
    public class Receipt
    {
        public string bundle_id { get; set; } = string.Empty;
        public string application_version { get; set; } = string.Empty;
        public long receipt_creation_date_ms { get; set; }
        public long original_purchase_date_ms { get; set; }
        public List<InAppPurchase> in_app { get; set; } = new();
    }

    // 개별 인앱 구매 정보 클래스
    public class InAppPurchase
    {
        public string product_id { get; set; } = string.Empty; // ✅ 여기에 구매한 productId가 저장됨!
        public string transaction_id { get; set; } = string.Empty;
        public string original_transaction_id { get; set; } = string.Empty;
        public long purchase_date_ms { get; set; }
        public string cancellation_date_ms { get; set; } = string.Empty; // 환불 관련 필드 추가
    }

    public async Task<(int, List<string>)> VerifyReceiptAsync(VerifyAppleReceiptRequest request)
    {
        try
        {
            var (status, receiptResponse) = await FetchReceiptAsync(false, request); // 우선 제품 영역에 요청한다.
            if (status == 0)
            {
                // 성공하여 처리                
                var productIds = receiptResponse.receipt.in_app.Where(x => string.IsNullOrEmpty(x.cancellation_date_ms)).Select(x => x.product_id).ToList();
                return (status, productIds);
            }
            else if (status == 21007)
            {
                // 만약 제품에 위 코드면 sandbox영역에 한번더 요청하자
                var (status2, receiptResponse2) = await FetchReceiptAsync(true, request); // sendbox영역에 요청
                var productIds = receiptResponse2.receipt.in_app.Where(x => string.IsNullOrEmpty(x.cancellation_date_ms)).Select(x => x.product_id).ToList();
                return (status2, productIds);
            }

            return (status, new List<string>());
        }
        catch (Exception ex)
        {
            _logger.Error($"Error VerifyReceiptAsync.", ex);
            return (-1, new List<string>());
        }
    }
    private async Task<(int, AppleReceiptResponse)> FetchReceiptAsync(bool isSandbox, VerifyAppleReceiptRequest request)
    {
        var receiptData = request.ReceiptData; // Base64 인코딩된 영수증 데이터
        var userDeviceType = request.UserDeviceType;
        var sharedSecret = AppleConstHelper.AppSharedSecret;

        var endpoint = isSandbox
            ? "https://sandbox.itunes.apple.com/verifyReceipt"
            : "https://buy.itunes.apple.com/verifyReceipt";

        try
        {
            // 요청 데이터 생성
            var jsonContent = new JObject(new JProperty("exclude-old-transactions", true),
                new JProperty("receipt-data", receiptData),
                new JProperty("password", sharedSecret)).ToString();

            var content = new StringContent(jsonContent);

            // API 호출
            var response = await _httpClient.PostAsync(endpoint, content); // 

            if (false == response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                string loggedContent = errorContent.Length > 500 ? errorContent.Substring(0, 500) + "..." : errorContent;

                _logger.Warn($"Apple API call failed. Status: {response.StatusCode}, Content: {loggedContent} Reuqest:{jsonContent} ");
                return (-1, new AppleReceiptResponse());
            }
            var responseString = await response.Content.ReadAsStringAsync();

            _logger.DebugEx(() => $"endpoint:{endpoint} VerifyReceiptAsync Response:{responseString} ");

            if (string.IsNullOrEmpty(responseString))
            {
                _logger.Warn("VerifyReceiptAsync No response.");
                return (-1, new AppleReceiptResponse());
            }

            // 응답 JSON 파싱
            var receiptResponse = JsonConvert.DeserializeObject<AppleReceiptResponse>(responseString);
            var statusCode = receiptResponse?.status ?? -1;
            return (statusCode, receiptResponse ?? new AppleReceiptResponse());

        }
        catch (Exception ex)
        {
            _logger.Warn("FetchReceiptAsync failed .", ex);
            return (-1, new AppleReceiptResponse());

        }

    }
}
