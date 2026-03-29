// DomusMind - CloudHosted Azure Baseline
// Minimal first hosted baseline for Azure App Service + PostgreSQL Flexible Server

@description('Short environment prefix used in resource names, e.g. prod or staging')
param envPrefix string = 'prod'

@description('Azure region for all resources')
param location string = 'spaincentral'

@description('PostgreSQL administrator username')
param dbAdminUser string = 'domusmind'

@description('PostgreSQL administrator password')
@secure()
param dbAdminPassword string

@description('JWT signing key, minimum 32 chars')
@secure()
param jwtSigningKey string

@description('Container image to deploy. Format: registry/owner/image:tag — use an immutable semver tag.')
param appImage string = 'ghcr.io/juangcarmona/domusmind:1.0.0'

@description('Temporary bootstrap switch. Only use true for the very first deployment if PostgreSQL connectivity blocks startup. Set back to false after adding specific firewall rules.')
param allowBroadAzurePostgresAccess bool = false

var appName = 'domusmind-${envPrefix}'
var planName = 'plan-domusmind-${envPrefix}'
var dbServerName = 'pg-domusmind-${envPrefix}'
var dbName = 'domusmind'
var kvName = 'kv-domusmind-${envPrefix}'
var logWorkspaceName = 'log-domusmind-${envPrefix}'
var appInsightsName = 'ai-domusmind-${envPrefix}'

// ---------- Observability ----------

resource logWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logWorkspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logWorkspace.id
  }
}

// ---------- Key Vault ----------

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: kvName
  location: location
  properties: {
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 30
    sku: {
      family: 'A'
      name: 'standard'
    }
  }
}

// Store secrets in Key Vault from secure deployment parameters.
// Fine for bootstrap. Do not commit secret values to repo.
resource kvSecretDbPassword 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'db-password'
  properties: {
    value: dbAdminPassword
  }
}

resource kvSecretJwtKey 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'jwt-signing-key'
  properties: {
    value: jwtSigningKey
  }
}

// Store full connection string as one secret.
// App Service Key Vault references work best when the full setting value is the reference.
resource kvSecretConnectionString 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'connectionstrings--domusmind'
  properties: {
    value: 'Host=${dbServerName}.postgres.database.azure.com;Port=5432;Database=${dbName};Username=${dbAdminUser};Password=${dbAdminPassword};Ssl Mode=Require;Trust Server Certificate=false'
  }
}

// ---------- PostgreSQL ----------

resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-12-01' = {
  name: dbServerName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    administratorLogin: dbAdminUser
    administratorLoginPassword: dbAdminPassword
    version: '16'
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 14
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
  }
}

resource postgresDb 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-12-01-preview' = {
  parent: postgresServer
  name: dbName
}

// Temporary bootstrap rule only.
// Remove after you add specific firewall rules for the web app outbound IPs.
resource postgresFirewallAllowAzure 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-12-01-preview' = if (allowBroadAzurePostgresAccess) {
  parent: postgresServer
  name: 'AllowAllAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// ---------- App Service ----------

resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: planName
  location: location
  kind: 'linux'
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2023-01-01' = {
  name: appName
  location: location
  kind: 'app,linux,container'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOCKER|${appImage}'
      alwaysOn: true
      healthCheckPath: '/api/setup/status'
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'Deployment__Mode'
          value: 'CloudHosted'
        }
        {
          name: 'Deployment__AllowHouseholdCreation'
          value: 'false'
        }
        {
          name: 'Deployment__InvitationsEnabled'
          value: 'true'
        }
        {
          name: 'Deployment__RequireInvitationForSignup'
          value: 'true'
        }
        {
          name: 'Deployment__AdminToolsEnabled'
          value: 'true'
        }
        {
          name: 'ConnectionStrings__domusmind'
          value: '@Microsoft.KeyVault(VaultName=${kvName};SecretName=connectionstrings--domusmind)'
        }
        {
          name: 'Jwt__Issuer'
          value: 'domusmind'
        }
        {
          name: 'Jwt__Audience'
          value: 'domusmind'
        }
        {
          name: 'Jwt__ExpiryMinutes'
          value: '60'
        }
        {
          name: 'Jwt__RefreshTokenExpiryDays'
          value: '30'
        }
        {
          name: 'Jwt__SigningKey'
          value: '@Microsoft.KeyVault(VaultName=${kvName};SecretName=jwt-signing-key)'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://ghcr.io'
        }
      ]
    }
  }
  dependsOn: [
    kvSecretConnectionString
    kvSecretJwtKey
    appInsights
  ]
}

// Key Vault Secrets User
resource kvRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, webApp.id, '4633458b-17de-408a-b874-0445c86b69e6')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '4633458b-17de-408a-b874-0445c86b69e6'
    )
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// ---------- Outputs ----------

output webAppName string = webApp.name
output webAppHostname string = webApp.properties.defaultHostName
output appServicePlanName string = appServicePlan.name
output postgresServerName string = postgresServer.name
output postgresServerFqdn string = postgresServer.properties.fullyQualifiedDomainName
output keyVaultName string = keyVault.name
output appInsightsConnectionString string = appInsights.properties.ConnectionString
output webAppPrincipalId string = webApp.identity.principalId
output webAppOutboundIpAddresses string = webApp.properties.outboundIpAddresses
