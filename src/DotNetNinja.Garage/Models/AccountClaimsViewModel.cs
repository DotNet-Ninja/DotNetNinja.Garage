namespace DotNetNinja.Garage.Models;

public sealed record AccountClaimsViewModel(IReadOnlyList<UserClaimViewModel> Claims);

public sealed record UserClaimViewModel(string Type, string Value, string Issuer);
