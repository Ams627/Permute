using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class Program
{
    private static void Main(string[] args)
    {
        var pooleClusters = new[] { "Q202", "Q943", "QA97", "QB68", "QB83", "QB95", "QD12", "QW01", "R046", "SSW4", "T208",
                                    "T258", "T510" };
        var oxfordClusters = new[] { "Q027", "Q028", "Q033", "Q056", "Q057", "Q083", "Q718", "Q739", "Q814", "Q947", "QA91",
                                     "QB60", "QB73", "QB91", "QC10", "QC15", "QC51", "QD03", "QQ31", "R402", "R675", "S310",
                                     "SGW3", "T115", "T215", "T256", "T310", "T401", "T504" };

        var permutedList = from orig in pooleClusters from dest in oxfordClusters select orig + dest;

        Console.WriteLine($"number of permutations is {permutedList.Count()}");

        permutedList.ToList().ForEach(x => Console.WriteLine($"{x}"));
    }
}
