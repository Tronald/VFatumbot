## VFatumbot README ##

# TODO: fill in stuff #

### Setup ###
1. Sign up for a free account to Azure (Microsoft's cloud service, used to run the bot) and Google's Map API.
1. This bot is just an ASP.NET Core C# based project so technically you could host it yourself, or elsewhere. I chose Azure because of its integration with the Bot 1. Framework SDK and its so-called "channels" which make linking into other messaging platforms easy - Facebook, LINE, Slack etc.
1. Look in Consts.cs and fill in the various API keys.
1. Cosmos is a NoSQL DB service offered by Azure. Follow Step 1 here (https://www.c-sharpcorner.com/article/preserve-conversation-to-cosmos-db-using-azure-bot-framework-sdk-v4/) to setup a DB.


### Running this repo's code on Azure ###

1. Assuming you've signed up and are logged into https://portal.azure.com/#home, Click "+Create a resource" in the top of the left-hand side menu
1. Using the marketplace search box, search "bot" and select "Web App bot" from the auto-suggestions, then click `Create`.
1. Enter/select the following:
    1. `Bot handle`: the name of your bot.
    1. `Subscription`: most likely you'll see "Free subscription", choose that (keep in mind this is only valid for 1 month but there is a free-tier you can continue to use for the bot's App Service (the web app hosting service Azure provides).
    1. `Resource group`: Create a new one. Think of these is as the logical container grouping all the components that are created for your bot.
    1. `Location`: Azure has datacenters all around the world. Chose a location that's good for you (pricing can vary from region to region if you're going to be paying).
    1. `Pricing tier`: I chose S1 (1K Premium Msgs/Unit).
    1. `App name`: whatever you like... I just used the same as the bot handle.
    1. `Bot template`: just stick with the pre-selected `Echo Bot (C#)`.
    1. `App service plan/Location`: create a new one, I set it in the same location is the resource group.
    1. `Application Insights`: leave turned on.
    1. `Application Insights Location`: set to the same location as the others above.
    1. `Microsoft App ID and password`: leave set to the auto-create option.
    1. 
    1. 
    1. 
    1. 
    1. 

