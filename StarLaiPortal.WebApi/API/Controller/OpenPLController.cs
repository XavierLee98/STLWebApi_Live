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
using System.Text.Json;
using StarLaiPortal.Module.Controllers;
using StarLaiPortal.Module.BusinessObjects.Pack_List;
using StarLaiPortal.Module.BusinessObjects.Load;
using StarLaiPortal.Module.BusinessObjects.Delivery_Order;
using StarLaiPortal.Module.BusinessObjects.Setup;
using System.Diagnostics;

namespace StarLaiPortal.WebApi.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OpenPLController : ControllerBase
    {
        private IConfiguration Configuration { get; }
        IObjectSpaceFactory objectSpaceFactory;
        ISecurityProvider securityProvider;
        public OpenPLController(IConfiguration configuration, IObjectSpaceFactory objectSpaceFactory, ISecurityProvider securityProvider)
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
                ISecurityStrategyBase security = securityProvider.GetSecurity();
                var userId = security.UserId;
                var userName = security.UserName;

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { userGuid = userId.ToString() });
                    var val = conn.Query($"exec sp_getdatalist 'OpenPL', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("ReasonCode")]
        public IActionResult GetReasonCode()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    var val = conn.Query("exec sp_getdatalist 'ReasonCode'").ToList();
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
                    string json = JsonConvert.SerializeObject(new { oid = oid });
                    var val = conn.Query($"exec sp_getdatalist 'OpenPL', '{json}'").ToList();
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
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string jsonString = JsonConvert.SerializeObject(obj);

                    jsonString = jsonString = jsonString.Replace("'", "''");

                    var test = $"{obj}";

                    var validatejson = conn.Query<ValidateJson>($"exec ValidateJsonInput 'PickListDetailsActual', '{jsonString}'").FirstOrDefault();
                    if (validatejson.Error)
                    {
                        return Problem(validatejson.ErrorMessage);
                    }
                }

                var detailsObject = (IEnumerable<dynamic>)dynamicObj.PickListDetailsActuals;

                if (detailsObject != null)
                {
                    using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                    {
                        foreach (var line in detailsObject)
                        {
                            if (string.IsNullOrEmpty(line.ToBin))
                            {
                                return Problem("The ToBin value is null. Please select Tobin.");
                            }

                            string json = JsonConvert.SerializeObject(new { itemcode = line.ItemCode, bincode = line.FromBin, quantity = line.PickQty });

                            var validateBalance = conn.Query<ValidateJson>($"exec sp_beforedatasave 'ValidateStockBalance', '{json}'").FirstOrDefault();
                            if (validateBalance.Error)
                            {
                                return Problem(validateBalance.ErrorMessage);
                            }
                        }
                    }
                }
                else
                {
                    return Problem("Pick List Actuals are not found.");
                }

                IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<PickListDetailsActual>();
                ISecurityStrategyBase security = securityProvider.GetSecurity();
                IObjectSpace plOS = objectSpaceFactory.CreateObjectSpace<PickList>();
                PickList plobj = plOS.FindObject<PickList>(CriteriaOperator.Parse("Oid = ?", dynamicObj.PickOid));

                var userId = security.UserId;
                var userName = security.UserName;

                if (plobj.Status != DocStatus.Draft)
                {
                    return Problem($"Update Failed. Pick List No. {plobj.DocNum} already {plobj.Status}.");
                }

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    foreach (var line in plobj.PickListDetails)
                    {
                        string itemjson = JsonConvert.SerializeObject(new { picklist = dynamicObj.PickOid, picklistdetailoid = line.Oid });
                        var pickedQty = conn.Query<decimal>($"exec sp_afterdatasave 'PickListDetailsActual', '{itemjson}'").FirstOrDefault();

                        var inputQty = detailsObject.Where(x => x.PickList == dynamicObj.PickOid && x.PickListDetailOid == line.Oid).Sum(y => (decimal)y.PickQty);

                        if (pickedQty + inputQty > line.PlanQty) return Problem($"Actual pick quantity ({(pickedQty + inputQty):F2}) cannot more than plan quantity ({line.PlanQty:F2}). Please check picklist in portal.");
                    }
                }

                plobj.Picker = plOS.GetObjectByKey<ApplicationUser>(userId);

                plOS.CommitChanges();

                List<PickListDetailsActual> objs = new List<PickListDetailsActual>();
                foreach (ExpandoObject exobj in dynamicObj.PickListDetailsActuals)
                {
                    PickListDetailsActual curobj = newObjectSpace.CreateObject<PickListDetailsActual>();
                    ExpandoParser.ParseExObjectXPO<PickListDetailsActual>(new Dictionary<string, object>(exobj), curobj, newObjectSpace);

                    curobj.CreateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                    curobj.UpdateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                    curobj.Save();
                    objs.Add(curobj);
                }

                newObjectSpace.CommitChanges();

                //Update Quantity
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    foreach (var line in plobj.PickListDetails)
                    {
                        string json = JsonConvert.SerializeObject(new { picklist = dynamicObj.PickOid, picklistdetailoid = line.Oid });
                        var pickqty = conn.Query<decimal>($"exec sp_afterdatasave 'PickListDetailsActual', '{json}'").FirstOrDefault();

                        if (pickqty > line.PlanQty) return Problem($"Actual pick quantity cannot more than plan quantity. Please check picklist in portal.");

                        line.PickQty = pickqty;

                        if (dynamicObj.PickListDetails != null && dynamicObj.PickListDetails.Count > 0)
                        {
                            var list = (IEnumerable<dynamic>)dynamicObj.PickListDetails;

                            var selectedLine = list.Where(x => x.Oid == line.Oid).FirstOrDefault();

                            if (selectedLine != null)
                            {
                                line.Reason = plOS.GetObjectByKey<DiscrepancyReason>((int)selectedLine.Reason);
                            }
                        }
                    }
                }

                plobj.Status = DocStatus.Submitted;

                plOS.CommitChanges();

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { oid = objs.FirstOrDefault().PickList.Oid, username = userName });
                    conn.Query($"exec sp_afterdatasave 'PickListStatus', '{json}'");
                }

                IObjectSpace packos = objectSpaceFactory.CreateObjectSpace<PackList>();
                IObjectSpace loados = objectSpaceFactory.CreateObjectSpace<Load>();
                IObjectSpace deliveryos = objectSpaceFactory.CreateObjectSpace<DeliveryOrder>();

                var companyPrefix = CompanyCommanHelper.GetCompanyPrefix(dynamicObj.companyDB);

                GeneralControllers con = new GeneralControllers();
                var result = con.GenerateAutoDO(Configuration.GetConnectionString("ConnectionString"), objs.FirstOrDefault().PickList, newObjectSpace, packos, loados, deliveryos, companyPrefix);

                if (result == 0) throw new Exception($"Fail to generate Auto Delivery for Pick List ({plobj.DocNum}). ");

                return Ok(new { oid = plobj.Oid, docnum = plobj.DocNum, IsAutoDO = result });

            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

    }
}

//foreach (var aaa in objs)
//{
//    using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
//    {
//        string json = JsonConvert.SerializeObject(new { oid = aaa.Oid });
//        conn.Query($"exec sp_afterdatasave 'PickListDetailsActual', '{json}'");
//    }
//}

//using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
//{
//    string json = JsonConvert.SerializeObject(dynamicObj.PickListDetails);
//    conn.Query($"exec sp_updateData 'SetPickListDetailsReason', '{json}'");
//}

