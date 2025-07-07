using Akka.Actor;
using Akka.TestKit.NUnit;
using Library.AkkaActors.MessageQueue;
using Library.DTO;
using Library.Helper;
using LobbyServer.Component;
using LobbyServer.Controllers;
using LobbyServer.Helper;
using LobbyServer.Repository.Alarm;
using LobbyServer.Repository.Article;
using LobbyServer.Service;
using LobbyServer.World;
using Messages;
using Microsoft.AspNetCore.Mvc;

namespace ApiServer.Tests.LobbyServer
{
    [TestFixture]
    public partial class LobbyControllerTest : TestKit
    {
        [Test]
        public void Should_AddedArticle()
        {
            var repoFacade = new ArticleRepo();
            var alarmRepo = new AlarmRepo();
            var components = new AlarmComponent(alarmRepo);
            var componentFacade = new ComponentFacade(components);
            var service = new ArticleService();
            var controller = new ArticleController(repoFacade, componentFacade, service);

            // 잘못된 파라미터
            {
                var request = new ArticleAddedRequest
                {
                };
                var rtn = controller.AddedArticle(request).Result as BadRequestObjectResult;
                var response = rtn.Value as ArticleAddedResponse;
                var errorCode = response.ErrorCode;
                Assert.That(errorCode, Is.EqualTo((int)ErrorCode.NotFoundCharBySeesionGuid));
            }

            {
                var hashTags = new List<string>();
                hashTags.Add("#test1");
                hashTags.Add("#test2");
                hashTags.Add("#test3");

                var userTags = new List<string>();                


                var mentions = new List<string>();
                mentions.Add("@9f734297-4b3e-45d1-ad82-01a5b433c38f");

                var request = new ArticleAddedRequest
                {                    
                    JsonData = "{}",
                    HashTags = string.Join(",", hashTags),
                    Mentions = string.Join(",", mentions),
                    UserSeqTags = string.Join(",", userTags),
                };

                var rtn = controller.AddedArticle(request).Result as OkObjectResult;
                var response = rtn.Value as ArticleAddedResponse;
                var errorCode = response.ErrorCode;
                Assert.That(errorCode, Is.EqualTo((int)ErrorCode.Succeed));
            }
        }        
    }
}
