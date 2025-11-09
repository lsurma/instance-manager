# Postman Quick Reference - InstanceManager API

## Authentication Headers Cheat Sheet

### No Authentication (Default)
```
No headers required
```

**Configuration in local.settings.json:**
```json
"Authentication__RequireAuthentication": "false"
```

---

### API Key Authentication
```
X-API-Key: dev-key-12345
```

**Configuration in local.settings.json:**
```json
"Authentication__RequireAuthentication": "true",
"Authentication__ApiKeys__Enabled": "true",
"Authentication__ApiKeys__Keys__Development": "dev-key-12345"
```

**In Postman:**
- Header Name: `X-API-Key`
- Header Value: `{{apiKey}}` (uses environment variable)

---

### JWT Bearer Token Authentication
```
Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGc...
```

**Configuration in local.settings.json:**
```json
"Authentication__RequireAuthentication": "true",
"Authentication__EntraId__Enabled": "true",
"Authentication__EntraId__TenantId": "your-tenant-id",
"Authentication__EntraId__ClientId": "your-client-id",
"Authentication__EntraId__Audience": "api://your-api-id"
```

**In Postman:**
- Header Name: `Authorization`
- Header Value: `Bearer {{jwtToken}}` (uses environment variable)

---

### APIM Simulation
```
X-APIM-Secret: apim-backend-secret-12345
X-User-Id: alice@example.com
X-User-Name: Alice Johnson
X-User-Email: alice@example.com
X-Subscription-Name: PremiumSubscription
X-Subscription-Id: sub-12345-abcde
X-Auth-Method: JWT
```

**Configuration in local.settings.json:**
```json
"Authentication__RequireAuthentication": "true",
"Authentication__Apim__TrustApim": "true",
"Authentication__Apim__SharedSecret": "apim-backend-secret-12345",
"Authentication__Apim__RequireSharedSecret": "true"
```

**In Postman:**
- Use requests from "With APIM Simulation" folder
- All APIM headers use environment variables (e.g., `{{apimSecret}}`, `{{apimUserId}}`)
- See **[APIM_TESTING.md](APIM_TESTING.md)** for detailed guide

**Required Headers:**
- `X-APIM-Secret` (security - must match config)
- `X-User-Id` (recommended)

**Optional Headers:**
- `X-User-Name`, `X-User-Email`, `X-Subscription-Name`, `X-Subscription-Id`, `X-Auth-Method`

---

## Common Request Patterns

### Get All Items
```
GET {{baseUrl}}/query/GetProjectInstancesQuery
```

### Get Item by ID
```
GET {{baseUrl}}/query/GetProjectInstanceByIdQuery?body={"id":"{{instanceId}}"}
```

### Create Item
```
GET {{baseUrl}}/query/SaveProjectInstanceCommand?body={"name":"Test","description":"Test item"}
```

### Update Item
```
GET {{baseUrl}}/query/SaveProjectInstanceCommand?body={"id":"{{instanceId}}","name":"Updated"}
```

### Delete Item
```
GET {{baseUrl}}/query/DeleteProjectInstanceCommand?body={"id":"{{instanceId}}"}
```

---

## Pagination Parameters

### Basic Pagination
```json
{
  "pagination": {
    "page": 1,
    "pageSize": 10
  }
}
```

### With Ordering
```json
{
  "pagination": {
    "page": 1,
    "pageSize": 10
  },
  "ordering": {
    "orderBy": "name",
    "orderDirection": "asc"
  }
}
```

### With Search
```json
{
  "filtering": {
    "searchTerm": "test"
  },
  "pagination": {
    "page": 1,
    "pageSize": 10
  }
}
```

### With Custom Filters (Translations)
```json
{
  "filtering": {
    "queryFilters": [
      {
        "filterType": "CultureFilter",
        "value": "en-US"
      }
    ]
  },
  "pagination": {
    "page": 1,
    "pageSize": 10
  }
}
```

---

## Environment Variables

| Variable | Example Value | Usage |
|----------|---------------|-------|
| `{{baseUrl}}` | `http://localhost:7233/api` | Base API URL |
| `{{apiKey}}` | `dev-key-12345` | API Key authentication |
| `{{jwtToken}}` | `eyJ0eXAi...` | JWT Bearer token |
| `{{instanceId}}` | `3fa85f64-5717-...` | Sample Project Instance ID |
| `{{translationId}}` | `8b2c4e6f-...` | Sample Translation ID |
| `{{dataSetId}}` | `a1b2c3d4-...` | Sample DataSet ID |

---

## Available Request Types

### ProjectInstances
- `GetProjectInstancesQuery` - List all instances
- `GetProjectInstanceByIdQuery` - Get single instance by ID
- `SaveProjectInstanceCommand` - Create or update instance
- `DeleteProjectInstanceCommand` - Delete instance

### Translations
- `GetTranslationsQuery` - List all translations
- `GetTranslationByIdQuery` - Get single translation by ID
- `SaveTranslationCommand` - Create or update translation
- `DeleteTranslationCommand` - Delete translation

### DataSets
- `GetDataSetsQuery` - List all data sets
- `GetDataSetByIdQuery` - Get single data set by ID
- `SaveDataSetCommand` - Create or update data set
- `DeleteDataSetCommand` - Delete data set

---

## Status Codes

| Code | Meaning | Cause |
|------|---------|-------|
| 200 | OK | Request succeeded |
| 400 | Bad Request | Invalid JSON, missing required fields, or invalid GUID |
| 401 | Unauthorized | Missing or invalid authentication credentials |
| 404 | Not Found | Invalid request name or item doesn't exist |
| 500 | Internal Server Error | Server-side error (check API logs) |

---

## Testing Checklist

### Initial Setup
- [ ] Import collection from `postman/InstanceManager.postman_collection.json`
- [ ] Import environment from `postman/InstanceManager-Local.postman_environment.json`
- [ ] Select "InstanceManager - Local" environment (top-right)
- [ ] Start API: `dotnet run --project InstanceManager.Host.AzFuncAPI/...`
- [ ] Test basic request: "Get All Project Instances"

### Testing CRUD Operations
- [ ] Create a new item (copy the returned GUID)
- [ ] Set `instanceId` environment variable to the GUID
- [ ] Get item by ID (verify data matches)
- [ ] Update the item (change name/description)
- [ ] Get item by ID again (verify changes)
- [ ] Delete the item
- [ ] Get item by ID again (should return null)

### Testing Authentication (Optional)
- [ ] Enable API Key in `local.settings.json`
- [ ] Restart API
- [ ] Test without header (should get 401)
- [ ] Add `X-API-Key` header with valid key (should get 200)
- [ ] Test with invalid key (should get 401)

### Testing Pagination (Optional)
- [ ] Request with pagination parameters
- [ ] Verify `page`, `pageSize`, `totalCount` in response
- [ ] Request next page (increment `page` number)
- [ ] Test different page sizes (5, 10, 25, 50)

---

## Troubleshooting Quick Fixes

### Problem: 401 Unauthorized
**Fix:** Add authentication header
- API Key: `X-API-Key: dev-key-12345`
- JWT: `Authorization: Bearer YOUR_TOKEN`

### Problem: 404 Not Found
**Fix:** Check request name is correct (case-sensitive)
- Correct: `GetProjectInstancesQuery`
- Wrong: `getprojectinstancesquery`

### Problem: 400 Bad Request
**Fix:** Validate JSON syntax
- Check for missing quotes, commas, or brackets
- Verify GUID format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
- Ensure required fields are present (e.g., `name` for SaveCommand)

### Problem: Connection Refused
**Fix:** Start the API
```bash
dotnet run --project InstanceManager.Host.AzFuncAPI/InstanceManager.Host.AzFuncAPI.csproj
```

### Problem: Variables not working
**Fix:** Select the environment
- Click environment dropdown (top-right)
- Select "InstanceManager - Local"

---

## Getting a JWT Token (Quick)

### PowerShell
```powershell
$body = @{
    grant_type    = "client_credentials"
    client_id     = "YOUR-CLIENT-ID"
    client_secret = "YOUR-CLIENT-SECRET"
    scope         = "api://YOUR-API-ID/.default"
}
$response = Invoke-RestMethod -Method Post `
    -Uri "https://login.microsoftonline.com/YOUR-TENANT-ID/oauth2/v2.0/token" `
    -ContentType "application/x-www-form-urlencoded" `
    -Body $body
$response.access_token
```

### curl
```bash
curl -X POST "https://login.microsoftonline.com/YOUR-TENANT-ID/oauth2/v2.0/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=YOUR-CLIENT-ID" \
  -d "client_secret=YOUR-CLIENT-SECRET" \
  -d "scope=api://YOUR-API-ID/.default" \
  -d "grant_type=client_credentials"
```

Copy `access_token` from response and paste into Postman environment `jwtToken` variable.

---

## Useful Postman Shortcuts

| Action | Windows/Linux | Mac |
|--------|---------------|-----|
| Send request | Ctrl+Enter | Cmd+Enter |
| Save request | Ctrl+S | Cmd+S |
| Open console | Ctrl+Alt+C | Cmd+Alt+C |
| Create new request | Ctrl+N | Cmd+N |
| Search collection | Ctrl+K | Cmd+K |
| Toggle sidebar | Ctrl+\\ | Cmd+\\ |

---

## More Help

See detailed documentation:
- **[POSTMAN_TESTING.md](../POSTMAN_TESTING.md)** - Complete Postman guide
- **[AUTHENTICATION.md](../AUTHENTICATION.md)** - Authentication setup details
- **[LOCAL_TESTING.md](../LOCAL_TESTING.md)** - curl-based testing
