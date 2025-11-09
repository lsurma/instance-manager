# Authorization Fix - API Key Authentication Now Enforced

## Problem Identified

API Key authentication was enabled in `local.settings.json` but **not being enforced**. Requests without the `X-API-Key` header were succeeding when they should have been rejected with 401 Unauthorized.

### Root Cause

Azure Functions Isolated Worker (.NET 9) requires explicit middleware to enforce ASP.NET Core authorization policies. The FallbackPolicy was configured, but there was no middleware to check it.

## Solution Implemented

### 1. Created Authorization Middleware

**File:** `InstanceManager.Host.AzFuncAPI/Middleware/FunctionsAuthorizationMiddleware.cs`

This middleware:
- Checks if authentication is required (from `AuthenticationSettings`)
- Skips check if APIM bypass is active
- Validates that the user is authenticated via HttpContext
- Returns **401 Unauthorized** if not authenticated
- Logs all authorization attempts

### 2. Updated Program.cs

**Changes:**
- Moved `authSettings` loading to the beginning (needed for middleware configuration)
- Added `FunctionsAuthorizationMiddleware` to the middleware pipeline
- Configured APIM bypass middleware to only run when enabled
- Fixed duplicate `authSettings` registration

**Key Lines:**
```csharp
// Line 22: Store settings first
builder.Services.AddSingleton(authSettings);

// Line 25: Configure middleware
var functionsBuilder = builder.ConfigureFunctionsWebApplication();

// Line 28-34: APIM bypass (conditional)
functionsBuilder.UseWhen<ApimBypassMiddleware>(context => ...);

// Line 37: Authorization enforcement (always runs if RequireAuthentication=true)
functionsBuilder.UseMiddleware<FunctionsAuthorizationMiddleware>();
```

## Testing the Fix

### Step 1: Verify Configuration

Your `local.settings.json` should have:
```json
{
  "Values": {
    "Authentication__RequireAuthentication": "true",
    "Authentication__ApiKeys__Enabled": "true",
    "Authentication__ApiKeys__Keys__Development": "dev-key-12345"
  }
}
```

### Step 2: Start the API

```bash
dotnet run --project InstanceManager.Host.AzFuncAPI/InstanceManager.Host.AzFuncAPI.csproj
```

**Check the logs for:**
```
Now listening on: http://localhost:7233
```

### Step 3: Test WITHOUT API Key (Should FAIL)

**Using Postman:**
- Open: `ProjectInstances → Get All Project Instances`
- Make sure NO `X-API-Key` header is present
- Click **Send**

**Expected Result:**
- **Status: 401 Unauthorized**
- Response body:
  ```json
  {
    "error": "Unauthorized",
    "message": "Authentication is required. Please provide valid credentials.",
    "authMethods": ["API Key (X-API-Key header)"]
  }
  ```

**Using curl:**
```bash
curl http://localhost:7233/api/query/GetProjectInstancesQuery
```

**Expected:** 401 Unauthorized

### Step 4: Test WITH Valid API Key (Should SUCCEED)

**Using Postman:**
- Open: `With API Key Auth → Get Project Instances (API Key)`
- Verify `X-API-Key: dev-key-12345` header is present
- Click **Send**

**Expected Result:**
- **Status: 200 OK**
- Response body contains project instances data

**Using curl:**
```bash
curl -H "X-API-Key: dev-key-12345" http://localhost:7233/api/query/GetProjectInstancesQuery
```

**Expected:** 200 OK with data

### Step 5: Test WITH Invalid API Key (Should FAIL)

**Using Postman:**
- Add header: `X-API-Key: wrong-key-12345`
- Click **Send**

**Expected Result:**
- **Status: 401 Unauthorized**

**Using curl:**
```bash
curl -H "X-API-Key: wrong-key-12345" http://localhost:7233/api/query/GetProjectInstancesQuery
```

**Expected:** 401 Unauthorized

### Step 6: Verify Logs

**Check API logs for:**

**Unauthorized request:**
```
[Warning] Unauthorized request | User not authenticated
```

**Authorized request:**
```
[Information] Request authorized | User: Development | Authenticated: True
```

## What Changed

### New Files

1. **`FunctionsAuthorizationMiddleware.cs`** - Enforces authentication based on configuration

### Modified Files

1. **`Program.cs`**
   - Restructured middleware configuration
   - Added authorization middleware to pipeline
   - Fixed authSettings registration order

### Behavior Changes

| Scenario | Before Fix | After Fix |
|----------|------------|-----------|
| API Key enabled, NO key sent | ✅ 200 OK (WRONG!) | ❌ 401 Unauthorized (CORRECT!) |
| API Key enabled, Valid key sent | ✅ 200 OK | ✅ 200 OK |
| API Key enabled, Invalid key sent | ✅ 200 OK (WRONG!) | ❌ 401 Unauthorized (CORRECT!) |
| RequireAuthentication=false | ✅ 200 OK | ✅ 200 OK |

## Authentication Methods Supported

All authentication methods are now properly enforced:

### 1. API Key (X-API-Key header)

```bash
curl -H "X-API-Key: dev-key-12345" http://localhost:7233/api/query/...
```

### 2. JWT Bearer Token (Authorization header)

```bash
curl -H "Authorization: Bearer YOUR_TOKEN" http://localhost:7233/api/query/...
```

### 3. APIM Simulation (Multiple headers)

```bash
curl -H "X-APIM-Secret: apim-backend-secret-12345" \
     -H "X-User-Id: alice@example.com" \
     http://localhost:7233/api/query/...
```

## Middleware Execution Order

When a request comes in, middleware runs in this order:

1. **ApimBypassMiddleware** (if `TrustApim: true`)
   - Checks for `X-APIM-Secret` header
   - Validates secret matches configuration
   - Extracts user info from APIM headers

2. **ASP.NET Core Authentication** (built-in)
   - Runs API Key authentication handler
   - Runs JWT Bearer authentication handler
   - Sets `HttpContext.User`

3. **FunctionsAuthorizationMiddleware** (new!)
   - Checks if authentication is required
   - Validates `HttpContext.User.Identity.IsAuthenticated`
   - Returns 401 if not authenticated
   - Allows request if authenticated

4. **QueryController.Query()** method
   - Processes the request
   - Uses `ICurrentUserService` to get user identity
   - Executes MediatR handler

## Configuration Reference

### Disable Authentication (for quick testing)

```json
{
  "Authentication__RequireAuthentication": "false"
}
```

### Enable API Key Only

```json
{
  "Authentication__RequireAuthentication": "true",
  "Authentication__ApiKeys__Enabled": "true",
  "Authentication__ApiKeys__Keys__Development": "dev-key-12345",
  "Authentication__EntraId__Enabled": "false"
}
```

### Enable JWT Only

```json
{
  "Authentication__RequireAuthentication": "true",
  "Authentication__EntraId__Enabled": "true",
  "Authentication__EntraId__TenantId": "YOUR-TENANT-ID",
  "Authentication__EntraId__ClientId": "YOUR-CLIENT-ID",
  "Authentication__EntraId__Audience": "api://YOUR-API-ID",
  "Authentication__ApiKeys__Enabled": "false"
}
```

### Enable Both (API Key OR JWT)

```json
{
  "Authentication__RequireAuthentication": "true",
  "Authentication__ApiKeys__Enabled": "true",
  "Authentication__ApiKeys__Keys__Development": "dev-key-12345",
  "Authentication__EntraId__Enabled": "true",
  "Authentication__EntraId__TenantId": "YOUR-TENANT-ID",
  "Authentication__EntraId__ClientId": "YOUR-CLIENT-ID",
  "Authentication__EntraId__Audience": "api://YOUR-API-ID"
}
```

### Enable APIM Bypass

```json
{
  "Authentication__RequireAuthentication": "true",
  "Authentication__Apim__TrustApim": "true",
  "Authentication__Apim__SharedSecret": "apim-backend-secret-12345",
  "Authentication__Apim__RequireSharedSecret": "true"
}
```

**Note:** When APIM bypass is enabled, the `FunctionsAuthorizationMiddleware` skips the authentication check because APIM is trusted to have already authenticated the user.

## Troubleshooting

### Issue: Still getting 200 OK without API key

**Check:**
1. Did you restart the API after making changes?
2. Is `RequireAuthentication` set to `"true"` (string, not boolean)?
3. Check logs for "Authentication not required or APIM bypass active"
   - If you see this, APIM bypass might be enabled
   - Set `TrustApim: "false"` in config

### Issue: Getting 401 when API key IS provided

**Check:**
1. Header name is `X-API-Key` (case-sensitive)
2. Key value matches `Authentication__ApiKeys__Keys__Development` in config
3. Check logs for "Unauthorized request | User not authenticated"
4. Try restarting the API

### Issue: HttpContext is null

**Check:**
1. `Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore` package is installed
2. Version is 2.0.2 or higher
3. Check for this warning in logs: "HttpContext is null - unable to perform authorization"

## Security Best Practices

✅ **DO:**
- Always use `RequireAuthentication: true` in production
- Use strong, random API keys
- Store API keys in Azure Key Vault for production
- Rotate API keys regularly
- Use HTTPS in production (automatic with Azure Functions)

❌ **DON'T:**
- Commit `local.settings.json` with real secrets to git
- Use simple/guessable API keys
- Disable authentication in production
- Use the same API key across environments

## Related Documentation

- **[AUTHENTICATION.md](AUTHENTICATION.md)** - Complete authentication guide
- **[POSTMAN_TESTING.md](POSTMAN_TESTING.md)** - Postman testing with all auth methods
- **[postman/APIM_TESTING.md](postman/APIM_TESTING.md)** - APIM simulation testing
- **[USER_TRACKING.md](USER_TRACKING.md)** - User identity tracking

## Summary

✅ Authorization is now properly enforced
✅ Middleware checks authentication before allowing requests
✅ 401 Unauthorized returned when credentials are missing/invalid
✅ User identity tracking works with all auth methods
✅ APIM bypass continues to work when enabled
✅ All existing tests should still pass

**The fix ensures that when you enable authentication, it is actually enforced!**
