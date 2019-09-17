# Zupa.Authentication

The Zupa.Authentication project provides an OAuth/OIDC authentication service for the Zupa platform. This is built using ASPNET Core 2.0, ASPNET Identity, Identity Server 4, Application Insights, Azure App Service, Azure SQL, ARM Templates and Cake.

## External Logins

The external logins operate on callback/redirect URIs. If the application URL changes, then these need to be updated for each external provider.

## Building

To build the Zupa.Authentication solution call the cake script using PowerShell `build/build.ps1` *from the respository root folder* (it has a relative path to build.cake).

## Smoke tests

In order for the smoke tests to work:
1. Chrome must be installed as we're using the Chrome headless driver in Selenium
2. Run the Authentication project without debugger.
3. The url to Zupa.Authentication needs to be in an environment variable `SmokeTestUrl`. This is because test environment urls won't be known until deployment we can't use a settings file and there's no reliable way of passing variables across from the command line. 

   The following Powershell will set the environment variable _for the current process only_ and run the smoke tests. 
   ```
   if (-not (Test-Path env:SmokeTestUrl)) {$env:SmokeTestUrl = 'http://localhost:2662'}
   
   $testPath = "C:\Dev\Zupa.Authentication\test\Zupa.Authentication.SmokeTests"

   dotnet test $testPath -l:trx
   ```

## Azure Deployment (local subscription)

[Azure Resource Manager](https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-group-overview) is the service used to provision resources in your azure subscription via scripts. You can deploy, update, or delete all the resources for your solution in a single, coordinated operation. Handy Quickstart templates are available [here](https://github.com/Azure/azure-quickstart-templates).

To deploy resources using arm then use the following powershell script alongside the *parameters.json* and *template.json* (found in the build folder of the repo). More info can be found [here.](https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-group-template-deploy)
```PowerShell
Login-AzureRmAccount

New-AzureRmResourceGroup -Name TestRG -Location "West Europe" 
New-AzureRmResourceGroupDeployment -Name testdeployment `
                                   -ResourceGroupName TestRG `
                                   -TemplateFile build\template.json `
                                   -TemplateParameterFile build\parameters.json
```
The ps script above will deploy all the azure resources for Zupa Authentication (website, sql server etc). 

Note: The SQL Database Server does not accept being created in `UK South` and `UK West` so it is recommended to use `West Europe` for local deployments. This does not cause issues in Release or Production.

## Octopus Prerequisites

You will need two environment variables when pushing to Octopus:
* **OctopusDeployUrl** should contain the url to the OctopusDeploy instance to use
* **OctopusDeployApiKey** is the value of the Octopus Deploy API key. See the [Octopus Documentation](https://octopus.com/docs/api-and-integration/api/how-to-create-an-api-key) for instructions on how to create the API key.

To build the Zupa.Authentication solution call the cake script using PowerShell `build/build.ps1`.

## First time setup

### Database

The local database will be automatically created if it doesn't already exist using the available migrations. There are currently 3 stores:
  1. EntityFrameworkStore 
  
     This one uses the ApplicationDbContext migrations and stores users
   
  1. ConfigurationStore
  
     This one uses the ConfigurationDbContext migrations and stores clients and resources
   
  1. OpperationalStore
  
     This one uses the PersistedGrantDbContext migrations and stores tokens, codes and consents

The first 2 stores are populated with data found in the Config.cs file in the AuthService project.

### Handling User secrets

During development we will be handling sensitive information such as OAuth client Ids and Secrets, obviously this information should never be committed to the repo. To handle this during the development process we will be using the "Manage User Secrets" function within .Net Core.

When first cloning the repo you should do the following:

1. Find our OAuth IDs/Keys and Secrets for [Twitter](https://apps.twitter.com/app/14680836/keys)  , [Facebook](https://developers.facebook.com/apps/1563899313688247/dashboard/) and [Google](https://console.developers.google.com/apis/credentials/oauthclient/837144219784-3f0c1i6eb68ibk9enn0m9j2jono1t292.apps.googleusercontent.com?project=zupaauthentication) (the login details are in KeyPass)
1. Right click the **"Zupa.Authentication.AuthService"** project and click on the **"Manage User Secrets"** menu item, this will create a file called **"secrets.json"**  
1. Replace the contents of **"secrets.json"** with the following:

```json
{
  "FacebookAuthentication": {
    "FacebookKey": "Swap for real key",
    "FacebookSecret": "Swap for real secret",
  },
  "GoogleAuthentication": {
    "GoogleKey": "Swap for real key",
    "GoogleSecret": "swap for real secret",
  },
  "TwitterAuthentication": {
    "TwitterKey": "Swap for real key",
    "TwitterSecret": "Swap for real secret"
  },
  "ServiceBusConnection": {
    "ServiceBusConnectionString": "Swap for real connection string",
    "QueueName": "Swap for real queue name"
  },
   "AuthenticationTopicClientSettings": {
    "ServiceBusConnectionString": "Swap for real connection string",
    "TopicName":  "Swap for real topic name"
  }
}
```

4. Swap out the placeholder values to the appropriate Keys/Secrets you found in the links above
4. Save and then you're done! 

### Further Reading

* [Authentication Service](/docs/authenticationservice.md)
* [PowerShell Commands](/docs/powershellcommands.md)

## Using the Admin Management Page

### Adding a Client
Click `Add Client` on the Admin Management page and fill in the form.

![Add Client](/docs/AddClient.PNG)

The Api Scopes is a space separated list of the desired scopes. These include:
1. The `openid` scope provides information about an authorised user's unique identity, such as their user ID. Currently this scope is required for all Zupa services.
2. The `profile` scope provides personally identifiable information, such as a users name. If access to these is necessary, this scope may also be needed. All required scopes should be applied within the space separated list, e.g. "openid profile".

Currently the Cors Origin, Redirect Uri and Post Logout Redirect Uri only accept one value on this page.

### Update a Client
Click `Update` next to the desired client on the Admin Management page and fill in the form.

**Note**: The Client ID cannot be changed.

![Update Client](/docs/UpdateClient.PNG)

The Cors Origin is a semicolon separated list and the Redirect Uri and Post Logout Redirect Uri are comma separated.

The Api Scopes are space separated.

### Adding an API Resource
Click `Add Api Resource` on the Admin Management page and fill in the form.

![Add Api Resource](/docs/AddApiResource.PNG)

### Update an Api Resource
Click `Update` next to the desired resource on the Admin Management page and fill in the form.

**Note**: The Resource Name cannot be changed.

![Update Api Resource](/docs/UpdateApiResource.PNG)

### Adding an Identity Resource
Click `Add` next to the desired resource.

![Add Identity Resource](/docs/AddIdentityResource.PNG)

### Adding the Token Client in Release

Click `Add Test Client` on the Admin Management page and fill in the form.

**Note**: The Client ID has to be `TokenClient` as this is hardcoded in the client.

![Add Test Client](/docs/AddTestClient.PNG)

The Api Scopes is a space separated list of the desired scopes.

## Authentication Release Setup Client

This console app was created to seed data to an Auth database, so that releases can be tested more easily.

The aim on the client is to seed data to the Authenticaton database during deployment, including Api Resources, Identity Resources, Clients, and Users.

This can be done manually and automatically on Octopus.

### Usage

#### Manual

The app requires three arguments

* `ConnectionString` - the connection string to use to connect to the Authentication database
* `UserConfig` - a link to a UserConfig json file, containing user seed data
* `ServerConfig` - a link to a ServerConfig json file, containing server configuration data

These can be given in any order, but must be in the format `ArgumentName="value"`. e.g.

```powershell
PS> Zupa.Authentication.ReleaseSetupClient.exe ConnectionString="<connectionString>" ServerConfig="C:\path\to\serverconfig.json" UserConfig="C:\path\to\userconfig.json"
```

Ideally paths to config files shoiuld be absolute (rather than relative) to avoid confusion.

#### Octopus

1. Before the deployment can automatically setup the database, it is necessary to add the `serverconfig.json` and the `userconfig.json` files to the project requiring the setup. Eg when deploying Zupa.Apps.Chat.Web, this is the project which will need to have and run the config setup script.
1. On Octopus, for the project you want to deploy and setup the database automatically, add the following variables - `ConfigProjectExtractPath` and `ReleaseClientExtractPath`.
   * ConfigProjectExtractPath `#{Octopus.Action.Package[<Project that needs setup>].ExtractedPath}` 
   * ReleaseClientExtractPath `#{Octopus.Action.Package[Zupa.Authentication.ReleaseSetupClient].ExtractedPath}`
1. Add a step to `Run a Script`. This step's aim is to run the configuration script which will execute the ReleaseSetupClient with the necessary parameters.

   * In the `Script` section, choose the script's source to be from inside a package.
   * Reference the `Zupa.Authentication.ReleaseSetupClient` package with the script name as `RunConfig.json`.
   * Add the `Zupa.Authentication.ReleaseSetupClient` as an additional package. This allows you to reference a specific version of the package for running the script.
   * Add the project that needs the config setup as an additional package. Eg Zupa.Apps.Chat.Web. This is necessary to allow the script to reference the config files.
   * **Note** If you need to substitute any values in your config files, this is the place to do it, not in your deploy steps. Simply reference the respective file like the following: `#{ConfigProjectExtractPath}\serverconfig.json`.

### User Config

This file contains config to seed users to the database, including an admin user. This removes the need to go through the registration process for each user, or to run manual scripts to give users the admin role. An example is shown below.

```json
{
  "UserDetails": [
    {
      "Email": "test@zupatech.co.uk",
      "Password": "Password0-",
      "IsAdmin": true
    },
    {
      "Email": "test2@zupatech.co.uk",
      "Password": "Password0-",
      "IsAdmin": false
    }    
  ]
}
```

### Server Config

This file sets up configuration for authentication clients (such as services) which want to authenticate with Zupa.Auth, including any API or Identity resources they require. This removes the need to set these up in the UI, which in turn requires a user account with the admin role. An example is shown below.

```json
{
  "Clients": [
    {
      "ClientId": "Zupa.Apps.Chat.Web",
      "ClientName": "Zupa Apps Chat Web",
      "GrantTypes": [ "implicit" ],
      "RedirectUris": [ "http://localhost:33156/callback.html" ],
      "PostLogoutRedirectUris": [ "http://localhost:33156/index.html" ],
      "AllowedCorsOrigins": [ "http://localhost:33156" ],
      "AllowedScopes": [ "openid", "profile" ]
    },
    {
        "ClientId": "TokenClient",
        "ClientName": "Token Client",
        "GrantTypes": [ "password" ],
        "ClientSecrets": [ "secretToken" ],
        "AllowedScopes": [ 
            "openid",
            "profile",
            "Zupa.Products",
            "Zupa.Recipes",
            "Zupa.Organisations",
            "Zupa.Contacts",
            "Zupa.Media"
        ]
    }
  ],
  "ApiResources": [
    {
      "Name": "Zupa.Messaging",
      "DisplayName": "Zupa Messaging"
    },
    {
      "Name": "Zupa.Media",
      "DisplayName": "Zupa Media"
    },
    {
      "Name": "Zupa.Safe",
      "DisplayName": "Zupa Safe"
    },
    {
      "Name": "Zupa.Contacts",
      "DisplayName": "Zupa Contacts"
    },
    {
      "Name": "Zupa.Groups",
      "DisplayName": "Zupa Groups"
    }
  ],
  "IdentityResources": [ "openid", "profile" ]
}
```
