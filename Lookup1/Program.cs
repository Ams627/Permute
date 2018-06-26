using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lookup1
{
    class Program
    {
        private static DateTime GetRJISDate(string line, int offset)
        {
            var res1 = DateTime.TryParseExact(line.Substring(offset, 8), "ddMMyyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date);
            if (!res1)
            {
                throw new Exception("Cannot parse date.");
            }
            return date;
        }

        static void Main(string[] args)
        {
            try
            {
                var pooleClusters = new[] { "Q202", "Q943", "QA97", "QB68", "QB83", "QB95", "QD12", "QW01", "R046", "SSW4", "T208",
                                    "T258", "T510" };
                var oxfordClusters = new[] { "Q027", "Q028", "Q033", "Q056", "Q057", "Q083", "Q718", "Q739", "Q814", "Q947", "QA91",
                                     "QB60", "QB73", "QB91", "QC10", "QC15", "QC51", "QD03", "QQ31", "R402", "R675", "S310",
                                     "SGW3", "T115", "T215", "T256", "T310", "T401", "T504" };

                var permutedList = from orig in pooleClusters from dest in oxfordClusters select orig + dest;


                var dir = (args.Length > 0) ? args[0] : throw new Exception("You must supply a folder containing an RJIS .LOC file.");
                var files = Directory.GetFiles(dir, "RJFAF*").ToList();
                files.RemoveAll(x => !Regex.Match(Path.GetFileName(x), @"^RJFAF\d\d\d.[A-Z]{3}$").Success);
                files.RemoveAll(x => Regex.Match(x, "zip", RegexOptions.IgnoreCase).Success);
                var extMap = files.ToLookup(x => Path.GetExtension(x), StringComparer.OrdinalIgnoreCase);
                var dups = extMap.Where(x => x.Count() > 1);
                foreach (var entry in dups)
                {
                    Console.Error.WriteLine($"More than one RJFAF file with extension {entry.Key}");
                }
                var filename = extMap[".ffl"].FirstOrDefault();


                var flowDict = new Dictionary<string, List<int>>();
                var fareDict = new Dictionary<int, List<(string ticketType, string resCode, int fare)>>();

                var today = DateTime.Today;
                foreach (var line in File.ReadAllLines(filename))
                {
                    // look at location L records first:
                    if (line.StartsWith("RF"))
                    {
                        var endDate = GetRJISDate(line, 20);
                        var startDate = GetRJISDate(line, 28);
                        if (startDate <= endDate && startDate <= today && endDate >= today)
                        {
                            var orig = line.Substring(2, 4);
                            var dest = line.Substring(6, 4);
                            var reversible = line[19] == 'R';
                            var flowid = Convert.ToInt32(line.Substring(42, 7));

                            DictUtils.AddEntryToList(flowDict, orig + dest, flowid);
                            if (reversible)
                            {
                                DictUtils.AddEntryToList(flowDict, dest + orig, flowid);
                            }
                        }
                    }
                    else if (line.StartsWith("RT"))
                    {
                        var flowId = Convert.ToInt32(line.Substring(2, 7));
                        var ticketType = line.Substring(9, 3);
                        var fare = Convert.ToInt32(line.Substring(12, 8));
                        var rescode = line.Substring(20, 2);
                        DictUtils.AddEntryToList(fareDict, flowId, (ticketType, rescode, fare));
                    }
                }

                foreach (var flow in permutedList)
                {
                    if (!flowDict.TryGetValue(flow, out var flowids))
                    {
                        continue;
                    }
                    foreach (var flowid in flowids)
                    {
                        if (!fareDict.TryGetValue(flowid, out var fareRecords))
                        {
                            continue;
                        }
                        foreach (var (ticketType, resCode, fare) in fareRecords)
                        {
                            Console.WriteLine($"{ticketType} {fare/100.0:F2} {resCode}");
                        }
                    }
                }
                Console.WriteLine("finished");
            }
            catch (Exception ex)
            {
                var codeBase = System.Reflection.Assembly.GetEntryAssembly().CodeBase;
                var progname = Path.GetFileNameWithoutExtension(codeBase);
                Console.Error.WriteLine(progname + ": Error: " + ex.Message);
            }
        }
    }
}
