﻿using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System;
using System.Data;
using System.Dynamic;
using System.Text;

namespace cshw
{
    class Program
    {
        private static double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
        }

        private static bool IsDuplicate(DataRow r1, DataRow r2)
        {
            return (GetDistance(r1.Field<double>("lat"), r1.Field<double>("lon"), r2.Field<double>("lat"), r2.Field<double>("lon")) < 3) &&
                (r2.Field<double>("weight") > r1.Field<double>("weight")) &&
                (Math.Abs(r1.Field<int>("panoid") - r2.Field<int>("panoid")) > 8) &&
                (Math.Abs(r1.Field<int>("azimuth") - r2.Field<int>("azimuth")) < 30) &&
                (r1.Field<int>("id") != r2.Field<int>("id"));
        }

        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var records = new List<dynamic>();
            using (var reader = new StreamReader("../../../input.csv", Encoding.GetEncoding(1251)))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.Delimiter = ";";
                using (var dr = new CsvDataReader(csv))
                {
                    var dt = new DataTable();
                    dt.Locale = CultureInfo.InvariantCulture;
                    dt.Columns.Add("id", typeof(int));
                    dt.Columns.Add("lat", typeof(double));
                    dt.Columns.Add("lon", typeof(double));
                    dt.Columns.Add("weight", typeof(double));
                    dt.Columns.Add("panoid", typeof(int));
                    dt.Columns.Add("azimuth", typeof(int));
                    dt.Load(dr);
                    foreach (DataRow row1 in dt.Rows)
                    {
                        foreach (DataRow row2 in dt.Rows)
                        {
                            if (IsDuplicate(row1, row2))
                            {
                                dynamic record = new ExpandoObject();
                                record.id = row1["id"];
                                record.parentid = row2["id"];
                                records.Add(record);
                            };
                        }
                    }
                }
            }
            using (var writer = new StreamWriter("../../../output.csv", false, Encoding.GetEncoding(1251)))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(records);
            }
        }
    }
}