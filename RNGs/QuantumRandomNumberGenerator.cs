using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace VFatumbot
{
    public class QuantumRandomNumberGenerator : BaseRandomProvider, IDisposable
    {
        ~QuantumRandomNumberGenerator()
        {
            Dispose(false);
        }

        private class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = 5 * 60 * 1000;
                return w;
            }
        }

        private string[] _randomData; //random pool: array of strings containing value on interval [0, 255]
        private int _randomDataIndex; //current position of random pool
        private const int RANDOM_DATA_LENGTH = 1024; //how many random bytes do we request
        /// <summary>
        /// Quantum Random Number data source.
        /// </summary>
        private void FetchRandomData()
        {
            _randomData = null;
            ServicePointManager.DefaultConnectionLimit = 50;
#if RELEASE_PROD
            //Client.Proxy = new WebProxy("123.243.160.27", 80);
            string data = Client.DownloadString(string.Format("http://qrng.anu.edu.au/API/jsonI.php?length={0}&type=uint8", RANDOM_DATA_LENGTH));
#else
            //string data = Client.DownloadString(string.Format("http://fatumemeplex.randonauts.com/anu/API/jsonI.php?length={0}&type=uint8", RANDOM_DATA_LENGTH));
            string data = Client.DownloadString(string.Format("http://qrng.anu.edu.au/API/jsonI.php?length={0}&type=uint8", RANDOM_DATA_LENGTH));
#endif
            var m = Regex.Match(data, "\"data\":\\[(?<rnd>[0-9,]*?)\\]", RegexOptions.Singleline); //parse JSON with regex
            if (m.Success)
            {
                var g = m.Groups["rnd"];
                if (g != null && g.Success)
                {
                    string[] values = g.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (values.Length == RANDOM_DATA_LENGTH)
                    {
                        _randomData = values;
                        _randomDataIndex = 0;
                        return;
                    }
                }
            }

            throw new Exception("Could not fetch new random data.");
        }

        private string FetchRandomHexData(int lnts)
        {
            _randomData = null;
            string result = "";
            int cou = 0;
            ServicePointManager.DefaultConnectionLimit = 50;
            while ((lnts > 0) && (cou < 3))
            {
                int blnts = 500;
                int bsize = 520;
                if (lnts < 1040) { blnts = 1; bsize = (int)Math.Ceiling((double)(lnts / 2)); }
                else
                {
                    double bl1 = Math.Ceiling((double)(lnts / 1040));
                    if (blnts > bl1) { blnts = (int)bl1; }
                }
#if RELEASE_PROD
                //Client.Proxy = new WebProxy("123.243.160.27", 80);
                string data = Client.DownloadString(string.Format("http://qrng.anu.edu.au/API/jsonI.php?length={0}&type=hex16&size={1}", blnts, bsize));
#else
                //string data = Client.DownloadString(string.Format("http://fatumemeplex.randonauts.com/anu/API/jsonI.php?length={0}&type=hex16&size={1}", blnts, bsize));
                string data = Client.DownloadString(string.Format("http://qrng.anu.edu.au/API/jsonI.php?length={0}&type=hex16&size={1}", blnts, bsize));
#endif
                // var m = Regex.Match(data, "\"data\":\\[(?<rnd>[a-f0-9,\"]*?)\\]", RegexOptions.Singleline); //parse JSON with regex
                var m = Regex.Match(data, "\"data\":\\[\"(?<rnd>[a-f0-9,\"]+?)\"\\]", RegexOptions.Singleline); //parse JSON with regex
                if (m.Success)
                {
                    var g = m.Groups["rnd"];
                    if (g != null && g.Success)
                    {
                        string buf = g.Value.Replace("\"", "");
                        buf = buf.Replace(",", "");
                        result += buf;
                        lnts -= buf.Length;
                    }
                    else { cou++; }
                }
                else { cou++; }
            }
            return result;
        }

        /// <summary>
        /// Gets random byte from cached data source. If source is not available it tries to fetch it.
        /// </summary>
        protected override byte GetRandomByte()
        {
            if (_randomData == null || _randomDataIndex == RANDOM_DATA_LENGTH)
            {
                FetchRandomData();
            }

            if (_randomData == null)
            {
                throw new InvalidDataException("Service did not return random data.");
            }

            return byte.Parse(_randomData[_randomDataIndex++]);
        }

        protected override string GetRandomHex(int lnts)
        {
            string hexstring = "";
            int cou = 0;
            while ((hexstring.Length < lnts)&&(cou<5))
            {
                cou++;
                try { hexstring = FetchRandomHexData(lnts); }
                catch (Exception e) { hexstring = ""; }
            }
            if (hexstring.Length < lnts)
            {
                throw new InvalidDataException("Service did not return random data.");
            }
            return hexstring;
        }


        protected WebClient Client
        {
            get { return _client ?? (_client = new WebClient()); }
        }
        private WebClient _client;

#region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //clean up managed resources
                if (_client != null)
                {
                    _client.Dispose();
                    _client = null;
                }
            }

            //clean up unmanaged resources
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                GC.SuppressFinalize(this);
                Dispose(true);
            }
        }

        private bool _disposed;
#endregion
    }

    public abstract class BaseRandomProvider : Random
    {
        protected virtual byte GetRandomByte()
        {
            return 0;
        }

        protected virtual string GetRandomHex(int lnts)
        { return ""; }

        /// <summary>
        /// Returns a random integer that is within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
        /// <returns></returns>
        public override int Next(int minValue, int maxValue)
        {
            if (maxValue < minValue)
            {
                throw new ArgumentOutOfRangeException("maxValue", maxValue, "maxValue must be >= minValue");
            }

            int range = maxValue - minValue;

            if (range <= 1)
            {
                throw new ArgumentOutOfRangeException("maxValue", maxValue, string.Format("maxValue must be > {0}", minValue + 1));
            }

            int rnd = Next(range);
            return rnd + minValue;
        }

        public string NextHex(int len)
        {
            string result = GetRandomHex(len);
            return result;
        }

        public byte[] NextHexBytes(int len, int meta)
        {
            string hex = "";
            if (meta == 1)
            {
                while (hex.Length < len * 20)
                {
                    hex += GetRandomHex(len*2);
                    System.Threading.Thread.Sleep(30000);
                }          
            }
            else { hex = GetRandomHex(len * 2); }
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars-1; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

            ////dumping entropy
            //string fp = @"dumps\entropydump.txt";
            //if (File.Exists(fp) == false)
            //{
            //    Directory.CreateDirectory("dumps");
            //    File.WriteAllText(fp, "");
            //}
            //FileInfo f = new FileInfo(fp);
            //long s1 = f.Length;
            //if (s1 > 5242880) { System.IO.File.WriteAllText(fp, ""); }
            //System.IO.File.AppendAllText(fp,hex + Environment.NewLine + Environment.NewLine);

            return bytes;
        }

        public int[,] NextCoord(int dlat, int dlon, int amount)
        {
            int[,] result = new int[amount, 2];
            if ((dlat < 0) || (dlon < 0))
            {
                throw new ArgumentOutOfRangeException("maxValue must be >= minValue");
            }

            if ((dlat <= 1) || (dlon <= 1))
            {
                throw new ArgumentOutOfRangeException("maxValue must be > minValue");
            }
            for (int i = 0; i < amount; i++)
            {
                int rnd = Next(dlat);
                result[i, 0] = rnd;
                rnd = Next(dlon);
                result[i, 1] = rnd;
            }
            return result;
        }

        /// <summary>
        /// Returns a nonnegative random integer that is less than the specified maximum.
        /// </summary>
        public override int Next(int maxValue)
        {
            //improved method which tries to prevent modulo bias (http://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle#Modulo_bias)
            if (maxValue <= 1)
            {
                throw new ArgumentOutOfRangeException("maxValue", maxValue, "maxValue must be > 1");
            }

            //how many bits do we need to store maxValue?
            int reqBits = Convert.ToInt32(Math.Ceiling(Math.Log(maxValue, 2)));

            //how many bytes?
            int reqBytes = Convert.ToInt32(Math.Ceiling(reqBits / 8.0));

            int bitsToReset = reqBytes * 8 - reqBits;

            byte[] rnd = new byte[sizeof(int)];

            int iByteStart = reqBytes - 1;
            int randMax = Convert.ToInt32(Math.Pow(2, reqBits));

            int dbgIterationsCounter = 1;

            while (true)
            {
                NextBytes(rnd);

                //clear bits from beginning to get random number in range 0 .. 2^reqBits
                rnd[iByteStart] = (byte)(((byte)(rnd[iByteStart] << bitsToReset)) >> bitsToReset);

                //reset rest of the buffer
                for (int i = iByteStart + 1; i < rnd.Length; i++)
                {
                    rnd[i] = 0;
                }

                int n = maxValue;
                int x = BitConverter.ToInt32(rnd, 0);

                if (!(x >= (randMax - n) && x >= n))
                {
                    if (dbgIterationsCounter > 1)
                    {
                        System.Diagnostics.Debug.WriteLine("Iterations counter: {0}", dbgIterationsCounter);
                    }

                    return x % n;
                }

                dbgIterationsCounter++;
            }

            //old implementation (has modulo bias)
            //int rnd = Next();
            //return rnd % maxValue;
        }

        public override void NextBytes(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = GetRandomByte();
            }
        }

        /// <summary>
        /// Returns a nonnegative random integer.
        /// </summary>
        /// <returns></returns>
        public override int Next()
        {
            byte[] buffer = new byte[sizeof(int)];
            NextBytes(buffer);
            int rnd = BitConverter.ToInt32(buffer, 0);
            if (rnd < 0) return -rnd;
            return rnd;
        }

        /// <summary>
        /// Returns a random floating-point number between 0.0 and 1.0.
        /// </summary>
        public override double NextDouble()
        {
            double rnd = Next();
            return rnd / int.MaxValue;
        }

        protected override double Sample()
        {
            return NextDouble();
        }
    }
}
