# VFatumbot

VFatumbot is a Bot Framework SDK implementation of the Fatum Project's Telegram bot, with the ability to "Virtually" visit points and integrate with other messenger platforms.

### Fatum
The Fatum Project was born as an attempt to research unknown spaces outside predetermined probability-tunnels of the holistic world and has become a fully functional reality-tunnel creating machine that digs rabbit holes to wonderland. We are utilizing the Fatum Project's quantum random location generator Telegram bot to generate random coordinates to travel the multiverse.

### Background
The V in VFatumbot comes from the project codename when I first forked the bot. It stands for "Virtual Fatumbot" as the first feature I wanted was lazy-man shortcuts to Google Street View and Earth to see points I couldn't always visit.
Before I even knew there was a dev community (with whom I later on got involved with), I'd already come across the bot's source code [here](https://github.com/JamalYusuf/Fatum). VFatumbot was coined and forked and I began rewriting it using the Bot Framework SDK. Later on I discovered this was Fatumbot2 code and since then Fatumbot3 had been born and was now using this closed-source native C++ library called "the Newton lib" to do the calculations for finding attractors.
So I went ahead and merged the Fatumbot3's C# code into VFatumbot and along with those changes did the implementations necessary to use libAttract.dll.

#### Jan 2020 updates
So now we've made the latest bot's source code publicly available. TODO: update this README properly...
Here's the highlights:
- The original open-sourced code was Fatumbot2.. a Telegram-specific bot before libAttract was integrated
- Then came Fatumbot3 which was still Telegram only, but included the closed-source libAttract
- LibAttract is the C++ library that does the calculations for IDAs (intent-driven anomalies, i.e. Attractors/Voids)
- Then came VFatumbot... based on Fatumbot3 but overhauled to not only support Telegram, but also support a web-based bot and other "channels" like Facebook, Slack, Skype (this was 3.1.x)
- The original core logic from Fatumbot3 is still the same, with a minor update to libAttract (currently v0.96.1)
- The "overhaul" saw the bot move to something called Bot Framework; a Microsoft-provided solution for developing bots
- VFatumbot was originally hosted completely on Azure (Microsoft's cloud platform; read AWS) using their Bot Service and App Service
- Then we moved to having the bot run on a Linux virtual machine whilst keeping the Azure Bot Service so we can still integrate with the various "channels"
- It's .NET Core 2.1 based so can run on Linux / OS X too
- "**REMOVE***" appears in a bunch of files to hide the various keys/passwords we obviously don't want to make public
- Consts.cs gives you an idea of the various external services you need to sign up for (or just disable if you don't want them)
- The rest of the README below was written circa Nov 2019 when everything was still fully running on Azure and not using a Linux VM to host
- Furthermore we have the "libwrapper" which is a NodeJS-based API server that exposes libAttracts functions (but you can still skip this and natively called it from C#)
- There's probably a bunch of other info I should write here to really help anybody interested in hacking the bot, but for now I'll leave it here
- Any questions direct to soliax on reddit.com/r/randonauts or in the Telegram lobby (https://t.me/randonauts)
- Ah lastly, to run libAttract with the bot calling it from C#, unzip libAttract/libAttract.0.96.1.tar.gz, and copy libAttract* from the <platform>/release folder into the bin/*** output from Visual Studio.

## Features
- Implemented using Microsoft's Bot Framework SDK so allows access to the bot from multiple channels: Facebook, Telegram, LINE, Slack, Web, Skype and more...
- Hosted on Microsoft's cloud platform Azure which allows for more reliability and scaling.
- Interactive buttons ("prompts" in bot-speak) to improve the UX.
- Points generated are now sent in a carousel of cards with photos/text/links to view the point in Google Maps, Street View or Earth.
- [What 3 Words](https://www.what3words.com) are also displayed for any location you send or point generated.
- Your location, or the location you want to start generating a point from, can now be searched for by just typing in the place name, address etc.
- Points generated that turn out to be on water can be set to be ignored.
- Trip reporting

## Getting it running

### Initial setup ###
1. Sign up for a free account on Azure (Microsoft's cloud service, used to run the bot), Google's Map API (enable Maps, Street View, Static Images, Geocode APIs) and What3Words.com's developer API.
1. This bot is just an ASP.NET Core C# based project so technically you could host it yourself, or elsewhere. I chose Azure because of its integration with the Bot Framework SDK and its so-called "channels" which make integrating with other messaging platforms easy - Facebook, LINE, Slack etc.
1. Cosmos is a NoSQL DB service offered by Azure which we use to persist data. Follow Step 1 [here](https://www.c-sharpcorner.com/article/preserve-conversation-to-cosmos-db-using-azure-bot-framework-sdk-v4/) to set it up.
1. Look in repo/BotLogic/Consts.cs and fill in the various API keys and other config.

### Running this repo's code on Azure ###
1. Download Visual Studio Community (the free edition. I used the Mac OS version).
1. Open the VFatumbot.sln solution project in the root folder.
1. In the Solution Explorer panel, open the tree until you see Dependencies. Right-click and choose Restore, this will download all the libraries that the project requires (Netwonsoft JSON serializer, Bot Framework SDK, Azure stuff etc.)
1. Assuming you've signed up and are logged into [https://portal.azure.com/#home](https://portal.azure.com/#home), Click "+Create a resource" in the top of the left-hand side menu.
1. Using the marketplace search box, search "bot" and select "Web App bot" from the auto-suggestions, then click Create.
1. Enter/select the following:
    1. `Bot handle`: the name of your bot.
    1. `Subscription`: most likely you'll see "Free subscription", choose that (keep in mind this is only valid for 1 month but there is a free tier you can continue to use for the bot's App Service (the web app hosting service Azure provides).
    1. `Resource group`: Create a new one. Think of these is as the logical container grouping all the components that are created for your bot.
    1. `Location`: Azure has datacenters all around the world. Chose a location that's good for you (pricing can vary from region to region if you're going to be paying).
    1. `Pricing tier`: I chose Free F1 (10K Premium Messages).
    1. `App name`: whatever you like... I just used the same as the bot handle.
    1. `Bot template`: just stick with the pre-selected `Echo Bot (C#)`.
    1. `App service plan/Location`: create a new one, I set it in the same location is the resource group.
    1. `Application Insights`: leave turned on.
    1. `Application Insights Location`: set to the same location as the others above.
    1. `Microsoft App ID and password`: leave set to the auto-create option.
    1. Then click Create and wait a bit. If you get any errors it might be because one of the regions you selected doesn't support something so try changing it.
1. Navigate to left menu → Resource groups → \<the group you created\>
1. You should see 4 resources like this:

	| Name            | Type                  | Location           |
	|:----------------|:----------------------|:-------------------| 
	| VFatumbot       | Web App Bot           | global             |
	| VFatumbot       | App Service plan      | Australia Central  |
	| VFatumbot       | App Service           | Australia Central  |
	| VFatumbotror2ie | Application Insights  | Australia East     |
1. If you want to switch to the free tier immediately (or a cheaper one) to avoid the bot stop working after the trial month then:
    1. Click the App Service resource.
    1. Goto "Change App Service plan" under "App Service plan" in the left-side menu.
    1. Create a new App Service plan.
    1. Click "Pricing tier" (it'll probably be set to "Standard (S1)". Apply F1 for the free tier, D1 for a shared tier or whatever else you want.
1. Click on the Web Bot App resource.
1. Goto Channels under Bot management in the left-menu -> Click the "Configure Direct Line Line" channel (the globe icon under "Add a featured channel"). This is the channel to be used between the web-interface to the bot and the bot itself. 
1. Click Show and set one of the keys inside repo/wwwroot/v***REMOVED***.html on the line below.
`
let directLine = window.WebChat.createDirectLine({  
     token: '<INSERT TOKEN (KEY) HERE>'  
});
`
1. Goto Configuration under App Service Settings in the left-menu, and set the copy the `MicrosoftAppId` to Consts.cs and appsettings.json. Copy the `MicrosoftAppPassword` to appsettings.json too.
1. Goto the App Service -> Configuration -> Application settings -> "New application setting" and add key:ASPNETCORE_ENVIRONMENT with a value of "production" or "development". This determines which appsettings.<env>.json is used.
1. Download the Bot Framework Emulator if you want to test it locally.
1. Make sure the project can build with no errors in Visual Studio.
1. If so you're ready to deploy, click Build -> Publish to Azure. Log in with your Azure credentials and then chose the App Service you created above and publish it. Further deployments can be done from Build -> Publish to \<botname\> Web Deploy.
1. Try accessing https://\<your bot name\>.azurewebsites.net/bot.html to try out your bot!
1. If you want to integrate it with Facebook etc, look into the documentation on setting up other Channels.




