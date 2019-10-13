using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Geocoding;
using Geocoding.Google;
using Newtonsoft.Json;

namespace VFatumbot.BotLogic
{
    public static class Helpers
    {
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
    }
}
