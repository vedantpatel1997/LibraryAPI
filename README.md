# Library Management API

This repository contains various GitHub Action workflows to deploy a .NET Web API application to different environments and platforms. Below are the details of each workflow, the deployment method, and the required credentials.

## Workflows

### 1. Deploy .NET Web API to Linux Web App via GitHub Action (Code Deployment)

**Workflow File:** `main_librarylinuxcode.yml`  
**Build Process:** GitHub Action workflow file, auto-generated workflow. No additional configuration needed. Make sure to set the correct .NET version.  
**Deployment Method:** Code  
**Credential:** Publish Profile  

#### Workflow:

```yaml
name: Build and deploy ASP.Net Core app to Azure Web App - LibraryLinuxCode

on:
  push:
    branches:
      - main
  workflow_dispatch:

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v4

      - name: Set up .NET Core
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '7.x'

      - name: Build with dotnet
        run: dotnet build --configuration Release

      - name: dotnet publish
        run: dotnet publish -c Release -o ${{env.DOTNET_ROOT}}/myapp

      - name: Upload artifact for deployment job
        uses: actions/upload-artifact@v4
        with:
          name: .net-app
          path: ${{env.DOTNET_ROOT}}/myapp

  deploy:
    runs-on: ubuntu-latest
    needs: build
    environment:
      name: 'Production'
      url: ${{ steps.deploy-to-webapp.outputs.webapp-url }}
    
    steps:
      - name: Download artifact from build job
        uses: actions/download-artifact@v4
        with:
          name: .net-app
      
      - name: Deploy to Azure Web App
        id: deploy-to-webapp
        uses: azure/webapps-deploy@v3
        with:
          app-name: 'LibraryLinuxCode'
          slot-name: 'Production'
          package: .
          publish-profile: ${{ secrets.AZUREAPPSERVICE_PUBLISHPROFILE_8BB94465E6B14B4DA5FE267CDDD2B9D3 }}
```

### 2. Deploy .NET Web API to Windows Web App via GitHub Action (Code Deployment)

**Workflow File:** `main_librarywincode.yml`  
**Build Process:** GitHub Action workflow file, auto-generated workflow. No additional configuration needed. Make sure to set the correct .NET version.  
**Deployment Method:** Code  
**Credential:** Publish Profile  

#### Workflow:

```yaml
name: Build and deploy ASP.Net Core app to Azure Web App - libraryWincode

on:
  push:
    branches:
      - main
  workflow_dispatch:

jobs:
  build:
    runs-on: windows-latest

    steps:
      - uses: actions/checkout@v4

      - name: Set up .NET Core
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '8.x'
          include-prerelease: true

      - name: Build with dotnet
        run: dotnet build --configuration Release

      - name: dotnet publish
        run: dotnet publish -c Release -o ${{env.DOTNET_ROOT}}/myapp

      - name: Upload artifact for deployment job
        uses: actions/upload-artifact@v3
        with:
          name: .net-app
          path: ${{env.DOTNET_ROOT}}/myapp

  deploy:
    runs-on: windows-latest
    needs: build
    environment:
      name: 'Production'
      url: ${{ steps.deploy-to-webapp.outputs.webapp-url }}

    steps:
      - name: Download artifact from build job
        uses: actions/download-artifact@v3
        with:
          name: .net-app

      - name: Deploy to Azure Web App
        id: deploy-to-webapp
        uses: azure/webapps-deploy@v2
        with:
          app-name: 'libraryWincode'
          slot-name: 'Production'
          publish-profile: ${{ secrets.AZUREAPPSERVICE_PUBLISHPROFILE_F6F6CD0A9DE2406B9858590230D51505 }}
          package: .
```

### 3. Deploy .NET Web API to Linux Web App via GitHub Action (Docker Container Deployment)

**Workflow File:** `main_librarylinuxcontainer.yml`  
**Build Process:** GitHub Action workflow file, auto-generated workflow. No additional configuration needed.  
**Deployment Method:** Docker Container  
**Credential:** ACR credentials, publish profile credentials for web app  

#### Workflow:

```yaml
name: Build docker image, push to ACR and deploy to linux web app

on:
  push:
    branches:
      - main
  workflow_dispatch:

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v2

      - name: Log in to container registry
        uses: docker/login-action@v2
        with:
          registry: libraryregistry.azurecr.io
          username: ${{ secrets.ACR_USERNAME_LibraryRegistry }}
          password: ${{ secrets.ACR_PASSWORD_LibraryRegistry }}

      - name: Build and push container image to registry
        uses: docker/build-push-action@v3
        with:
          context: .
          push: true
          tags: libraryregistry.azurecr.io/library_api:${{ github.sha }}
          file: ./Dockerfile

  deploy:
    runs-on: ubuntu-latest
    needs: build

    steps:
      - name: Deploy to Azure Web App
        id: deploy-to-webapp
        uses: azure/webapps-deploy@v2
        with:
          app-name: 'LibraryLinuxContainer'
          slot-name: 'production'
          publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE_LibraryLinuxContainer }}
          images: 'libraryregistry.azurecr.io/library_api:${{ github.sha }}'
```

### 4. Deploy .NET Web API to Container App via GitHub Action (Docker Container Deployment)

**Workflow File:** `main_libraryapicontainerapp.yml`  
**Build Process:** GitHub Action workflow file, auto-generated workflow. No additional configuration needed.  
**Deployment Method:** Docker Container  
**Credential:** ACR credentials  
**Managed Identity Container Environment Credentials:** clientID, tenantID, subscriptionID  

#### Workflow:

```yaml
name: Build image, push image to ACR and deploy to library-api-container

on:
  push:
    branches:
      - main
    paths:
      - '**'
      - '.github/workflows/library-api-container-AutoDeployTrigger-677cdbcd-cad2-43c2-abe2-8c1a9a85f08a.yml'

  workflow_dispatch:

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    permissions:
      id-token: write # This is required for requesting the OIDC JWT Token
      contents: read # Required when GH token is used to authenticate with private repo

    steps:
      - name: Checkout to the branch
        uses: actions/checkout@v2

      - name: Print Base64 Encoded Secrets
        run: |
          echo "Client ID: $(echo ${{secrets.LIBRARYAPICONTAINER_AZURE_CLIENT_ID}} | sed 's/./& /g')"
          echo "Tenant ID: $(echo ${{secrets.LIBRARYAPICONTAINER_AZURE_TENANT_ID}} | sed 's/./& /g')"
          echo "Subscription ID: $(echo ${{secrets.LIBRARYAPICONTAINER_AZURE_SUBSCRIPTION_ID}} | sed 's/./& /g')"
          echo "Registry Username: $(echo ${{secrets.ACR_USERNAME_LIBRARYREGISTRY}} | sed 's/./& /g')"
          echo "Registry Password: $(echo ${{secrets.ACR_PASSWORD_LIBRARYREGISTRY}} | sed 's/./& /g')"
            
      - name: Azure Login
        uses: azure/login@v1
        with:
          client-id: ${{ secrets.LIBRARYAPICONTAINER_AZURE_CLIENT_ID }}
          tenant-id: ${{ secrets.LIBRARYAPICONTAINER_AZURE_TENANT_ID }}
          subscription-id: ${{ secrets.LIBRARYAPICONTAINER_AZURE_SUBSCRIPTION_ID }}

      - name: Build and push container image to registry
        uses: azure/container-apps-deploy-action@v2
        with:
          appSourcePath: ${{ github.workspace }} 
          registryUrl: libraryregistry.azurecr.io
          registryUsername: ${{ secrets.ACR_USERNAME_LIBRARYREGISTRY }}
          registryPassword: ${{ secrets.ACR_PASSWORD_LIBRARYREGISTRY }}
          containerAppName: library-api-container
          resourceGroup: Library
          imageToBuild: libraryregistry.azurecr.io/library_api:${{ github.sha }}
```

### 5. Deploy .NET Web API to Container App & Linux Web App via GitHub Action (Docker Container Deployment)

**Workflow File:** `main_libraryapicontainerapp_linuxwebapp_combine_deployment.yml`  
**Build Process:** GitHub Action workflow file, manually written workflow.  
**Deployment Method:** Docker Container  
**Credential:** ACR credentials  
**Managed Identity Container Environment Credentials:** clientID, tenantID, subscriptionID  
**Publish Profile for

 Linux Web App**

#### Workflow:

```yaml
name: Build docker image, push to ACR and deploy to linux web app and container app

on:
  push:
    branches:
      - main
  workflow_dispatch:

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v2

      - name: Log in to container registry
        uses: docker/login-action@v2
        with:
          registry: libraryregistry.azurecr.io
          username: ${{ secrets.ACR_USERNAME_LibraryRegistry }}
          password: ${{ secrets.ACR_PASSWORD_LibraryRegistry }}

      - name: Build and push container image to registry
        uses: docker/build-push-action@v3
        with:
          context: .
          push: true
          tags: libraryregistry.azurecr.io/library_api:${{ github.sha }}
          file: ./Dockerfile

  deploy-to-webapp:
    runs-on: ubuntu-latest
    needs: build

    steps:
      - name: Deploy to Azure Web App
        id: deploy-to-webapp
        uses: azure/webapps-deploy@v2
        with:
          app-name: 'LibraryLinuxContainer'
          slot-name: 'production'
          publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE_LibraryLinuxContainer }}
          images: 'libraryregistry.azurecr.io/library_api:${{ github.sha }}'

  deploy-to-containerapp:
    runs-on: ubuntu-latest
    needs: build
    permissions:
      id-token: write # This is required for requesting the OIDC JWT Token
      contents: read # Required when GH token is used to authenticate with private repo

    steps:
      - name: Azure Login
        uses: azure/login@v1
        with:
          client-id: ${{ secrets.LIBRARYAPICONTAINER_AZURE_CLIENT_ID }}
          tenant-id: ${{ secrets.LIBRARYAPICONTAINER_AZURE_TENANT_ID }}
          subscription-id: ${{ secrets.LIBRARYAPICONTAINER_AZURE_SUBSCRIPTION_ID }}

      - name: Deploy to Azure Container App
        uses: azure/container-apps-deploy-action@v2
        with:
          appSourcePath: ${{ github.workspace }} 
          registryUrl: libraryregistry.azurecr.io
          registryUsername: ${{ secrets.ACR_USERNAME_LIBRARYREGISTRY }}
          registryPassword: ${{ secrets.ACR_PASSWORD_LIBRARYREGISTRY }}
          containerAppName: library-api-container
          resourceGroup: Library
          imageToBuild: libraryregistry.azurecr.io/library_api:${{ github.sha }}
```

## Setting Up Secrets

To set up the secrets required for the workflows, follow these steps:

1. Go to the repository on GitHub.
2. Click on `Settings`.
3. In the left sidebar, click on `Secrets and variables` and then `Actions`.
4. Click on `New repository secret`.
5. Add the required secrets:
    - `AZUREAPPSERVICE_PUBLISHPROFILE_8BB94465E6B14B4DA5FE267CDDD2B9D3`
    - `AZUREAPPSERVICE_PUBLISHPROFILE_F6F6CD0A9DE2406B9858590230D51505`
    - `ACR_USERNAME_LibraryRegistry`
    - `ACR_PASSWORD_LibraryRegistry`
    - `AZURE_PUBLISH_PROFILE_LibraryLinuxContainer`
    - `LIBRARYAPICONTAINER_AZURE_CLIENT_ID`
    - `LIBRARYAPICONTAINER_AZURE_TENANT_ID`
    - `LIBRARYAPICONTAINER_AZURE_SUBSCRIPTION_ID`

## Conclusion

This repository demonstrates different approaches to deploying a .NET Web API using GitHub Actions. Whether deploying to a Linux or Windows Web App, or using Docker containers, these workflows cover various scenarios. Ensure that all necessary secrets are correctly set up in your GitHub repository to allow for smooth deployment processes.
