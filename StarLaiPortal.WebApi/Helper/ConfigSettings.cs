namespace StarLaiPortal.WebApi.Helper
{
    public class ConfigSettings
    {
        public static string Conn { get; set; }

        public ConfigSettings(string conn)
        {
            Conn = conn;
        }
    }
}
