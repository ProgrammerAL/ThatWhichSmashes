# Introduction 
The code in this repo is meant to deploy resources to Azure which will send many HTTP requests to specific endpoints. It deploys one Azure Function and one Azure Queue Storage resource to each Region you configure. The Azure Function then reads in JSON messages from its respective queue storage to send one or more HTTP requests to the specified endpoint(s). The deployment to Azure also creates an Application Insights instance the Azure Functions use for logging.

This is a demo application to perform what I call Hammer Testing. Which is where the project name comes from. `That Which Smashes` is English for Mjölnir. Hammer Testing is different from traditional Load Testing because it does not use a measured approach. Load Testing uses the concept of Virtual Users to let you control how your application will behave under specific loads. Hammer Testing does none of that. It sends as many requests as possible as quickly as possible.

# Setup Instructions
In order to use this project you will need a couple of things installed and setup on your machine.
- You must have the Azure CLI installed on your machine and be logged in with it
- You already have [Pulumi](https://pulumi.io/) setup in your local environment
- You must have .NET Core installed on your machine in order to run a setup console application


# Disclaimer
I DO NOT condone anyone using the code in this repo for any malicious activities. If you choose to do you, you would bebreaking some laws and usage policies. I assume. I haven't checked. 

Also, at the point of this writing I have not run it at large scale against an endpoint, so I'm not 100% positive it works as advertised. Will update this ReadMe with results when that happens.


# How to Run

Running this is a manual effort. You're running a couple of scripts on your machine that automate everything, but the setup and 

1. Deploy the resources to Azure
  - Open the `ThatWhichSmashes Pulumi Deployer/index.ts` file and set the `resourceLocations` array with the list of azure regions you want to deploy to
    - You can get a list of all available regions using the `az account list-locations` command from the Azure CLI
  - Use the `pulumi up` command to deploy the resources to Azure
  - Note: The Azure Functions that are deployed are disbaled right now. They will be enabled in a later step
1. Use the `RequestQueueAdder` console application to fill in the queues with the messages
  - Fill in the `RequestQueueAdder/data/QueueEndpoints.json` file with the connection strings of all the queues that were created with Pulumi. You can find them in the console output after running the `pulumi up` command or online in the Pulumi portal
  - Next fill in the `RequestQueueAdder/data/Requests.json` file with all of the json messages you want to be added to each of the Azure Queues
  - Now that those json files have been filled out, run the `RequestQueueAdder` console application
1. Redeploy the resources to Azure, but with the Azure Functions enabled
  - Open the `Pulumi.dev.yaml` file and change the `functionIsEnabled` variable to `true`
  - Run the `pulumi up` command again. This will enable all of the Azure Functions and now they will begin grabbing messages from their respective queues and sending messages
1. Wait for the functions to complete
  - View the statistics in the instance of Application Insights that was deployed with Pulumi
1. When you no longer need any of the Azure resources, remove them with the `pulumi destroy` command

