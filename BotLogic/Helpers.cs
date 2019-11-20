using System;
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

namespace VFatumbot.BotLogic
{
    public static class Helpers
    {
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
            return turnContext.Activity.ChannelId.Equals(Enums.ChannelPlatform.directline.ToString()) ? mesg.Replace("\n\n", "\n") : mesg;
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
            var client = new OneSignalClient(Consts.ONE_SIGNAL_API_KEY); // Use your Api Key
            var options = new NotificationCreateOptions
            {
                AppId = new Guid(Consts.ONE_SIGNAL_APP_ID),
                IncludePlayerIds = new List<string>() { userProfileTemporary.PushUserId },
                // IncludedSegments = new List<string>() { "All" } // To send to all 
            };
            options.Headings.Add(LanguageCodes.English, title);
            options.Contents.Add(LanguageCodes.English, body);
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
