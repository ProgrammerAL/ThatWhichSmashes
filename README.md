//TODO: This readme is still a work in progress. Waiting to finish a blog post before completing it.

# Introduction 
This is a demo application to explain how to perform what I call Hammer Testing. Which is where the name comes from. That Which Smashes in English for Mjölnir.
This is still pretty manual
You can find the reasons for this application at //TODO: Add link to blog post when it exists
Assumptions:
- You already have [Pulumi](https://pulumi.io/) setup in your local environment

# What This Is

# Getting Started

1. Deploy the resources to Azure
  - Open the `ThatWhichSmashes Pulumi Deployer/index.ts` file and set the `resourceLocations` array with the list of azure regions you want to deploy to
  - Use the `pulumi up` command to deploy the resources to Azure
1. Use the `RequestQueueAdder` console application to fill in the queues with the messages
  - Fill in the `RequestQueueAdder/data/QueueEndpoints.json` file with the connection strings of all the queues that were created with Pulumi. You can find them in the console output or online in the portal
1. Redeploy the resource to Azure, but with the Azure Functions enabled
  - You may have noticed that the Azure Functions deployed are disabled by default. This is to give you time to fill in the queues
  - Change the `functionIsEnabled` variable in Pulumi.dev.yaml  to `true`
  - Run the `pulumi up` command again
1. Wait for the functions to complete
  - View the statistics in the instance of Application Insights that was deployted with Pulumi
1. When you're done, remove the resources from Azure with `pulumi destroy`


# Build and Test
TODO: Describe and show how to build your code and run the tests. 

