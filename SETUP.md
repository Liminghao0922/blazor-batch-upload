# Setup Guide for Blazor Batch Upload Site

This project is an ASP.NET Core Blazor WebAssembly application with an Azure Functions backend, designed to file batch upload to Azure Blob Storage using AntDesign UI.

## Project Structure

- `src/Client`: Blazor WebAssembly Client (UI, Auth, Upload Logic).
- `src/Api`: Azure Functions (Backend API, Blob Storage Integration).

## Prerequisites

- .NET 8.0 SDK
- Azure Subscription
- Azure CLI (optional)
- GitHub Account

## Deployment Steps

### 1. Azure Resources

1. **Create Azure Blob Storage Account**:

   - Create a Storage Account in Azure.
   - Create a container named `uploads` (or the code will create it).
   - Get the **Connection String**.
2. **Create Azure Static Web App**:

   - Create a Static Web App resource in Azure.
   - Connect it to your GitHub repository.
   - Select Build Preset: **Blazor**.
   - **App location**: `/src/Client`
   - **Api location**: `/src/Api`
   - **Output location**: `wwwroot`

### 2. Configuration

#### backend (Azure Functions)

- In your Azure Static Web App, go to **Environment variables**.
- Add a new setting:
  - Name: `BlobStorageConnection`
  - Value: `<Your Blob Storage Connection String>`

#### Frontend (Azure AD Authentication)

1. **Register an App in Entra ID (Azure AD)**:

   - Register a new application.
   - Authentication > Add a platform > Single-page application.
   - Redirect URI: `https://<your-swa-domain>.azurestaticapps.net/authentication/login-callback` (and `https://localhost:7068/authentication/login-callback` for local dev).
   - Implicit grant and hybrid flows: Check **ID tokens** (and Access tokens if needed).
2. **Update `appsettings.json`**:

   - Update `src/Client/wwwroot/appsettings.json` with your Tenant ID and Client ID.

```json
  "AzureAd": {
    "Authentication": {
      "Authority": "https://login.microsoftonline.com/<TenantId>",
      "ClientId": "<ClientId>",
      "ValidateAuthority": true
    },
    "DefaultAccessTokenScopes": [ "openid", "profile" ]
  }
```

### 3. Local Development

1. **Install AntDesign Assets**:

   - The project uses AntDesign Blazor. Assets are included via `_content` references.
2. **Run Locally**:

   - You can run the Client and Api independently or use the Azure Static Web Apps CLI (SWA CLI).
   - To run Api: `cd src/Api` -> `dotnet run` (Ensure `local.settings.json` has `BlobStorageConnection`).
   - To run Client: `cd src/Client` -> `dotnet run`.

#### Handling CORS (Ports Issue)

When running locally without SWA CLI:

1. **API**: The `local.settings.json` is configured to allow CORS `*` for local dev.
2. **Client**: The `src/Client/wwwroot/appsettings.Development.json` sets `ApiBaseUrl` to `http://localhost:7071`. ensure your API is running on this port.

#### Using SWA CLI (Recommended)

1. Install SWA CLI: `npm install -g @azure/static-web-apps-cli`
2. Run from root:
   ```bash
   swa start https://localhost:7068 --api-location http://localhost:7071
   ```

   (Ensure both projects are running first)

### 4. GitHub Actions

The workflow file is located at `.github/workflows/azure-static-web-apps-deploy.yml`.
Ensure the secret `AZURE_STATIC_WEB_APPS_API_TOKEN` is set in your GitHub Repository settings if you didn't link it automatically via Azure Portal.

## Usage

1. Log in using the Azure AD button.
2. Drag and drop files or click to select.
3. Files will upload to the configured Blob Storage.
