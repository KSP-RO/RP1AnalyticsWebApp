# RP-1 Analytics web application
Receives career progress information from the game and shows various useful visualizations for comparing careers and balancing RP-1.

## Technology stack
* .NET 9
* ASP.NET Core 9 (Razor pages + web API)
* Vue.js 3
* MongoDB
* Microsoft.AspNetCore.OData
* Application Insights
* AspNetCore.Identity.Mongo
* AspNet.Security.OAuth.GitHub
* Swagger / OpenAPI
* Vite.AspNetCore
* Node.js 20

## Install
### Production
Primarily meant to run on the MS Azure platform but should also run on other compatible hosting options (minus Application Insights).

User roles can be configured on the `/Admin/Users` page which is only available to the `Admin` role. The first admin user needs to be manually configured at the database-level. To do this, find the id of the admin role in the `Roles` table and add it to the user document in `Users` table.
I.e:
```
    "Roles" : [
        "60ef11b715457b02f350d3c9"
    ],
```

### Development
Add appsettings.json to \RP1AnalyticsWebApp\ and hit F5 in VS. Npm install will be run automatically and vite dev server will start on first request.
Example settings file:
```
{
  "ApplicationInsights": {
    "InstrumentationKey": "[GUID, optional]"
  },
  "CareerLogDatabaseSettings": {
    "CareerLogsCollectionName": "[Mongo Collection name, mandatory]",
    "DatabaseName": "[Mongo DB name, mandatory]",
    "ConnectionString": "[Mongo DB connection string, needs to contain the same DB name configured above as a parameter, mandatory]"
  },
  "Authentication": {
    "GitHub": {
      "ClientId": "[Client ID of the GitHub OAuth App, mandatory]",
      "ClientSecret": "[Client secret of the app, mandatory]"
    }
  },
  "AppSettings": {
    "UserDefaultRoles": ["Member"]
  }
}
```

## API documentation
Swagger definitions are available at the following endpoint: /swagger/
