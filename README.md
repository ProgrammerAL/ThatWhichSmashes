# That Which Smashes

The code in this repo is meant to deploy resources to Azure which will send many HTTP requests to specific endpoints. It deploys one Azure Function and one Azure Queue Storage resource to each Region you configure. The Azure Function then reads in JSON messages from its respective queue storage to send one or more HTTP requests to the specified endpoint(s). The deployment to Azure also creates an Application Insights instance the Azure Functions use for logging.

This is a demo application to perform what I call Hammer Testing. Which is where the project name comes from. `That Which Smashes` is English for Mjölnir. Hammer Testing is different from traditional Load Testing because it does not use a measured approach. Load Testing uses the concept of Virtual Users to let you control how your application will behave under specific loads. Hammer Testing does none of that. It sends as many requests as possible as quickly as possible. You can also think of this as a form of DDoS attack, but with less attackers.

## Disclaimers

1. I DO NOT condone anyone using the code in this repo for any malicious activities. If you choose to, you would probably be breaking some laws and usage policies. I assume. I haven't checked.
1. This is probably not a full solution for you. This project was designed to be a starting point that you customize for your own solution. I assume you will be taking this code and modifying it heavily to work for your applicaion(s). So be ready to make changes.

## Setup Instructions

In order to use this project you will need the following installed and setup on your machine.

- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest)
- [Pulumi](https://pulumi.io/)
- [npm](https://www.npmjs.com/get-npm)
- [.NET Core](https://dotnet.microsoft.com/download)

## How to Run

Running this tool is a manual effort. You're running a couple of scripts on your machine that automate everything. Below are the steps you must take to deploy and run this project.

1. Get a published zip folder of the Azure Function
    - Open the `~/ThatWhichSmashes/ThatWhichSmashes.sln` file and publish the Azure Function to a local zip file
    - Copy/paste this zip file to the `~/ThatWhichSmashes Pulumi Deployer/publishedApp` folder
    - If you use this a lot, consider creating an automated build so you don't have to manually do this each time.
1. (One Time) Initialize the Pulumi deployment code
    - Open a terminal to the `~/ThatWhichSmashes Pulumi Deployer` folder
    - Ensure you are logged in to the correct account in the Azure CLI  using the command `az login`
    - Ensure you are logged in to the correct account in the Pulumi CLI using the command `pulumi login`
    - Ensure you have the proper npm packages downloaded locally using the command `npm install`
1. Deploy the resources to Azure
    - Open the `ThatWhichSmashes Pulumi Deployer/index.ts` file and set the `resourceLocations` array with the list of azure regions you want to deploy to
        - You can get a list of all available regions from the Azure CLI using the command `az account list-locations`
    - Use the `pulumi up` command to deploy the resources to Azure
1. Manual Step - Stop all of the Azure Functions
    - The purpose of this step is to let you fill the queues with messages (next step) without them running. This way all functions start processing at the same time.
    - This is (hopefully) a temporary workaround. If you deploy the function as disabled it might not enable the queue trigger later on, so it'll never run.
        - I've tried a handful of things to work around this with no luck. Even using the Azure CLI to stop/start the functions didn't help. Only stop/starting with the web portal worked.
    - In the Azure Portal, navigate to each Azure Function you've deployed and click the stop button.
        - I recommed opening a new tab for each. You'll be turning them back on in a bit. Plus, it's a little faster that way.
1. Use the `RequestQueueAdder` console application to fill in the queues with the messages
    - Fill in the `RequestQueueAdder/data/QueueEndpoints.json` file with the connection strings of all the queues that were created with Pulumi.
        - You can find them in the console output after running the `pulumi up` command or online in the Pulumi web portal at `https://app.pulumi.com/`.
    - Next fill in the `RequestQueueAdder/data/Requests.json` file with all of the JSON messages you want to be added to each of the Azure Queues.
    - Now that those JSON files have been filled out, run the `RequestQueueAdder` console application.
        - The amount of time this takes is entirely dependent on how many messages you are adding.
1. Manual Step - Start all of the Azure Functions
    - As mentioned above, this manual step is a temporary workaround.
    - For each Azure Function you've deployed and have stopped, click the start button in the Azure Web Portal. (See why I said you'll want to keep them in individual tabs?)
1. Wait for the functions to complete
    - The Pulumi script also deployed an Application Insights instance to a Resource Group called ThatWhichSmashesParentRG. All functions log to this one instance.
1. Destroy the Infrastructure when done
    - When you no longer need any of the Azure resources, remove them with the `pulumi destroy` command.
