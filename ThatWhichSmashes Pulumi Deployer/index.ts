import * as pulumi from "@pulumi/pulumi";
import * as azure from "@pulumi/azure";

const config = new pulumi.Config()

const resourceLocations:string[] = ["westus", "centralus", "eastus"];//, "canadacentral", "canadaeast", "westeurope", "uksouth", "ukwest", "eastasia", "japanwest", "brazilsouth", "australiaeast", "southindia", "francecentral"];
const connectionStrings:pulumi.Output<string>[] = [];

// Create global Application Insights instance to track all logs
const parentRG = new azure.core.ResourceGroup("ThatWhichSmashesParentRG", {
    location: "eastus"
});

const appInsightsName = "ThatWhichSmashesAppInsights";

const parentAppInsights = new azure.appinsights.Insights(appInsightsName, {
    applicationType: "web",
    location: "eastus",
    name: appInsightsName,
    resourceGroupName: parentRG.name,
});

//Now create an Azure Function and Queue for each region we want to deploy to
resourceLocations.forEach(location =>
{
    const resourceGroupName = "rg-" + location;
    const storageAccountName = "stg" + location;
    const queueName = "tws-requests-queue";
    const queueAccountName = queueName + "-" + location;
    const blobContainerName = "tws-requests-blob-" + location;
    const zipFileName = config.require("functionAppZipFileName");
    const zipBlobName = location + "-" + zipFileName;
    const appServicePlanName = "tws-" + location;
    const functionName = "tws-" + location;

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
        name: queueName
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
        name: zipFileName,
        type: "block",
        source: config.require("functionAppZipFolderPath") + "/" + zipFileName
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
