using Google.Apis.AndroidPublisher.v3;
using Google.Apis.Services;
using Library.Connector;
using Library.DTO;
using log4net;
using System.Reflection;

namespace LobbyServer.Service.Shop;

public class GoogleShopService
{
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    public async Task<(ErrorCode, List<string>)> VerifyReceiptAsync(string applicationName, string packageName, string productId, string googlePurchaseToken)
    {
        try
        {
            var credential = GoogleCredentialConnector.Instance;
            using (var service = new AndroidPublisherService(new BaseClientService.Initializer() { HttpClientInitializer = credential, ApplicationName = applicationName, }))
            {
                // 여기서 API 호출을 진행합니다. 예를 들어, 구매 정보 확인
                var purchaseRequest = service.Purchases.Products.Get(packageName, productId, googlePurchaseToken);
                var purchase = await purchaseRequest.ExecuteAsync();
            }
            return (ErrorCode.Succeed, new List<string> { productId });
        }
        catch (Google.GoogleApiException e)
        {
            // Google API 호출 중 오류 처리
            _logger.Error("VerifyReceipt An error occurred: " + e.Message);
            if (e.InnerException != null)
            {
                _logger.Error("VerifyReceipt Inner exception: " + e.InnerException.Message);
            }
            return (ErrorCode.ExceptionGoogleApi, new List<string>());
        }
        catch (Exception ex)
        {
            _logger.Error("fail to VerifyReceipt", ex);
            return (ErrorCode.DbInsertedError, new List<string>());
        }

    }
}
