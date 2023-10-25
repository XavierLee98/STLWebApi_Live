using Dapper;
using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace StarLaiPortal.WebApi.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StockCountController : ControllerBase
    {
        private IConfiguration Configuration { get; }
        IObjectSpaceFactory objectSpaceFactory;
        ISecurityProvider securityProvider;
        public StockCountController(IConfiguration configuration, IObjectSpaceFactory objectSpaceFactory, ISecurityProvider securityProvider)
        {
            this.objectSpaceFactory = objectSpaceFactory;
            this.securityProvider = securityProvider;
            this.Configuration = configuration;
        }

        [HttpGet("startdate/enddate")]
        public IActionResult GetITRFrom(DateTime startdate, DateTime enddate)
        {
            try
            {
                ISecurityStrategyBase security = securityProvider.GetSecurity();
                var userId = security.UserId;
                var userName = security.UserName;
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { userName, startdate, enddate });

                    var val = conn.Query($"exec sp_getdatalist 'CountList', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }

            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
