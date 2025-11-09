# Entra ID Authentication Setup

This document explains how to configure Azure Entra ID (formerly Azure AD) authentication for the InstanceManager Blazor WebAssembly application.

## Overview

The application now uses Microsoft Authentication Library (MSAL) to authenticate users via Azure Entra ID. The authentication flow uses OpenID Connect and OAuth 2.0 protocols.

## Azure Entra ID Configuration

### 1. Register the Blazor WebAssembly Application

1. Go to the [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Active Directory** > **App registrations** > **New registration**
3. Configure the app registration:
   - **Name**: `InstanceManager Web App` (or your preferred name)
   - **Supported account types**: Choose based on your requirements (typically "Accounts in this organizational directory only")
   - **Redirect URI**:
     - Platform: **Single-page application (SPA)**
     - URI: `https://localhost:7023/authentication/login-callback` (for local development)
     - URI: `http://localhost:5070/authentication/login-callback` (for local development HTTP)
     - Add production URLs as needed
4. Click **Register**
5. Note the **Application (client) ID** and **Directory (tenant) ID** from the Overview page

### 2. Configure Authentication

1. In your app registration, go to **Authentication**
2. Under **Single-page application**, ensure redirect URIs are set:
   - `https://localhost:7023/authentication/login-callback`
   - `http://localhost:5070/authentication/login-callback`
3. Add logout redirect URIs:
   - `https://localhost:7023/authentication/logout-callback`
   - `http://localhost:5070/authentication/logout-callback`
4. Under **Implicit grant and hybrid flows**, ensure nothing is checked (SPA uses PKCE flow)
5. Save changes

### 3. Register the API Application (if using separate API authentication)

If your Azure Functions API also needs to validate tokens:

1. Create another app registration for the API:
   - **Name**: `InstanceManager API`
   - **Supported account types**: Same as the web app
   - **Redirect URI**: Leave blank (this is for the API)
2. Go to **Expose an API**
3. Click **Add a scope**
   - **Application ID URI**: Accept the default or customize (e.g., `api://instancemanager-api`)
   - **Scope name**: `access_as_user`
   - **Who can consent**: Admins and users
   - **Admin consent display name**: `Access InstanceManager API`
   - **Admin consent description**: `Allows the app to access InstanceManager API on behalf of the signed-in user`
   - **User consent display name**: `Access InstanceManager API`
   - **User consent description**: `Allows the app to access InstanceManager API on your behalf`
   - **State**: Enabled
4. Click **Add scope**
5. Note the **Application ID URI** (e.g., `api://abc123-def456...`)

### 4. Authorize the Web App to Access the API

1. Go back to your **Web App** registration
2. Navigate to **API permissions**
3. Click **Add a permission** > **My APIs**
4. Select your **InstanceManager API** app
5. Select **Delegated permissions**
6. Check `access_as_user`
7. Click **Add permissions**
8. Click **Grant admin consent** (if you have admin rights)

## Application Configuration

### Update appsettings.json

Edit `/mnt/c/Workspace/Projects/InstanceManager/InstanceManager.Host.WA/wwwroot/appsettings.json`:

```json
{
  "AzureAd": {
    "Authority": "https://login.microsoftonline.com/{TENANT_ID}",
    "ClientId": "{WEB_APP_CLIENT_ID}",
    "ValidateAuthority": true,
    "DefaultScopes": [
      "api://{API_CLIENT_ID}/access_as_user"
    ]
  },
  "ApiBaseUrl": "http://localhost:7233/api/"
}
```

Replace the placeholders:
- `{TENANT_ID}`: Your Azure AD tenant ID (from the Overview page)
- `{WEB_APP_CLIENT_ID}`: The Application (client) ID of your Web App registration
- `{API_CLIENT_ID}`: The Application ID URI of your API registration (e.g., `abc123-def456...`)

### Example Configuration

```json
{
  "AzureAd": {
    "Authority": "https://login.microsoftonline.com/12345678-1234-1234-1234-123456789012",
    "ClientId": "87654321-4321-4321-4321-210987654321",
    "ValidateAuthority": true,
    "DefaultScopes": [
      "api://abcdef12-3456-7890-abcd-ef1234567890/access_as_user"
    ]
  },
  "ApiBaseUrl": "http://localhost:7233/api/"
}
```

## Features Implemented

### 1. Authentication Components

- **LoginDisplay**: Shows login/logout button and user name in the header
- **Authentication.razor**: Handles authentication callbacks and redirects
- **App.razor**: Configured with `CascadingAuthenticationState` and `AuthorizeRouteView`

### 2. Default Authorization Policy

**All pages require authentication by default** via a `FallbackPolicy` configured in `Program.cs`:

```csharp
builder.Services.AddAuthorizationCore(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
```

This approach:
- Requires authentication for all pages without needing individual `[Authorize]` attributes
- Pages can opt out using `[AllowAnonymous]` if needed
- Only the authentication callback page (`/authentication/{action}`) is marked as `[AllowAnonymous]`

### 3. API Integration

The `HttpRequestSender` service automatically attaches access tokens to all API requests using `BaseAddressAuthorizationMessageHandler`. It also handles:
- Token unavailability (redirects to login)
- 401 Unauthorized responses (with error handling)

### 4. User Experience

- Users see a login button when not authenticated
- After login, user name is displayed in the header
- Attempting to access protected pages without authentication shows a login prompt
- Access token is automatically renewed when needed

## Testing Authentication

### Local Development

1. Update `appsettings.json` with your Azure AD configuration
2. Run the application:
   ```bash
   dotnet run --project InstanceManager.Host.WA/InstanceManager.Host.WA.csproj
   ```
3. Navigate to `http://localhost:5070`
4. Click **Log in** button
5. Sign in with your Azure AD credentials
6. You should be redirected back and see your user name in the header

### Troubleshooting

**Issue**: Infinite redirect loop
- **Solution**: Verify redirect URIs in Azure AD exactly match your app URLs

**Issue**: CORS errors
- **Solution**: Ensure the API allows requests from your web app origin

**Issue**: "User not found" or "Cannot sign in"
- **Solution**: Verify the user account exists in your Azure AD tenant

**Issue**: Token not attached to API requests
- **Solution**: Verify the `DefaultScopes` in appsettings.json match your API's exposed scope

**Issue**: "Invalid audience" error from API
- **Solution**: Ensure your API is configured to validate tokens from your Azure AD tenant

## Next Steps

### API Authentication (Optional)

If you want the Azure Functions API to also validate JWT tokens:

1. Add `Microsoft.Identity.Web` NuGet package to `InstanceManager.Host.AzFuncAPI`
2. Configure JWT bearer authentication in the Azure Functions startup
3. Update the API to validate tokens from your Azure AD tenant
4. Ensure the API validates the audience matches your API's Application ID URI

### Production Deployment

1. Add production redirect URIs to Azure AD app registration
2. Update `appsettings.json` (or use Azure App Configuration) with production URLs
3. Configure Azure App Service authentication if hosting in Azure

## Additional Resources

- [Microsoft Identity Platform documentation](https://learn.microsoft.com/en-us/azure/active-directory/develop/)
- [Secure ASP.NET Core Blazor WebAssembly](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/)
- [MSAL.js documentation](https://learn.microsoft.com/en-us/azure/active-directory/develop/msal-overview)
