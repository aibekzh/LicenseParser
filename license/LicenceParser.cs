using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using Npgsql;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace license
{
    public class LicensesParser
    {
        private string _connectionString;
        private string _apiKey;
        private int _id;
        private Configuration _configuration;

        public LicensesParser(int id, Configuration configuration)
        {
            _id = id;
            _configuration = configuration;
        }

        public void ParseAllLicenses()
        {
            try
            {
                _connectionString = $"Host={_configuration.DbHost}; " +
                                    $"Port={_configuration.DbPort}; " +
                                    $"Username={_configuration.DbUserName}; " +
                                    $"Password={_configuration.DbPassword}; " +
                                    $"Database={_configuration.DbName}; " +
                                    $"Search Path={_configuration.DbScheme};";
                _apiKey = _configuration.ProxyApiKey;
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    var command = conn.CreateCommand();
                    command.Connection = conn;
                    command.CommandText =
                        $"SELECT {_configuration.BinColumn}, id FROM {_configuration.BinTable} WHERE {_configuration.BinColumn} is not null and id%{_configuration.WorkersNumber}={_id} order by id ASC";
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        while (true)
                        {
                            try
                            {
                                //Console.WriteLine(reader[0].ToString());
                                ParseLicenses(reader[0].ToString());
                                //Console.WriteLine(reader[0] + " " + reader[1]);
                                break;
                            }
                            catch (Exception e)
                            {
                                // Console.WriteLine(e + "\n" + reader[0]);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ParseLicenses(string iinBin)
        {
           
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                var command = conn.CreateCommand();
                command.Connection = conn;
                var today = DateTime.Today.ToString("yyyy-MM-dd");
                var pageCounter = 1;
                var counter = 0;
                var counterOther = 0;
                var counterLicense = 0;
                //var (url, port) = GetProxy();
                while (true)
                {
                    try
                    {
                        var htmlDoc = new HtmlDocument();
                        htmlDoc = Selenium.GetLicense(iinBin);

                        if (!htmlDoc.DocumentNode.SelectSingleNode("//*[@id='globalMainMaster']/div[4]/div[3]")
                            .InnerText.ToUpper().Contains("НЕ НАЙДЕНО"))
                        {
                            Console.WriteLine("content is not empty");
                        }
                    }
                    catch (AuthenticationException)
                    {
                        Console.WriteLine("Captcha Wrong :" + iinBin);
                    }
                    catch (ExternalException)
                    {
                        //(url, port) = GetProxy();
                        Console.WriteLine("Trying With Another Proxy :" + iinBin);
                    }
                }
            }
        }

       

        private (string, string) GetProxy()
        {
            var random = new Random();
            var index = random.Next(0, 9);
            List<string> list = new List<string>();
            list.Add("45.152.84.106");
            list.Add("185.120.79.126");
            list.Add("185.120.78.145");
            list.Add("185.120.77.211");
            list.Add("185.120.78.247");
            list.Add("185.120.78.131");
            list.Add("185.120.78.249");
            list.Add("185.120.78.125");
            list.Add("185.120.78.124");
            list.Add("185.120.76.210");
            return (list[index], "65233");
        }
    }
}