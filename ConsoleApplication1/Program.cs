using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;

namespace Kontur.LogPacker
{
    internal static class EntryPoint
    {
//        public static string DecimalToArbitrarySystem(long decimalNumber, int radix)
//        {
//            const int BitsInLong = 64;
//            const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
//
//            if (radix < 2 || radix > Digits.Length)
//                throw new ArgumentException("The radix must be >= 2 and <= " + Digits.Length.ToString());
//
//            if (decimalNumber == 0)
//                return "0";
//
//            int index = BitsInLong - 1;
//            long currentNumber = Math.Abs(decimalNumber);
//            char[] charArray = new char[BitsInLong];
//
//            while (currentNumber != 0)
//            {
//                int remainder = (int)(currentNumber % radix);
//                charArray[index--] = Digits[remainder];
//                currentNumber = currentNumber / radix;
//            }
//
//            string result = new String(charArray, index + 1, BitsInLong - index - 1);
//            if (decimalNumber < 0)
//            {
//                result = "-" + result;
//            }
//
//            return result;
//        }
        public static void Compress(string filename)
        {
            FileInfo fileToBeGZipped = new FileInfo(filename);
            FileInfo gzipFileName = new FileInfo(string.Concat(fileToBeGZipped.FullName, ".gz"));
             
            using (FileStream fileToBeZippedAsStream = fileToBeGZipped.OpenRead())
            {
                using (FileStream gzipTargetAsStream = gzipFileName.Create())
                {
                    using (GZipStream gzipStream = new GZipStream(gzipTargetAsStream, CompressionLevel.Optimal))
                    {
                        try
                        {
                            fileToBeZippedAsStream.CopyTo(gzipStream);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
        }
        
        public static string CodeDate(string line)
        {
            string myPattern = @"([\d]{4})-([\d]{2})-([\d]{2}).([\d]{2}):([\d]{2}):([\d]{2}),([\d]{3})";
            var regex = new Regex(myPattern);
            var match = regex.Match(line);
            var split = match.Groups[0].Value.Split(',');
            try
            {
                var date = Convert.ToDateTime(split[0]);
                var millis = Convert.ToInt16(split[1]);
                var newDatetime = new DateTimeOffset(date).ToUnixTimeMilliseconds();
//                var anotherBasedDate = DecimalToArbitrarySystem(newDatetime, 32);
//                var anotherBasedMillis= DecimalToArbitrarySystem(millis, 32);
//                return anotherBasedDate + anotherBasedMillis;
                return Convert.ToString(newDatetime + millis);
            }
            catch
            {
                return null;
            }
            ;



        }

        public static string DecodeDate(string date_)
        {
            var date = Convert.ToInt64(date_);
            var millies = date % 1000;
            date = date - millies;
            string fmt = "yyyy-MM-dd HH:mm:ss";
            var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(date).DateTime.ToLocalTime();
            var res = dateTime.ToString(fmt) + ',' + Convert.ToString(millies).PadLeft(3, '0') + ' ';
            return res.PadRight(23);
        }

        public static string GetId(string line)
        {
            string myPattern = @"\s[\d]{1,}\s";
            var regex = new Regex(myPattern);
            var match = regex.Match(line);
            var id = Convert.ToInt32(match.Groups[0].Value);
//            var anotherId = DecimalToArbitrarySystem(id, 32);
//            return anotherId; 
            return match.Groups[0].Value.Trim();
        }

        public static string CodeLog(string line)
        {
            var logLevel = new Dictionary<string, string>(7);
            logLevel.Add("FATAL", "5");
            logLevel.Add("DEBUG", "1");
            logLevel.Add("ERROR", "4");
            logLevel.Add("INFO", "2");
            logLevel.Add("NONE", "6");
            logLevel.Add("TRACE", "0");
            logLevel.Add("WARN", "3");
            string myPattern = @"[A-Z]{4,5}";
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

            return key.PadRight(5) + " ";
        }

        public static ArrayList GetLogString(string line)
        {
            var myStruct1 = new ArrayList();
            var message = "";
            if (CodeDate(line) != null)
            {
                message += line.Substring(37);
                var date = CodeDate(line);
                var id = GetId(line);
                var log = CodeLog(line);

                myStruct1.Add(date);
                myStruct1.Add(id);
                myStruct1.Add(log);
                myStruct1.Add(message);
                return myStruct1;
            }

            myStruct1.Add("xz" + line);

            return myStruct1;
        }

        public static void CodeLogFile(string pathFrom, string pathTo)
        {
            var objReader = new StreamReader(pathFrom);
            while (true)
            {
                var line = objReader.ReadLine();
                if (line == null)
                {
                    break;
                }

                var myStruct = GetLogString(line);
                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(pathTo, true))
                {
                    foreach (var v in myStruct)
                    {
                        file.Write(v);
                    }

                    file.Close();
                }
            }

            objReader.Close();
        }

        public static void DecodeLogFile(string pathFrom, string pathTo)
        {
            var objReader = new StreamReader(pathFrom);
            while (true)
            {
                var line = objReader.ReadLine();
                if (line == null)
                {
                    break;
                }


                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(pathTo, true))
                {
                    try
                    {
                        if (line.StartsWith("bamboleo"))
                        {
                            file.WriteLine(line);
                        }
                        else
                        {
                            file.Write(DecodeDate(line));
                            line = objReader.ReadLine();
                            file.Write(line.PadRight(6) + " ");
                            line = objReader.ReadLine();
                            file.Write(DecodeLog(line));
                            line = objReader.ReadLine();
                            file.Write(line);
                            file.Write("\n");
                        }
                    }
                    catch
                    {
                        file.WriteLine(line);
                    }

                    

                    file.Close();
                }
            }

            objReader.Close();
        }
      
        
        
        
        public static void Main(string[] args)
        {

            string bytefile = "byte.txt"; 
            var bytes = new byte[1024 * 1024];
            new Random().NextBytes(bytes);
            File.WriteAllBytes(bytefile, bytes);
//            var pathWithBytes = "byte.txt";
            var pathFromCode = "/Users/dantsushko/Downloads/Kontur.LogPacker-master/Kontur.LogPacker/bin/Debug/netcoreapp2.1/example.log";
            var pathToCode = "/Users/dantsushko/Downloads/Kontur.LogPacker-master/Kontur.LogPacker/bin/Debug/netcoreapp2.1/newlog.txt";
            var pathFromDecode = pathToCode;
            var pathToDecode = "/Users/dantsushko/Downloads/Kontur.LogPacker-master/Kontur.LogPacker/bin/Debug/netcoreapp2.1/decoded.txt";
//            if (args.Length == 2)
//            {
//                CodeLogFile(args[0], args[1]);
//            }
//
//            if (args.Length == 3)
//            {
//                DecodeLogFile(args[1], args[2]);
//            }
             CodeLogFile(pathFromCode, pathToCode);
             Compress(pathToCode);
//             DecodeLogFile(pathFromDecode, pathToDecode);
           


        }
    }
}
