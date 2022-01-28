// Azure Functions - Bicep module
// Generated by NubesGen (www.nubesgen.com)

@description('The name of your application')
param applicationName string

@description('The environment (dev, test, prod, ...')
@maxLength(4)
param environment string = 'dev'

@description('The number of this specific instance')
@maxLength(3)
param instanceNumber string = '001'

@description('The Azure region where all resources in this example should be created')
param location string

@description('An array of NameValues that needs to be added as environment variables')
param environmentVariables array

@description('A list of tags to apply to the resources')
param resourceTags object

var appServicePlanName = 'plan-${applicationName}-${environment}-${instanceNumber}'

resource storageAccount 'Microsoft.Storage/storageAccounts@2019-06-01' = {
  name: 'stf${take(applicationName,4)}${take(environment,2)}${instanceNumber}'
  location: location
  kind: 'StorageV2'
  tags: resourceTags
  sku: {
    name: 'Standard_LRS'
  }
}

resource hostingPlan 'Microsoft.Web/serverfarms@2020-10-01' = {
  name: appServicePlanName
  location: location
  tags: resourceTags
  kind: 'functionapp'
  properties: {
    reserved: true
  }
  sku: {
    name: 'Y1' 
  }
}

resource functionApp 'Microsoft.Web/sites@2020-06-01' = {
  name: 'app-${applicationName}-${environment}-${instanceNumber}'
  location: location
  kind: 'functionapp,linux'
  properties: {
    httpsOnly: true
    serverFarmId: hostingPlan.id
    clientAffinityEnabled: false
    siteConfig: {
      linuxFxVersion: 'DOTNET|6.0'
      // use32BitWorkerProcess: true
      appSettings: union(environmentVariables, [
        
        // {
        //   'name': 'APPINSIGHTS_INSTRUMENTATIONKEY'
        //   'value': appInsights.properties.InstrumentationKey
        // }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${az.environment().suffixes.storage};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}'
        }
        {
          'name': 'FUNCTIONS_EXTENSION_VERSION'
          'value': '~4'
        }
        {
          'name': 'FUNCTIONS_WORKER_RUNTIME'
          'value': 'dotnet'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${az.environment().suffixes.storage};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}'
        }
        {
          'name': 'WEBSITE_RUN_FROM_PACKAGE'
          'value': '1'
        }
      ])
    }
  }
}

output application_name string = functionApp.name
output application_url string = functionApp.properties.hostNames[0]
