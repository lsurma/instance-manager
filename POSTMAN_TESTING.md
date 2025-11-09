# Testing with Postman - Complete Guide

This guide explains how to test the InstanceManager API locally using Postman with different authentication scenarios.

## Prerequisites

1. **Postman** installed (Desktop or Web version) - [Download here](https://www.postman.com/downloads/)
2. **InstanceManager API** running locally
3. **Database** initialized (happens automatically on API startup)

## Quick Start

### 1. Import Collection and Environment

Two files are provided in the `postman/` directory:

- **Collection**: `postman/InstanceManager.postman_collection.json` - Contains all API requests
- **Environment**: `postman/InstanceManager-Local.postman_environment.json` - Contains variables (URLs, API keys, etc.)

**How to Import:**

1. Open Postman
2. Click **Import** button (top left corner)
3. Drag and drop both JSON files OR click "Choose Files" and select them
4. Click **Import**

You should now see:
- **InstanceManager API** collection in the left sidebar
- **InstanceManager - Local** environment in the environment dropdown (top-right)

### 2. Select Environment

- In the **top-right corner** of Postman, click the environment dropdown
- Select **"InstanceManager - Local"**
- The variables will now be available for use in requests

### 3. Start the API

Open a terminal and run:

```bash
cd /mnt/c/Workspace/Projects/InstanceManager
dotnet run --project InstanceManager.Host.AzFuncAPI/InstanceManager.Host.AzFuncAPI.csproj
```

The API should start on `http://localhost:7233`

Look for these messages in the output:
```
Now listening on: http://localhost:7233
Application started. Press Ctrl+C to shut down.
```

### 4. Test Your First Request

1. In Postman, open the **InstanceManager API** collection
2. Navigate to **ProjectInstances → Get All Project Instances**
3. Click **Send**

You should receive a 200 OK response with JSON data containing project instances.

---

## Understanding the Collection Structure

The collection is organized into these folders:

```
InstanceManager API/
├── ProjectInstances/          # CRUD operations for project instances
│   ├── Get All Project Instances
│   ├── Get Project Instances (Paginated)
│   ├── Get Project Instance By ID
│   ├── Create Project Instance
│   ├── Update Project Instance
│   └── Delete Project Instance
├── Translations/              # CRUD operations for translations
│   ├── Get All Translations
│   ├── Get Translations (With Filters)
│   ├── Get Translation By ID
│   ├── Create Translation
│   └── Delete Translation
├── DataSets/                  # CRUD operations for data sets
│   ├── Get All DataSets
│   ├── Get DataSets (Paginated & Searched)
│   ├── Get DataSet By ID
│   ├── Create DataSet
│   └── Delete DataSet
├── With API Key Auth/         # Example requests with API Key authentication
│   ├── Get Project Instances (API Key)
│   └── Get Translations (API Key)
└── With JWT Auth/             # Example requests with JWT authentication
    ├── Get Project Instances (JWT)
    └── Get Translations (JWT)
```

---

## API Request Structure

All InstanceManager API requests follow this pattern:

```
GET /api/query/{RequestName}?body={urlEncodedJson}
```

### Key Points:

1. **All requests use GET** (even for create/update/delete operations)
2. **Request name** is in the URL path (e.g., `GetProjectInstancesQuery`)
3. **Request parameters** are passed as JSON in the `body` query parameter
4. **JSON is URL-encoded** automatically by Postman

### Examples:

**Simple query (no parameters):**
```
GET /api/query/GetProjectInstancesQuery
```

**Query with parameters:**
```
GET /api/query/GetProjectInstanceByIdQuery?body={"id":"123e4567-e89b-12d3-a456-426614174000"}
```

**Create/Update command:**
```
GET /api/query/SaveProjectInstanceCommand?body={"name":"Test","description":"Test instance"}
```

---

## Environment Variables

The **InstanceManager - Local** environment contains these variables:

| Variable | Default Value | Purpose |
|----------|---------------|---------|
| `baseUrl` | `http://localhost:7233/api` | API base URL |
| `apiKey` | `dev-key-12345` | API Key for authentication tests |
| `jwtToken` | (empty) | JWT Bearer token (set when testing JWT auth) |
| `instanceId` | (empty) | Sample Project Instance ID for testing |
| `translationId` | (empty) | Sample Translation ID for testing |
| `dataSetId` | (empty) | Sample DataSet ID for testing |
| `paginationBody` | (JSON) | Reusable pagination parameters |

### Using Variables in Requests

Variables are referenced using double curly braces: `{{variableName}}`

**Example:**
```
GET {{baseUrl}}/query/GetProjectInstanceByIdQuery?body={"id":"{{instanceId}}"}
```

### Editing Environment Variables

1. Click the environment dropdown (top-right)
2. Click the eye icon next to "InstanceManager - Local"
3. Click **Edit**
4. Modify the values
5. Click **Save**

---

## Authentication Scenarios

The API supports three authentication methods. Choose the scenario that matches your testing needs.

### Scenario 1: No Authentication (Default) ✓ Easiest

This is the **default configuration** for local development - perfect for quick testing without setup.

**Configuration:** Already set in `local.settings.json`
```json
{
  "Values": {
    "Authentication__RequireAuthentication": "false"
  }
}
```

**How to Test:**
1. Make sure the API is running
2. Use any request from the **ProjectInstances**, **Translations**, or **DataSets** folders
3. Click **Send**
4. No authentication headers needed!

**Example Requests to Try:**
- Get All Project Instances
- Create Project Instance
- Get All Translations
- Create DataSet

---

### Scenario 2: API Key Authentication ⭐ Recommended for Testing

API Key authentication is the **easiest way to test with authentication** locally. No Azure setup required!

#### Step 1: Enable API Key Authentication

Edit `InstanceManager.Host.AzFuncAPI/local.settings.json`:

```json
{
  "Values": {
    "Authentication__RequireAuthentication": "true",
    "Authentication__ApiKeys__Enabled": "true",
    "Authentication__ApiKeys__Keys__Development": "dev-key-12345",
    "Authentication__ApiKeys__Keys__Alice": "alice-key-67890",
    "Authentication__ApiKeys__Keys__Bob": "bob-key-abcdef"
  }
}
```

**Notes:**
- Each key has a **name** (e.g., "Development", "Alice") and a **value** (the actual key)
- The name will appear in the `CreatedBy` and `UpdatedBy` fields
- You can add as many keys as you need

#### Step 2: Restart the API

Stop the API (Ctrl+C in terminal) and restart it:
```bash
dotnet run --project InstanceManager.Host.AzFuncAPI/InstanceManager.Host.AzFuncAPI.csproj
```

#### Step 3: Test Without Authentication (Should Fail)

1. Try any request from the main folders (e.g., "Get All Project Instances")
2. Click **Send**
3. You should get **401 Unauthorized**

This confirms authentication is now required.

#### Step 4: Test With API Key (Should Succeed)

**Option A: Use Pre-configured Requests**

1. Navigate to **With API Key Auth** folder
2. Open **Get Project Instances (API Key)**
3. Click **Send**
4. You should get **200 OK** with data

**Option B: Add Header to Any Request**

1. Open any request
2. Go to the **Headers** tab
3. Add a new header:
   - **Key**: `X-API-Key`
   - **Value**: `{{apiKey}}` (or paste the key directly: `dev-key-12345`)
4. Click **Send**

#### Step 5: Test User Tracking

Create an instance to see user tracking in action:

1. Open **ProjectInstances → Create Project Instance**
2. Add the `X-API-Key` header: `dev-key-12345`
3. Click **Send**
4. Copy the returned GUID
5. Open **Get Project Instance By ID**
6. Replace `{{instanceId}}` in the body with the GUID you copied
7. Send the request
8. In the response, check the `createdBy` field - it should show **"Development"**

#### Testing Multiple Users:

**Create as Alice:**
1. Edit environment
2. Change `apiKey` to `alice-key-67890`
3. Create a new instance
4. Check `createdBy` → should be **"Alice"**

**Update as Bob:**
1. Change `apiKey` to `bob-key-abcdef`
2. Update the instance Alice created
3. Check `updatedBy` → should be **"Bob"**

---

### Scenario 3: JWT Bearer Token Authentication (Azure AD)

Test with real JWT tokens from Azure Entra ID (formerly Azure AD). Requires Azure setup.

#### Prerequisites

You need an **Azure AD App Registration**:

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Active Directory → App registrations**
3. Create new registration (or use existing)
4. Note these values:
   - **Tenant ID**
   - **Client ID**
   - **Audience** (API URI, usually `api://your-app-id`)
5. Create a **Client Secret** (Certificates & secrets → New client secret)

#### Step 1: Configure JWT Authentication

Edit `InstanceManager.Host.AzFuncAPI/local.settings.json`:

```json
{
  "Values": {
    "Authentication__RequireAuthentication": "true",
    "Authentication__EntraId__Enabled": "true",
    "Authentication__EntraId__Instance": "https://login.microsoftonline.com/",
    "Authentication__EntraId__TenantId": "YOUR-TENANT-ID-HERE",
    "Authentication__EntraId__ClientId": "YOUR-CLIENT-ID-HERE",
    "Authentication__EntraId__Audience": "api://YOUR-APP-ID-HERE",
    "Authentication__ApiKeys__Enabled": "false"
  }
}
```

Replace `YOUR-TENANT-ID-HERE`, `YOUR-CLIENT-ID-HERE`, and `YOUR-APP-ID-HERE` with your actual values.

#### Step 2: Obtain a JWT Token

**Using PowerShell (Windows):**

```powershell
$tenantId = "YOUR-TENANT-ID"
$clientId = "YOUR-CLIENT-ID"
$clientSecret = "YOUR-CLIENT-SECRET"
$scope = "api://YOUR-APP-ID/.default"

$body = @{
    grant_type    = "client_credentials"
    client_id     = $clientId
    client_secret = $clientSecret
    scope         = $scope
}

$response = Invoke-RestMethod -Method Post `
    -Uri "https://login.microsoftonline.com/$tenantId/oauth2/v2.0/token" `
    -ContentType "application/x-www-form-urlencoded" `
    -Body $body

$token = $response.access_token
Write-Host "Token: $token"
```

**Using curl (Linux/Mac/Git Bash):**

```bash
curl -X POST "https://login.microsoftonline.com/YOUR-TENANT-ID/oauth2/v2.0/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=YOUR-CLIENT-ID" \
  -d "client_secret=YOUR-CLIENT-SECRET" \
  -d "scope=api://YOUR-APP-ID/.default" \
  -d "grant_type=client_credentials"
```

Copy the `access_token` from the response.

**Optional: Decode the Token**

Visit [https://jwt.ms](https://jwt.ms) and paste your token to see its contents.

#### Step 3: Set Token in Postman Environment

1. Click the environment dropdown → Click the eye icon → **Edit**
2. Find the `jwtToken` variable
3. Paste your access token into the **Current Value** field
4. Click **Save**

#### Step 4: Test with JWT Token

**Option A: Use Pre-configured Requests**

1. Navigate to **With JWT Auth** folder
2. Open **Get Project Instances (JWT)**
3. The request already has `Authorization: Bearer {{jwtToken}}` header
4. Click **Send**
5. You should get **200 OK**

**Option B: Add Header to Any Request**

1. Open any request
2. Go to the **Headers** tab
3. Add a new header:
   - **Key**: `Authorization`
   - **Value**: `Bearer {{jwtToken}}`
4. Click **Send**

#### Token Expiration

JWT tokens expire (typically after 1 hour). If you get 401 Unauthorized:
1. Get a new token (repeat Step 2)
2. Update the `jwtToken` environment variable
3. Try the request again

---

## Common Workflows

### Workflow 1: Complete CRUD Test

**Create:**
1. Open **ProjectInstances → Create Project Instance**
2. Review the body parameter: `{"name":"Test Instance","description":"Created from Postman","mainHost":"test.example.com"}`
3. Modify the name if desired
4. Click **Send**
5. Response: A GUID (e.g., `3fa85f64-5717-4562-b3fc-2c963f66afa6`)
6. **Copy this GUID**

**Set Environment Variable:**
1. Click environment dropdown → Edit
2. Set `instanceId` to the GUID you copied
3. Save

**Read (Single Item):**
1. Open **ProjectInstances → Get Project Instance By ID**
2. The body uses `{{instanceId}}`: `{"id":"{{instanceId}}"}`
3. Click **Send**
4. Verify the response matches what you created

**Read (List):**
1. Open **ProjectInstances → Get All Project Instances**
2. Click **Send**
3. Find your instance in the `items` array

**Update:**
1. Open **ProjectInstances → Update Project Instance**
2. Body includes `id` plus fields to update
3. Modify the name: `{"id":"{{instanceId}}","name":"UPDATED Instance","description":"Modified","mainHost":"updated.example.com"}`
4. Click **Send**
5. Response: The same GUID
6. Read by ID again to verify changes

**Delete:**
1. Open **ProjectInstances → Delete Project Instance**
2. Body: `{"id":"{{instanceId}}"}`
3. Click **Send**
4. Try to read by ID again → should return `null` or not found

---

### Workflow 2: Pagination Testing

**Get Page 1:**
1. Open **ProjectInstances → Get Project Instances (Paginated)**
2. The `{{paginationBody}}` variable contains: `{"pagination":{"page":1,"pageSize":10},"ordering":{"orderBy":"name","orderDirection":"asc"}}`
3. Click **Send**
4. Check response:
   ```json
   {
     "items": [...],
     "page": 1,
     "pageSize": 10,
     "totalCount": 25,
     "totalPages": 3,
     "hasPreviousPage": false,
     "hasNextPage": true
   }
   ```

**Get Page 2:**
1. Modify the body parameter to: `{"pagination":{"page":2,"pageSize":10},"ordering":{"orderBy":"name","orderDirection":"asc"}}`
2. Click **Send**
3. Check `hasPreviousPage: true` and `hasNextPage: true` (if there are more items)

**Change Page Size:**
1. Modify to: `{"pagination":{"page":1,"pageSize":5}}`
2. Now you'll see only 5 items per page

---

### Workflow 3: Search and Filter

**Search DataSets:**
1. Open **DataSets → Get DataSets (Paginated & Searched)**
2. Body contains search term: `{"filtering":{"searchTerm":"prod"}}`
3. This searches across all text fields
4. Click **Send**
5. Results will only include items matching "prod"

**Filter Translations by Culture:**
1. Open **Translations → Get Translations (With Filters)**
2. Body contains custom filter: `{"filtering":{"queryFilters":[{"filterType":"CultureFilter","value":"en-US"}]}}`
3. Click **Send**
4. Results will only include translations for "en-US" culture

**Combine Search + Pagination + Ordering:**
```json
{
  "filtering": {
    "searchTerm": "test"
  },
  "pagination": {
    "page": 1,
    "pageSize": 10
  },
  "ordering": {
    "orderBy": "createdAt",
    "orderDirection": "desc"
  }
}
```

---

### Workflow 4: Testing User Tracking with API Keys

This workflow demonstrates the user tracking feature.

**Setup:**
1. Enable API Key authentication (see Scenario 2)
2. Add multiple keys for different users:
   ```json
   "Authentication__ApiKeys__Keys__Alice": "alice-key-12345",
   "Authentication__ApiKeys__Keys__Bob": "bob-key-67890"
   ```
3. Restart the API

**Create as Alice:**
1. Edit environment, set `apiKey` to `alice-key-12345`
2. Create a new project instance
3. Copy the returned GUID

**Update as Bob:**
1. Edit environment, set `apiKey` to `bob-key-67890`
2. Update the instance (use the GUID from above)

**Verify Tracking:**
1. Set `apiKey` back to `alice-key-12345`
2. Get the instance by ID
3. Check the response:
   ```json
   {
     "id": "...",
     "name": "...",
     "createdBy": "Alice",
     "updatedBy": "Bob",
     "createdAt": "2025-11-09T10:30:00Z",
     "updatedAt": "2025-11-09T11:45:00Z"
   }
   ```

---

## Tips and Best Practices

### 1. Use Postman Console for Debugging

- **View → Show Postman Console** (or Ctrl+Alt+C / Cmd+Alt+C)
- See all requests and responses, including headers
- Helpful for debugging URL encoding and authentication issues

### 2. Save Requests After Modifying

- After changing a request, click **Save** (or Ctrl+S)
- This preserves your changes for future use
- You can create variations by clicking the **...** menu → **Duplicate**

### 3. Use Pre-request Scripts for Dynamic Values

Example: Generate a unique name for each test:

1. Open a request
2. Go to the **Pre-request Script** tab
3. Add:
   ```javascript
   pm.environment.set("uniqueName", "Test-" + Date.now());
   ```
4. In the body, use: `{"name":"{{uniqueName}}"}`

### 4. Use Tests for Automation

Example: Automatically save the created ID:

1. Open **Create Project Instance**
2. Go to the **Tests** tab
3. Add:
   ```javascript
   if (pm.response.code === 200) {
       pm.environment.set("instanceId", pm.response.json());
   }
   ```
4. Now the ID is automatically saved to `{{instanceId}}`

### 5. Organize with Folders

Create your own folders for specific testing scenarios:
- Right-click the collection → **Add Folder**
- Drag requests into the folder
- Example folders: "Smoke Tests", "Integration Tests", "Bug Reproductions"

### 6. Export and Share

Share your collection with team members:
1. Right-click the collection → **Export**
2. Choose **Collection v2.1**
3. Save the JSON file
4. Share with team (they can import it)

---

## Troubleshooting

### Issue: 401 Unauthorized

**Possible Causes:**

1. **Authentication is enabled but no credentials provided**
   - Solution: Add `X-API-Key` or `Authorization: Bearer` header

2. **Invalid API key**
   - Solution: Check `local.settings.json` for the correct key
   - Verify the key matches what's in the environment variable

3. **Expired JWT token**
   - Solution: Get a new token (tokens typically expire after 1 hour)

4. **Wrong authentication method enabled**
   - Check which method is enabled in `local.settings.json`
   - If `ApiKeys__Enabled: true`, use `X-API-Key` header
   - If `EntraId__Enabled: true`, use `Authorization: Bearer` header

### Issue: 404 Not Found

**Possible Causes:**

1. **Request name is incorrect**
   - Request names are case-sensitive
   - Example: `GetProjectInstancesQuery` (correct) vs `getprojectinstancesquery` (wrong)

2. **API is not running**
   - Check terminal for API output
   - Visit `http://localhost:7233/api/query/GetProjectInstancesQuery` in browser

3. **Wrong base URL**
   - Check environment variable `baseUrl` is `http://localhost:7233/api`

**To see all available requests:**
- Send a request with an invalid name (e.g., `GetFooQuery`)
- The error response will list all available request names

### Issue: 400 Bad Request

**Possible Causes:**

1. **Invalid JSON in body parameter**
   - Check for syntax errors (missing quotes, commas, brackets)
   - Use a JSON validator: [jsonlint.com](https://jsonlint.com)

2. **Missing required fields**
   - Example: `SaveProjectInstanceCommand` requires `name` field
   - Check the error message for details

3. **Invalid GUID format**
   - GUIDs must be: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
   - Example: `3fa85f64-5717-4562-b3fc-2c963f66afa6`

### Issue: Connection Refused / Cannot Connect

**Solutions:**

1. **Start the API:**
   ```bash
   dotnet run --project InstanceManager.Host.AzFuncAPI/InstanceManager.Host.AzFuncAPI.csproj
   ```

2. **Check the port:**
   - API should be on port 7233
   - Look for "Now listening on: http://localhost:7233" in terminal output

3. **Check firewall:**
   - Windows: Allow dotnet.exe through firewall
   - Linux: Check iptables rules

### Issue: Empty Response or Null

**Possible Causes:**

1. **Item doesn't exist**
   - Verify the ID exists in the database
   - Try listing all items first

2. **Database is empty**
   - The database auto-seeds on startup
   - If empty, delete `db/instanceManager.db` and restart the API

### Issue: Variables Not Working

**Symptoms:** `{{variableName}}` appears literally in requests

**Solutions:**

1. **Select the environment:**
   - Check the environment dropdown (top-right)
   - Select "InstanceManager - Local"

2. **Variable doesn't exist:**
   - Click environment dropdown → Edit
   - Check if the variable is defined
   - Add it if missing

3. **Variable is empty:**
   - Edit the environment
   - Set the **Current Value** (not just Initial Value)
   - Save

---

## Advanced Usage

### Running Multiple Requests in Sequence

Use **Postman Collection Runner**:

1. Right-click collection → **Run collection**
2. Select requests to run
3. Set iterations (how many times to run)
4. Click **Run InstanceManager API**
5. View results summary

### Creating Test Suites

Add tests to automatically verify responses:

**Example test script:**
```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Response has items", function () {
    const jsonData = pm.response.json();
    pm.expect(jsonData.items).to.be.an('array');
    pm.expect(jsonData.items.length).to.be.greaterThan(0);
});

pm.test("Response has pagination info", function () {
    const jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('page');
    pm.expect(jsonData).to.have.property('pageSize');
    pm.expect(jsonData).to.have.property('totalCount');
});
```

### Chaining Requests with Variables

**Create → Get → Update → Delete workflow:**

1. **Create** (Tests tab):
   ```javascript
   if (pm.response.code === 200) {
       pm.environment.set("instanceId", pm.response.json());
   }
   ```

2. **Get** uses `{{instanceId}}` automatically

3. **Update** uses `{{instanceId}}` automatically

4. **Delete** uses `{{instanceId}}` automatically

Run these in sequence with Collection Runner.

---

## Additional Resources

- **[AUTHENTICATION.md](AUTHENTICATION.md)** - Detailed authentication setup
- **[USER_TRACKING.md](USER_TRACKING.md)** - User identity tracking details
- **[PAGINATION_QUICKSTART.md](PAGINATION_QUICKSTART.md)** - Pagination examples
- **[LOCAL_TESTING.md](LOCAL_TESTING.md)** - curl-based testing guide

---

## Need Help?

**Postman Resources:**
- [Postman Learning Center](https://learning.postman.com/)
- [Postman Documentation](https://www.postman.com/api-platform/api-documentation/)

**API Issues:**
- Check terminal output for errors
- Check `db/instanceManager.db` exists
- Review `local.settings.json` configuration
- Check [AUTHENTICATION.md](AUTHENTICATION.md) for auth setup
