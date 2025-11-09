# Postman Testing Files

This folder contains everything you need to test the InstanceManager API with Postman.

## Quick Start

1. **Import these files into Postman:**
   - `InstanceManager.postman_collection.json` - API request collection
   - `InstanceManager-Local.postman_environment.json` - Environment variables

2. **Select environment:**
   - In Postman, select "InstanceManager - Local" from the environment dropdown (top-right)

3. **Start the API:**
   ```bash
   dotnet run --project ../InstanceManager.Host.AzFuncAPI/InstanceManager.Host.AzFuncAPI.csproj
   ```

4. **Send your first request:**
   - Open: `ProjectInstances → Get All Project Instances`
   - Click **Send**
   - You should get 200 OK with data

## Files in This Folder

### Collection & Environment (Import These)

| File | Purpose |
|------|---------|
| `InstanceManager.postman_collection.json` | Complete API request collection with all endpoints and authentication scenarios |
| `InstanceManager-Local.postman_environment.json` | Environment variables for local testing (URLs, API keys, test data) |

### Documentation (Read These)

| File | Description | When to Read |
|------|-------------|--------------|
| **README.md** | This file - Quick overview | **Start here** |
| **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** | One-page cheat sheet with headers, patterns, troubleshooting | **Need quick answers** |
| **[APIM_TESTING.md](APIM_TESTING.md)** | Complete guide for testing APIM simulation with headers | **Testing APIM integration** |

### Additional Documentation (Parent Folder)

| File | Description |
|------|-------------|
| **[../POSTMAN_TESTING.md](../POSTMAN_TESTING.md)** | Complete Postman testing guide (setup, workflows, all auth scenarios) |
| **[../AUTHENTICATION.md](../AUTHENTICATION.md)** | Authentication configuration reference |
| **[../LOCAL_TESTING.md](../LOCAL_TESTING.md)** | Alternative testing with curl/PowerShell scripts |

## Collection Structure

The collection is organized into these folders:

```
InstanceManager API/
├── ProjectInstances/          # CRUD operations for project instances
├── Translations/              # CRUD operations for translations
├── DataSets/                  # CRUD operations for data sets
├── With API Key Auth/         # Example requests with API Key headers
├── With JWT Auth/             # Example requests with JWT Bearer token
└── With APIM Simulation/      # Example requests simulating APIM headers
```

## Authentication Scenarios

Choose the scenario that matches your testing needs:

### 1. No Authentication (Default) - Easiest ✓

**No setup required** - Just send requests!

- Already configured in `local.settings.json`
- No headers needed
- Perfect for quick API testing

**Use any request in:** ProjectInstances, Translations, DataSets folders

---

### 2. API Key Authentication - Recommended ⭐

**Easy local auth testing** with user tracking.

**Setup:**
```json
// Edit local.settings.json
"Authentication__RequireAuthentication": "true",
"Authentication__ApiKeys__Enabled": "true",
"Authentication__ApiKeys__Keys__Alice": "alice-key-12345",
"Authentication__ApiKeys__Keys__Bob": "bob-key-67890"
```

**Test:**
- Use requests in "With API Key Auth" folder
- Or add header: `X-API-Key: alice-key-12345`
- CreatedBy will show "Alice"

**See:** [QUICK_REFERENCE.md](QUICK_REFERENCE.md#api-key-authentication)

---

### 3. JWT Bearer Token - Production-like

**Real Azure AD auth** for production testing.

**Setup:**
- Requires Azure AD app registration
- See [../AUTHENTICATION.md](../AUTHENTICATION.md) for setup

**Test:**
- Get JWT token from Azure AD
- Set `jwtToken` environment variable
- Use requests in "With JWT Auth" folder

**See:** [../POSTMAN_TESTING.md](../POSTMAN_TESTING.md#scenario-3-jwt-bearer-token-authentication-azure-ad)

---

### 4. APIM Simulation - Gateway Testing 🆕

**Test APIM integration locally** by simulating forwarded headers.

**Setup:**
```json
// Edit local.settings.json
"Authentication__RequireAuthentication": "true",
"Authentication__Apim__TrustApim": "true",
"Authentication__Apim__SharedSecret": "apim-backend-secret-12345",
"Authentication__Apim__RequireSharedSecret": "true"
```

**Test:**
- Use requests in "With APIM Simulation" folder
- Headers simulate APIM forwarding user info
- Test as different users by changing environment variables

**See:** [APIM_TESTING.md](APIM_TESTING.md) - Complete APIM guide

---

## Environment Variables

Pre-configured in `InstanceManager-Local.postman_environment.json`:

### API Configuration
- `baseUrl` - API base URL (`http://localhost:7233/api`)

### Authentication
- `apiKey` - API key for testing
- `jwtToken` - JWT Bearer token (set after obtaining from Azure AD)

### APIM Headers
- `apimSecret` - APIM shared secret
- `apimUserId` - User ID forwarded from APIM
- `apimUserName` - User display name
- `apimUserEmail` - User email
- `apimSubscriptionName` - APIM subscription name
- `apimSubscriptionId` - APIM subscription ID
- `apimAuthMethod` - Original auth method (JWT, ApiKey, etc.)

### Test Data
- `instanceId` - Sample Project Instance GUID
- `translationId` - Sample Translation GUID
- `dataSetId` - Sample DataSet GUID
- `paginationBody` - Reusable pagination parameters

## Common Workflows

### CRUD Testing

1. **Create** an instance → Get GUID from response
2. **Set** `instanceId` environment variable to the GUID
3. **Read** the instance by ID → Verify data
4. **Update** the instance → Change name/description
5. **Delete** the instance → Confirm deletion

### User Tracking with API Key

1. **Enable API Key** auth with multiple keys (Alice, Bob)
2. **Create** as Alice → Check `createdBy` = "Alice"
3. **Update** as Bob → Check `updatedBy` = "Bob"
4. **Verify** in database or by fetching the item

### APIM Multi-User Testing

1. **Enable APIM** trust in config
2. **Create** as Alice (use "Create Project Instance (APIM as Alice)")
3. **Create** as Bob (use "Create Project Instance (APIM as Bob)")
4. **Verify** `createdBy` shows different users
5. **Test security** (requests without APIM secret should fail)

## Troubleshooting

### 401 Unauthorized

**Check:**
- Is authentication enabled? (`RequireAuthentication: true`)
- Are you sending the correct header?
  - API Key: `X-API-Key: your-key`
  - JWT: `Authorization: Bearer your-token`
  - APIM: `X-APIM-Secret: your-secret`

**Fix:**
- Add the appropriate header
- Or disable authentication: `RequireAuthentication: false`

### 404 Not Found

**Check:**
- Is API running? (`http://localhost:7233`)
- Is request name correct? (case-sensitive)

**Fix:**
- Start API: `dotnet run --project ...`
- Verify request name matches exactly

### Connection Refused

**Check:**
- Is API running?
- Is it on the correct port?

**Fix:**
```bash
dotnet run --project ../InstanceManager.Host.AzFuncAPI/InstanceManager.Host.AzFuncAPI.csproj
```

Check output for: `Now listening on: http://localhost:7233`

### Variables Not Working ({{variable}} shows literally)

**Check:**
- Is environment selected?

**Fix:**
- Click environment dropdown (top-right)
- Select "InstanceManager - Local"

## Tips

- **Postman Console** (Ctrl+Alt+C / Cmd+Alt+C) - View all request/response details
- **Save requests** after modifying - Click Save or Ctrl+S
- **Duplicate requests** to create variations - Right-click → Duplicate
- **Collection Runner** - Run multiple requests in sequence
- **Tests tab** - Add automated assertions

## Need More Help?

| Question | See Document |
|----------|--------------|
| "How do I get started?" | [README.md](README.md) (this file) |
| "What headers do I need?" | [QUICK_REFERENCE.md](QUICK_REFERENCE.md) |
| "How do I test APIM?" | [APIM_TESTING.md](APIM_TESTING.md) |
| "How does authentication work?" | [../AUTHENTICATION.md](../AUTHENTICATION.md) |
| "Complete Postman guide?" | [../POSTMAN_TESTING.md](../POSTMAN_TESTING.md) |
| "Prefer curl over Postman?" | [../LOCAL_TESTING.md](../LOCAL_TESTING.md) |

## Feedback

If you find issues or have suggestions for improving these testing tools:
1. Check the main repository documentation
2. Contact the development team
3. Submit an issue or pull request

---

**Happy Testing!** 🚀
