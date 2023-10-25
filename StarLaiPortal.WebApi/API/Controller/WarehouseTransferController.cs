using Dapper;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp.Security;
using DevExpress.Xpo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StarLaiPortal.Module.BusinessObjects;
using StarLaiPortal.Module.BusinessObjects.Setup;
using StarLaiPortal.Module.BusinessObjects.View;
using StarLaiPortal.Module.BusinessObjects.Warehouse_Transfer;
using StarLaiPortal.Module.Controllers;
using StarLaiPortal.WebApi.Helper;
using StarLaiPortal.WebApi.Model;
using System.Data.SqlClient;
using System.Dynamic;
using System.Text.Json.Nodes;

namespace StarLaiPortal.WebApi.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WarehouseTransferController : ControllerBase
    {
        private IConfiguration Configuration { get; }
        IObjectSpaceFactory objectSpaceFactory;
        ISecurityProvider securityProvider;
        public WarehouseTransferController(IConfiguration configuration, IObjectSpaceFactory objectSpaceFactory, ISecurityProvider securityProvider)
        {
            this.objectSpaceFactory = objectSpaceFactory;
            this.securityProvider = securityProvider;
            this.Configuration = configuration;
        }

        [HttpGet("from/startdate/enddate")]
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

                    var val = conn.Query($"exec sp_getdatalist 'WTRequestList', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }

            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("Request/oid")]
        public IActionResult GetITDetailReq(int oid)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { oid });

                    var val = conn.Query($"exec sp_getdatalist 'TransferDetailsReq', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("oid")]
        public IActionResult GetITDetail(int oid)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { oid });

                    var val = conn.Query($"exec sp_getdatalist 'WarehouseTransferDetails', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("to/startdate/enddate")]
        public IActionResult GetITRTo(DateTime startdate, DateTime enddate)
        {
            try
            {
                ISecurityStrategyBase security = securityProvider.GetSecurity();
                var userId = security.UserId;
                var userName = security.UserName;
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { userName, startdate, enddate });

                    var val = conn.Query($"exec sp_getdatalist 'WTDraftList', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }

            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }


        [HttpPost("PostDraft")]
        public IActionResult PostDraft([FromBody] ExpandoObject obj)
        {
            try
            {
                dynamic dynamicObj = obj;

                IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<WarehouseTransfers>();
                ISecurityStrategyBase security = securityProvider.GetSecurity();

                var detailsObject = (IEnumerable<dynamic>)dynamicObj.WarehouseTransferDetails;
                if (detailsObject == null || detailsObject.Count() <= 0)
                    return Problem("Warehouse Transfer Details are null.");

                string ReqDocnum = string.Empty;

                WarehouseTransferReq req = newObjectSpace.FindObject<WarehouseTransferReq>(CriteriaOperator.Parse("Oid = ?", dynamicObj.BaseId));
                ReqDocnum = req.DocNum;

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

                var userId = security.UserId;
                var userName = security.UserName;

                WarehouseTransfers curobj = null;
                curobj = newObjectSpace.CreateObject<WarehouseTransfers>();
                ExpandoParser.ParseExObjectXPO<WarehouseTransfers>(obj, curobj, newObjectSpace);

                curobj.Picker = userName;
                curobj.CreateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                curobj.UpdateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                curobj.WarehouseTransferReqNo = ReqDocnum;
                curobj.TransferType = TransferType.WT;

                foreach (var dtl in curobj.WarehouseTransferDetails)
                {
                    dtl.CreateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                    dtl.UpdateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                }

                curobj.Save();

                var companyPrefix = CompanyCommanHelper.GetCompanyPrefix(dynamicObj.companyDB);

                GeneralControllers con = new GeneralControllers();
                curobj.DocNum = con.GenerateDocNum(DocTypeList.WT, objectSpaceFactory.CreateObjectSpace<DocTypes>(), TransferType.WT, 0, companyPrefix);
                newObjectSpace.CommitChanges();

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { username = userName, reqOid = dynamicObj.BaseId });
                    conn.Query($"exec sp_afterdatasave 'WTDraft', '{json}'");
                }

                return Ok(new { oid = curobj.Oid, docnum = curobj.DocNum });
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpPost("Post")]
        public IActionResult Post([FromBody] ExpandoObject obj)
        {
            try
            {
                dynamic dynamicObj = obj;

                IObjectSpace objectSpace = objectSpaceFactory.CreateObjectSpace<WarehouseTransfers>();
                WarehouseTransfers transfers = objectSpace.FindObject<WarehouseTransfers>(CriteriaOperator.Parse("Oid = ?", dynamicObj.Oid));

                if (transfers.Status != DocStatus.Draft)
                {
                    return Problem($"Update Failed. Warehouse Transfer No.{transfers.DocNum} already {transfers.Status}.");
                }

                ISecurityStrategyBase security = securityProvider.GetSecurity();
                var userId = security.UserId;
                var userName = security.UserName;

                if (dynamicObj.WarehouseTransferDetails != null && ((IEnumerable<dynamic>)dynamicObj.WarehouseTransferDetails).Count() > 0)
                {
                    foreach (dynamic dtl in dynamicObj.WarehouseTransferDetails)
                    {
                        WarehouseTransferDetails detail = transfers.WarehouseTransferDetails.FirstOrDefault(line => line.Oid == dtl.Oid);
                        if (detail != null)
                        {
                            if (detail.FromBin.BinCode.Equals(dtl.ToBin))
                            {
                                return Problem($"[{detail.ItemCode.ItemCode}]\nTo Bin cannot same as From Bin. Please select other bin to receive.");
                            }

                            using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                            {
                                string json = JsonConvert.SerializeObject(new { itemcode = detail.ItemCode.ItemCode, bincode = detail.FromBin.BinCode, quantity = detail.Quantity });

                                var validateBalance = conn.Query<ValidateJson>($"exec sp_beforedatasave 'ValidateStockBalance', '{json}'").FirstOrDefault();
                                if (validateBalance.Error)
                                {
                                    return Problem(validateBalance.ErrorMessage);
                                }
                            }

                            detail.ToWarehouse = objectSpace.GetObjectByKey<vwWarehouse>(dtl.ToWarehouse);
                            detail.ToBin = objectSpace.GetObjectByKey<vwBin>(dtl.ToBin);
                        }
                    }
                }
                else
                {
                    return Problem("Warehouse Transfer Details are null.");
                }

                objectSpace.CommitChanges();

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { oid = transfers.Oid, username = userName });
                    conn.Query($"exec sp_afterdatasave 'WTPost', '{json}'");
                }

                return Ok(new { successful = 1 });
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("CancelTransfer/oid")]
        public IActionResult CancelTransfer(int oid)
        {
            try
            {
                IObjectSpace objectSpace = objectSpaceFactory.CreateObjectSpace<WarehouseTransfers>();
                WarehouseTransfers doc = objectSpace.FindObject<WarehouseTransfers>(CriteriaOperator.Parse("Oid = ?", oid));

                if (doc.Status != DocStatus.Draft)
                {
                    return Problem($"Cancel Failed. Warehouse Transfer No.{doc.DocNum} already {doc.Status}.");
                }

                ISecurityStrategyBase security = securityProvider.GetSecurity();
                var userId = security.UserId;
                var userName = security.UserName;

                doc.Status = DocStatus.Cancelled;

                objectSpace.CommitChanges();

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { oid = oid, username = userName });
                    conn.Query($"exec sp_afterdatasave 'CancelTransfer', '{json}'");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
