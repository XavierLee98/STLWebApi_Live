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
using StarLaiPortal.Module.BusinessObjects.Purchase_Return;
using StarLaiPortal.Module.BusinessObjects.Setup;
using StarLaiPortal.Module.BusinessObjects.View;
using StarLaiPortal.Module.Controllers;
using StarLaiPortal.WebApi.Helper;
using StarLaiPortal.WebApi.Model;
using System.Data.SqlClient;
using System.Dynamic;

namespace StarLaiPortal.WebApi.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PurchaseReturnController : ControllerBase
    {
        private IConfiguration Configuration { get; }
        IObjectSpaceFactory objectSpaceFactory;
        ISecurityProvider securityProvider;
        public PurchaseReturnController(IConfiguration configuration, IObjectSpaceFactory objectSpaceFactory, ISecurityProvider securityProvider)
        {
            this.objectSpaceFactory = objectSpaceFactory;
            this.securityProvider = securityProvider;
            this.Configuration = configuration;
        }

        [HttpGet("startdate/enddate")]
        public IActionResult Get(DateTime startdate, DateTime enddate)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { startdate, enddate });
                    var val = conn.Query($"exec sp_getdatalist 'PurchaseReturnRequestList', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("Reason")]
        public IActionResult GetReasonList()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { type = "PurchaseReturn" });
                    var val = conn.Query($"exec sp_getdatalist 'ReasonList', '{json}'").ToList();
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

                    var val = conn.Query($"exec sp_getdatalist 'PurchaseReturnRequestDetails', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpPost()]
        public IActionResult Post([FromBody] ExpandoObject obj)
        {
            try
            {
                dynamic dynamicObj = obj;
                //using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                //{
                //    var validatejson = conn.Query<ValidateJson>($"exec ValidateJsonInput 'PurchaseReturn', '{JsonConvert.SerializeObject(obj)}'").FirstOrDefault();
                //    if (validatejson.Error)
                //    {
                //        return Problem(validatejson.ErrorMessage);
                //    }
                //}

                var detailsObject = (IEnumerable<dynamic>)dynamicObj.PurchaseReturnDetails;
                if (detailsObject == null && detailsObject.Count() > 0)
                {
                    return Problem("Purchase Return Details are not found.");
                }

                foreach (var x in detailsObject)
                {
                    if (x.Quantity <= 0)
                    {
                        return Problem("Picked Quantity must greater than zero. Please try again.");
                    }

                    if (string.IsNullOrEmpty(x.Bin))
                    {
                        return Problem("Item Bin is missing. Please try again.");
                    }
                }

                if (string.IsNullOrEmpty(dynamicObj.ReqOid.ToString())) return Problem("Missing Request Oid");

                IObjectSpace objectSpace = objectSpaceFactory.CreateObjectSpace<PurchaseReturnRequests>();
                PurchaseReturnRequests prr = objectSpace.FindObject<PurchaseReturnRequests>(CriteriaOperator.Parse("Oid = ?", dynamicObj.ReqOid));

                if (prr.Status != DocStatus.Submitted)
                {
                    return Problem($"Document {prr.DocNum} is {prr.Status} Status.");
                }

                if (prr.CopyTo)
                {
                    return Problem($"Document {prr.DocNum} already copied to Purchase Return Request.");
                }

                var requestor = prr.Requestor;

                IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<PurchaseReturns>();
                ISecurityStrategyBase security = securityProvider.GetSecurity();
                var userId = security.UserId;
                var userName = security.UserName;

                PurchaseReturns curobj = null;
                curobj = newObjectSpace.CreateObject<PurchaseReturns>();
                ExpandoParser.ParseExObjectXPO<PurchaseReturns>(obj, curobj, newObjectSpace);

                curobj.GRPOCorrection = prr.GRPOCorrection;

                curobj.Requestor = newObjectSpace.FindObject<vwSalesPerson>(CriteriaOperator.Parse("SlpCode = ?", requestor.SlpCode));
                curobj.CreateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                curobj.UpdateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);

                foreach (var dtl in curobj.PurchaseReturnDetails)
                {
                    PurchaseReturnRequestDetails prrDetail = objectSpace.FindObject<PurchaseReturnRequestDetails>(CriteriaOperator.Parse("Oid = ?", dtl.BaseId));

                    if(prrDetail != null)
                    {
                        dtl.GRNBaseDoc = prrDetail.BaseDoc;
                        dtl.GRNBaseId = prrDetail.BaseId;
                    }

                    dtl.CreateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                    dtl.UpdateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                }
                curobj.Save();

                var companyPrefix = CompanyCommanHelper.GetCompanyPrefix(dynamicObj.companyDB);

                GeneralControllers con = new GeneralControllers();
                curobj.DocNum = con.GenerateDocNum(DocTypeList.PR, objectSpaceFactory.CreateObjectSpace<DocTypes>(), TransferType.NA, 0, companyPrefix);
                newObjectSpace.CommitChanges();

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { oid = curobj.Oid, username = userName, oidreq = dynamicObj.ReqOid });
                    conn.Query($"exec sp_afterdatasave 'PurchaseReturn', '{json}'");
                    return Ok(new { oid = curobj.Oid, docnum = curobj.DocNum });
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("CancelPRR/oid")]
        public IActionResult CancelPRR(int oid)
        {
            try
            {
                IObjectSpace objectSpace = objectSpaceFactory.CreateObjectSpace<PurchaseReturnRequests>();
                PurchaseReturnRequests doc = objectSpace.FindObject<PurchaseReturnRequests>(CriteriaOperator.Parse("Oid = ?", oid));

                ISecurityStrategyBase security = securityProvider.GetSecurity();
                var userId = security.UserId;
                var userName = security.UserName;

                doc.Status = DocStatus.Cancelled;

                objectSpace.CommitChanges();

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { oid = oid, username = userName });
                    conn.Query($"exec sp_afterdatasave 'CancelPRR', '{json}'");
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

