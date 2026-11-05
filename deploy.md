# Azure deployment

Garage deploys to an Azure App Service Web App when a commit reaches `main`. Pull requests targeting `main` run the build and xUnit tests in `.github/workflows/ci.yml`. The main workflow in `.github/workflows/semantic-versioning.yml` calculates a semantic version, builds and tests with that version, publishes a ZIP package, deploys it, verifies `/Home/About`, then creates an annotated `vMAJOR.MINOR.PATCH` tag and GitHub release with the ZIP attached. The manual workflow in `.github/workflows/release.yml` only produces a downloadable package; it does not deploy or create a release.

## Azure prerequisites

1. Create an Azure App Service Web App with the **.NET 10** runtime and HTTPS enabled. This workflow deploys directly to its production slot. Record the Web App name. A Linux Web App also needs the forwarded headers setting listed below because App Service terminates HTTPS before requests reach the app.
2. Create or select an Azure SQL database reachable from the Web App. Configure its firewall/network access and a database identity or credential that the application can use. The app currently expects a SQL Server connection string in its `Databases` configuration array.
3. Create a Microsoft Entra application or user-assigned managed identity for GitHub Actions deployment. Give it deployment rights to the Web App (for example, Website Contributor scoped to that Web App). Add a federated credential for GitHub's `production` environment. **This repository's GitHub OIDC token currently presents the legacy subject** `repo:DotNet-Ninja/DotNetNinja.Garage:environment:production`. In the identity's **Federated credentials → Add credential** form, choose **Other issuer** if the GitHub Actions preset forces an ID-based subject. Set issuer `https://token.actions.githubusercontent.com`, subject to the legacy value above, and audience `api://AzureADTokenExchange`. All three must match the token exactly. The workflow uses OpenID Connect and requires no long-lived Azure client secret.
4. Create a production Auth0 Regular Web Application. Register `https://<your-web-app-host>/callback` as an allowed callback URL, `https://<your-web-app-host>/` as an allowed logout URL, and `https://<your-web-app-host>` as an allowed web origin. Use the Web App's actual hostname, which may include a generated suffix. Register any custom domain separately.

## GitHub environment secrets

Create a GitHub environment named **`production`** in the repository's Settings → Environments. Set these environment secrets there (Settings → Environments → production → Environment secrets):

| Secret | Value |
| --- | --- |
| `AZURE_CLIENT_ID` | Client ID of the deployment identity with the GitHub federated credential. |
| `AZURE_TENANT_ID` | Microsoft Entra tenant ID for that identity. |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID containing the Web App. |
| `AZURE_WEBAPP_NAME` | Exact Azure Web App resource name. |

These values identify the deployment identity and Web App. They can be stored as secrets even though they are not passwords. The deployment job checks that each one is present before signing in. The environment may have protection rules; a required reviewer will pause deployment until approved. For fully automatic deployment after a main merge, leave required reviewers disabled.

If the `Check Azure deployment settings` step reports an empty value, check that the four entries exist under **Environment secrets** in the GitHub `production` environment with exactly the names above. Azure Web App app settings are a separate store; this workflow reads GitHub environment secrets through the `secrets` context. Rerun the failed workflow after the workflow change reaches `main`.

The workflow uses the repository's `GITHUB_TOKEN` to read pull request labels and create tags/releases. Repository Actions settings must allow workflows to run, and repository tag protection or rulesets must permit the Actions identity to create `v*` tags. The release job declares `contents: write`; no personal access token is required. A branch protection rule can require the pull request **Build and Test** check before a merge.

## Azure Web App settings

Set the following under the Web App's **Environment variables → App settings**. App Service supplies them to ASP.NET Core as environment variables; double underscores represent configuration section separators. For secrets, use App Service settings backed by Azure Key Vault references if available.

| App setting | Value |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | `true` for a Linux Web App, so HTTPS and Auth0 redirect URLs use the original request scheme. |
| `Auth__Domain` | Auth0 tenant domain without `https://` or a trailing slash. |
| `Auth__ClientId` | Production Auth0 Regular Web Application client ID. |
| `Auth__ClientSecret` | Production Auth0 client secret. |
| `Databases__0__ContextName` | `GarageDbContext` |
| `Databases__0__ConnectionString` | Azure SQL connection string for Garage. |

Use **App settings** for `Databases__0__ConnectionString`. A value entered only in App Service's **Connection strings** section is exposed under a different configuration key and will not fill the `Databases` array this application binds. The committed `appsettings.json` contains placeholders; replace them with the settings above in Azure. Do not put live credentials in source control or GitHub workflow YAML.

If Azure SQL uses a managed identity, grant the Web App's identity the required database permissions and use a compatible SQL connection string. If it uses SQL authentication, store the password in the App Service setting or Key Vault. Enable HTTPS-only on the Web App. App Service shares ASP.NET Core Data Protection keys across instances in one slot. Separate deployment slots have separate key rings; configure external shared keys before using slot swaps if existing sign-in cookies must survive a swap.

## Database migrations

The deployment workflow does **not** run EF Core migrations or change production data. Review each migration and apply it to the intended Azure SQL database before deploying code that requires its schema. The current editor requires `AddEditConcurrency`; its unique user/slug index will reject existing duplicate slugs. Back up important data and resolve duplicates first.

To produce a reviewed, idempotent SQL script from the solution root:

```powershell
dotnet ef migrations script --idempotent --project src/DotNetNinja.Garage --startup-project src/DotNetNinja.Garage --context GarageDbContext --output garage-migrations.sql
```

Run that script using an approved database deployment process against the verified database. The startup project requires valid `Auth` and `Databases` configuration while generating the script. The README has local `dotnet ef` commands. A schema rollback must be planned for the specific migration and data; redeploying an older ZIP alone does not undo a database change.

## Versioning and verification

The latest reachable stable `vMAJOR.MINOR.PATCH` tag is the base version. The workflow examines all first-parent commits since that tag so a failed deployment does not lose a PR's version bump. If any merged PR in that range has a `major` or `breaking` label, it increments major; otherwise `minor` or `feature` increments minor; otherwise it increments patch. A direct push to `main` also increments patch. The `no-version-bump` label currently does not suppress a version, because every successful main deployment gets a distinct version. Concurrent main workflows run in order.

The build passes the calculated `MAJOR.MINOR.PATCH` version to `Version`, `AssemblyVersion`, `FileVersion`, and `InformationalVersion`. Source revision metadata is disabled for the informational version, so `/Home/About` displays only `MAJOR.MINOR.PATCH`. Local builds also use a plain three-part version. The ZIP is retained as a workflow artifact and attached to the GitHub release after deployment succeeds. A failed build, test, deployment, or About-page check does not create a tag or release.

After the first deployment, verify the public home page, Auth0 sign-in and sign-out, a signed-in page backed by Azure SQL, and `/Home/About`. The workflow's About-page check confirms startup and the displayed version, but does not exercise Auth0 or database access. To roll back application code, redeploy the ZIP from a previous release through an approved deployment process and separately assess database compatibility.
