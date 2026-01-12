namespace Futebol.Api.Configuration;

public class AppSettings
{
    public const string SectionName = "AppSettings";
    
    public string FrontendUrl { get; set; } = string.Empty;
}
