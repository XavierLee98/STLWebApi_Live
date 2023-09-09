using Dapper;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.Authentication;
using DevExpress.ExpressApp.Security.Authentication.ClientServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StarLaiPortal.WebApi.Helper;
using Swashbuckle.AspNetCore.Annotations;
using System.Configuration;
using System.Data.SqlClient;

namespace StarLaiPortal.WebApi.JWT;

[ApiController]
[Route("api/[controller]")]
// This is a JWT authentication service sample.
public class AuthenticationController : ControllerBase {
    ISecurityProvider securityProvider;
    readonly IAuthenticationTokenProvider tokenProvider;
    public AuthenticationController(IAuthenticationTokenProvider tokenProvider, ISecurityProvider securityProvider)
    {
        this.tokenProvider = tokenProvider;
        this.securityProvider = securityProvider;
    }
    [HttpPost("Authenticate")]
    [SwaggerOperation("Checks if the user with the specified logon parameters exists in the database. If it does, authenticates this user.", "Refer to the following help topic for more information on authentication methods in the XAF Security System: <a href='https://docs.devexpress.com/eXpressAppFramework/119064/data-security-and-safety/security-system/authentication'>Authentication</a>.")]
    public IActionResult Authenticate(
        [FromBody]
        [SwaggerRequestBody(@"For example: <br /> { ""userName"": ""Admin"", ""password"": """"}")]
        AuthenticationStandardLogonParameters logonParameters
    ) {
        try {

            var token = tokenProvider.Authenticate(logonParameters);

            ISecurityStrategyBase security = securityProvider.GetSecurity();
            var userId = security.UserId;
            var userName = security.UserName;

            string connString = ConfigSettings.Conn;
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string json = JsonConvert.SerializeObject(new { userId = userId });
                var val = conn.Query<int>($"exec sp_getdatalist 'ValidateRole', '{json}'").FirstOrDefault();
                if (val <= 0) return Unauthorized("You do not have enough role or permission to login. Please contact your administrator for assistance.");
            }

            return Ok(token);
        }
        catch(AuthenticationException) {
            return Unauthorized("User name or password is incorrect.");
        }
    }

    [HttpGet("VersionChecking")]
    public IActionResult VersionChecking(string appVersion, string appName)
    {
        try
        {
            string connString = ConfigSettings.Conn;
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string json = JsonConvert.SerializeObject(new { appName = appName });
                var val = conn.Query($"exec sp_getdatalist 'AppVersion', '{json}'").FirstOrDefault();

                if (val == null) throw new Exception("Invalid AppName.");
                if (string.IsNullOrEmpty(val.AppName)) throw new Exception("Invalid AppName.");

                if(appVersion != val.AppVersion)
                {
                    return BadRequest($"App version is not updated. Please try to install latest version.({val.AppVersion})");
                }

                return Ok();
            }
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }
}
