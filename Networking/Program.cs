using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Networking
{
    class Program
    {
        public static readonly object LockObj = new object();

        public static readonly object LockObjCount = new object();
        private static int _AliveInstances = 0;
        public static int AliveInstances
        {
            get
            {
                lock (LockObjCount)
                {
                    return _AliveInstances;
                }

            }
            set
            {
                lock (LockObjCount)
                {
                    _AliveInstances = value;
                }
            }
        }
        private static Semaphore Semaphore = new Semaphore(10, 10);
        private static int GetIP(string Msg, bool AllowEmpty)
        {
            int Num;
            bool done = false;
            do
            {
                Console.WriteLine(Msg + " : ");
                string str = Console.ReadLine();
                if (String.IsNullOrWhiteSpace(str) && (AllowEmpty == true))
                {
                    Num = -1;
                    break;
                }
                done = int.TryParse(str, out Num);
                if (done == true)
                {
                    if (!(Num >= 0 && Num <= 255))
                    {
                        Console.WriteLine("Error: Please enter a number between 0 and 255");
                        done = false;
                    }
                }
                else
                {
                    Console.WriteLine("Error: Please enter a number");
                }
            }
            while (done == false);
            return Num;
        }
        public static void Main()
        {
            int FirstNum = GetIP("Please enter first Number of IP", false);
            int SecondNum = GetIP("Please enter second Number of IP", false);
            int ThirdNum = GetIP("Please enter third Number of IP or leave it empty to scan this range from 0 - 255", true);
            int FourthNum = GetIP("Please enter fourth Number of IP or leave it empty to scan this range from 0 - 255", true);
            int Timeout = 30;
            Console.WriteLine("Enter data to be entered in the Ping: ");
            string data = Console.ReadLine();
            if (String.IsNullOrWhiteSpace(data))
            {
                data = "Ping!";
            }
            FileStore.AppendFile("Scanning for: " + FirstNum + "." + SecondNum + "." +
                (ThirdNum == -1 ? "XXX" : ThirdNum.ToString()) + "."
                + (FourthNum == -1 ? "XXX" : FourthNum.ToString()));
            foreach (IPAddress iP in GetIPAddresses(FirstNum, SecondNum, ThirdNum, FourthNum))
            {
                Ping ping = new Ping();
                ping.PingCompleted += Ping_PingCompleted;
                ping.SendAsync(iP, Timeout, Encoding.UTF8.GetBytes(data), iP.ToString());
            }
            SpinWait spin = new SpinWait();
            while (AliveInstances!=0)
            {
                spin.SpinOnce();//Wait
            }
            Console.WriteLine("Completed, hit any key to exit");
            FileStore.SetSortAtEnd(true);
            Console.ReadKey();
        }

        private static void Ping_PingCompleted(object sender, PingCompletedEventArgs e)
        {
            if (e.Reply.Status == IPStatus.Success)
            {
                string msg = e.Reply.Address.ToString() + " : " + e.Reply.RoundtripTime + "ms";
                Console.WriteLine(msg);
            }
            lock (LockObj)
            {
                FileStore.AppendFile(e.Reply.Status + ":" + (e.UserState) + ":" + e.Reply.RoundtripTime + "ms");
            }
            AliveInstances -= 1;
            Semaphore.Release();
            (sender as Ping).Dispose();
        }

        public static IEnumerable<IPAddress> GetIPAddresses(int FirstNum, int SecondNum, int ThirdNum, int FourthNum)
        {
            int MaxThirdNum = ThirdNum == -1 ? 255 : ThirdNum;
            int MaxFourthNum = FourthNum == -1 ? 255 : FourthNum;

            if (ThirdNum == -1)
                ThirdNum = 0;
            if (FourthNum == -1)
                FourthNum = 0;

            for (int i = ThirdNum; i <= MaxThirdNum; i++)
            {
                for (int j = FourthNum; j <= MaxFourthNum; j++)
                {
                    Semaphore.WaitOne();
                    AliveInstances += 1;
                    string ip = FirstNum + "." + SecondNum + "." + i.ToString() + "." + j.ToString();
                    yield return IPAddress.Parse(ip);
                }
            }
        }
    }
}
