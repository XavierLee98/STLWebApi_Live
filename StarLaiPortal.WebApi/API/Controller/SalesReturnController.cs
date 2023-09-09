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
using StarLaiPortal.Module.BusinessObjects.Sales_Return;
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
    public class SalesReturnController : ControllerBase
    {
        private IConfiguration Configuration { get; }
        IObjectSpaceFactory objectSpaceFactory;
        ISecurityProvider securityProvider;
        public SalesReturnController(IConfiguration configuration, IObjectSpaceFactory objectSpaceFactory, ISecurityProvider securityProvider)
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
                    var val = conn.Query($"exec sp_getdatalist 'SalesReturnRequestList', '{json}'").ToList();
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
                    string json = JsonConvert.SerializeObject(new { type = "SalesReturn" });
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

                    var val = conn.Query($"exec sp_getdatalist 'SalesReturnRequestDetails', '{json}'").ToList();
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
                //    var validatejson = conn.Query<ValidateJson>($"exec ValidateJsonInput 'SalesReturns', '{JsonConvert.SerializeObject(obj)}'").FirstOrDefault();
                //    if (validatejson.Error)
                //    {
                //        return Problem(validatejson.ErrorMessage);
                //    }
                //}

                var detailsObject = (IEnumerable<dynamic>)dynamicObj.SalesReturnDetails;

                if (detailsObject != null && detailsObject.Count() > 0)
                {
                    var isValid = true;

                    string errMsg = string.Empty;

                    foreach (var x in detailsObject)
                    {
                        if (x.RtnQuantity <= 0)
                        {
                            isValid = false;
                            errMsg = "Returned quantity cannot be zero. Please fully returned.";
                            break;
                        }
                        else if (x.RtnQuantity > x.Quantity)
                        {
                            isValid = false;
                            errMsg = "Returned quantity cannot greater than required quantity.";
                            break;
                        }
                        else if (x.RtnQuantity < x.Quantity)
                        {
                            isValid = false;
                            errMsg = "Returned quantity cannot less than required quantity. Please fully returned.";
                            break;
                        }
                        if (string.IsNullOrEmpty(x.Bin))
                        {
                            isValid = false;
                            errMsg = "Item Bin is missing. Please try again.";
                            break;
                        }
                    }

                    if (!isValid)
                    {
                        return Problem(errMsg);
                    }
                }
                else
                {
                    return Problem("Sales Return Details are not found.");
                }


                IObjectSpace objectSpace = objectSpaceFactory.CreateObjectSpace<SalesReturnRequests>();
                SalesReturnRequests srr = objectSpace.FindObject<SalesReturnRequests>(CriteriaOperator.Parse("Oid = ?", dynamicObj.ReqOid));

                if (srr.Status != DocStatus.Submitted)
                {
                    return Problem($"Document {srr.DocNum} is {srr.Status} Status.");
                }

                if (srr.CopyTo)
                {
                    return Problem($"Document {srr.DocNum} already copied to Sales Return Request.");
                }

                var requestor = srr.Salesperson;

                IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<SalesReturns>();
                ISecurityStrategyBase security = securityProvider.GetSecurity();
                var userId = security.UserId;
                var userName = security.UserName;

                SalesReturns curobj = null;
                curobj = newObjectSpace.CreateObject<SalesReturns>();
                ExpandoParser.ParseExObjectXPO<SalesReturns>(obj, curobj, newObjectSpace);

                curobj.Salesperson = newObjectSpace.FindObject<vwSalesPerson>(CriteriaOperator.Parse("SlpCode = ?", requestor.SlpCode));
                curobj.CreateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                curobj.UpdateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);

                foreach (var dtl in curobj.SalesReturnDetails)
                {
                    dtl.CreateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                    dtl.UpdateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                }
                curobj.Save();

                var companyPrefix = CompanyCommanHelper.GetCompanyPrefix(dynamicObj.companyDB);

                GeneralControllers con = new GeneralControllers();
                curobj.DocNum = con.GenerateDocNum(DocTypeList.SR, objectSpaceFactory.CreateObjectSpace<DocTypes>(), TransferType.NA, 0, companyPrefix);
                newObjectSpace.CommitChanges();

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { oid = curobj.Oid, username = userName, oidreq = dynamicObj.ReqOid });
                    conn.Query($"exec sp_afterdatasave 'SalesReturns', '{json}'");
                    return Ok(new { oid = curobj.Oid, docnum = curobj.DocNum });
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("CancelSRR/oid")]
        public IActionResult CancelSRR(int oid)
        {
            try
            {
                IObjectSpace objectSpace = objectSpaceFactory.CreateObjectSpace<SalesReturnRequests>();
                SalesReturnRequests doc = objectSpace.FindObject<SalesReturnRequests>(CriteriaOperator.Parse("Oid = ?", oid));

                ISecurityStrategyBase security = securityProvider.GetSecurity();
                var userId = security.UserId;
                var userName = security.UserName;

                doc.Status = DocStatus.Cancelled;

                objectSpace.CommitChanges();

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { oid = oid, username = userName});
                    conn.Query($"exec sp_afterdatasave 'CancelSRR', '{json}'");
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
