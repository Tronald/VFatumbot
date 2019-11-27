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
        <name>Randotrip {0}</name>
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
@"          <gx:FlyTo>
                <gx:duration>10.0</gx:duration>
                <gx:flyToMode>smooth</gx:flyToMode>
                <LookAt>
                    <latitude>{0}</latitude>
                    <longitude>{1}</longitude>
                    <altitude>0</altitude>
	                <heading>{2}]</heading>
                    <tilt>70</tilt>
	                <range>100</range>
                    <gx:altitudeMode>relativeToGround</gx:altitudeMode>
                </LookAt>
            </gx:FlyTo>

            <gx:Wait>
              <gx:duration>5.0</gx:duration>
            </gx:Wait>

            <gx:AnimatedUpdate>
              <Update>
                <targetHref/>
                <Change>
                  <Placemark targetId=""{3}"">
                    <gx:balloonVisibility>1</gx:balloonVisibility>
                  </Placemark>
                </Change>
              </Update>
            </gx:AnimatedUpdate>

            <gx:Wait>
              <gx:duration>5.0</gx:duration>
            </gx:Wait>

            <gx:AnimatedUpdate>
              <Update>
                <targetHref/>
                <Change>
                  <Placemark targetId=""{3}"">
                    <gx:balloonVisibility>0</gx:balloonVisibility>
                  </Placemark>
                </Change>
              </Update>
            </gx:AnimatedUpdate>

";

        public const string POINT_TEMPLATE =
@"      <Placemark id=""{0}"">
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
        }

        public static string Generate(string date)
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
                    ssb.Append("SELECT point_type,intent_set,what_3_words,text,photos,short_hash_id,latitude,longitude,final_bearing,power,z_score FROM ");
                    //                      0          1           2        3     4        5            6         7          8          9      10                       
#if RELEASE_PROD
                ssb.Append("reports");
#else
                    ssb.Append("reports");
                    //ssb.Append("reports_dev");
#endif
                    ssb.Append($" WHERE VISITED = '1' AND DATETIME LIKE '%{date}%' ORDER BY DATETIME ASC;");
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

                                flytos.Add(flyto);
                                points.Add(point);

                            }
                        }
                    }

                    var flyTosAppender = new StringBuilder();
                    int pointNo = 1;
                    foreach (FlyTo flyto in flytos)
                    {
                        flyTosAppender.Append(string.Format(FLYTO_TEMPLATE, flyto.latitude, flyto.longitude, flyto.final_bearing, $"point{pointNo++}"));
                    }

                    var pointsAppender = new StringBuilder();
                    pointNo = 1; // reset
                    foreach (Point point in points)
                    {
                        var formatted = string.Format(POINT_TEMPLATE,
                                                $"point{pointNo++}",
                                                point.point_type,
                                                $"{point.what_3_words}\nPower: {point.power}\nZ-score:{point.z_score}\nReport: {point.report}",
                                                point.longitude,
                                                point.latitude
                                                );
                        pointsAppender.Append(formatted);
                    }

                    var output = string.Format(MAIN_TEMPLATE, date, flyTosAppender.ToString(), pointsAppender.ToString());
                    var filename = $"randotrip{date.Replace("-", "")}.kml";
                    System.IO.File.WriteAllText($"wwwroot/flythrus/{filename}", output);
                    return filename;
                }
            }
            catch (Exception e)
            {
                // My error handling is getting lazy
                Console.Write(e);
                return null;
            }
        }
    }
}
