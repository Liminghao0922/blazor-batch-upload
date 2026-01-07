# Blazor Batch Upload Site

This project is an ASP.NET Core Blazor WebAssembly application with an Azure Functions backend, designed to facilitate batch file uploading to Azure Blob Storage using AntDesign UI.

## Project Structure

- `src/Client`: Blazor WebAssembly Client (UI, Auth, Upload Logic).
- `src/Api`: Azure Functions (Backend API, Blob Storage Integration).

## Prerequisites

- .NET 8.0 SDK
- Azure Subscription
- Azure CLI (optional)
- GitHub Account

## Getting Started

### 1. Azure Resources Setup

1.  **Azure Blob Storage Account**:
    - Create a Storage Account in Azure.
    - Create a container named `uploads` (this is optional as the code can create it).
    - Retrieve the **Connection String**.

2.  **Azure Static Web App**:
    - Create a Static Web App resource in Azure.
    - Connect it to your GitHub repository.
    - Select Build Preset: **Blazor**.
    - **App location**: `/src/Client`
    - **Api location**: `/src/Api`
    - **Output location**: `wwwroot`

### 2. Configuration

#### Backend (Azure Functions)

- Navigate to your Azure Static Web App in the portal, then to **Environment variables**.
- Add a new setting:
  - **Name**: `BlobStorageConnection`
  - **Value**: `<Your Blob Storage Connection String>`

#### Frontend (Azure AD Authentication)

1.  **Register an App in Entra ID (Azure AD)**:
    - Register a new application.
    - Under Authentication > Add a platform > Single-page application.
    - Set Redirect URI: `https://<your-swa-domain>.azurestaticapps.net/authentication/login-callback` (and `https://localhost:7068/authentication/login-callback` for local development).
    - Under Implicit grant and hybrid flows: Check **ID tokens** (and Access tokens if needed).

2.  **Update `appsettings.json`**:
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

1.  **Install Dependencies**:
    - Ensure .NET 8.0 SDK is installed.
    - The project uses AntDesign Blazor (assets are automatically included).

2.  **Run Locally**:
    - You can run the Client and Api independently or use the Azure Static Web Apps CLI (SWA CLI).
    - **Api**: `cd src/Api` -> `dotnet run` (Ensure `local.settings.json` contains `BlobStorageConnection` and `CORS` is enabled).
    - **Client**: `cd src/Client` -> `dotnet run`.

#### Handling CORS (Ports Issue)

When running locally without SWA CLI:
1.  **API**: `local.settings.json` should allow CORS `*` for local dev.
2.  **Client**: `src/Client/wwwroot/appsettings.Development.json` sets `ApiBaseUrl` to `http://localhost:7071`. Verify your API is running on this port.

#### Using SWA CLI (Recommended)

1.  Install SWA CLI: `npm install -g @azure/static-web-apps-cli`
2.  Run from the root directory:
    ```bash
    swa start https://localhost:7068 --api-location http://localhost:7071
    ```
    (Note: Typically SWA CLI will start the projects for you if configured, or you can run it against running servers).

### 4. GitHub Actions Deployment

The workflow file is located at `.github/workflows/azure-static-web-apps-deploy.yml`.
Ensure the secret `AZURE_STATIC_WEB_APPS_API_TOKEN` is set in your GitHub Repository settings if you didn't link the repository automatically via the Azure Portal during creation.

## Usage

1.  Log in using the Azure AD button on the top right.
2.  Drag and drop files into the upload area or click to select files.
3.  Files will upload to the configured Azure Blob Storage container.
