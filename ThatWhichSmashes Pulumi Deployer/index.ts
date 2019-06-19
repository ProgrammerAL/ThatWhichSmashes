import * as pulumi from "@pulumi/pulumi";
import * as azure from "@pulumi/azure";

const config = new pulumi.Config()

const resourceLocations:string[] = ["westus"];//, "eastasia"];
const connectionStrings:pulumi.Output<string>[] = [];

// Create global Application Insights instance to track all logs
const parentRG = new azure.core.ResourceGroup("HammerTestingParentRG", {
    location: "eastus"
});

const parentAppInsights = new azure.appinsights.Insights("HammerTestingAppInsights", {
    applicationType: "web",
    location: "eastus",
    name: "HammerTestingAppInsights",
    resourceGroupName: parentRG.name,
});

//Now create an Azure Function and Queue for each region we want to deploy to
resourceLocations.forEach(location =>
{
    const resourceGroupName = "rg-" + location;
    const storageAccountName = "storage" + location;
    const queueAccountName = "hammer-requests-queue";
    const blobContainerName = "hammer-requests-blob";
    const zipBlobName = "HammerTestingFunctionApp.zip";
    const appServicePlanName = "hammer-" + location;
    const functionName = "hammer-" + location;

    // Create an Azure Resource Group
    const resourceGroup = new azure.core.ResourceGroup(resourceGroupName, {
        location: location       
    });

    // Create an Azure resource (Storage Account)
    const storageAccount = new azure.storage.Account(storageAccountName, {
        resourceGroupName: resourceGroup.name,
        location: resourceGroup.location,
        accountTier: "Standard",
        accountReplicationType: "LRS" 
    });

    //Create the Storage Queue
    const queue = new azure.storage.Queue(queueAccountName, {
        resourceGroupName: resourceGroup.name,
        storageAccountName: storageAccount.name,
        name: queueAccountName
    });

    const blobContainer = new azure.storage.Container(blobContainerName, {
        resourceGroupName: resourceGroup.name,
        storageAccountName: storageAccount.name,
        name: blobContainerName,
        containerAccessType: "blob"
    }); 

    const zipBlob = new azure.storage.Blob(zipBlobName, {
        resourceGroupName: resourceGroup.name,
        storageAccountName: storageAccount.name,
        storageContainerName: blobContainer.name,
        name: zipBlobName,
        type: "block",
        source: config.require("functionAppZipPath")
    });

    //Now deploy the Azure Function App
    const appServicePlan = new azure.appservice.Plan(appServicePlanName, {
        resourceGroupName: resourceGroup.name,
        location: location,
        kind: "FunctionApp",
            sku: {
            size: "Y1",
            tier: "Dynamic",
        },
    });

    const functionConnectionString = pulumi.interpolate `${storageAccount.primaryConnectionString};QueueEndpoint=${storageAccount.primaryQueueEndpoint};`;

    // Export the connection string for the storage account
    connectionStrings.push(functionConnectionString);

    const functionApp = new azure.appservice.FunctionApp(functionName, {
        resourceGroupName: resourceGroup.name,
        appServicePlanId: appServicePlan.id,
        location: location,
        storageConnectionString: storageAccount.primaryConnectionString,
        version: "~2",
        enableBuiltinLogging: true,
        enabled: config.requireBoolean("functionIsEnabled"),
        appSettings: {
            "FUNCTIONS_WORKER_RUNTIME": "dotnet",
            "WEBSITE_RUN_FROM_ZIP": zipBlob.url,
            "FunctionConnectionString": functionConnectionString,
            "APPINSIGHTS_INSTRUMENTATIONKEY": parentAppInsights.instrumentationKey
        },
    });
});

exports.connectionStrings = connectionStrings;
