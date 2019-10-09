using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System;
using System.Dynamic;
using System.Text;
using CsvHelper.Configuration;
using System.Linq;

namespace cshw
{
    class Program
    {
        public class Sign
        {
            public int Id { get; set; }
            public double Lat { get; set; }
            public double Lon { get; set; }
            public double Weight { get; set; }
            public int Panoid { get; set; }
            public double Plat { get; set; }
            public double Plon { get; set; }
            public int Azimuth { get; set; }
            public int Type { get; set; }
        }

        public class SignMap : ClassMap<Sign>
        {
            public SignMap()
            {
                AutoMap();
                Map(m => m.Plat).Default(0);
                Map(m => m.Plon).Default(0);
            }
        }

        const int R = 6371000;

        private static double ToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }
        private static double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var lat = ToRadians(lat1 - lat2);
            var lon = ToRadians(lon1 - lon2);
            var h1 = Math.Sin(lat * 0.5) * Math.Sin(lat*0.5) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(lon * 0.5) * Math.Sin(lon * 0.5);
            var h2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h1)));
            return R * h2;
        }

        private static bool IsDuplicate(Sign s1, Sign s2)
        {
            return (s1.Type == s2.Type) &&
                (GetDistance(s1.Lat, s1.Lon, s2.Lat, s2.Lon) < 3) &&
                (Math.Abs(s1.Panoid - s2.Panoid) > 8) &&
                ((Math.Abs(s1.Azimuth - s2.Azimuth) < 30) || (Math.Abs(s1.Azimuth - s2.Azimuth) > 329));
        }

        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            List<Sign> signs = new List<Sign>();
            var duplicates = new List<dynamic>();
            using (var reader = new StreamReader("../../../input.csv", Encoding.GetEncoding(1251)))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.Delimiter = ";";
                csv.Configuration.PrepareHeaderForMatch = (string header, int index) => header.ToLower();
                csv.Configuration.CultureInfo = CultureInfo.InvariantCulture;
                csv.Configuration.RegisterClassMap(new SignMap());
                signs = csv.GetRecords<Sign>().ToList();
            }
            for (int i = 1; i < signs.Count; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (IsDuplicate(signs[j], signs[i]))
                    {
                        dynamic record = new ExpandoObject();
                        record.id = signs[i].Id;
                        record.parentid = signs[j].Id;
                        duplicates.Add(record);
                    }
                }
            }
            using (var writer = new StreamWriter("../../../output.csv", false, Encoding.GetEncoding(1251)))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(duplicates);
            }
        }
    }
}
