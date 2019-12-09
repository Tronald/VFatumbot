using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using VFatumbot.BotLogic;

namespace VFatumbot
{
    public class RandotripKMLGenerator
    {
        public const string MAIN_TEMPLATE =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"">
  
    <Document>
        <name>Randotrips {0}</name>
        <open>1</open>

        <gx:Tour>
            <name>PLAY ▶︎</name>
            <gx:Playlist>

{1}
            </gx:Playlist>
        </gx:Tour>

{2}
  </Document>
</kml>";

        public const string FLYTO_TEMPLATE =
@"            <!-- #################### FLYTO #################### -->
            <gx:FlyTo>
                <gx:duration>6.5</gx:duration>
                <gx:flyToMode>bounce</gx:flyToMode>
                <LookAt>
                    <latitude>{0}</latitude>
                    <longitude>{1}</longitude>
                    <altitude>0</altitude>
                    <heading>{3}</heading>
                    <tilt>30</tilt>
                    <range>100</range>
                    <gx:altitudeMode>relativeToGround</gx:altitudeMode>
                </LookAt>
            </gx:FlyTo>

            <gx:FlyTo>
                <gx:duration>1.0</gx:duration>
                <gx:flyToMode>smooth</gx:flyToMode>
                <LookAt>
                    <latitude>{0}</latitude>
                    <longitude>{1}</longitude>
                    <altitude>0</altitude>
                    <heading>{4}</heading>
                    <tilt>70</tilt>
                    <range>100</range>
                    <gx:altitudeMode>relativeToGround</gx:altitudeMode>
                </LookAt>
            </gx:FlyTo>

            <gx:Wait>
                <gx:duration>1.0</gx:duration>
            </gx:Wait>

            <gx:AnimatedUpdate>
              <Update>
                <targetHref/>
                <Change>
                  <Placemark targetId=""{2}"">
                    <gx:balloonVisibility>1</gx:balloonVisibility>
                  </Placemark>
                </Change>
              </Update>
            </gx:AnimatedUpdate>

            <gx:FlyTo>
                <gx:duration>5.0</gx:duration>
                <gx:flyToMode>smooth</gx:flyToMode>
                <LookAt>
                    <latitude>{0}</latitude>
                    <longitude>{1}</longitude>
                    <altitude>0</altitude>
                    <heading>{5}</heading>
                    <tilt>70</tilt>
                    <range>100</range>
                    <gx:altitudeMode>relativeToGround</gx:altitudeMode>
                </LookAt>
            </gx:FlyTo>

            <gx:FlyTo>
                <gx:duration>5.0</gx:duration>
                <gx:flyToMode>smooth</gx:flyToMode>
                <LookAt>
                   <latitude>{0}</latitude>
                    <longitude>{1}</longitude>
                    <altitude>0</altitude>
                    <heading>{6}</heading>
                    <tilt>70</tilt>
                    <range>100</range>
                    <gx:altitudeMode>relativeToGround</gx:altitudeMode>
                </LookAt>
            </gx:FlyTo>

            <gx:FlyTo>
                <gx:duration>5.0</gx:duration>
                <gx:flyToMode>smooth</gx:flyToMode>
                <LookAt>
                   <latitude>{0}</latitude>
                    <longitude>{1}</longitude>
                    <altitude>0</altitude>
                    <heading>{7}</heading>
                    <tilt>70</tilt>
                    <range>100</range>
                    <gx:altitudeMode>relativeToGround</gx:altitudeMode>
                </LookAt>
            </gx:FlyTo>

            <gx:FlyTo>
                <gx:duration>5.0</gx:duration>
                <gx:flyToMode>smooth</gx:flyToMode>
                <LookAt>
                    <latitude>{0}</latitude>
                    <longitude>{1}</longitude>
                    <altitude>0</altitude>
                    <heading>{8}</heading>
                    <tilt>70</tilt>
                    <range>100</range>
                    <gx:altitudeMode>relativeToGround</gx:altitudeMode>
                </LookAt>
            </gx:FlyTo>

            <gx:AnimatedUpdate>
                <Update>
                <targetHref/>
                <Change>
                    <Placemark targetId=""{2}"">
                    <gx:balloonVisibility>0</gx:balloonVisibility>
                    </Placemark>
                </Change>
                </Update>
            </gx:AnimatedUpdate>

            <gx:Wait>
                <gx:duration>1.0</gx:duration>
            </gx:Wait>

";

        public const string POINT_TEMPLATE =
@"        <!-- #################### POINT #################### -->
        <Placemark id=""{0}"">
          <name>{1}</name>
          <description>{2}</description>
          <Point>
            <gx:altitudeMode>relativeToGround</gx:altitudeMode>
            <coordinates>{3},{4},0</coordinates>
          </Point>
        </Placemark>

";
        public struct FlyTo
        {
            public double latitude;
            public double longitude;
            public double final_bearing;
        }

        public struct Point
        {
            public string point_type;
            public string intent_set;
            public string what_3_words;
            public string report;
            public string photos;
            public string short_hash_id;
            public double latitude;
            public double longitude;
            public double power;
            public double z_score;
            public string nearest_place;
            public string country;
            public DateTime datetime;
        }

        public static int Generate(string whereClause, string filename, bool isMyRandotrips)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = Consts.DB_SERVER;
                builder.UserID = Consts.DB_USER;
                builder.Password = Consts.DB_PASSWORD;
                builder.InitialCatalog = Consts.DB_NAME;

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();

                    StringBuilder ssb = new StringBuilder();
                    ssb.Append("SELECT point_type,intent_set,what_3_words,text,photos,short_hash_id,latitude,longitude,final_bearing,power,z_score,nearest_place,country,datetime FROM ");
                    //                      0          1           2        3     4        5            6         7          8          9      10       11          12                   
#if RELEASE_PROD
                ssb.Append("reports");
#else
                    ssb.Append("reports");
                    //ssb.Append("reports_dev");
#endif
                    ssb.Append($" WHERE VISITED = '1' {whereClause} ORDER BY DATETIME ASC;");
                    Console.WriteLine("SQL:" + ssb.ToString());

                    List<FlyTo> flytos = new List<FlyTo>();
                    List<Point> points = new List<Point>();

                    using (SqlCommand sqlCommand = new SqlCommand(ssb.ToString(), connection))
                    {
                        using (SqlDataReader reader = sqlCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var flyto = new FlyTo();
                                var point = new Point();

                                // latitude
                                if (!reader.IsDBNull(6)) flyto.latitude = point.latitude = reader.GetDouble(6);
                                // longitude
                                if (!reader.IsDBNull(7)) flyto.longitude = point.longitude = reader.GetDouble(7);
                                // bearing
                                if (!reader.IsDBNull(8)) flyto.final_bearing = reader.GetDouble(8);
                                if (flyto.final_bearing < 0) flyto.final_bearing = (flyto.final_bearing + 360) % 360;

                                if (!reader.IsDBNull(0)) point.point_type = reader.GetString(0);
                                if (!reader.IsDBNull(1)) point.intent_set = reader.GetString(1);
                                if (!reader.IsDBNull(2)) point.what_3_words = reader.GetString(2);
                                if (!reader.IsDBNull(3)) point.report = reader.GetString(3);
                                if (!reader.IsDBNull(4)) point.photos = reader.GetString(4);
                                if (!reader.IsDBNull(5)) point.short_hash_id = reader.GetString(5);
                                if (!reader.IsDBNull(9)) point.power = reader.GetDouble(9);
                                if (!reader.IsDBNull(10)) point.z_score = reader.GetDouble(10);
                                if (!reader.IsDBNull(11)) point.nearest_place = reader.GetString(11);
                                if (!reader.IsDBNull(12)) point.country = reader.GetString(12);
                                if (!reader.IsDBNull(13)) point.datetime = reader.GetDateTime(13);

                                flytos.Add(flyto);
                                points.Add(point);

                            }
                        }
                    }

                    var flyTosAppender = new StringBuilder();
                    int pointNo = 1;
                    foreach (FlyTo flyto in flytos)
                    {
                        var var3 = (flyto.final_bearing + 360) % 360;
                        var var4 = (flyto.final_bearing + 360) % 360;
                        var var5 = (flyto.final_bearing + 360 + 90) % 360;
                        var var6 = (flyto.final_bearing + 360 + 180) % 360;
                        var var7 = (flyto.final_bearing + 360 + 270) % 360;
                        var var8 = (flyto.final_bearing + 360) % 360;
                        flyTosAppender.Append(string.Format(FLYTO_TEMPLATE, flyto.latitude, flyto.longitude, $"point{pointNo++}", var3, var4, var5, var6, var7, var8));
                    }

                    var pointsAppender = new StringBuilder();
                    pointNo = 1; // reset
                    foreach (Point point in points)
                    {
                        // Format content to show inside the balloon popup
                        var balloonString = new StringBuilder();
                        balloonString.Append("<![CDATA[");
                        if (!string.IsNullOrEmpty(point.nearest_place))
                        {
                            balloonString.Append($"in {point.nearest_place}, {point.country}<br>");
                        }
                        balloonString.Append($"@{point.latitude.ToString("G6")},{point.longitude.ToString("G6")} ({point.what_3_words})<br>");
                        balloonString.Append($"Power: {point.power.ToString("G3")}<br>");
                        balloonString.Append($"z-score: {point.z_score.ToString("G3")}<br>");
                        if (!string.IsNullOrEmpty(point.intent_set)) balloonString.Append($"Intent: {point.intent_set}<br>");
                        if (!string.IsNullOrEmpty(point.report)) balloonString.Append($"Report: {point.report}<br>");
                        if (!string.IsNullOrEmpty(point.photos))
                        {
                            var photos = point.photos.Split(",");
                            int i = 0;
                            foreach (string url in photos)
                            {
                                balloonString.Append($"<a href=\"{url}\">photo {++i}</a> ");
                            }
                            balloonString.Append("<br>");
                        }
                        balloonString.Append($"{point.short_hash_id}<br>");
                        balloonString.Append("]]>");

                        var dateTimeStr = "";
                        if (isMyRandotrips)
                        {
                            // TODO: one day figure out how to show My Randotrips in user's local timezone
                            dateTimeStr = $"{point.point_type} {point.datetime.ToString("yyyy-MM-dd HH':'mm")} UTC";
                        }
                        else
                        {
                            dateTimeStr = $"{point.point_type} {point.datetime.ToString("yyyy-MM-dd HH':'mm")} UTC";
                        }

                        var formatted = string.Format(POINT_TEMPLATE,
                                                $"point{pointNo++}",
                                                dateTimeStr,
                                                balloonString.ToString(),
                                                point.longitude,
                                                point.latitude
                                                );

                        pointsAppender.Append(formatted);
                    }

                    var output = string.Format(MAIN_TEMPLATE, filename.Replace(".kml", "").Replace("randotrips_", ""), flyTosAppender, pointsAppender);
                    System.IO.File.WriteAllText($"wwwroot/flythrus/{filename}", output);
                    return pointNo;
                }
            }
            catch (Exception e)
            {
                // My error handling is getting lazy
                Console.Write(e);
                return -1;
            }
        }
    }
}
