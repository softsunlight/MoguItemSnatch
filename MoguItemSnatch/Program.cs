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
            MoguUtil.GetShopData("1os6vu", 1, 60);
            MoguCrawler moguCrawler = new MoguCrawler();
            moguCrawler.Start();
        }
    }
}
