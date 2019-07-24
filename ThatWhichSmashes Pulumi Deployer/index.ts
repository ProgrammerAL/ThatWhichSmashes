import * as pulumi from "@pulumi/pulumi";
import * as azure from "@pulumi/azure";

const zipFilePathRelative = "publishedApp/ThatWhichSmashesFunctionApp.zip";

const resourceLocations:string[] = ["westus", "centralus"];//, "eastus", "canadacentral", "canadaeast", "westeurope", "uksouth", "ukwest", "eastasia", "japanwest", "brazilsouth", "australiaeast", "southindia", "francecentral"];
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
    const appServicePlanName = "tws-" + location;
    const functionName = "tws-func-" + location;

    const resourceGroup = new azure.core.ResourceGroup(resourceGroupName, {
        location: location       
    });

    const storageAccount = new azure.storage.Account(storageAccountName, {
        resourceGroupName: resourceGroup.name,
        location: resourceGroup.location,
        accountTier: "Standard",
        accountReplicationType: "LRS",
        accountKind: "StorageV2"
    });

    //Create the Storage Queue that'll hold the messages the Function will grab from
    const queue = new azure.storage.Queue(queueAccountName, {
        resourceGroupName: resourceGroup.name,
        storageAccountName: storageAccount.name,
        name: queueName
    });

    //Now deploy the Azure Function App
    const appServicePlan = new azure.appservice.Plan(appServicePlanName, {
        resourceGroupName: resourceGroup.name,
        location: location,
        kind: "FunctionApp",
            sku: {
            size: "Y1",
            tier: "Dynamic"
        },
    });

    const functionConnectionString = pulumi.interpolate `${storageAccount.primaryConnectionString};QueueEndpoint=${storageAccount.primaryQueueEndpoint};`;

    // Export the connection string for the storage account
    connectionStrings.push(functionConnectionString);

    const dotnetApp = new azure.appservice.ArchiveFunctionApp(functionName, {
        resourceGroup : resourceGroup,
        location: location,
        account: storageAccount,
        name: functionName,
        archive: new pulumi.asset.FileArchive(zipFilePathRelative),
        version: "~2",
        enableBuiltinLogging: true,
        plan: appServicePlan,
        appSettings: {
            "runtime": "dotnet",
            "FunctionConnectionString": functionConnectionString,
            "APPINSIGHTS_INSTRUMENTATIONKEY": parentAppInsights.instrumentationKey,
        },
    });
});

exports.connectionStrings = connectionStrings;
