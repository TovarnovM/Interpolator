using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiCalibr
{
    static class FileSearcher
    {
        public static List<string> Search(string sDir,params string[] endsWith)
        {
            var lst = new List<string>();
            Search(sDir,lst,endsWith);
            return lst;
        }

        static void Search(string sDir,List<string> allFilesList,string[] endsWith)
        {
            try
            {
                foreach (string f in Directory
                    .EnumerateFiles(sDir,"*.*")
                    .Where(s => EndsWithMulty(s,endsWith)))
                {
                    allFilesList.Add(f);
                }
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    Search(d,allFilesList,endsWith);
                }
            } catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        static bool EndsWithMulty(string s,string[] endsWith)
        {
            foreach (var ew in endsWith)
            {
                if (s.EndsWith(ew))
                    return true;
            }
            return false;
        }
    }
}
