## VFatumbot README ##

# TODO: fill in stuff #

### Setup ###
Sign up for a free account to Azure (Microsoft's cloud service, used to run the bot) and Google's Map API.
This bot is just an ASP.NET Core C# based project so technically you could host it yourself, or elsewhere. I chose Azure because of its integration with the Bot Framework SDK and its so-called "channels" which make linking into other messaging platforms easy - Facebook, LINE, Slack etc.
Look in Consts.cs and fill in the various API keys.
Cosmos is a NoSQL DB service offered by Azure. Follow Step 1 here (https://www.c-sharpcorner.com/article/preserve-conversation-to-cosmos-db-using-azure-bot-framework-sdk-v4/) to setup a DB.


### Running this repo's code on Azure ###

* Assuming you've signed up and are logged into https://portal.azure.com/#home, Click "+Create a resource" in the top of the left-hand side menu
* Using the marketplace search box, search "bot" and select "Web App bot" from the auto-suggestions, then click `Create`.
* Enter/select the following:
    * Bot handle: the name of your bot
    * Subscription: most likely you'll see "Free subscription", choose that (keep in mind this is only valid for 1 month)
    * Resource group: Create a new one. Think of these is as the logical container grouping all the components that are created for your bot.
