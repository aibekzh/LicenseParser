using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace license
{
    class Program
    {
        static void Main(string[] args)
        {
            var getConfiguration = new ConfigurationBuilder().AddJsonFile("Configuration.json").Build();

            var conf = new Configuration
            {
                DbHost = getConfiguration["DbHost"],
                DbPort = getConfiguration["DbPort"],
                DbUserName = getConfiguration["DbUserName"],
                DbPassword = getConfiguration["DbPassword"],
                DbName = getConfiguration["DbName"],
                DbScheme = getConfiguration["DbScheme"],
                WorkersNumber = int.Parse(getConfiguration["WorkersNumber"]),
                ProxyApiKey = getConfiguration["ProxyApiKey"],
                BinTable = getConfiguration["BinTable"],
                BinColumn = getConfiguration["BinColumn"]
            };

            var tasks = new Task[conf.WorkersNumber];
            var parsers = new LicensesParser[conf.WorkersNumber];
            
            for (var i = 0; i < parsers.Length; i++)
            {
                parsers[i] = new LicensesParser(i, conf);
            }
            
            for (var i = 0; i < tasks.Length; i++)
            {
                var j = i;
                tasks[i] = Task.Run(() => parsers[j].ParseAllLicenses());
            }

            Task.WaitAll(tasks);
        }
    }
}