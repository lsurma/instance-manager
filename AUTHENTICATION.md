# Authentication Configuration

This document describes how authentication is configured in the InstanceManager API.

## Overview

The API supports three authentication methods:
1. **JWT Bearer Tokens** from Entra ID (Azure AD)
2. **Custom API Keys** via X-API-Key header
3. **Azure API Management (APIM)** with shared secret

All authentication methods are integrated with the **user identity tracking system** that automatically logs who performs each operation and tracks entity creators/modifiers. See [USER_TRACKING.md](USER_TRACKING.md) for details.

Both JWT and API Key methods can be enabled simultaneously, and the API will accept either form of authentication.

## Configuration

Authentication is configured in `local.settings.json` (local development) or Application Settings (Azure):

### Disable Authentication (Default for Development)

```json
{
  "Values": {
    "Authentication__RequireAuthentication": "false"
  }
}
```

### Enable Entra ID (Azure AD) Authentication

```json
{
  "Values": {
    "Authentication__RequireAuthentication": "true",
    "Authentication__EntraId__Enabled": "true",
    "Authentication__EntraId__Instance": "https://login.microsoftonline.com/",
    "Authentication__EntraId__TenantId": "your-tenant-id",
    "Authentication__EntraId__ClientId": "your-client-id",
    "Authentication__EntraId__Audience": "api://your-api-id"
  }
}
```

#### Setting up Entra ID

1. Register an application in Azure AD
2. Configure API permissions
3. Create App Roles (optional)
4. Copy the Tenant ID and Client ID to configuration
5. Set the Audience to match your API's identifier

### Enable API Key Authentication

```json
{
  "Values": {
    "Authentication__RequireAuthentication": "true",
    "Authentication__ApiKeys__Enabled": "true",
    "Authentication__ApiKeys__Keys__ServiceName": "your-api-key-here",
    "Authentication__ApiKeys__Keys__AnotherService": "another-key-here"
  }
}
```

**Note:** Each key is a dictionary entry where:
- **Key**: Name/description of the service using the API key
- **Value**: The actual API key

## Using Authentication

### JWT Bearer Token

Include the token in the Authorization header:

```http
GET /api/query/GetTranslationsQuery
Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGc...
```

### API Key

Include the API key in the X-API-Key header:

```http
GET /api/query/GetTranslationsQuery
X-API-Key: your-api-key-here
```

## Authorization Policies

The following policies are available:

### Default Policy

When `RequireAuthentication` is `true`, all endpoints require authentication by default (any valid token or API key).

### Named Policies

- **ApiKeyPolicy**: Requires API Key authentication only
- **JwtBearerPolicy**: Requires JWT Bearer authentication only
- **ApiOrJwtPolicy**: Accepts either authentication method (default behavior)

### Using Policies in Code

```csharp
[Authorize(Policy = "ApiKeyPolicy")]
public async Task<IActionResult> SomeMethod() { }
```

## Security Best Practices

1. **Never commit API keys to source control**
   - Use local.settings.json (excluded from git)
   - Use Azure App Service Configuration for production

2. **Rotate API keys regularly**
   - Update the configuration
   - Deploy the new configuration

3. **Use HTTPS in production**
   - Azure Functions automatically use HTTPS
   - Enforce HTTPS in your infrastructure

4. **Monitor authentication failures**
   - Check Application Insights logs
   - Set up alerts for unusual patterns

5. **Limit API key distribution**
   - Create separate keys for different services
   - Revoke keys when no longer needed

## Troubleshooting

### Authentication not working

1. Check `RequireAuthentication` is set to `"true"`
2. Verify the authentication method is enabled
3. Ensure credentials are correct
4. Check Application Insights for detailed error messages

### JWT token validation fails

1. Verify Tenant ID, Client ID, and Audience match your Azure AD configuration
2. Ensure token is not expired
3. Check token has required scopes/permissions

### API key not recognized

1. Verify the key exists in configuration under `Authentication__ApiKeys__Keys__*`
2. Ensure the key is sent in the `X-API-Key` header
3. Check for typos in the key value

## Development vs Production

### Local Development (Disabled by Default)

```json
{
  "Authentication__RequireAuthentication": "false"
}
```

This allows easy local testing without authentication.

### Production (Enabled)

Set in Azure Portal > Function App > Configuration:

```
Authentication__RequireAuthentication = true
Authentication__EntraId__Enabled = true
Authentication__EntraId__TenantId = <your-tenant>
Authentication__EntraId__ClientId = <your-client>
Authentication__EntraId__Audience = <your-api-uri>
```

Or enable API keys:

```
Authentication__RequireAuthentication = true
Authentication__ApiKeys__Enabled = true
Authentication__ApiKeys__Keys__Production = <secure-key>
```

## Example: Getting a JWT Token from Azure AD

```bash
# Replace with your values
TENANT_ID="your-tenant-id"
CLIENT_ID="your-client-id"
CLIENT_SECRET="your-client-secret"
SCOPE="api://your-api-id/.default"

curl -X POST "https://login.microsoftonline.com/$TENANT_ID/oauth2/v2.0/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=$CLIENT_ID" \
  -d "client_secret=$CLIENT_SECRET" \
  -d "scope=$SCOPE" \
  -d "grant_type=client_credentials"
```

The response will contain an `access_token` that can be used as the Bearer token.
