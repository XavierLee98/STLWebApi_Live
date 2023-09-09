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
using Newtonsoft.Json.Linq;
using StarLaiPortal.Module.BusinessObjects.Pick_List;
using StarLaiPortal.WebApi.Helper;
using System.Dynamic;
using DevExpress.Xpo;
using StarLaiPortal.Module.BusinessObjects.Warehouse_Transfer;
using StarLaiPortal.Module.Controllers;
using StarLaiPortal.Module.BusinessObjects.Setup;

namespace StarLaiPortal.WebApi.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PutAwayController : ControllerBase
    {
        private IConfiguration Configuration { get; }
        IObjectSpaceFactory objectSpaceFactory;
        ISecurityProvider securityProvider;
        public PutAwayController(IConfiguration configuration, IObjectSpaceFactory objectSpaceFactory, ISecurityProvider securityProvider)
        {
            this.objectSpaceFactory = objectSpaceFactory;
            this.securityProvider = securityProvider;
            this.Configuration = configuration;
        }

        [HttpPost("Post")]
        public IActionResult PostDraft([FromBody] ExpandoObject obj)
        {
            try
            {
                dynamic dynamicObj = obj;

                var detailsObject = (IEnumerable<dynamic>)dynamicObj.WarehouseTransferDetails;
                if (detailsObject == null || detailsObject.Count() <= 0)
                    return Problem("Put Away Details are null.");

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    foreach (var itemline in detailsObject)
                    {
                        string json = JsonConvert.SerializeObject(new { itemcode = itemline.ItemCode, bincode = itemline.FromBin, quantity = itemline.Quantity });

                        var validateBalance = conn.Query<ValidateJson>($"exec sp_beforedatasave 'ValidateStockBalance', '{json}'").FirstOrDefault();
                        if (validateBalance.Error)
                        {
                            return Problem(validateBalance.ErrorMessage);
                        }
                    }
                }

                IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<WarehouseTransfers>();
                ISecurityStrategyBase security = securityProvider.GetSecurity();
                var userId = security.UserId;
                var userName = security.UserName;

                WarehouseTransfers curobj = null;
                curobj = newObjectSpace.CreateObject<WarehouseTransfers>();
                ExpandoParser.ParseExObjectXPO<WarehouseTransfers>(obj, curobj, newObjectSpace);

                curobj.Picker = userName;
                curobj.CreateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                curobj.UpdateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                curobj.TransferType = TransferType.PW;
                curobj.Status = DocStatus.Draft;

                foreach (var dtl in curobj.WarehouseTransferDetails)
                {
                    dtl.CreateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                    dtl.UpdateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                }

                curobj.Save();

                var companyPrefix = CompanyCommanHelper.GetCompanyPrefix(dynamicObj.companyDB);

                GeneralControllers con = new GeneralControllers();
                curobj.DocNum = con.GenerateDocNum(DocTypeList.WT, objectSpaceFactory.CreateObjectSpace<DocTypes>(), TransferType.PW, 0, companyPrefix);
                newObjectSpace.CommitChanges();

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { oid = curobj.Oid, username = userName });
                    conn.Query($"exec sp_afterdatasave 'WTPost', '{json}'");
                }

                return Ok(new { oid = curobj.Oid, docnum = curobj.DocNum });
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("startdate/enddate")]
        public IActionResult Get(DateTime startdate, DateTime enddate)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    ISecurityStrategyBase security = securityProvider.GetSecurity();
                    var userId = security.UserId;
                    var userName = security.UserName;

                    string json = JsonConvert.SerializeObject(new { userName, startdate, enddate });

                    var val = conn.Query($"exec sp_getdatalist 'PWDraftList', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("oid")]
        public IActionResult Get(int oid)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { oid });

                    var val = conn.Query($"exec sp_getdatalist 'TransferDetails', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("BinCode")]
        public IActionResult GetAvaillableItem(string bincode)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { bincode = bincode });

                    var val = conn.Query($"exec sp_getdatalist 'GetAvaillableItemFromBin', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
        //[HttpPost("Post")]
        //public IActionResult Post([FromBody] ExpandoObject obj)
        //{
        //    try
        //    {
        //        dynamic dynamicObj = obj;

        //        IObjectSpace objectSpace = objectSpaceFactory.CreateObjectSpace<WarehouseTransfers>();
        //        WarehouseTransfers transfers = objectSpace.FindObject<WarehouseTransfers>(CriteriaOperator.Parse("Oid = ?", dynamicObj.Oid));

        //        ISecurityStrategyBase security = securityProvider.GetSecurity();
        //        var userId = security.UserId;
        //        var userName = security.UserName;


        //        if (dynamicObj.WarehouseTransferDetails != null)
        //        {
        //            foreach (dynamic dtl in dynamicObj.WarehouseTransferDetails)
        //            {
        //                WarehouseTransferDetails detail = transfers.WarehouseTransferDetails.FirstOrDefault(line => line.Oid == dtl.Oid);
        //                if (detail != null)
        //                {
        //                    detail.ToBin = objectSpace.GetObjectByKey<vwBin>(dtl.ToBin);
        //                }
        //            }
        //        }

        //        objectSpace.CommitChanges();

        //        using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
        //        {
        //            string json = JsonConvert.SerializeObject(new { oid = transfers.Oid, username = userName });
        //            conn.Query($"exec sp_afterdatasave 'WTPost', '{json}'");
        //        }

        //        return Ok(new { oid = transfers.Oid, docnum = transfers.DocNum });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Problem(ex.Message);
        //    }
        //}

        //[HttpGet("CancelTransfer/oid")]
        //public IActionResult CancelTransfer(int oid)
        //{
        //    try
        //    {
        //        IObjectSpace objectSpace = objectSpaceFactory.CreateObjectSpace<WarehouseTransfers>();
        //        WarehouseTransfers doc = objectSpace.FindObject<WarehouseTransfers>(CriteriaOperator.Parse("Oid = ?", oid));

        //        ISecurityStrategyBase security = securityProvider.GetSecurity();
        //        var userId = security.UserId;
        //        var userName = security.UserName;

        //        doc.Status = DocStatus.Cancelled;

        //        objectSpace.CommitChanges();

        //        using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
        //        {
        //            string json = JsonConvert.SerializeObject(new { oid = oid, username = userName });
        //            conn.Query($"exec sp_afterdatasave 'CancelTransfer', '{json}'");
        //        }

        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        return Problem(ex.Message);
        //    }
        //}
    }
}
