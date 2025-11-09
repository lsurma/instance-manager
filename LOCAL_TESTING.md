# Local Testing Guide - Authentication & User Tracking

This guide shows how to test all authentication methods locally, including simulating APIM headers.

## Quick Start

### 1. Start the Backend API

```bash
cd InstanceManager.Host.AzFuncAPI
dotnet run
```

API will be available at `http://localhost:7233`

### 2. Choose Your Testing Method

- **Option A**: No Authentication (default) - Quick testing
- **Option B**: API Key Authentication - Easy local testing
- **Option C**: JWT Token (Azure AD) - Production-like
- **Option D**: Simulated APIM - Test APIM integration locally

---

## Option A: No Authentication (Development Mode)

**Configuration** (`local.settings.json`):
```json
{
  "Values": {
    "Authentication__RequireAuthentication": "false"
  }
}
```

**Test:**
```bash
# Windows PowerShell
.\test-scripts\test-no-auth.ps1

# Linux/Mac
./test-scripts/test-no-auth.sh

# Or manually
curl "http://localhost:7233/api/query/GetProjectInstancesQuery?body=%7B%7D"
```

**Expected User Tracking:**
- `CreatedBy`: "system"
- `UpdatedBy`: "system"
- Logs show: "User: system (None)"

---

## Option B: API Key Authentication

This is the **easiest way** to test user tracking locally!

### Setup

**1. Update `local.settings.json`:**
```json
{
  "Values": {
    "Authentication__RequireAuthentication": "true",
    "Authentication__ApiKeys__Enabled": "true",
    "Authentication__ApiKeys__Keys__Alice": "alice-key-12345",
    "Authentication__ApiKeys__Keys__Bob": "bob-key-67890",
    "Authentication__ApiKeys__Keys__DevTeam": "dev-team-key-abcdef",
    "Authentication__EntraId__Enabled": "false"
  }
}
```

**2. Restart the API** (Ctrl+C and `dotnet run`)

### Test with Different Users

**Test as Alice:**
```bash
# Windows PowerShell
.\test-scripts\test-api-key.ps1 -ApiKey "alice-key-12345"

# Linux/Mac
./test-scripts/test-api-key.sh "alice-key-12345"

# Or manually with curl
curl -X GET "http://localhost:7233/api/query/SaveProjectInstanceCommand?body=%7B%22name%22%3A%22Alice%27s%20Project%22%7D" \
  -H "X-API-Key: alice-key-12345"
```

**Test as Bob:**
```bash
curl -X GET "http://localhost:7233/api/query/SaveProjectInstanceCommand?body=%7B%22name%22%3A%22Bob%27s%20Project%22%7D" \
  -H "X-API-Key: bob-key-67890"
```

**Expected User Tracking:**
- `CreatedBy`: "Alice" (or "Bob", "DevTeam")
- Logs show: "User: Alice (APIKey)"

### Verify User Tracking

**Check the database:**
```bash
# Windows PowerShell
.\test-scripts\check-database.ps1

# Linux/Mac
./test-scripts/check-database.sh
```

Or manually:
```bash
sqlite3 db/instanceManager.db "SELECT Id, Name, CreatedBy, UpdatedBy FROM ProjectInstances;"
```

**Expected output:**
```
guid-here|Alice's Project|Alice|(null)
guid-here|Bob's Project|Bob|(null)
```

---

## Option C: JWT Token (Azure AD / Entra ID)

Test with **real JWT tokens** from Azure AD.

### Prerequisites

You need an Azure AD app registration:
1. Go to [Azure Portal](https://portal.azure.com) → Azure Active Directory → App registrations
2. Create new registration or use existing
3. Note: **Tenant ID**, **Client ID**
4. Create a **Client Secret** (Certificates & secrets)
5. Expose an API and note the **Audience** (API URI)

### Configuration

**Update `local.settings.json`:**
```json
{
  "Values": {
    "Authentication__RequireAuthentication": "true",
    "Authentication__EntraId__Enabled": "true",
    "Authentication__EntraId__Instance": "https://login.microsoftonline.com/",
    "Authentication__EntraId__TenantId": "YOUR-TENANT-ID",
    "Authentication__EntraId__ClientId": "YOUR-CLIENT-ID",
    "Authentication__EntraId__Audience": "api://YOUR-API-ID",
    "Authentication__ApiKeys__Enabled": "false"
  }
}
```

### Get a JWT Token

**Method 1: Using PowerShell Script**
```powershell
.\test-scripts\get-jwt-token.ps1 `
  -TenantId "YOUR-TENANT-ID" `
  -ClientId "YOUR-CLIENT-ID" `
  -ClientSecret "YOUR-CLIENT-SECRET" `
  -Scope "api://YOUR-API-ID/.default"
```

**Method 2: Using curl (Linux/Mac/Windows)**
```bash
./test-scripts/get-jwt-token.sh "YOUR-TENANT-ID" "YOUR-CLIENT-ID" "YOUR-CLIENT-SECRET" "api://YOUR-API-ID/.default"
```

**Method 3: Manual curl**
```bash
curl -X POST "https://login.microsoftonline.com/YOUR-TENANT-ID/oauth2/v2.0/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=YOUR-CLIENT-ID" \
  -d "client_secret=YOUR-CLIENT-SECRET" \
  -d "scope=api://YOUR-API-ID/.default" \
  -d "grant_type=client_credentials"
```

Save the `access_token` from the response.

### Test with JWT Token

```bash
# Save token to variable
$TOKEN = "eyJ0eXAiOiJKV1QiLCJhbGc..."

# Test with PowerShell
.\test-scripts\test-jwt.ps1 -Token $TOKEN

# Or with curl
curl -X GET "http://localhost:7233/api/query/SaveProjectInstanceCommand?body=%7B%22name%22%3A%22JWT%20Test%22%7D" \
  -H "Authorization: Bearer $TOKEN"
```

**Expected User Tracking:**
- `CreatedBy`: "a1b2c3d4-e5f6-7890-abcd-ef1234567890" (Azure AD Object ID)
- Logs show: "User: John Doe (JWT)" or "User: {app-name} (JWT)"

### Decode JWT Token (Optional)

To see what's in your token:
```bash
# PowerShell
.\test-scripts\decode-jwt.ps1 -Token $TOKEN

# Or use jwt.ms
# Copy token and paste at: https://jwt.ms
```

---

## Option D: Simulated APIM (Local Testing)

Test APIM integration **without deploying to Azure**!

### Configuration

**Update `local.settings.json`:**
```json
{
  "Values": {
    "Authentication__RequireAuthentication": "true",
    "Authentication__Apim__TrustApim": "true",
    "Authentication__Apim__SharedSecret": "local-test-secret-12345",
    "Authentication__Apim__RequireSharedSecret": "true",
    "Authentication__EntraId__Enabled": "false",
    "Authentication__ApiKeys__Enabled": "false"
  }
}
```

### Test Simulating APIM

**Test as Alice (via APIM):**
```bash
# Windows PowerShell
.\test-scripts\test-apim-simulation.ps1 `
  -UserId "alice@example.com" `
  -UserName "Alice Johnson" `
  -UserEmail "alice@example.com" `
  -SubscriptionName "AliceSubscription"

# Linux/Mac
./test-scripts/test-apim-simulation.sh \
  "alice@example.com" \
  "Alice Johnson" \
  "alice@example.com" \
  "AliceSubscription"
```

**Manual curl:**
```bash
curl -X GET "http://localhost:7233/api/query/SaveProjectInstanceCommand?body=%7B%22name%22%3A%22APIM%20Test%22%7D" \
  -H "X-APIM-Secret: local-test-secret-12345" \
  -H "X-User-Id: alice@example.com" \
  -H "X-User-Name: Alice Johnson" \
  -H "X-User-Email: alice@example.com" \
  -H "X-Subscription-Name: AliceSubscription" \
  -H "X-Auth-Method: JWT"
```

**Test without shared secret (should fail):**
```bash
curl -X GET "http://localhost:7233/api/query/GetProjectInstancesQuery?body=%7B%7D" \
  -H "X-User-Id: hacker@evil.com"

# Expected: 401 Unauthorized (no X-APIM-Secret)
```

**Expected User Tracking:**
- `CreatedBy`: "alice@example.com"
- Logs show: "User: Alice Johnson (APIM)"
- Additional claims include: SubscriptionName, OriginalAuthMethod

---

## Testing Multiple Auth Methods Simultaneously

You can enable both JWT and API Key at the same time:

```json
{
  "Values": {
    "Authentication__RequireAuthentication": "true",
    "Authentication__EntraId__Enabled": "true",
    "Authentication__EntraId__TenantId": "YOUR-TENANT-ID",
    "Authentication__EntraId__ClientId": "YOUR-CLIENT-ID",
    "Authentication__EntraId__Audience": "api://YOUR-API-ID",
    "Authentication__ApiKeys__Enabled": "true",
    "Authentication__ApiKeys__Keys__DevTeam": "dev-key-12345"
  }
}
```

Then test:
```bash
# Test with JWT
curl -H "Authorization: Bearer $TOKEN" "http://localhost:7233/api/query/..."

# Test with API Key (same endpoint, different user)
curl -H "X-API-Key: dev-key-12345" "http://localhost:7233/api/query/..."
```

---

## Verification Checklist

### 1. Check Application Logs

Look for logs like:
```
[INFO] Executing request: SaveProjectInstanceCommand
  | User: Alice (APIKey)
  | Auth: APIKey
  | UserId: Alice

[INFO] Completed request: SaveProjectInstanceCommand
  | User: Alice (APIKey)
```

### 2. Check Database

```bash
sqlite3 db/instanceManager.db
```

```sql
-- View all instances with creators
SELECT Id, Name, CreatedBy, UpdatedBy, CreatedAt, UpdatedAt
FROM ProjectInstances
ORDER BY CreatedAt DESC
LIMIT 10;

-- Count instances by creator
SELECT CreatedBy, COUNT(*) as Count
FROM ProjectInstances
GROUP BY CreatedBy;

-- Find instances created by specific user
SELECT * FROM ProjectInstances WHERE CreatedBy = 'Alice';
```

### 3. Test Update Tracking

```bash
# Create an instance
curl -X GET "http://localhost:7233/api/query/SaveProjectInstanceCommand?body=%7B%22name%22%3A%22Test%22%7D" \
  -H "X-API-Key: alice-key-12345"

# Note the ID from response, then update as Bob
curl -X GET "http://localhost:7233/api/query/SaveProjectInstanceCommand?body=%7B%22id%22%3A%22GUID-HERE%22%2C%22name%22%3A%22Updated%22%7D" \
  -H "X-API-Key: bob-key-67890"

# Check database
sqlite3 db/instanceManager.db "SELECT Name, CreatedBy, UpdatedBy FROM ProjectInstances WHERE Name = 'Updated';"

# Expected: Name=Updated, CreatedBy=Alice, UpdatedBy=Bob
```

### 4. Check User Identity Extraction

Add this to any handler temporarily:
```csharp
var user = _currentUserService.GetCurrentUser();
_logger.LogInformation("DEBUG: UserId={UserId}, DisplayName={DisplayName}, Email={Email}, AuthMethod={AuthMethod}",
    user.UserId, user.DisplayName, user.Email, user.AuthenticationMethod);
```

---

## Common Test Scenarios

### Scenario 1: Multi-User Collaboration

```bash
# Alice creates a project
curl -H "X-API-Key: alice-key-12345" \
  "http://localhost:7233/api/query/SaveProjectInstanceCommand?body=%7B%22name%22%3A%22Shared%20Project%22%7D"

# Bob updates it
curl -H "X-API-Key: bob-key-67890" \
  "http://localhost:7233/api/query/SaveProjectInstanceCommand?body=%7B%22id%22%3A%22GUID%22%2C%22name%22%3A%22Updated%20by%20Bob%22%7D"

# DevTeam updates it
curl -H "X-API-Key: dev-team-key-abcdef" \
  "http://localhost:7233/api/query/SaveProjectInstanceCommand?body=%7B%22id%22%3A%22GUID%22%2C%22description%22%3A%22Team%20update%22%7D"

# Query: Who worked on this?
sqlite3 db/instanceManager.db \
  "SELECT CreatedBy, UpdatedBy FROM ProjectInstances WHERE Id = 'GUID';"
# Result: CreatedBy=Alice, UpdatedBy=DevTeam
```

### Scenario 2: Audit Trail

```bash
# Create multiple instances as different users
for user in "Alice" "Bob" "DevTeam"; do
  curl -H "X-API-Key: ${user,,}-key-..." \
    "http://localhost:7233/api/query/SaveProjectInstanceCommand?body=%7B%22name%22%3A%22${user}Project%22%7D"
done

# Generate audit report
sqlite3 db/instanceManager.db <<EOF
SELECT
  Name,
  CreatedBy,
  datetime(CreatedAt/10000000 - 62135596800, 'unixepoch') as CreatedDate,
  UpdatedBy,
  CASE WHEN UpdatedAt IS NOT NULL
    THEN datetime(UpdatedAt/10000000 - 62135596800, 'unixepoch')
    ELSE NULL
  END as UpdatedDate
FROM ProjectInstances
ORDER BY CreatedAt DESC;
EOF
```

### Scenario 3: APIM Gateway Simulation

```bash
# Simulate client -> APIM -> Backend flow

# Step 1: Client authenticates to APIM with JWT
TOKEN="<real-jwt-token>"

# Step 2: APIM validates token and extracts user info, then forwards to backend
curl "http://localhost:7233/api/query/SaveProjectInstanceCommand?body=%7B%22name%22%3A%22Via%20APIM%22%7D" \
  -H "X-APIM-Secret: local-test-secret-12345" \
  -H "X-User-Id: a1b2c3d4-e5f6-7890-abcd-ef1234567890" \
  -H "X-User-Name: John Doe" \
  -H "X-User-Email: john.doe@example.com" \
  -H "X-Subscription-Name: PremiumPlan" \
  -H "X-Auth-Method: JWT"

# Check logs - should show APIM authentication with forwarded user
```

---

## Troubleshooting

### Issue: "401 Unauthorized"

**Check:**
1. Is authentication enabled? (`RequireAuthentication: true`)
2. Is the correct auth method enabled? (EntraId, ApiKeys, or Apim)
3. Are you sending the correct header?
   - JWT: `Authorization: Bearer <token>`
   - API Key: `X-API-Key: <key>`
   - APIM: `X-APIM-Secret: <secret>`

### Issue: User shows as "system" or "anonymous"

**Check:**
1. `HttpContextAccessor` registered? (should be in `Program.cs`)
2. `ICurrentUserService` registered? (auto-registered by `AddInstanceManagerCore()`)
3. Are you sending authentication credentials?
4. Check logs for authentication errors

### Issue: JWT token validation fails

**Check:**
1. Token not expired? (decode at jwt.ms)
2. Audience (`aud`) matches `Authentication__EntraId__Audience`?
3. Issuer (`iss`) matches tenant?
4. Token issued for correct API scope?

### Issue: APIM headers ignored

**Check:**
1. `TrustApim: true`?
2. `SharedSecret` matches between config and header?
3. `RequireSharedSecret: true` means you MUST send `X-APIM-Secret`

### Issue: Database shows old data

**Solution:**
```bash
# Delete and recreate database
rm db/instanceManager.db
dotnet run

# Database will be recreated with new schema on startup
```

---

## Clean Testing Environment

To start fresh:

```bash
# Stop API (Ctrl+C)

# Delete database
rm db/instanceManager.db

# Clear logs (optional)
rm -rf InstanceManager.Host.AzFuncAPI/bin/Debug/net9.0/*.log

# Restart API
dotnet run

# Database auto-recreates with seed data
```

---

## Next Steps

1. **Run the test scripts** in `/test-scripts/`
2. **Check the logs** for user tracking
3. **Query the database** to see CreatedBy/UpdatedBy
4. **Test all auth methods** (API Key, JWT, APIM simulation)
5. **Deploy to Azure** and test with real APIM

All test scripts are in the `test-scripts` folder - see the next section!
