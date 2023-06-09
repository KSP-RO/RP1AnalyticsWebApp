namespace RP1AnalyticsWebApp.Models
{
    public class AppSettings : IAppSettings
    {
        public string[] UserDefaultRoles { get; set; }
    }

    public interface IAppSettings
    {
        string[] UserDefaultRoles { get; set; }
    }
}
