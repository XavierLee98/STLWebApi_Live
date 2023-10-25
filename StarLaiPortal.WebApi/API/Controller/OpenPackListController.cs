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
using StarLaiPortal.Module.BusinessObjects.Pack_List;
using StarLaiPortal.Module.Controllers;
using StarLaiPortal.Module.BusinessObjects.Setup;
using DevExpress.XtraPrinting.Native;
using DevExpress.CodeParser;
using StarLaiPortal.WebApi.Model.Packing;
using Microsoft.AspNetCore.OData.Query;
using DevExpress.Pdf.Native;

namespace StarLaiPortal.WebApi.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OpenPackListController : ControllerBase
    {
        private IConfiguration Configuration { get; }
        IObjectSpaceFactory objectSpaceFactory;
        ISecurityProvider securityProvider;
        public OpenPackListController(IConfiguration configuration, IObjectSpaceFactory objectSpaceFactory, ISecurityProvider securityProvider)
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
                    var val = conn.Query("exec sp_getdatalist 'OpenPLA'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("packlist/startdate/enddate")]
        public IActionResult Get(DateTime startdate, DateTime enddate)
        {
            try
            {
                ISecurityStrategyBase security = securityProvider.GetSecurity();
                var userId = security.UserId;
                var userName = security.UserName;
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { userId, startdate, enddate });

                    var val = conn.Query($"exec sp_getdatalist 'PartialPackList', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("oid/code")]
        public IActionResult Get(int oid, string code)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { oid = oid, tobin = code });
                    var val = conn.Query($"exec sp_getdatalist 'OpenPLA', '{json}'").ToList();
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
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { tobin = code });
                    var val = conn.Query($"exec sp_getdatalist 'OpenPLA', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }

            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("GetDraft")]
        public IActionResult GetDraft(int oid)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { oid = oid });
                    var packDetails = conn.Query<PackPickDetail>($"exec sp_getdatalist 'GetExistingPackDetails', '{json}'").ToList();
                    if (packDetails == null || packDetails.Count() <= 0) throw new Exception("Pack details not found.");

                    foreach(var detail in packDetails)
                    {
                        string detailjson = JsonConvert.SerializeObject(new { oid = oid, baseid = detail.OID });

                        var bundlelist = conn.Query<PackedBundle>($"exec sp_getdatalist 'GetExistingBundle', '{detailjson}'").ToList();
                        detail.packedBundles = bundlelist;

                        string bundleStr = String.Empty;
                        bundleStr = "Bundle List: \n";
                        detail.packedBundles.ForEach(x => bundleStr += $"({x.BundleName}) - {x.BundleQty}\n");

                        detail.BundleListStr = bundleStr;
                    }

                    return Ok(JsonConvert.SerializeObject(packDetails, Formatting.Indented));
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
                int PackOid = (int)dynamicObj.PackOid;
                try
                {
                    using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                    {
                        string jsonString = JsonConvert.SerializeObject(obj);

                        jsonString = jsonString.Replace("'", "''");

                        var validatejson = conn.Query<ValidateJson>($"exec ValidateJsonInput 'PackList', '{jsonString}'").FirstOrDefault();
                        if (validatejson.Error)
                        {
                            return Problem(validatejson.ErrorMessage);
                        }
                    }

                    //Check All Picklist whether already pack?
                    var detailsObject = (IEnumerable<dynamic>)dynamicObj.PackListDetails;
                    if (detailsObject == null)
                        return Problem("Pack List Details are null.");

                    var distinctIds = detailsObject?.Select(x => x.BaseDoc.ToString()).Distinct();

                    bool isFoundDuplicate = false;
                    string duplicateId = string.Empty;
                    foreach (var baseId in distinctIds)
                    {
                        using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                        {
                            var count = conn.Query<int>($"exec sp_beforedatasave 'ValidatePickToPack', '{JsonConvert.SerializeObject(new { picklist = baseId, packlist = PackOid })}'").FirstOrDefault();
                            if (count > 0)
                            {
                                isFoundDuplicate = true;
                                duplicateId = baseId;
                                break;
                            }
                        }
                    }

                    if (isFoundDuplicate)
                        return Problem($"Pick List No. {duplicateId} already been packed.");

                    int packOIDResult = -1;
                    string packDocNumResult = "";

                    ISecurityStrategyBase security = securityProvider.GetSecurity();
                    var userId = security.UserId;
                    var userName = security.UserName;

                    //New Document
                    if (PackOid == -1) 
                    {
                        IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<PackList>();

                        PackList curobj = null;
                        curobj = newObjectSpace.CreateObject<PackList>();
                        ExpandoParser.ParseExObjectXPO<PackList>(obj, curobj, newObjectSpace);

                        curobj.CreateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                        curobj.UpdateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                        foreach (var dtl in curobj.PackListDetails)
                        {
                            dtl.CreateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                            dtl.UpdateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                        }

                        List<string> soNumbers = new List<string>();
                        List<string> sAPSONo = new List<string>();

                        string jsonArray = JsonConvert.SerializeObject(new { PickLists = distinctIds });

                        int priority = -1;
                        string customer = string.Empty;
                        string customerGroup = string.Empty;
                        string warehouse = string.Empty;

                        try
                        {
                            using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                            {
                                warehouse = conn.Query<string>($"exec sp_beforedatasave 'GetWarehouseFromPick', '{jsonArray}'").FirstOrDefault();
                                soNumbers = conn.Query<string>($"exec sp_beforedatasave 'GetPickDistinctSONumber', '{jsonArray}'").ToList();
                                sAPSONo = conn.Query<string>($"exec sp_beforedatasave 'GetPickDistinctSAPSONo', '{jsonArray}'").ToList();
                                priority = conn.Query<int>($"exec sp_beforedatasave 'GetPickPriority', '{jsonArray}'").FirstOrDefault();
                                customer = conn.Query<string>($"exec sp_beforedatasave 'GetPickCustomer', '{jsonArray}'").FirstOrDefault();
                                customerGroup = conn.Query<string>($"exec sp_beforedatasave 'GetPickCustomerGroup', '{jsonArray}'").FirstOrDefault();
                            }
                        }
                        catch (Exception excep)
                        {
                            throw new Exception("Header Error. " + excep.Message);
                        }

                        curobj.CustomerGroup = customerGroup;
                        curobj.SONumber = string.Join(",", soNumbers);
                        curobj.SAPSONo = string.Join(",", sAPSONo);
                        curobj.PickListNo = string.Join(",", distinctIds);
                        curobj.Priority = newObjectSpace.GetObjectByKey<PriorityType>(priority);
                        curobj.Customer = customer;
                        curobj.Warehouse = newObjectSpace.GetObjectByKey<vwWarehouse>(warehouse);

                        curobj.Save();

                        var companyPrefix = CompanyCommanHelper.GetCompanyPrefix(dynamicObj.companyDB);

                        GeneralControllers con = new GeneralControllers();
                        curobj.DocNum = con.GenerateDocNum(DocTypeList.PAL, objectSpaceFactory.CreateObjectSpace<DocTypes>(), TransferType.NA, 0, companyPrefix);

                        newObjectSpace.CommitChanges();

                        packOIDResult = curobj.Oid;
                        packDocNumResult = curobj.DocNum;
                    }
                    else
                    {
                        IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<PackListDetails>();
                        IObjectSpace packOS = objectSpaceFactory.CreateObjectSpace<PackList>();
                        PackList packobj = packOS.FindObject<PackList>(CriteriaOperator.Parse("Oid = ?", PackOid));

                        if (packobj == null)
                        {
                            return Problem($"Pack Document not found. Id ({PackOid})");
                        }

                        if (packobj.Status != DocStatus.Draft)
                        {
                            return Problem($"Update Failed. Pack Document No. {packobj.DocNum} already {packobj.Status}.");
                        }

                        using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                        {
                            string json = JsonConvert.SerializeObject(new { packoid = packobj.Oid });
                            conn.Query($"exec sp_beforedatasave 'DeletePackDetail', '{json}'");
                        }

                        List<PackListDetails> objs = new List<PackListDetails>();
                        foreach (ExpandoObject exobj in dynamicObj.PackListDetails)
                        {
                            PackListDetails curobj = newObjectSpace.CreateObject<PackListDetails>();
                            ExpandoParser.ParseExObjectXPO<PackListDetails>(new Dictionary<string, object>(exobj), curobj, newObjectSpace);

                            curobj.PackList = newObjectSpace.GetObjectByKey<PackList>(packobj.Oid); ;
                            curobj.CreateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                            curobj.UpdateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                            curobj.Save();
                            objs.Add(curobj);
                        }

                        newObjectSpace.CommitChanges();
                        packOIDResult = packobj.Oid;
                        packDocNumResult = packobj.DocNum;

                        using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                        {
                            string json = JsonConvert.SerializeObject(new { oid = packOIDResult, username = userName });
                            conn.Query($"exec sp_afterdatasave 'PackListDraft', '{json}'");
                        }
                    }

                    return Ok(new { oid = packOIDResult, docnum = packDocNumResult });
                }
                catch (Exception excep)
                {
                    throw new Exception("Validation Error. " + excep.Message);
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpPost("DonePost")]
        public IActionResult DonePost([FromBody] ExpandoObject obj)
        {
            try
            {
                dynamic dynamicObj = obj;
                int PackOid = (int)dynamicObj.PackOid;

                ISecurityStrategyBase security = securityProvider.GetSecurity();
                var userId = security.UserId;
                var userName = security.UserName;

                try
                {
                    using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                    {
                        string jsonString = JsonConvert.SerializeObject(obj);

                        jsonString = jsonString.Replace("'", "''");

                        var validatejson = conn.Query<ValidateJson>($"exec ValidateJsonInput 'PackList', '{jsonString}'").FirstOrDefault();
                        if (validatejson.Error)
                        {
                            return Problem(validatejson.ErrorMessage);
                        }
                    }
                }
                catch (Exception excep)
                {
                    throw new Exception("Validation Error. " + excep.Message);
                }

                //Check All Picklist whether already pack?
                var detailsObject = (IEnumerable<dynamic>)dynamicObj.PackListDetails;
                if (detailsObject == null)
                {
                    if(PackOid == -1) return Problem("Pack List Details are null.");
                    else
                    {
                        IObjectSpace packOS = objectSpaceFactory.CreateObjectSpace<PackList>();
                        PackList packobj = packOS.FindObject<PackList>(CriteriaOperator.Parse("Oid = ?", PackOid));

                        if (packobj == null)
                        {
                            return Problem($"Pack Document not found. Id ({PackOid})");
                        }

                        if (packobj.Status != DocStatus.Draft)
                        {
                            return Problem($"Update Failed. Pack Document No. {packobj.DocNum} already {packobj.Status}.");
                        }

                        if (packobj.PackListDetails.Count() <= 0)
                        {
                            return Problem($"Update Failed. Pack Document No. {packobj.DocNum} has no pack bundle details. Please add some before save.");
                        }

                        packobj.Status = DocStatus.Submitted;

                        packobj.Save();

                        using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                        {
                            string json = JsonConvert.SerializeObject(new { oid = packobj.Oid, username = userName });
                            conn.Query($"exec sp_afterdatasave 'PackList', '{json}'");
                            return Ok(new { oid = packobj.Oid, docnum = packobj.DocNum });
                        }
                    }
                }

                var distinctIds = detailsObject?.Select(x => x.BaseDoc.ToString()).Distinct();

                bool isFoundDuplicate = false;
                string duplicateId = string.Empty;
                foreach (var baseId in distinctIds)
                {
                    using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                    {
                        var count = conn.Query<int>($"exec sp_beforedatasave 'ValidatePickToPack', '{JsonConvert.SerializeObject(new { picklist = baseId, packlist = PackOid })}'").FirstOrDefault();
                        if (count > 0)
                        {
                            isFoundDuplicate = true;
                            duplicateId = baseId;
                            break;
                        }
                    }
                }

                if (isFoundDuplicate)
                    return Problem($"Pick List No. {duplicateId} already been packed.");

                int packOIDResult = -1;
                string packDocNumResult = "";

                if (PackOid == -1)
                {
                    IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<PackList>();

                    PackList curobj = null;
                    curobj = newObjectSpace.CreateObject<PackList>();
                    ExpandoParser.ParseExObjectXPO<PackList>(obj, curobj, newObjectSpace);

                    curobj.CreateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                    curobj.UpdateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                    foreach (var dtl in curobj.PackListDetails)
                    {
                        dtl.CreateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                        dtl.UpdateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                    }

                    List<string> soNumbers = new List<string>();
                    List<string> sAPSONo = new List<string>();

                    string jsonArray = JsonConvert.SerializeObject(new { PickLists = distinctIds });

                    int priority = -1;
                    string customer = string.Empty;
                    string customerGroup = string.Empty;
                    string warehouse = string.Empty;

                    try
                    {
                        using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                        {
                            warehouse = conn.Query<string>($"exec sp_beforedatasave 'GetWarehouseFromPick', '{jsonArray}'").FirstOrDefault();
                            soNumbers = conn.Query<string>($"exec sp_beforedatasave 'GetPickDistinctSONumber', '{jsonArray}'").ToList();
                            sAPSONo = conn.Query<string>($"exec sp_beforedatasave 'GetPickDistinctSAPSONo', '{jsonArray}'").ToList();
                            priority = conn.Query<int>($"exec sp_beforedatasave 'GetPickPriority', '{jsonArray}'").FirstOrDefault();
                            customer = conn.Query<string>($"exec sp_beforedatasave 'GetPickCustomer', '{jsonArray}'").FirstOrDefault();
                            customerGroup = conn.Query<string>($"exec sp_beforedatasave 'GetPickCustomerGroup', '{jsonArray}'").FirstOrDefault();
                        }
                    }
                    catch (Exception excep)
                    {
                        throw new Exception("Header Error. " + excep.Message);
                    }

                    curobj.CustomerGroup = customerGroup;
                    curobj.SONumber = string.Join(",", soNumbers);
                    curobj.SAPSONo = string.Join(",", sAPSONo);
                    curobj.PickListNo = string.Join(",", distinctIds);
                    curobj.Priority = newObjectSpace.GetObjectByKey<PriorityType>(priority);
                    curobj.Customer = customer;
                    curobj.Warehouse = newObjectSpace.GetObjectByKey<vwWarehouse>(warehouse);

                    curobj.Save();

                    var companyPrefix = CompanyCommanHelper.GetCompanyPrefix(dynamicObj.companyDB);

                    GeneralControllers con = new GeneralControllers();
                    curobj.DocNum = con.GenerateDocNum(DocTypeList.PAL, objectSpaceFactory.CreateObjectSpace<DocTypes>(), TransferType.NA, 0, companyPrefix);

                    newObjectSpace.CommitChanges();

                    packOIDResult = curobj.Oid;
                    packDocNumResult = curobj.DocNum;
                }
                else
                {
                    IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<PackListDetails>();
                    IObjectSpace packOS = objectSpaceFactory.CreateObjectSpace<PackList>();
                    PackList packobj = packOS.FindObject<PackList>(CriteriaOperator.Parse("Oid = ?", PackOid));

                    if (packobj == null)
                    {
                        return Problem($"Pack Document not found. Id ({PackOid})");
                    }

                    if (packobj.Status != DocStatus.Draft)
                    {
                        return Problem($"Update Failed. Pack Document No. {packobj.DocNum} already {packobj.Status}.");
                    }

                    using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                    {
                        string json = JsonConvert.SerializeObject(new { packoid = packobj.Oid });
                        conn.Query($"exec sp_beforedatasave 'DeletePackDetail', '{json}'");
                    }

                    List<PackListDetails> objs = new List<PackListDetails>();
                    foreach (ExpandoObject exobj in dynamicObj.PackListDetails)
                    {
                        PackListDetails curobj = newObjectSpace.CreateObject<PackListDetails>();
                        ExpandoParser.ParseExObjectXPO<PackListDetails>(new Dictionary<string, object>(exobj), curobj, newObjectSpace);

                        curobj.PackList = newObjectSpace.GetObjectByKey<PackList>(packobj.Oid); ;
                        curobj.CreateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                        curobj.UpdateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                        curobj.Save();
                        objs.Add(curobj);
                    }

                    newObjectSpace.CommitChanges();
                    packOIDResult = packobj.Oid;
                    packDocNumResult = packobj.DocNum;
                }

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { oid = packOIDResult, username = userName });
                    conn.Query($"exec sp_afterdatasave 'PackList', '{json}'");
                    return Ok(new { oid = packOIDResult, docnum = packDocNumResult });
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
                try
                {
                    using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                    {
                        string jsonString = JsonConvert.SerializeObject(obj);

                        jsonString = jsonString.Replace("'", "''");

                        var validatejson = conn.Query<ValidateJson>($"exec ValidateJsonInput 'PackList', '{jsonString}'").FirstOrDefault();
                        if (validatejson.Error)
                        {
                            return Problem(validatejson.ErrorMessage);
                        }
                    }
                }
                catch (Exception excep)
                {
                    throw new Exception("Validation Error. " + excep.Message);
                }

                //Check All Picklist whether already pack?
                var detailsObject = (IEnumerable<dynamic>)dynamicObj.PackListDetails;
                if(detailsObject == null) 
                    return Problem("Pack List Details are null.");

                var distinctIds = detailsObject?.Select(x => x.BaseDoc.ToString()).Distinct();

                bool isFoundDuplicate = false;
                string duplicateId = string.Empty;
                foreach (var baseId in distinctIds)
                {
                    using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                    {
                        //var count = conn.Query<int>($"exec sp_beforedatasave 'ValidatePickToPack', '{JsonConvert.SerializeObject(new { picklist = baseId, packlist = -1 })}'").FirstOrDefault();

                        var count = conn.Query<int>($"exec sp_beforedatasave 'ValidatePickToPack', '{JsonConvert.SerializeObject(new { picklist = baseId })}'").FirstOrDefault();
                        if (count > 0)
                        {
                            isFoundDuplicate = true;
                            duplicateId = baseId;
                            break;
                        }
                    }
                }

                if (isFoundDuplicate) 
                    return Problem($"Pick List No. {duplicateId} already been packed.");

                IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<PackList>();
                ISecurityStrategyBase security = securityProvider.GetSecurity();

                var userId = security.UserId;
                var userName = security.UserName;

                PackList curobj = null;
                curobj = newObjectSpace.CreateObject<PackList>();
                ExpandoParser.ParseExObjectXPO<PackList>(obj, curobj, newObjectSpace);

                curobj.CreateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                curobj.UpdateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                foreach(var dtl in curobj.PackListDetails)
                {
                    dtl.CreateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                    dtl.UpdateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                }

                List<string> soNumbers = new List<string>();
                List<string> sAPSONo = new List<string>();

                string jsonArray = JsonConvert.SerializeObject(new { PickLists = distinctIds });

                int priority = -1;
                string customer = string.Empty;
                string customerGroup = string.Empty;
                string warehouse = string.Empty;

                try
                {
                    using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                    {
                        warehouse = conn.Query<string>($"exec sp_beforedatasave 'GetWarehouseFromPick', '{jsonArray}'").FirstOrDefault();
                        soNumbers = conn.Query<string>($"exec sp_beforedatasave 'GetPickDistinctSONumber', '{jsonArray}'").ToList();
                        sAPSONo = conn.Query<string>($"exec sp_beforedatasave 'GetPickDistinctSAPSONo', '{jsonArray}'").ToList();
                        priority = conn.Query<int>($"exec sp_beforedatasave 'GetPickPriority', '{jsonArray}'").FirstOrDefault();
                        customer = conn.Query<string>($"exec sp_beforedatasave 'GetPickCustomer', '{jsonArray}'").FirstOrDefault();
                        customerGroup = conn.Query<string>($"exec sp_beforedatasave 'GetPickCustomerGroup', '{jsonArray}'").FirstOrDefault();
                    }
                }
                catch (Exception excep)
                {
                    throw new Exception("Header Error. " + excep.Message);
                }

                curobj.CustomerGroup = customerGroup;
                curobj.SONumber = string.Join(",", soNumbers);
                curobj.SAPSONo = string.Join(",", sAPSONo);
                curobj.PickListNo = string.Join(",", distinctIds);
                curobj.Priority = newObjectSpace.GetObjectByKey<PriorityType>(priority);
                curobj.Customer = customer;
                curobj.Warehouse = newObjectSpace.GetObjectByKey<vwWarehouse>(warehouse);

                curobj.Save();

                var companyPrefix = CompanyCommanHelper.GetCompanyPrefix(dynamicObj.companyDB);

                GeneralControllers con = new GeneralControllers();
                curobj.DocNum = con.GenerateDocNum(DocTypeList.PAL, objectSpaceFactory.CreateObjectSpace<DocTypes>(), TransferType.NA, 0, companyPrefix);

                newObjectSpace.CommitChanges();

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { oid = curobj.Oid, username = userName });
                    conn.Query($"exec sp_afterdatasave 'PackList', '{json}'");
                    return Ok(new { oid = curobj.Oid, docnum = curobj.DocNum });
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

    }
}
