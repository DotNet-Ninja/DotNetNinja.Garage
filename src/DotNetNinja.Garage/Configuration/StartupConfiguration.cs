namespace DotNetNinja.Garage.Configuration;

public class StartupConfiguration
{
    public StartupConfiguration(AuthSettings auth, DatabaseSettings databases)
    {
        Authentication = auth;
        Databases = databases;
    }
    public AuthSettings Authentication { get; set; }
    public DatabaseSettings Databases { get; set; }
}