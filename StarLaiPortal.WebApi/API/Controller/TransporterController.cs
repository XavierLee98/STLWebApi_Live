using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp;
using Newtonsoft.Json;
using System.Data.SqlClient;
using Dapper;
using StarLaiPortal.WebApi.Model;
using DevExpress.ExpressApp.Security;
using StarLaiPortal.Module.BusinessObjects.View;
using StarLaiPortal.Module.BusinessObjects;
using StarLaiPortal.Module.BusinessObjects.Sales_Order;
using DevExpress.Data.Filtering;

namespace StarLaiPortal.WebApi.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransporterController : ControllerBase
    {
        private IConfiguration Configuration { get; }
        IObjectSpaceFactory objectSpaceFactory;
        ISecurityProvider securityProvider;
        public TransporterController(IConfiguration configuration, IObjectSpaceFactory objectSpaceFactory, ISecurityProvider securityProvider)
        {
            this.objectSpaceFactory = objectSpaceFactory;
            this.securityProvider = securityProvider;
            this.Configuration = configuration;
        }
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                //using IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<vwWarehouse>();
                //ISecurityStrategyBase security = securityProvider.GetSecurity();
                //var userId = security.UserId;
                //var userName = security.UserName;
                //ApplicationUser user = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);

                //List<vwWarehouse> obj = newObjectSpace.GetObjects<vwWarehouse>(new BinaryOperator("Inactive", "N", BinaryOperatorType.Equal)).ToList();
                //var rtn = obj.Select(pp => new { WarehouseCode = pp.WarehouseCode, WarehouseName = pp.WarehouseName });
                ////return Ok(rtn.ToList());
                //string json = JsonConvert.SerializeObject(rtn, Formatting.Indented);
                //return Ok(json);
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    var val = conn.Query("exec sp_getdatalist 'Transporter'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
        [HttpGet("code")]
        public IActionResult Get(string code)
        {
            try
            {
                //using IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<vwWarehouse>();
                //ISecurityStrategyBase security = securityProvider.GetSecurity();
                //var userId = security.UserId;
                //var userName = security.UserName;
                //ApplicationUser user = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);

                //List<vwWarehouse> obj = newObjectSpace.GetObjects<vwWarehouse>(CriteriaOperator.Parse("WarehouseCode=?", code)).ToList();
                //var rtn = obj.Select(pp => new { WarehouseCode = pp.WarehouseCode, WarehouseName = pp.WarehouseName }).FirstOrDefault();
                ////return Ok(rtn.ToList());
                //string json = JsonConvert.SerializeObject(rtn, Formatting.Indented);
                //return Ok(json);
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { code = code });
                    var val = conn.Query($"exec sp_getdatalist 'Transporter', '{json}'").ToList().FirstOrDefault();
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
