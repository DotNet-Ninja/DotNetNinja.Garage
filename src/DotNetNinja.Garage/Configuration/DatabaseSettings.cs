using DotNetNinja.AutoBoundConfiguration;

namespace DotNetNinja.Garage.Configuration;

[AutoBind("Databases")]
public class DatabaseSettings: List<DbConfiguration>
{

}
