using System;
using System.Linq;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Bot.Builder;

namespace VFatumbot.BotLogic
{
    public class FatumFunctions
    {
        public static double significance = 2.5; //significance absolute threshold for calculation, recommended value is in [2.5, 3.0]. Higher value speeds up the calculation resulting in less findings.
        public static double filtering_significance = 4.0; //significance absolute threshold for filtering of results, recommended value is 4.0 or higher, this usualy produces 0..10 results
        public static int pointid = 0;
        public static int busythreads = 0;
        public static bool resetdone = true;

        public static NumberFormatInfo nfi = new NumberFormatInfo();

        [StructLayout(LayoutKind.Explicit)]
        public struct LatLng
        {
            [FieldOffset(0)]
            public double latitude;
            [FieldOffset(8)]
            public double longitude;

            public LatLng(double Latitude, double Longitude) : this()
            {
                this.latitude = Latitude;
                this.longitude = Longitude;
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public class DistanceBearring
        {
            [FieldOffset(0)]
            public double distance;
            [FieldOffset(8)]
            public double initialBearing;
            [FieldOffset(16)]
            public double finalBearing;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Coordinate
        {
            [FieldOffset(0)]
            public LatLng point;
            [FieldOffset(16)]
            public DistanceBearring bearing; //how it was calculated
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct FinalAttr
        {
            [FieldOffset(0)] public ulong GID; //global id, defined by user (for databases)
            [FieldOffset(8)] public ulong TID; //timestamp id
            [FieldOffset(16)] public ulong LID; //local id, i.e number in array
            [FieldOffset(24)] public uint type;
            [FieldOffset(32)] public double x;
            [FieldOffset(40)] public double y;
            [FieldOffset(48)] public Coordinate center; //center of attractor peak
            [FieldOffset(88)] public uint side;
            [FieldOffset(96)] public double distanceErr; //calculation error due to neglected curvature etc
            [FieldOffset(104)] public double radiusM; //radius of attractor peak
            [FieldOffset(112)] public ulong n; //number of points
            [FieldOffset(120)] public double mean; //mean average
            [FieldOffset(128)] public uint rarity; //significance simplified ;)
            [FieldOffset(136)] public double power_old; //oldstyle power - area taken by 10 central points of attractor/mean value for that area
            [FieldOffset(144)] public double power; //area taken by all points of attractor/mean value for the area
            [FieldOffset(152)] public double z_score; // poisson z-score of single random event, if we chose this area on random and got this distribution
            [FieldOffset(160)] public double probability_single; //exact probability of the above event being random.
            [FieldOffset(168)] public double integral_score; //abstract value of integral significance - how z-score varies with growth of radius, along with power characterises the density of condentsation/rarefaction
            [FieldOffset(176)] public double significance; // poisson z-score of entire random event, i.e how possible is, that the event of finding this one attractor after the whole calculation was random.
            [FieldOffset(184)] public double probability; //exact probability of the above event being random.
        }

        [StructLayout(LayoutKind.Sequential)]
        public class FinalAttractor
        {
            public FinalAttr X;
        }

        //wrapper to prevent windows error dialog
        //[Flags]
        //public enum ErrorModes
        //{
        //    Default = 0x0,
        //    FailCriticalErrors = 0x1,
        //    NoGpFaultErrorBox = 0x2,
        //    NoAlignmentFaultExcept = 0x4,
        //    NoOpenFileErrorBox = 0x8000,
        //}

        //public class ErrorModeContext : IDisposable
        //{
        //    private readonly int _oldMode;
        //    public ErrorModeContext(ErrorModes mode)
        //    {
        //        _oldMode = SetErrorMode((int)mode);
        //    }
        //    ~ErrorModeContext()
        //    {
        //        Dispose(false);
        //    }
        //    private void Dispose(bool disposing)
        //    {
        //        SetErrorMode(_oldMode);
        //    }
        //    public void Dispose()
        //    {
        //        Dispose(true);
        //        GC.SuppressFinalize(this);
        //    }
        //    [DllImport("kernel32.dll")]
        //    private static extern int SetErrorMode(int newMode);
        //}
        //end of wrapper

        [DllImport("libAttract", CallingConvention = CallingConvention.Cdecl)]
        public extern static int getOptimizedDots(double areaRadiusM); //how many coordinates is needed for requested radius, optimized for performance on larger areas
        [DllImport("libAttract", CallingConvention = CallingConvention.Cdecl)]
        private extern static int getDotsBySpotRadius(double spotRadiusM, double areaRadiusM); //how many dots are needed for the chosen spot size
        [DllImport("libAttract", CallingConvention = CallingConvention.Cdecl)]
        private extern static double getSpotSize(int N, double areaRadiusM); //reverse problem: what is the expected minimum attractor radius given number of dots and area
        //Second: obtain the necessary amounts of enthropy from RNG, either as raw bytes, or as hex string
        [DllImport("libAttract", CallingConvention = CallingConvention.Cdecl)]
        private extern static int requiredEnthropyBytes(int N); // N* POINT_ENTROPY_BYTES 
        //Third: init engine, the number returned is engine index;
        //for testing init with pseudo circular coords in [0..1):
        [DllImport("libAttract", CallingConvention = CallingConvention.Cdecl)]
        private extern static int initWithPseudo(int handle, int N, int seed);
        [DllImport("libAttract", CallingConvention = CallingConvention.Cdecl)]
        private extern static int getHandle();
        [DllImport("libAttract", CallingConvention = CallingConvention.Cdecl)]
        private extern static int initWithBytes(int handle, byte[] byteinput, int bytelength);
        [DllImport("libAttract", CallingConvention = CallingConvention.Cdecl)]
        private extern static int initWithCoords(int handle, double[] coords, int N);
        //Fourth: 
        //long long the object and do the calculation, returns engine instance
        [DllImport("libAttract", CallingConvention = CallingConvention.Cdecl)]
        private extern static int findAttractors(int engineInstance, double significance, double filtering_significance);
        //apply geometry fixes and get result
        [DllImport("libAttract", CallingConvention = CallingConvention.Cdecl)]
        private extern static int getAttractorsLength(int engineInstance);
        [DllImport("libAttract", CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr getAttractors(int engineInstance, double radiusM, LatLng center, int gid);
        // free memory allocated for array:
        [DllImport("libAttract", CallingConvention = CallingConvention.Cdecl)]
        private extern static void releaseAttractors(IntPtr results, int length);
        //apply geometry fixes and get all coords (optional):
        [DllImport("libAttract", CallingConvention = CallingConvention.Cdecl)]
        private extern static int getFinalCoordsLength(int engineInstance);
        [DllImport("libAttract", CallingConvention = CallingConvention.Cdecl)]
        private extern static Coordinate[] getFinalCoords(int engineInstance, double radiusM, LatLng center);
        // free memory allocated for array:
        [DllImport("libAttract", CallingConvention = CallingConvention.Cdecl)]
        private extern static void releaseCoords(Coordinate[] results, int length);
        //free allocated resources
        [DllImport("libAttract", CallingConvention = CallingConvention.Cdecl)]
        private extern static void releaseEngine(int engineInstance); // release single instance
        [DllImport("libAttract", CallingConvention = CallingConvention.Cdecl)]
        private extern static void releaseHandle(int handle); 	// this frees all engines tied to handle
        [DllImport("libAttract", CallingConvention = CallingConvention.Cdecl)]
        private extern static void finalize(); // !!!CAUTION!!! this frees all engines systemwide, call before unloading the dll

        public static string[] SplitIt1(string buf)
        {
            string[] seps = new string[] { "[", "]" };
            string[] buf1 = buf.Split(seps, StringSplitOptions.RemoveEmptyEntries);
            return buf1;
        }
        public static string[] SplitIt(string buf, string sep)
        {
            string[] seps = new string[] { sep, "\n\n" };
            string[] buf1 = buf.Split(seps, StringSplitOptions.RemoveEmptyEntries);
            return buf1;
        }

        public static string CutCommand(string buf)
        {
            string result = "";
            string[] seps = new string[] { "@", "\n\n" };
            string[] buf1 = buf.Split(seps, StringSplitOptions.RemoveEmptyEntries);
            result = buf1[0];
            return result;
        }

        public static string Tolog(ITurnContext context, string type, FinalAttractor ida, string shortCode) //idas
        {
            string resp = "Intention Driven Anomaly found" + "\n\n";
            if (type == "blind") { resp = "Mystery Point Generated" + "\n\n"; }

            pointid++;
            var code = "";
            if (type == "blind") { code = "X-" + shortCode; }
            else
            if (ida.X.type == 1) { code = "A-" + shortCode; }
            else
            if (ida.X.type == 2) { code = "V-" + shortCode; }

            resp += code + " (" + ida.X.center.point.latitude.ToString("#0.000000", System.Globalization.CultureInfo.InvariantCulture)
                + " " + ida.X.center.point.longitude.ToString("#0.000000", System.Globalization.CultureInfo.InvariantCulture) + ")" + "\n\n";
            if (ida.X.type == 1)
            {
                if (type != "blind")
                {
                    resp += "Type: Attractor" + "\n\n";
                    resp += "Radius: " + (int)(ida.X.radiusM) + "m" + "\n\n";
                    resp += "Power: " + ida.X.power.ToString("#0.00", System.Globalization.CultureInfo.InvariantCulture) + "\n\n";
                    resp += "Bearing: " + ida.X.center.bearing.distance.ToString("#0.00m", System.Globalization.CultureInfo.InvariantCulture) + " / "
                                        + ida.X.center.bearing.finalBearing.ToString("#0.00°", System.Globalization.CultureInfo.InvariantCulture) + "\n\n";
                }
            }
            else if (ida.X.type == 2)
            {
                if (type != "blind")
                {
                    resp += "Type: Void" + "\n\n";
                    resp += "Radius: " + (int)(ida.X.radiusM) + "m" + "\n\n";
                    resp += "Power: " + (1 / ida.X.power).ToString("#0.00", System.Globalization.CultureInfo.InvariantCulture) + "\n\n";
                    resp += "Bearing: " + ida.X.center.bearing.distance.ToString("#0.00m", System.Globalization.CultureInfo.InvariantCulture) + " / "
                                        + ida.X.center.bearing.finalBearing.ToString("#0.00°", System.Globalization.CultureInfo.InvariantCulture) + "\n\n";
                }
            }
            string pl = "";
            if (ida.X.rarity == 0) { pl = @"N/A"; }
            else
                if (ida.X.rarity == 1) { pl = "POOR"; }
            else
                if (ida.X.rarity == 2) { pl = "COMMON"; }
            else
                if (ida.X.rarity == 3) { pl = "UNCOMMON"; }
            else
                if (ida.X.rarity == 4) { pl = "RARE"; }
            else
                if (ida.X.rarity == 5) { pl = "EPIC"; }
            else
                if (ida.X.rarity == 6) { pl = "LEGENDARY"; }
            else
                if (ida.X.rarity == 7) { pl = "UNICORN"; }
            else
                if (ida.X.rarity == 8) { pl = "SINGULARITY"; }

            if (type != "blind")
            {
                if (ida.X.rarity > 0) { resp += "Abnormality Rank: " + pl + "\n\n"; }
                resp += "z-score: " + ida.X.z_score.ToString("#0.00", System.Globalization.CultureInfo.InvariantCulture) + "\n\n";
            }
            return resp;
        }

        public static string Tolog(ITurnContext context, string type, double Lat, double Lng, string ptype, string shortCode) //randoms
        {
            string resp = "Random Point generated" + "\n\n";
            if (type == "blind") { resp = "Mystery Point Generated" + "\n\n"; }

            pointid++;
            var code = "";
            if (type == "blind") { code = "X-" + shortCode; }
            else
            if ((type == "random") && (ptype == "pseudo")) { code = "P-" + shortCode; }
                else
            if ((type == "random") && ((ptype == "quantum") || (ptype == "qtime"))) { code = "Q-" + shortCode; }

            resp += code + " (" + Lat.ToString("#0.000000", System.Globalization.CultureInfo.InvariantCulture)
                + " " + Lng.ToString("#0.000000", System.Globalization.CultureInfo.InvariantCulture) + ")" + "\n\n";
            if (ptype == "qtime")
            {
                QuantumRandomNumberGenerator rnd = new QuantumRandomNumberGenerator();
                resp += "Suggested time: " + ((int)rnd.Next(23)).ToString("#0") + ":" + ((int)rnd.Next(59)).ToString("00") + "\n\n";
            }
            return resp;
        }

        public static FinalAttractor[] GetIDA(LatLng startcoord, double radius, int u)
        {
            return GetIDA(startcoord, radius, u, 0);
        }

        public static FinalAttractor[] GetIDA(LatLng startcoord, double radius, int u, int meta)
        {
            FinalAttractor[] result = new FinalAttractor[0];
            QuantumRandomNumberGenerator rnd = new QuantumRandomNumberGenerator();
            resetdone = false;
            //using (new ErrorModeContext(ErrorModes.FailCriticalErrors | ErrorModes.NoGpFaultErrorBox))
            //{
            //    try
            //    {
                    int al = 0; int cou = 0;
                    while ((al == 0) && (cou < 10))
                    {
                        cou++;
                        int No = getOptimizedDots(radius);
                        int bytesSize = requiredEnthropyBytes(No);
                        //byte[] byteinput = new byte[No]; // todo use byte or hex dependent on sourcetype
                        //rnd.NextBytes(byteinput); 
                        byte[] byteinput = rnd.NextHexBytes((int)bytesSize, meta);
                        if (meta == 1) { bytesSize = bytesSize * 10; }
                        int engin1 = initWithBytes(getHandle(), byteinput, bytesSize);
                        int fa = findAttractors(engin1, significance, filtering_significance);
                        al = getAttractorsLength(engin1);
                        result = new FinalAttractor[al];
                        unsafe
                        {
                            IntPtr value;
                            value = getAttractors(engin1, radius, startcoord, 23);
                            if (value != null)
                            {
                                for (int j = 0; j < (int)al; j++)
                                {
                                    result[j] = new FinalAttractor();
                                    Marshal.PtrToStructure(value, result[j]);
                                    value += 192;
                                }
                            }
                            //releaseAttractors(value, al);
                            //todo make release stuff here
                        }
                        releaseEngine(engin1);
                    }
                    Console.WriteLine("IDA Calculation succeeded with " + result.Count() + " results");
            //    }
            //    catch (Exception e) { Console.WriteLine("IDA calculation error"); }
            //}
            return result;
        }

        public static FinalAttractor[] BubbleSort(FinalAttractor[] source, int count)
        {
            FinalAttractor temp;
            for (int write = 0; write < count; write++)
            {
                for (int sort = 0; sort < count - 1; sort++)
                {
                    double p1 = source[sort].X.power;
                    double p2 = source[sort + 1].X.power;
                    if (source[sort].X.type == 2) { p1 = 1 / p1; }
                    if (source[sort + 1].X.type == 2) { p2 = 1 / p2; }

                    if ((Math.Abs(source[sort].X.z_score) < Math.Abs(source[sort + 1].X.z_score)) ||
                            ((Math.Abs(source[sort].X.z_score) == Math.Abs(source[sort + 1].X.z_score)) && (p1 < p2)))
                    {
                        temp = source[sort + 1];
                        source[sort + 1] = source[sort];
                        source[sort] = temp;
                    }
                }
            }

            return source;
        }


        public static FinalAttractor[] SortIDA(FinalAttractor[] source, string idatype, int idacount)
        {
            FinalAttractor[] result = new FinalAttractor[0];
            //try
            //{
                int att = 0; int voi = 0;
                foreach (FinalAttractor ida in source)
                {
                    if (ida.X.type == 1) { att++; }
                    else if (ida.X.type == 2) { voi++; }
                }
                FinalAttractor[] aatt = new FinalAttractor[att];
                FinalAttractor[] avoi = new FinalAttractor[voi];
                att = 0; voi = 0;
                foreach (FinalAttractor ida in source)
                {
                    if (ida.X.type == 1) { aatt[att] = ida; att++; }
                    else if (ida.X.type == 2) { avoi[voi] = ida; voi++; }
                }

                if ((idatype == "attractor") && (att > 0))
                {
                    aatt = BubbleSort(aatt, att);
                    if (att < idacount) { idacount = att; }
                    result = new FinalAttractor[idacount];
                    for (int j = 0; j < idacount; j++)
                    {
                        result[j] = aatt[j];
                    }
                }
                else
                if ((idatype == "void") && (voi > 0))
                {
                    avoi = BubbleSort(avoi, voi);
                    if (voi < idacount) { idacount = voi; }
                    result = new FinalAttractor[idacount];
                    for (int j = 0; j < idacount; j++)
                    {
                        result[j] = avoi[j];
                    }
                }
                else
                if ((idatype == "any") && ((att > 0) || (voi > 0)))
                {
                    source = BubbleSort(source, source.Count());
                    if ((att + voi) < idacount) { idacount = att + voi; }
                    result = new FinalAttractor[idacount];
                    int c = 0;
                    int j = 0;
                    while ((j < source.Count()) && (c < idacount))
                    {
                        if ((source[j].X.type == 1) || (source[j].X.type == 2))
                        { result[c] = source[j]; c++; j++; }
                        else { j++; }
                    }
                }
            //}
            //catch (Exception e) { Console.WriteLine("Sorting error"); }
            return result;
        }


        public static int GetDistance(double lat0, double lon0, double lat1, double lon1)
        {
            double dlon = (lon1 - lon0) * Math.PI / 180;
            double dlat = (lat1 - lat0) * Math.PI / 180;

            double a = (Math.Sin(dlat / 2) * Math.Sin(dlat / 2)) + Math.Cos(lat0 * Math.PI / 180) * Math.Cos(lat1 * Math.PI / 180) * (Math.Sin(dlon / 2) * Math.Sin(dlon / 2));
            double angle = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return Convert.ToInt32(angle * 6371000);
        }

        public static double[] GetPseudoRandom(double lat, double lon, int radius)
        {
            double[] result = new double[2];
            Random rnd = new Random();

            bool dnn = false;
            while (dnn == false)
            {
                double lat01 = lat + radius * Math.Cos(180 * Math.PI / 180) / (6371000 * Math.PI / 180);
                double dlat = ((lat + radius / (6371000 * Math.PI / 180)) - lat01) * 1000000;
                double lon01 = lon + radius * Math.Sin(270 * Math.PI / 180) / Math.Cos(lat * Math.PI / 180) / (6371000 * Math.PI / 180);
                double dlon = ((lon + radius * Math.Sin(90 * Math.PI / 180) / Math.Cos(lat * Math.PI / 180) / (6371000 * Math.PI / 180)) - lon01) * 1000000;
                double lat1 = lat;
                double lon1 = lon;

                double rlat = rnd.Next(0, (int)dlat);
                double rlon = rnd.Next(0, (int)dlon);
                lat1 = lat01 + (rlat / 1000000);
                lon1 = lon01 + (rlon / 1000000);
                int dif = GetDistance(lat, lon, lat1, lon1);
                if (dif > radius) { }
                else
                {
                    result[0] = lat1;
                    result[1] = lon1;
                    dnn = true;
                }
            }
            Console.WriteLine("Pseudorandom Link Created");
            return result;
        }

        public static double[] GetQuantumRandom(double lat, double lon, int radius)
        {
            double[] result = new double[2];
            QuantumRandomNumberGenerator rnd = new QuantumRandomNumberGenerator();
            Random prnd = new Random();

            bool dnn = false;
            while (dnn == false)
            {
                double lat01 = lat + radius * Math.Cos(180 * Math.PI / 180) / (6371000 * Math.PI / 180);
                double dlat = ((lat + radius / (6371000 * Math.PI / 180)) - lat01) * 1000000;
                double lon01 = lon + radius * Math.Sin(270 * Math.PI / 180) / Math.Cos(lat * Math.PI / 180) / (6371000 * Math.PI / 180);
                double dlon = ((lon + radius * Math.Sin(90 * Math.PI / 180) / Math.Cos(lat * Math.PI / 180) / (6371000 * Math.PI / 180)) - lon01) * 1000000;
                double lat1 = lat;
                double lon1 = lon;
                double rlat;
                double rlon;


                rlat = rnd.Next(0, (int)dlat);
                rlon = rnd.Next(0, (int)dlon);


                lat1 = lat01 + (rlat / 1000000);
                lon1 = lon01 + (rlon / 1000000);
                int dif = GetDistance(lat, lon, lat1, lon1);
                if (dif > radius) { }
                else
                {
                    result[0] = lat1;
                    result[1] = lon1;
                    dnn = true;
                }
            }
            Console.WriteLine("Quantum Link Created");
            return result;
        }
    }
}
