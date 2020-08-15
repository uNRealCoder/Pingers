using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Networking
{
    public static class FileStore
    {
        public static string FileName { get; private set; }

        public static bool SortAtEnd { get; private set; }

        public static void SetSortAtEnd(bool value) => SortAtEnd = value;

        static FileStore()
        {
            FileName = "Pings" + System.DateTime.Now.Ticks.ToString() + ".txt";
            using (File.Create(FileName)) 
            { 
            }
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if(SortAtEnd==true)
            {
                var contents = File.ReadAllLines(FileName);
                Array.Sort(contents);
                File.WriteAllLines(FileName, contents);
            }
        }

        public static void AppendFile(string Msg)
        {
            File.AppendAllText(FileName, Msg+Environment.NewLine, Encoding.UTF8);
        }
    }
}
