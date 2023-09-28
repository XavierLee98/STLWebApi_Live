using Dapper;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp.Security;
using DevExpress.Xpo;
using DevExpress.Xpo.DB.Helpers;
using DevExpress.XtraPrinting.Native;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StarLaiPortal.Module.BusinessObjects;
using StarLaiPortal.Module.BusinessObjects.Load;
using StarLaiPortal.Module.BusinessObjects.Pack_List;
using StarLaiPortal.Module.BusinessObjects.Setup;
using StarLaiPortal.Module.BusinessObjects.View;
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
    public class LoadController : ControllerBase
    {
        private IConfiguration Configuration { get; }
        IObjectSpaceFactory objectSpaceFactory;
        ISecurityProvider securityProvider;
        public LoadController(IConfiguration configuration, IObjectSpaceFactory objectSpaceFactory, ISecurityProvider securityProvider)
        {
            this.objectSpaceFactory = objectSpaceFactory;
            this.securityProvider = securityProvider;
            this.Configuration = configuration;
        }

        [HttpGet("bundleId")]
        public IActionResult Get(string packbundleid)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { packbundleid = packbundleid });
                    var val = conn.Query($"exec sp_getdatalist 'PackBundle', '{json}'").ToList();
                    return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }


        //[HttpGet("PackBundle/BundleId/Bincode")]
        //public IActionResult GetbyBundleIdAndBinCode(string packbundleid, string bincode)
        //{
        //    try
        //    {
        //        using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
        //        {
        //            string json = JsonConvert.SerializeObject(new { packbundleid = packbundleid, bincode });
        //            var val = conn.Query($"exec sp_getdatalist 'PackBundle', '{json}'").ToList();
        //            return Ok(JsonConvert.SerializeObject(val, Formatting.Indented));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Problem(ex.Message);
        //    }
        //}

        //class PackList
        //{
        //    public string PackListID { get; set; }
        //    public int Bundle { get; set; }
        //}

        //[HttpGet("test")]
        //[AllowAnonymous]
        //public IActionResult Test()
        //{
        //    try
        //    {
        //        using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
        //        {

        //            List<PackList> list = new List<PackList>
        //            {
        //                new PackList{ PackListID = "PAL-GSG-1000045", Bundle = 1 },
        //                new PackList{ PackListID = "PAL-GSG-1000045", Bundle = 2 },
        //                new PackList{ PackListID = "PAL-GSG-1000046", Bundle = 1 }
        //            };


        //            var PackBundle = list?.Select(x => new { PackListID = x.PackListID.ToString(), Bundle = x.Bundle });
        //            var Packlistid = list?.Select(x => x.PackListID.ToString()).Distinct();

        //            string packBundleJson = JsonConvert.SerializeObject(new { PackBundle }); 
        //            string packIdJson = JsonConvert.SerializeObject(new { Packlistid });

        //            var SoNumbers = conn.Query<string>($"exec sp_beforedatasave 'GetPackSONumber', '{packBundleJson}'").ToList();

        //            var priority = conn.Query<int>($"exec sp_beforedatasave 'GetPackPriority', '{packIdJson}'").FirstOrDefault();

        //            return Ok(new { SONumbers = SoNumbers,priority = priority});
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        return Problem(ex.Message);
        //    }
        //}

        [HttpPost()]
        public IActionResult Post([FromBody] ExpandoObject obj)
        {
            try
            {
                dynamic dynamicObj = obj;

                var detailsObject = (IEnumerable<dynamic>)dynamicObj.LoadDetails;

                try
                {
                    using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                    {
                        string jsonString = JsonConvert.SerializeObject(obj);

                        jsonString = jsonString.Replace("'", "''");

                        var validatejson = conn.Query<ValidateJson>($"exec ValidateJsonInput 'Loading', '{jsonString}'").FirstOrDefault();
                        if (validatejson.Error)
                        {
                            return Problem(validatejson.ErrorMessage);
                        }
                    }

                    if (detailsObject != null)
                    {
                        using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                        {
                            foreach (var line in detailsObject)
                            {
                                string json = JsonConvert.SerializeObject(new { packlist = line.PackList, bundleid = line.Bundle });

                                var validatejson = conn.Query<ValidateJson>($"exec sp_beforedatasave 'ValidateBundle', '{json}'").FirstOrDefault();
                                if (validatejson.Error)
                                {
                                    return Problem(validatejson.ErrorMessage);
                                }
                            }
                        }
                    }
                }
                catch (Exception excep)
                {
                    throw new Exception("Validation Error. " + excep.Message);
                }


                IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<Load>();
                ISecurityStrategyBase security = securityProvider.GetSecurity();
                var userId = security.UserId;
                var userName = security.UserName;

                Load curobj = null;
                //curobj = new PickListDetailsActual(((DevExpress.ExpressApp.Xpo.XPObjectSpace)newObjectSpace).Session);
                curobj = newObjectSpace.CreateObject<Load>();
                ExpandoParser.ParseExObjectXPO<Load>(obj, curobj, newObjectSpace);

                curobj.CreateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                curobj.UpdateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                foreach (var dtl in curobj.LoadDetails)
                {
                    dtl.CreateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                    dtl.UpdateUser = newObjectSpace.GetObjectByKey<ApplicationUser>(userId);
                }

                var Packlistid = detailsObject?.Select(x => x.PackList.ToString()).Distinct();

                string packIdJson = JsonConvert.SerializeObject(new { Packlistid });

                int priority = -1;
                string warehouse = string.Empty;

                List<string> soNumbers = new List<string>();

                try
                {
                    using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                    {
                        warehouse = conn.Query<string>($"exec sp_beforedatasave 'GetWarehouseFromPack', '{packIdJson}'").FirstOrDefault();
                        soNumbers = conn.Query<string>($"exec sp_beforedatasave 'GetPackSONumber', '{packIdJson}'").ToList();
                        priority = conn.Query<int>($"exec sp_beforedatasave 'GetPackPriority', '{packIdJson}'").FirstOrDefault();
                    }
                }
                catch (Exception excep)
                {
                    throw new Exception("Header Error. " + excep.Message);
                }

                curobj.SONumber = string.Join(",", soNumbers);
                curobj.PackListNo = string.Join(",", Packlistid);
                curobj.Priority = newObjectSpace.GetObjectByKey<PriorityType>(priority);
                curobj.Warehouse = newObjectSpace.GetObjectByKey<vwWarehouse>(warehouse);

                curobj.Save();

                var companyPrefix = CompanyCommanHelper.GetCompanyPrefix(dynamicObj.companyDB);

                GeneralControllers con = new GeneralControllers();
                curobj.DocNum = con.GenerateDocNum(DocTypeList.Load, objectSpaceFactory.CreateObjectSpace<DocTypes>(), TransferType.NA, 0, companyPrefix);
                newObjectSpace.CommitChanges();

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("ConnectionString")))
                {
                    string json = JsonConvert.SerializeObject(new { oid = curobj.Oid, username = userName });
                    conn.Query($"exec sp_afterdatasave 'Loading', '{json}'");
                }

                var result = con.GenerateDO(Configuration.GetConnectionString("ConnectionString"), curobj, newObjectSpace, companyPrefix);

                if (result == 0) throw new Exception($"Fail to generate Delivery for Load ({curobj.DocNum}). ");

                return Ok(new { oid = curobj.Oid, docnum = curobj.DocNum });

            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
