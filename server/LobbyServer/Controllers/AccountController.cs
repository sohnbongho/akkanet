using Library.DTO;
using Library.Logger;
using LobbyServer.Service;
using log4net;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace LobbyServer.Controllers;

[ApiController]
[Route("Account")]
public class AccountController : CommonController
{
    private IAccountService _service;
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    public AccountController(IAccountService service)
    {
        _service = service;
    }

    /// <summary>
    /// 계정 탈퇴 (비활성화)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("Deactivate")]
    public async Task<IActionResult> Deactivate([FromBody] AccountDeactivateRequest request)
    {
        var response = await _service.Deactivate(request);
        if (response.ErrorCode != (int)ErrorCode.Succeed)
        {
            _logger.WarnWithContentError((int)response.ErrorCode, request, response);
            return BadRequest(response);
        }
        _logger.DebugEx(request, response);
        return Ok(response);
    }
}
