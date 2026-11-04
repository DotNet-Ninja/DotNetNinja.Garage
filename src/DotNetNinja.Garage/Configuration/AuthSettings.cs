using DotNetNinja.AutoBoundConfiguration;

namespace DotNetNinja.Garage.Configuration;

[AutoBind("Auth")]
public class AuthSettings
{
    public string Domain { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;

    public void ThrowIfInvalid()
    {
        if (string.IsNullOrWhiteSpace(Domain) || string.IsNullOrWhiteSpace(ClientId) || string.IsNullOrWhiteSpace(ClientSecret))
        {
            throw new InvalidOperationException("Auth:Domain, Auth:ClientId, and Auth:ClientSecret must be configured. See the README for local setup.");
        }
    }
}
