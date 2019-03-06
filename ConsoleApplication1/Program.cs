using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;


namespace Compressor
{               

    class Class1
    {
        public static long GetDateCode(string line)
        {
            string myPattern = @"([\d]{4})-([\d]{2})-([\d]{2}).([\d]{2}):([\d]{2}):([\d]{2}),([\d]{3})";
            var regex = new Regex(myPattern);
            var match = regex.Match(line);
            var split = match.Groups[0].Value.Split(',');
            var date = Convert.ToDateTime(split[0]);
            var millis = Convert.ToInt16(split[1]);
            var newDatetime = new DateTimeOffset(date).ToUnixTimeMilliseconds();
            return newDatetime + millis;
        }

        public static string DecodeDate(long date)
        {
            var millies = date % 1000;
            date = date - millies;
            string fmt = "yyyy-MM-dd HH:mm:ss";
            var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(date).DateTime.ToLocalTime();
            var res = dateTime.ToString(fmt) + ',' + Convert.ToString(millies) + ' ';
            return res;

        }    
    
        public static ulong GetId(string line)
        {
            string myPattern = @"\s[\d]{1,}\s";
            var regex = new Regex(myPattern);
            var match = regex.Match(line);
            var id = Convert.ToUInt64(match.Groups[0].Value);
            return id;
        }

        public static string GetLogLevel(string line)
        {
            var logLevel = new Dictionary<string, string>(7);
            logLevel.Add("FATAL", "5");
            logLevel.Add("DEBUG", "1");
            logLevel.Add("ERROR", "4");
            logLevel.Add("INFO", "2");
            logLevel.Add("NONE", "6");
            logLevel.Add("TRACE", "0");
            logLevel.Add("WARN", "3");
            string myPattern = @"[A-Z]{4,6}";
            var regex = new Regex(myPattern);
            var match = regex.Match(line);
            if (regex.IsMatch((line)))
            {
                string logName = match.Groups[0].Value;
                var res = logLevel[logName];
                return res;
            }

            return null;
        }

        public static string DecodeLog(string log)
        {
            var logLevel = new Dictionary<string, string>(7); // ask how to make for two
            logLevel.Add("FATAL", "5");
            logLevel.Add("DEBUG", "1");
            logLevel.Add("ERROR", "4");
            logLevel.Add("INFO", "2");
            logLevel.Add("NONE", "6");
            logLevel.Add("TRACE", "0");
            logLevel.Add("WARN", "3");
            var key = "";
            foreach (KeyValuePair<string, string> pair in logLevel)
            {
                if (pair.Value == log)
                { 
                    key = pair.Key; 
                    break; 
                }
            }

            return key;
        }

        public static ArrayList GetMessage(string line)
        {
            var myStruct = new ArrayList();
            if (GetLogLevel(line) != null)
            {
                var message = "";
                message = line.Substring(37);
                var date = GetDateCode(line);
                var id = GetId(line);
                var log = GetLogLevel(line);
                
                myStruct.Add(date);
                myStruct.Add(id);
                myStruct.Add(log);
                myStruct.Add(message);
                return myStruct;
            }

            myStruct.Add(line);
            return myStruct;
        }

        public static void WriteNewLog(string pathFrom, string pathTo)
        {
            var objReader = new StreamReader(pathFrom);
            var line = "";
            while (true)
            {
                line = objReader.ReadLine();
                if (line == null)
                {
                    break;
                }

                var myStruct = GetMessage(line);
                using (System.IO.StreamWriter file = 
                    new System.IO.StreamWriter(pathTo, true))
                {
                    foreach (var v in myStruct)
                    {
                        file.WriteLine(v);
                        Console.Write(v);

                    }
                    //file.Close();
                }
                
            }   
            objReader.Close();
            
        }

        static void Main(string[] args)
        {
            var pathFrom = "/Users/dantsushko/RiderProjects/ConsoleApplication1/log.txt";
            var pathTo = "/Users/dantsushko/RiderProjects/ConsoleApplication1/newlog2.txt";
            
            WriteNewLog(@"/Users/dantsushko/RiderProjects/ConsoleApplication1/log.txt",
                @"/Users/dantsushko/RiderProjects/ConsoleApplication1/newlog3.txt");
             
//           var objReader = new StreamReader(pathFrom);
//            var sLine = "";
//            while (true)
//            {
//                sLine = objReader.ReadLine();
//                if (sLine == null)
//                {
//                    break;
//                }
//
//                var myStr = GetMessage(sLine);
//                using (System.IO.StreamWriter file = 
//                    new System.IO.StreamWriter(@"/Users/dantsushko/RiderProjects/ConsoleApplication1/ConsoleApplication1/newlog2.txt", true))
//                {
//                    foreach (var v in myStr)
//                    {
//                        file.WriteLine(v);
//
//                    }
//                    file.Close();
//                }
//                
//
//            }
        }
    }
}