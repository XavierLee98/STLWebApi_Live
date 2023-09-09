using Dapper;
using Newtonsoft.Json;
using StarLaiPortal.WebApi.Model;
using System.Configuration;
using System.Data.SqlClient;

namespace StarLaiPortal.WebApi.Helper
{
    public class CompanyCommanHelper
    {
        public static string GetCompanyPrefix(string dbName)
        {
            string connString = ConfigSettings.Conn;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string json = JsonConvert.SerializeObject(new { dbName = dbName });

                var result = conn.Query<string>($"exec sp_getdatalist 'CompanyPrefix', '{json}'").FirstOrDefault();

                if (string.IsNullOrEmpty(result)) throw new Exception("Company Prefix not found.");
                return result;
            }
        }
    }
}
