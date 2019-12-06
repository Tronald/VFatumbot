using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Geocoding;
using Geocoding.Google;
using OneSignal.RestAPIv3.Client;
using OneSignal.RestAPIv3.Client.Resources;
using OneSignal.RestAPIv3.Client.Resources.Notifications;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using Force.Crc32;
using Microsoft.Bot.Builder;
using Bia.Countries.Iso3166;
using Microsoft.Bot.Schema;
using static VFatumbot.BotLogic.Enums;
using Newtonsoft.Json.Linq;
using CoordinateSharp;
using System.Threading;

namespace VFatumbot.BotLogic
{
    public static class Helpers
    {
        public async static Task HelpAsync(ITurnContext turnContext, UserProfileTemporary userProfileTemporary, MainDialog mainDialog, CancellationToken cancellationToken)
        {
#if RELEASE_PROD
            var help1 = System.IO.File.ReadAllText("help-prod1.txt").Replace("APP_VERSION", Consts.APP_VERSION);
            var help2 = System.IO.File.ReadAllText("help-prod2.txt").Replace("APP_VERSION", Consts.APP_VERSION);
            var help3 = System.IO.File.ReadAllText("help-prod3.txt").Replace("APP_VERSION", Consts.APP_VERSION);
            var help4 = System.IO.File.ReadAllText("help-prod4.txt").Replace("APP_VERSION", Consts.APP_VERSION);
#else
            var help1 = System.IO.File.ReadAllText("help-dev1.txt").Replace("APP_VERSION", Consts.APP_VERSION);
            var help2 = System.IO.File.ReadAllText("help-dev2.txt").Replace("APP_VERSION", Consts.APP_VERSION);
            var help3 = System.IO.File.ReadAllText("help-dev3.txt").Replace("APP_VERSION", Consts.APP_VERSION);
            var help4 = System.IO.File.ReadAllText("help-dev4.txt").Replace("APP_VERSION", Consts.APP_VERSION);
#endif
            await turnContext.SendActivityAsync(MessageFactory.Text(help1), cancellationToken);
            await turnContext.SendActivityAsync(MessageFactory.Text(help2), cancellationToken);
            await turnContext.SendActivityAsync(MessageFactory.Text(help3), cancellationToken);
            await turnContext.SendActivityAsync(MessageFactory.Text(help4), cancellationToken);

            if (!string.IsNullOrEmpty(turnContext.Activity.Text) && !userProfileTemporary.IsLocationSet)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(Consts.NO_LOCATION_SET_MSG), cancellationToken);

                // Hack coz Facebook Messenge stopped showing "Send Location" button
                if (turnContext.Activity.ChannelId.Equals("facebook"))
                {
                    await turnContext.SendActivityAsync(CardFactory.CreateGetLocationFromGoogleMapsReply());
                }

                return;
            }
            else
            {
                await((AdapterWithErrorHandler)turnContext.Adapter).RepromptMainDialog(turnContext, mainDialog, cancellationToken);
                return;
            }
        }

        public static bool InterceptLocation(ITurnContext turnContext, out double lat, out double lon)
        {
            lat = lon = Consts.INVALID_COORD;

            var activity = turnContext.Activity;

            bool isFound = false;

            // Prioritize geo coordinates sent via entities
            if (activity.Entities != null)
            {
                foreach (Entity entity in activity.Entities)
                {
                    if (entity.Type == "Place")
                    {
                        Place place = entity.GetAs<Place>();
                        GeoCoordinates geo = JsonConvert.DeserializeObject<GeoCoordinates>(place.Geo + "");
                        lat = (double)geo.Latitude;
                        lon = (double)geo.Longitude;
                        isFound = true;
                        break;
                    }
                }
            }

            // Secondly is if there is a Google Map URL
            if (!isFound && activity.Text != null && (activity.Text.Contains("google.com/maps/") || activity.Text.Contains("Sending location @")))
            {
                string[] seps0 = { "@" };
                string[] entry0 = turnContext.Activity.Text.Split(seps0, StringSplitOptions.RemoveEmptyEntries);
                string[] seps = { "," };
                string[] entry = entry0[1].Split(seps, StringSplitOptions.RemoveEmptyEntries);
                if (Double.TryParse(entry[0], NumberStyles.Any, CultureInfo.InvariantCulture, out lat) &&
                    Double.TryParse(entry[1], NumberStyles.Any, CultureInfo.InvariantCulture, out lon))
                {
                    isFound = true;
                }
            }

            // Thirdly, geocode the address the user sent
            if (!isFound && !string.IsNullOrEmpty(activity.Text)
                && (activity.Text.ToLower().StartsWith("search", StringComparison.InvariantCulture) ||
                    activity.Text.ToLower().StartsWith("/setlocation", StringComparison.InvariantCulture)))
            {
                // dirty hack: get the calling method which is already async to do the Google Geocode async API call
                lat = lon = Consts.INVALID_COORD;
                isFound = true;
            }

            // Fourthly, sometime around late October 2019, about two months after I started coding this bot, Facebook
            // for whatever reason decided to stop displaying the "Location" button that allowed users to easily send
            // their location to us. So here's my workaround that intercepts a shared message sent to the bot from
            // Google Maps with coordinates that we decode here. FYI I could do with some more 酎ハイ right now.
            if (!isFound && activity.ChannelId.Equals("facebook"))
            {
                try
                {
                    /* e.g.
                     * "channelData":{ 
                                      "sender":{ 
                                         "id":"2418280911623293"
                                      },
                                      "recipient":{ 
                                         "id":"422185445016594"
                                      },
                                      "timestamp":1571933853598,
                                      "message":{ 
                                         "mid":"WYvV4Nkos0LXSCT0xhwHz-QWY6PlmXHw1lzdArnJxcDyHORviVvB-22m880-8unGmfNfdwNwANdH4KxHFmVbrQ",
                                         "seq":0,
                                         "is_echo":false,
                                         "attachments":[ 
                                            { 
                                               "type":"fallback",
                                               "title":"33°35'06.1\"N 130°20'24.0\"E",
                                               "url":"https://l.facebook.com/l.php?u=https%3A%2F%2Fgoo.gl%2Fmaps%2Fzzbjm7nutWjmYdDo9&h=AT1d60WwFvNZF-1afhyRyFlCUZLvJqxlw5bgPcYga8z-oi_sA7RO1fn7OwP8Nn29Vi31OTIoWI-aKSTQe-UEJnTzoPA99f5E5nSnb2yZOxYhFNc6EEglmnflNMQ5vBC8KhWEnDt6dw1R&s=1",
                                               "payload":null
                                            }
                                         ]
                                      }
                                   },
                     */
                    JObject channelData = JObject.Parse(activity.ChannelData.ToString());
                    JToken title = channelData["message"]["attachments"][0]["title"];

                    // CoordinateSharp: https://github.com/Tronald/CoordinateSharp
                    Coordinate coordinates = null;
                    if (Coordinate.TryParse(title.ToString(), out coordinates))
                    {
                        lat = coordinates.Latitude.ToDouble();
                        lon = coordinates.Longitude.ToDouble();
                        isFound = true;
                    }
                }
                catch (Exception)
                {
                }
            }

            return isFound;
        }

        public static string GetNewLine(ITurnContext context)
        {
            if (context.Activity.ChannelId == Enums.ChannelPlatform.directline.ToString())
            {
                return "\n\n";
            }
            else
            {
                return "<br/><br/>";
            }
        }

        public static string DirectLineNewLineFix(ITurnContext turnContext, string mesg)
        {
            return turnContext.Activity.ChannelId.Equals(Enums.ChannelPlatform.directline.ToString()) ? mesg.Replace("\n\n", "  \n") : mesg;
        }

        public static string GetCountryFromW3W(dynamic w3wresult)
        {
            if (w3wresult == null || w3wresult.country == null)
                return "";

            var country = Bia.Countries.Iso3166.Countries.GetCountryByAlpha2(w3wresult.country.ToString().Replace("{","").Replace("}",""));

            if (country != null)
                return $" ({country.ShortName})";

            return "";
        }

        public static async Task<object> GetWhat3WordsAddressAsync(double[] incoords)
        {
            var response = await new HttpClient().GetAsync("https://api.what3words.com/v3/convert-to-3wa?coordinates=" + incoords[0] + "%2C" + incoords[1] + "&key=" + Consts.W3W_API_KEY);
            var jsonContent = response.Content.ReadAsStringAsync().Result;
            dynamic result = JsonConvert.DeserializeObject(jsonContent);
            return result;
        }

        public static async Task<Tuple<double, double>> GeocodeAddressAsync(string address)
        {
            IGeocoder geocoder = new GoogleGeocoder() { ApiKey = Consts.GOOGLE_MAPS_API_KEY };
            IEnumerable<Address> addresses = await geocoder.GeocodeAsync(address);

            if (addresses.Count() != 0)
            {
                return new Tuple<double, double>(addresses.First().Coordinates.Latitude, addresses.First().Coordinates.Longitude);
            }

            return null;
        }

        public static bool IsRandoLobby(ITurnContext turnContext)
        {
            return turnContext.Activity.ChannelId.Equals(ChannelPlatform.telegram.ToString())
                   && turnContext.Activity.Conversation.IsGroup == true
                   && ("RANDONAUTS (LOBBY)".Equals(turnContext.Activity.Conversation.Name) || "botwars".Equals(turnContext.Activity.Conversation.Name));
        }

        public static async Task<string[]> GetIntentSuggestionsAsync(QuantumRandomNumberGeneratorWrapper rnd)
        {
            int numSuggestions = 5;
            string[] result = new string[numSuggestions];
            string[] words = await System.IO.File.ReadAllLinesAsync("words.txt");
            for (int i = 0; i < numSuggestions; i++)
            {
                result[i] = words[rnd.Next(words.Length)];
            }
            return result;
        }

        public static async Task<bool> IsWaterCoordinatesAsync(double[] incoords)
        {
            // Get a static map with no frills, and make the water green to compare
            var url = "http://maps.googleapis.com/maps/api/staticmap?scale=2" +
                        "&zoom=13&size=1x1&sensor=false&visual_refresh=true" +
                        "&style=feature:water|color:0x00FF00" +
                        "&style=element:labels|visibility:off" +
                        "&style=feature:transit|visibility:off" +
                        "&style=feature:poi|visibility:off" +
                        "&style=feature:road|visibility:off" +
                        "&style=feature:administrative|visibility:off" +
                        "&format=png8&center=" + incoords[0] + "," + incoords[1] +
                        "&key=" + Consts.GOOGLE_MAPS_API_KEY;
            var response = await new HttpClient().GetAsync(url);
            var resultPng = response.Content.ReadAsByteArrayAsync().Result;
            var solidGreenPng = StringToByteArray("89504E470D0A1A0A0000000D494844520000000200000002010300000048789F6700000006504C544500FF00FFFFFF6FBD585100000001624B474401FF022DDE0000000C4944415408D7636060600000000400012734270A0000000049454E44AE426082");
            return resultPng.SequenceEqual(solidGreenPng);
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }

        // C# dependency installed via NuGut for access to the OneSignal API is:
        // https://github.com/Alegrowin/OneSignal.RestAPIv3.Client
        public static async Task<NotificationCreateResult> SendPushNotification(UserProfileTemporary userProfileTemporary, string title, string body)
        {
            if (string.IsNullOrEmpty(userProfileTemporary.PushUserId))
            {
                return new NotificationCreateResult();
            }

            if (title == null)
            {
                title = "";
            }

            if (body == null)
            {
                body = "";
            }

            var client = new OneSignalClient(Consts.ONE_SIGNAL_API_KEY); // Use your Api Key
            var options = new NotificationCreateOptions
            {
                AppId = new Guid(Consts.ONE_SIGNAL_APP_ID),
                IncludePlayerIds = new List<string>() { userProfileTemporary.PushUserId },
                // IncludedSegments = new List<string>() { "All" } // To send to all 
            };
            options.Headings.Add(LanguageCodes.English, title);
            options.Contents.Add(LanguageCodes.English, body.Replace("<br>", "\n").Replace("\n\n", "\n"));
            return await client.Notifications.CreateAsync(options);
        }

        // https://www.c-sharpcorner.com/article/compute-sha256-hash-in-c-sharp/
        public static string Sha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static string Crc32Hash(string rawData)
        {
            Crc32Algorithm hasher = new Crc32Algorithm();
            var ret = string.Format("{0:X8}", BitConverter.ToUInt32(hasher.ComputeHash(Encoding.UTF8.GetBytes(rawData)).Reverse().ToArray()));
            return ret;
        }
    }
}
