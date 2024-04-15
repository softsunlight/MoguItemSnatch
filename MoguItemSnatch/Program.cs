using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace MoguItemSnatch
{
    class Program
    {
        static void Main(string[] args)
        {
            MoguCrawler moguCrawler = new MoguCrawler();
            moguCrawler.Start2();
        }
    }
}
