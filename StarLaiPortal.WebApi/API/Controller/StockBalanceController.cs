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
    public class StockBalanceController : ControllerBase
    {
        private IConfiguration Configuration { get; }
        IObjectSpaceFactory objectSpaceFactory;
        ISecurityProvider securityProvider;
        public StockBalanceController(IConfiguration configuration, IObjectSpaceFactory objectSpaceFactory, ISecurityProvider securityProvider)
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
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    var val = conn.Query("exec sp_getdatalist 'StockBalance'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        //[HttpGet("page:int/rows:int")]
        //public IActionResult GetPage(int page, int rows)
        //{
        //    try
        //    {
        //        using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
        //        {
        //            string json = "";
        //            json = JsonConvert.SerializeObject(new { page = page, rows = rows });
        //            var val = conn.Query($"exec sp_getdatalist 'StockBalance', '{json}'").ToList();
        //            return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Problem(ex.Message);
        //    }
        //}

        [HttpGet("itemcode/bincode")]
        public IActionResult Get(string itemcode, string bincode)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = "";
                    json = JsonConvert.SerializeObject(new { itemcode = itemcode, bincode = bincode });
                    var val = conn.Query($"exec sp_getdatalist 'StockBalance', '{json}'").ToList().FirstOrDefault();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }

            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
        [HttpGet("itemcode")]
        public IActionResult Getitem(string itemcode)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = "";
                    json = JsonConvert.SerializeObject(new { itemcode = itemcode });
                    var val = conn.Query($"exec sp_getdatalist 'StockBalance', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }

            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("itemcode/whscode")]
        public IActionResult Getitem(string itemcode, string whscode)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = "";
                    json = JsonConvert.SerializeObject(new { itemcode = itemcode, whscode = whscode });
                    var val = conn.Query($"exec sp_getdatalist 'StockBalance', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }

            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("bincode")]
        public IActionResult GetBin(string bincode)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = "";
                    json = JsonConvert.SerializeObject(new { bincode = bincode });
                    var val = conn.Query($"exec sp_getdatalist 'StockBalance', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }

            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("barcode")]
        public IActionResult GetItemUOM(string code)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = "";
                    json = JsonConvert.SerializeObject(new { code = code });
                    var val = conn.Query($"exec sp_getdatalist 'ItemBarCode', '{json}'").ToList();
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
