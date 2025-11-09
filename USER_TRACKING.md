# User Identity Tracking and Auditing

This document explains how user identity tracking and auditing works in InstanceManager across all authentication methods.

## Overview

The application automatically tracks **who** performs **what** actions, regardless of authentication method:
- **JWT Bearer Tokens** (Entra ID / Azure AD)
- **API Keys** (X-API-Key header)
- **APIM** (Azure API Management with forwarded identity)

Every database operation automatically captures:
- `CreatedBy` - User ID who created the entity
- `CreatedAt` - Timestamp when created
- `UpdatedBy` - User ID who last updated the entity
- `UpdatedAt` - Timestamp when last updated

Additionally, every MediatR request is logged with full user context for auditing and troubleshooting.

## Architecture

### Key Components

1. **`UserIdentity`** - Universal user identity model
   - Location: `InstanceManager.Application.Contracts/Common/UserIdentity.cs`
   - Represents user across all auth methods
   - Includes UserId, DisplayName, Email, AuthenticationMethod

2. **`ICurrentUserService`** - Service to access current user
   - Location: `InstanceManager.Application.Contracts/Common/ICurrentUserService.cs`
   - Interface for getting current authenticated user
   - Implementation: `CurrentUserService` in Application.Core

3. **`CurrentUserService`** - Extracts user from HTTP context
   - Location: `InstanceManager.Application.Core/Common/CurrentUserService.cs`
   - Parses claims from JWT, API Key, or APIM headers
   - Returns universal `UserIdentity` object

4. **`LoggingBehavior`** - MediatR pipeline for request logging
   - Location: `InstanceManager.Application.Core/Common/LoggingBehavior.cs`
   - Automatically logs all requests with user context
   - Logs: Request name, User ID, Auth method, Success/Failure

5. **`InstanceManagerDbContext`** - Auto-sets audit fields
   - Location: `InstanceManager.Application.Core/Data/InstanceManagerDbContext.cs`
   - Automatically sets `CreatedBy`/`UpdatedBy` on SaveChanges
   - Uses `ICurrentUserService` to get current user

## How It Works

### User Identity Extraction

#### 1. JWT Bearer Token (Entra ID)

```csharp
// Extracts from standard JWT claims
UserId = oid (Object ID) or sub (Subject)
DisplayName = name or preferred_username
Email = email or upn (User Principal Name)
AuthenticationMethod = JWT
```

**Example:**
```json
{
  "oid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "name": "John Doe",
  "email": "john.doe@example.com",
  "tid": "tenant-id-here"
}
```

Results in:
```csharp
UserIdentity {
  UserId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  DisplayName = "John Doe",
  Email = "john.doe@example.com",
  AuthenticationMethod = JWT
}
```

#### 2. API Key Authentication

```csharp
// Extracts from API key claims
UserId = KeyName (e.g., "Development", "ServiceAccount")
DisplayName = KeyName
AuthenticationMethod = APIKey
```

**Example** with `X-API-Key: dev-key-12345` mapped to "Development":
```csharp
UserIdentity {
  UserId = "Development",
  DisplayName = "Development",
  AuthenticationMethod = APIKey,
  AdditionalClaims = { "ApiKeyHash": "***2345" }
}
```

#### 3. APIM Gateway

```csharp
// Extracts from forwarded headers
UserId = X-User-Id header (or "apim-gateway")
DisplayName = X-User-Name or X-User-Email
Email = X-User-Email
AuthenticationMethod = APIM
```

**Example** when APIM forwards identity:
```http
X-User-Id: a1b2c3d4-e5f6-7890-abcd-ef1234567890
X-User-Name: John Doe
X-User-Email: john.doe@example.com
X-Subscription-Name: Premium-Subscription
X-Auth-Method: JWT
```

Results in:
```csharp
UserIdentity {
  UserId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  DisplayName = "John Doe",
  Email = "john.doe@example.com",
  AuthenticationMethod = APIM,
  AdditionalClaims = {
    "SubscriptionName": "Premium-Subscription",
    "OriginalAuthMethod": "JWT"
  }
}
```

### Automatic Database Auditing

When you save an entity, audit fields are **automatically populated**:

```csharp
// In your handler - just save normally
var instance = new ProjectInstance
{
    Id = Guid.NewGuid(),
    Name = "New Project",
    // CreatedBy is set automatically!
};

await _context.ProjectInstances.AddAsync(instance);
await _context.SaveChangesAsync(); // <-- Audit fields set here

// After SaveChanges:
// instance.CreatedBy = "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
// instance.CreatedAt = 2025-11-09T15:30:00Z
```

For updates:
```csharp
var instance = await _context.ProjectInstances.FindAsync(id);
instance.Name = "Updated Name";

await _context.SaveChangesAsync(); // <-- UpdatedBy and UpdatedAt set automatically

// After SaveChanges:
// instance.UpdatedBy = "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
// instance.UpdatedAt = 2025-11-09T15:35:00Z
```

### Automatic Request Logging

Every MediatR request is automatically logged with user context:

```
[2025-11-09 15:30:00] INFO: Executing request: SaveProjectInstanceCommand
  | User: John Doe (JWT)
  | Auth: JWT
  | UserId: a1b2c3d4-e5f6-7890-abcd-ef1234567890

[2025-11-09 15:30:01] INFO: Completed request: SaveProjectInstanceCommand
  | User: John Doe (JWT)
```

On errors:
```
[2025-11-09 15:30:00] ERROR: Request failed: SaveProjectInstanceCommand
  | User: John Doe (JWT)
  | Error: Name is required
```

## Usage Examples

### Getting Current User in Code

```csharp
public class MyHandler : IRequestHandler<MyCommand, MyResponse>
{
    private readonly ICurrentUserService _currentUserService;

    public MyHandler(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public async Task<MyResponse> Handle(MyCommand request, CancellationToken ct)
    {
        // Get full user identity
        var user = _currentUserService.GetCurrentUser();
        Console.WriteLine($"User: {user.ToLogString()}"); // "John Doe (JWT)"

        // Get just the user ID
        var userId = _currentUserService.GetUserId();
        Console.WriteLine($"UserId: {userId}"); // "a1b2c3d4-e5f6-7890-abcd-ef1234567890"

        // Get display name
        var displayName = _currentUserService.GetUserDisplayName();
        Console.WriteLine($"Name: {displayName}"); // "John Doe"

        // Check if authenticated
        if (_currentUserService.IsAuthenticated())
        {
            // User is authenticated
        }

        // Access additional claims
        if (user.AdditionalClaims.TryGetValue("TenantId", out var tenantId))
        {
            Console.WriteLine($"Tenant: {tenantId}");
        }

        return new MyResponse();
    }
}
```

### Viewing Audit Information

```csharp
// Query entities and see who created/updated them
var instances = await _context.ProjectInstances.ToListAsync();

foreach (var instance in instances)
{
    Console.WriteLine($"Created by: {instance.CreatedBy} at {instance.CreatedAt}");

    if (instance.UpdatedBy != null)
    {
        Console.WriteLine($"Last updated by: {instance.UpdatedBy} at {instance.UpdatedAt}");
    }
}
```

### Custom Logging with User Context

```csharp
public class MyHandler : IRequestHandler<MyCommand, MyResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MyHandler> _logger;

    public async Task<MyResponse> Handle(MyCommand request, CancellationToken ct)
    {
        var user = _currentUserService.GetCurrentUser();

        _logger.LogInformation(
            "Processing important operation for user {User} via {AuthMethod}",
            user.UserId,
            user.AuthenticationMethod);

        // Your logic here...

        _logger.LogInformation(
            "Operation completed successfully for user {User}",
            user.ToLogString());

        return new MyResponse();
    }
}
```

## APIM Configuration for User Forwarding

To forward user identity from APIM to your backend, update your APIM inbound policy:

### Enhanced JWT Policy with User Forwarding

```xml
<policies>
    <inbound>
        <base />

        <!-- Validate JWT token from Azure AD -->
        <validate-jwt header-name="Authorization"
                      failed-validation-httpcode="401"
                      failed-validation-error-message="Unauthorized">
            <openid-config url="https://login.microsoftonline.com/{{entra-tenant-id}}/v2.0/.well-known/openid-configuration" />
            <audiences>
                <audience>{{entra-audience}}</audience>
            </audiences>
        </validate-jwt>

        <!-- Forward user identity to backend -->
        <set-header name="X-User-Id" exists-action="override">
            <value>@(context.Request.Headers.GetValueOrDefault("Authorization","").AsJwt()?.Claims.GetValueOrDefault("oid", "unknown"))</value>
        </set-header>

        <set-header name="X-User-Name" exists-action="override">
            <value>@(context.Request.Headers.GetValueOrDefault("Authorization","").AsJwt()?.Claims.GetValueOrDefault("name", "unknown"))</value>
        </set-header>

        <set-header name="X-User-Email" exists-action="override">
            <value>@(context.Request.Headers.GetValueOrDefault("Authorization","").AsJwt()?.Claims.GetValueOrDefault("email", "unknown"))</value>
        </set-header>

        <!-- Add subscription info for additional context -->
        <set-header name="X-Subscription-Name" exists-action="override">
            <value>@(context.Subscription?.Name ?? "unknown")</value>
        </set-header>

        <set-header name="X-Subscription-Id" exists-action="override">
            <value>@(context.Subscription?.Id ?? "unknown")</value>
        </set-header>

        <!-- Indicate auth method -->
        <set-header name="X-Auth-Method" exists-action="override">
            <value>JWT</value>
        </set-header>

        <!-- Add shared secret for backend authentication -->
        <set-header name="X-APIM-Secret" exists-action="override">
            <value>{{apim-backend-secret}}</value>
        </set-header>

        <set-backend-service base-url="https://your-function-app.azurewebsites.net" />
    </inbound>

    <backend>
        <base />
    </backend>

    <outbound>
        <base />

        <!-- Clean up internal headers -->
        <set-header name="X-APIM-Secret" exists-action="delete" />
        <set-header name="X-Subscription-Name" exists-action="delete" />
        <set-header name="X-Subscription-Id" exists-action="delete" />
        <set-header name="X-Auth-Method" exists-action="delete" />
    </outbound>

    <on-error>
        <base />
    </on-error>
</policies>
```

## Database Schema

All auditable entities inherit from `AuditableEntityBase` and have these fields:

```sql
CREATE TABLE ProjectInstances (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    -- ... other fields ...
    CreatedBy TEXT NOT NULL,        -- User ID who created
    CreatedAt INTEGER NOT NULL,     -- UTC ticks
    UpdatedBy TEXT NULL,            -- User ID who last updated
    UpdatedAt INTEGER NULL          -- UTC ticks
);
```

**Note:** `CreatedAt` and `UpdatedAt` are stored as UTC ticks (long) in SQLite for proper sorting/filtering, but exposed as `DateTimeOffset` in C# code.

## Anonymous and System Users

When no authentication is present or when running background jobs:

```csharp
// Anonymous user (no auth)
UserIdentity.Anonymous()
// => UserId = "anonymous", AuthenticationMethod = None

// System user (background jobs, migrations)
UserIdentity.System()
// => UserId = "system", AuthenticationMethod = System
```

These are automatically used when:
- No `HttpContext` is available (background jobs)
- No authentication credentials provided
- `ICurrentUserService` is not available (e.g., in migrations)

## Querying by User

You can filter entities by who created or updated them:

```csharp
// Get all instances created by a specific user
var userInstances = await _context.ProjectInstances
    .Where(i => i.CreatedBy == userId)
    .ToListAsync();

// Get all instances modified by a specific user
var modifiedByUser = await _context.ProjectInstances
    .Where(i => i.UpdatedBy == userId)
    .ToListAsync();

// Get recent activity by user
var recentUserActivity = await _context.ProjectInstances
    .Where(i => i.CreatedBy == userId || i.UpdatedBy == userId)
    .OrderByDescending(i => i.UpdatedAt ?? i.CreatedAt)
    .Take(10)
    .ToListAsync();
```

## Application Insights Integration

All user activity is automatically logged to Application Insights when enabled:

```json
{
  "timestamp": "2025-11-09T15:30:00Z",
  "level": "Information",
  "message": "Executing request: SaveProjectInstanceCommand",
  "properties": {
    "RequestName": "SaveProjectInstanceCommand",
    "User": "John Doe (JWT)",
    "AuthMethod": "JWT",
    "UserId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
  }
}
```

You can query Application Insights for user activity:

```kusto
traces
| where customDimensions.UserId == "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
| project timestamp, message, customDimensions.RequestName
| order by timestamp desc
| take 100
```

## Troubleshooting

### User ID shows as "system" or "anonymous"

**Possible causes:**
1. Authentication is disabled (`RequireAuthentication: false`)
2. No valid JWT token or API key provided
3. `ICurrentUserService` not registered in DI
4. `HttpContextAccessor` not registered in DI

**Solution:**
- Check authentication configuration in `local.settings.json`
- Ensure `AddHttpContextAccessor()` is called in `Program.cs`
- Verify `AddInstanceManagerCore()` is called (registers `ICurrentUserService`)

### APIM user ID shows as "apim-gateway"

**Possible causes:**
1. APIM not forwarding user identity headers
2. Headers (X-User-Id, X-User-Name) not set in APIM policy

**Solution:**
- Update APIM inbound policy to include user forwarding headers (see above)
- Verify headers are reaching backend using Application Insights

### UpdatedBy is always null

**Possible causes:**
1. Entity is new (never updated after creation)
2. Database migration not applied

**Solution:**
- Check if `UpdatedBy` column exists: `dotnet ef migrations list`
- Apply migrations: The app auto-applies migrations on startup
- Or manually: Delete and recreate database (data loss!)

## Security Considerations

1. **User IDs in Logs**: User IDs are logged for auditing. Ensure logs are properly secured.

2. **PII in Claims**: Email addresses may be considered PII. Consider data retention policies.

3. **API Key Hashing**: Only last 4 characters of API keys are logged (`***2345`).

4. **APIM Headers**: `X-User-*` headers should only be trusted when `X-APIM-Secret` is valid.

5. **Audit Trail Integrity**: `CreatedBy` cannot be changed after creation. `UpdatedBy` shows last modifier.

## Testing User Tracking

### Test with JWT Token

```bash
# Get token
TOKEN=$(curl -X POST "https://login.microsoftonline.com/$TENANT_ID/oauth2/v2.0/token" \
  -d "client_id=$CLIENT_ID" \
  -d "client_secret=$CLIENT_SECRET" \
  -d "scope=$SCOPE" \
  -d "grant_type=client_credentials" | jq -r '.access_token')

# Create instance
curl -X GET "http://localhost:7233/api/query/SaveProjectInstanceCommand?body=%7B%22name%22%3A%22Test%22%7D" \
  -H "Authorization: Bearer $TOKEN"

# Check logs - should show JWT user ID
```

### Test with API Key

```bash
# Enable API key auth in local.settings.json
# Authentication__ApiKeys__Enabled: "true"

# Create instance
curl -X GET "http://localhost:7233/api/query/SaveProjectInstanceCommand?body=%7B%22name%22%3A%22Test%22%7D" \
  -H "X-API-Key: dev-key-12345"

# Check logs - should show "Development" as user
```

### Test with APIM

```bash
# Via APIM with user forwarding
curl -X GET "https://your-apim.azure-api.net/api/query/SaveProjectInstanceCommand?body=%7B%22name%22%3A%22Test%22%7D" \
  -H "Authorization: Bearer $TOKEN"

# Check logs - should show forwarded user from APIM headers
```

## Summary

User identity tracking is **fully automatic** and works across all authentication methods:

✅ **No code changes needed** - Just save entities normally
✅ **Automatic audit fields** - CreatedBy, CreatedAt, UpdatedBy, UpdatedAt
✅ **Automatic request logging** - Every operation logged with user context
✅ **Works with JWT, API Key, and APIM** - Universal identity abstraction
✅ **Available via DI** - Inject `ICurrentUserService` anywhere
✅ **Application Insights integration** - Full audit trail in the cloud

The system is production-ready and provides complete accountability for all operations.
