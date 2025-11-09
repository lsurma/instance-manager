# Azure API Management (APIM) Policy Templates

This document contains policy templates for integrating your InstanceManager API with Azure API Management.

## Overview

These policies demonstrate two common APIM authentication scenarios:

1. **JWT Validation at APIM** - APIM validates JWT tokens and forwards authenticated requests to backend
2. **API Key at APIM** - APIM validates API keys and forwards authenticated requests to backend

In both scenarios, APIM adds a shared secret header (`X-APIM-Secret`) to authenticate itself to the backend Function App.

## Prerequisites

Before using these policies:

1. **Backend Function App Configuration**:
   ```json
   {
     "Authentication__RequireAuthentication": "true",
     "Authentication__Apim__TrustApim": "true",
     "Authentication__Apim__SharedSecret": "your-secure-shared-secret",
     "Authentication__Apim__RequireSharedSecret": "true"
   }
   ```

2. **APIM Named Values** (Settings > Named Values):
   - `apim-backend-secret` = `your-secure-shared-secret` (same as backend)
   - `entra-tenant-id` = `your-azure-ad-tenant-id` (for JWT validation)
   - `entra-audience` = `api://your-api-id` (for JWT validation)

3. **Network Security** (Recommended):
   - Configure Function App to accept traffic only from APIM subnet
   - Or use Azure Front Door / Application Gateway

## Policy Template 1: JWT Bearer Token Validation

This policy validates Azure AD (Entra ID) JWT tokens at the APIM layer.

### Inbound Policy

```xml
<policies>
    <inbound>
        <base />

        <!-- Validate JWT token from Azure AD -->
        <validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="Unauthorized. Valid JWT token required.">
            <openid-config url="https://login.microsoftonline.com/{{entra-tenant-id}}/v2.0/.well-known/openid-configuration" />
            <audiences>
                <audience>{{entra-audience}}</audience>
            </audiences>
            <issuers>
                <issuer>https://login.microsoftonline.com/{{entra-tenant-id}}/v2.0</issuer>
                <issuer>https://sts.windows.net/{{entra-tenant-id}}/</issuer>
            </issuers>
            <required-claims>
                <claim name="aud" match="any">
                    <value>{{entra-audience}}</value>
                </claim>
            </required-claims>
        </validate-jwt>

        <!-- Add shared secret header for backend authentication -->
        <set-header name="X-APIM-Secret" exists-action="override">
            <value>{{apim-backend-secret}}</value>
        </set-header>

        <!-- Optional: Forward user claims to backend -->
        <set-header name="X-User-Id" exists-action="override">
            <value>@(context.Request.Headers.GetValueOrDefault("Authorization","").AsJwt()?.Claims.GetValueOrDefault("oid", "unknown"))</value>
        </set-header>
        <set-header name="X-User-Email" exists-action="override">
            <value>@(context.Request.Headers.GetValueOrDefault("Authorization","").AsJwt()?.Claims.GetValueOrDefault("email", "unknown"))</value>
        </set-header>

        <!-- Set backend URL -->
        <set-backend-service base-url="https://your-function-app.azurewebsites.net" />
    </inbound>

    <backend>
        <base />
    </backend>

    <outbound>
        <base />

        <!-- Remove internal headers from response -->
        <set-header name="X-APIM-Secret" exists-action="delete" />
    </outbound>

    <on-error>
        <base />
    </on-error>
</policies>
```

## Policy Template 2: API Key Validation

This policy validates API keys using APIM's subscription keys.

### Inbound Policy

```xml
<policies>
    <inbound>
        <base />

        <!-- Validate APIM subscription key (API Key) -->
        <check-header name="Ocp-Apim-Subscription-Key" failed-check-httpcode="401" failed-check-error-message="Subscription key required" ignore-case="false">
            <value>@(context.Subscription?.Key ?? "")</value>
        </check-header>

        <!-- Add shared secret header for backend authentication -->
        <set-header name="X-APIM-Secret" exists-action="override">
            <value>{{apim-backend-secret}}</value>
        </set-header>

        <!-- Optional: Forward subscription info to backend -->
        <set-header name="X-Subscription-Name" exists-action="override">
            <value>@(context.Subscription?.Name ?? "unknown")</value>
        </set-header>
        <set-header name="X-Subscription-Id" exists-action="override">
            <value>@(context.Subscription?.Id ?? "unknown")</value>
        </set-header>

        <!-- Set backend URL -->
        <set-backend-service base-url="https://your-function-app.azurewebsites.net" />
    </inbound>

    <backend>
        <base />
    </backend>

    <outbound>
        <base />

        <!-- Remove internal headers from response -->
        <set-header name="X-APIM-Secret" exists-action="delete" />
        <set-header name="X-Subscription-Name" exists-action="delete" />
        <set-header name="X-Subscription-Id" exists-action="delete" />
    </outbound>

    <on-error>
        <base />
    </on-error>
</policies>
```

## Policy Template 3: Dual Authentication (JWT or API Key)

This policy accepts either JWT tokens or API keys.

### Inbound Policy

```xml
<policies>
    <inbound>
        <base />

        <choose>
            <!-- Option 1: JWT Bearer Token -->
            <when condition="@(context.Request.Headers.ContainsKey("Authorization") && context.Request.Headers.GetValueOrDefault("Authorization", "").StartsWith("Bearer "))">
                <validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="Invalid JWT token">
                    <openid-config url="https://login.microsoftonline.com/{{entra-tenant-id}}/v2.0/.well-known/openid-configuration" />
                    <audiences>
                        <audience>{{entra-audience}}</audience>
                    </audiences>
                    <issuers>
                        <issuer>https://login.microsoftonline.com/{{entra-tenant-id}}/v2.0</issuer>
                        <issuer>https://sts.windows.net/{{entra-tenant-id}}/</issuer>
                    </issuers>
                </validate-jwt>

                <set-header name="X-Auth-Method" exists-action="override">
                    <value>JWT</value>
                </set-header>
            </when>

            <!-- Option 2: APIM Subscription Key -->
            <when condition="@(context.Request.Headers.ContainsKey("Ocp-Apim-Subscription-Key"))">
                <check-header name="Ocp-Apim-Subscription-Key" failed-check-httpcode="401" failed-check-error-message="Invalid subscription key" ignore-case="false" />

                <set-header name="X-Auth-Method" exists-action="override">
                    <value>APIKey</value>
                </set-header>
            </when>

            <!-- No valid authentication provided -->
            <otherwise>
                <return-response>
                    <set-status code="401" reason="Unauthorized" />
                    <set-header name="WWW-Authenticate" exists-action="override">
                        <value>Bearer realm="API", charset="UTF-8"</value>
                    </set-header>
                    <set-body>@{
                        return new JObject(
                            new JProperty("error", "unauthorized"),
                            new JProperty("message", "Authentication required. Provide either a valid JWT Bearer token or API subscription key.")
                        ).ToString();
                    }</set-body>
                </return-response>
            </otherwise>
        </choose>

        <!-- Add shared secret for backend -->
        <set-header name="X-APIM-Secret" exists-action="override">
            <value>{{apim-backend-secret}}</value>
        </set-header>

        <!-- Set backend URL -->
        <set-backend-service base-url="https://your-function-app.azurewebsites.net" />
    </inbound>

    <backend>
        <base />
    </backend>

    <outbound>
        <base />

        <!-- Remove internal headers -->
        <set-header name="X-APIM-Secret" exists-action="delete" />
        <set-header name="X-Auth-Method" exists-action="delete" />
    </outbound>

    <on-error>
        <base />
    </on-error>
</policies>
```

## Policy Template 4: Rate Limiting + Authentication

This policy adds rate limiting on top of authentication.

### Inbound Policy

```xml
<policies>
    <inbound>
        <base />

        <!-- Validate JWT token -->
        <validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="Unauthorized">
            <openid-config url="https://login.microsoftonline.com/{{entra-tenant-id}}/v2.0/.well-known/openid-configuration" />
            <audiences>
                <audience>{{entra-audience}}</audience>
            </audiences>
        </validate-jwt>

        <!-- Rate limiting: 100 calls per minute per subscription -->
        <rate-limit-by-key calls="100" renewal-period="60" counter-key="@(context.Subscription?.Id ?? "anonymous")" />

        <!-- Quota: 10,000 calls per day per subscription -->
        <quota-by-key calls="10000" renewal-period="86400" counter-key="@(context.Subscription?.Id ?? "anonymous")" />

        <!-- Add shared secret for backend -->
        <set-header name="X-APIM-Secret" exists-action="override">
            <value>{{apim-backend-secret}}</value>
        </set-header>

        <!-- Set backend URL -->
        <set-backend-service base-url="https://your-function-app.azurewebsites.net" />
    </inbound>

    <backend>
        <base />
    </backend>

    <outbound>
        <base />

        <!-- Add rate limit headers to response -->
        <set-header name="X-RateLimit-Limit" exists-action="override">
            <value>100</value>
        </set-header>
        <set-header name="X-RateLimit-Remaining" exists-action="override">
            <value>@(context.Response.Headers.GetValueOrDefault("X-Rate-Limit-Remaining", "0"))</value>
        </set-header>

        <!-- Remove internal headers -->
        <set-header name="X-APIM-Secret" exists-action="delete" />
    </outbound>

    <on-error>
        <base />
    </on-error>
</policies>
```

## Testing Your APIM Setup

### 1. Test with JWT Token

```bash
# Get a token from Azure AD
ACCESS_TOKEN=$(curl -X POST "https://login.microsoftonline.com/{tenant-id}/oauth2/v2.0/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id={client-id}" \
  -d "client_secret={client-secret}" \
  -d "scope=api://{api-id}/.default" \
  -d "grant_type=client_credentials" | jq -r '.access_token')

# Call your API through APIM
curl -X GET "https://your-apim.azure-api.net/api/query/GetProjectInstancesQuery?body=%7B%7D" \
  -H "Authorization: Bearer $ACCESS_TOKEN"
```

### 2. Test with API Key

```bash
# Call your API through APIM with subscription key
curl -X GET "https://your-apim.azure-api.net/api/query/GetProjectInstancesQuery?body=%7B%7D" \
  -H "Ocp-Apim-Subscription-Key: your-subscription-key"
```

### 3. Test Direct Access (Should Fail)

```bash
# Try to call Function App directly without shared secret (should be rejected)
curl -X GET "https://your-function-app.azurewebsites.net/api/query/GetProjectInstancesQuery?body=%7B%7D" \
  -H "Authorization: Bearer $ACCESS_TOKEN"

# Expected response: 401 Unauthorized (because X-APIM-Secret header is missing)
```

### 4. Verify Shared Secret Protection

```bash
# Try with wrong shared secret (should fail)
curl -X GET "https://your-function-app.azurewebsites.net/api/query/GetProjectInstancesQuery?body=%7B%7D" \
  -H "X-APIM-Secret: wrong-secret" \
  -H "Authorization: Bearer $ACCESS_TOKEN"

# Expected: Still rejected because shared secret doesn't match
```

## Security Checklist

Before deploying to production:

- [ ] Backend Function App configured with `TrustApim: true` and strong `SharedSecret`
- [ ] APIM Named Values configured with correct values
- [ ] Function App network restricted to APIM subnet (or protected by other means)
- [ ] SharedSecret is long, random, and stored securely (not in code)
- [ ] JWT validation configured with correct tenant and audience
- [ ] Rate limiting policies applied to prevent abuse
- [ ] CORS policies configured if needed for browser clients
- [ ] Logging and monitoring enabled in APIM
- [ ] APIM SSL/TLS certificates valid and up to date
- [ ] Test all authentication scenarios before going live

## Deployment Steps

### 1. Create APIM Named Values

```bash
az apim nv create \
  --resource-group your-rg \
  --service-name your-apim \
  --named-value-id apim-backend-secret \
  --display-name "APIM Backend Secret" \
  --value "your-secure-shared-secret" \
  --secret true

az apim nv create \
  --resource-group your-rg \
  --service-name your-apim \
  --named-value-id entra-tenant-id \
  --display-name "Entra Tenant ID" \
  --value "your-tenant-id"

az apim nv create \
  --resource-group your-rg \
  --service-name your-apim \
  --named-value-id entra-audience \
  --display-name "Entra Audience" \
  --value "api://your-api-id"
```

### 2. Create API in APIM

```bash
# Import OpenAPI spec or create manually
az apim api create \
  --resource-group your-rg \
  --service-name your-apim \
  --api-id instance-manager-api \
  --path "/api" \
  --display-name "Instance Manager API" \
  --service-url "https://your-function-app.azurewebsites.net"
```

### 3. Apply Policy

Copy one of the policy templates above and apply it:

1. Go to Azure Portal > API Management > APIs > Your API
2. Select "All operations" or specific operation
3. In "Inbound processing", click the code editor `</>` icon
4. Paste the policy template
5. Update placeholders (backend URL, named value references)
6. Save

### 4. Update Function App Settings

```bash
az functionapp config appsettings set \
  --name your-function-app \
  --resource-group your-rg \
  --settings \
    "Authentication__RequireAuthentication=true" \
    "Authentication__Apim__TrustApim=true" \
    "Authentication__Apim__SharedSecret=your-secure-shared-secret" \
    "Authentication__Apim__RequireSharedSecret=true"
```

### 5. Test End-to-End

Run the test commands from the "Testing Your APIM Setup" section above.

## Troubleshooting

### Issue: 401 Unauthorized from APIM

**Solution:** Check JWT validation config, ensure token is valid and audience matches.

### Issue: 401 Unauthorized from Function App

**Solution:** Verify `X-APIM-Secret` header is being sent and matches backend configuration.

### Issue: 500 Internal Server Error

**Solution:** Check APIM trace logs and Function App Application Insights for detailed errors.

### Issue: Function App accepts direct requests (bypassing APIM)

**Solution:**
1. Restrict Function App networking to APIM subnet
2. Or ensure `RequireSharedSecret: true` and keep secret confidential

## Additional Resources

- [Azure APIM Policy Reference](https://learn.microsoft.com/en-us/azure/api-management/api-management-policies)
- [JWT Validation Policy](https://learn.microsoft.com/en-us/azure/api-management/validate-jwt-policy)
- [Rate Limiting Policies](https://learn.microsoft.com/en-us/azure/api-management/rate-limit-policy)
- [Azure Functions Networking](https://learn.microsoft.com/en-us/azure/azure-functions/functions-networking-options)
