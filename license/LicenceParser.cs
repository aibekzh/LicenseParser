using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using HtmlAgilityPack;
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
            ChromeOptions options = new ChromeOptions();
           
            options.AddArguments("--incognito");
            
            IWebDriver browser = new ChromeDriver(options);
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
                        htmlDoc = Selenium.GetLicense(browser, iinBin);

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

        private bool SendDetailsRequest(string iinBin, string id, string url, string port)
        {
            bool isLicense = false;
            var handler = new HttpClientHandler
            {
                Proxy = new WebProxy($"{url}:{port}", false),
                UseProxy = true
            };
            using (var httpClient = new HttpClient(handler))
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"),
                    $"http://www.elicense.kz/Licenses/Details/{id}?lang=ru")
                )
                {
                    
                    var response = httpClient.SendAsync(request).GetAwaiter().GetResult();
                    
                    if (response.StatusCode != HttpStatusCode.OK) 
                        throw new ExternalException();
                    
                    var htmlDoc = new HtmlDocument();
                    var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    htmlDoc.LoadHtml(content);
                    var nodes = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"licenseForm\"]/ul/li");
                    var documentUniqueNumber = "";
                    var applicationUniqueNumber = "";
                    var documentNikadNumber = "";
                    var applicationNikadNumber = "";
                    var licenseNumber = "";
                    var licensor = "";
                    var licensee = "";
                    var licenseType = "";
                    var status = "";
                    var licenseDateIssue = "";
                    var category = "";
                    var specialConditions = "";
                    var periodStart = "";
                    var periodFinish = "";
                    var city = "";
                    var activitySpecie = "";
                    var activitySubspecies = "";
                    var territory = "";
                    var originLicenseNumber = "";
                    var originLicenseDate = "";
                    var signature = "";
                    var series = "";
                    var authorizedBody="";
                    var appropriation = "";
                    var originUinrd = "";
                    
                    foreach (var node in nodes)
                    {
                        if (node.ChildNodes[1].InnerText.Contains("Уникальный номер документа"))
                        {
                            documentUniqueNumber=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("НИКАД документа"))
                        {
                            documentNikadNumber=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("НИКАД заявления"))
                        {
                            applicationNikadNumber=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("Уникальный номер заявления"))
                        {
                            applicationUniqueNumber=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("Серия"))
                        {
                            series=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("Номер"))
                        {
                            licenseNumber=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("Статус"))
                        {
                            status=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("Категория"))
                        {
                            category=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("Особые условия"))
                        {
                            specialConditions=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("Тип"))
                        {
                           licenseType=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("Начало периода действия"))
                        {
                            periodStart=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("Окончание периода действия"))
                        {
                            periodFinish=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("Лицензиат"))
                        {
                            licensee=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("Дата выдачи"))
                        {
                            licenseDateIssue=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("Лицензиар"))
                        {
                            licensor=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("Город"))
                        {
                            city=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("УИНРД первоначальной лицензии"))
                        {
                            originUinrd=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("Номер первоначальной лицензии"))
                        {
                            originLicenseNumber=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("Дата выдачи первоначальной лицензии"))
                        {
                            originLicenseDate=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("Предназначение"))
                        {
                           appropriation= node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("Подписано"))
                        {
                            signature=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("Уполномоченный орган"))
                        {
                            authorizedBody=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("Предназначение"))
                        {
                            appropriation=node.ChildNodes[3].InnerText.Trim();
                        }
                        //Территория деятельности Подвиды деятельности Вид деятельности
                        if (node.ChildNodes[1].InnerText.Contains("Территория деятельности"))
                        {
                            territory=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("Подвиды деятельности"))
                        {
                            activitySubspecies=node.ChildNodes[3].InnerText.Trim();
                        }
                        if (node.ChildNodes[1].InnerText.Contains("Вид деятельности"))
                        {
                            activitySpecie=node.ChildNodes[3].InnerText.Trim();
                        }
                    }

                    using (var conn = new NpgsqlConnection(_connectionString))
                    {
                        conn.Open();
                        var command = conn.CreateCommand();
                        command.Connection = conn;
                        command.CommandText =
                            "INSERT INTO counterparty.license_details_all (document_unique_number, application_unique_number, document_naead_number, application_naead_number, license_number, licensor, licensee, license_type, status, iin_bin, license_id, category, special_conditions, period_start, period_finish, city, activity_specie, activity_subspecies, territory, origin_license_number, origin_license_date, signature, series, relevance_date,issue_date,authorized_body,appropriation,is_license,origin_uinrd) "
                            + " VALUES (@document_unique_number, @application_unique_number, @document_naead_number, @application_naead_number, @license_number, @licensor, @licensee, @license_type, @status, @iin_bin, @license_id, @category, @special_conditions, @period_start, @period_finish, @city, @activity_specie, @activity_subspecies, @territory, @origin_license_number, @origin_license_date, @signature, @series, @relevance_date,@issue_date,@authorized_body,@appropriation,@is_license,@origin_uinrd) " +
                            "ON CONFLICT (license_id) DO NOTHING ";
                        try
                        {
                            command.Parameters.AddWithValue("document_unique_number", documentUniqueNumber);
                            command.Parameters.AddWithValue("application_unique_number", applicationUniqueNumber);
                            command.Parameters.AddWithValue("document_naead_number", documentNikadNumber);
                            command.Parameters.AddWithValue("application_naead_number", applicationNikadNumber);
                            command.Parameters.AddWithValue("license_number", licenseNumber);
                            command.Parameters.AddWithValue("licensor", licensor);
                            command.Parameters.AddWithValue("licensee", licensee);
                            command.Parameters.AddWithValue("license_type", licenseType);
                            command.Parameters.AddWithValue("status", status);
                            command.Parameters.AddWithValue("iin_bin", iinBin);
                            command.Parameters.AddWithValue("license_id", id);
                            command.Parameters.AddWithValue("category", category);
                            command.Parameters.AddWithValue("special_conditions", specialConditions);
                            command.Parameters.AddWithValue("period_start", periodStart);
                            command.Parameters.AddWithValue("period_finish", periodFinish);
                            command.Parameters.AddWithValue("city", city);
                            command.Parameters.AddWithValue("activity_specie", activitySpecie);
                            command.Parameters.AddWithValue("activity_subspecies", activitySubspecies);
                            command.Parameters.AddWithValue("territory", territory);
                            command.Parameters.AddWithValue("origin_license_number", originLicenseNumber);
                            command.Parameters.AddWithValue("origin_license_date", originLicenseDate);
                            command.Parameters.AddWithValue("signature", signature);
                            command.Parameters.AddWithValue("series", series);
                            command.Parameters.AddWithValue("issue_date", licenseDateIssue);
                            command.Parameters.AddWithValue("relevance_date", "");
                            command.Parameters.AddWithValue("authorized_body", authorizedBody);
                            command.Parameters.AddWithValue("appropriation", appropriation);
                            command.Parameters.AddWithValue("origin_uinrd", originUinrd);
                            if (licenseNumber!="")
                            {
                                command.Parameters.AddWithValue("is_license", true);
                            }
                            else
                            {
                                command.Parameters.AddWithValue("is_license", false);
                            }

                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
            }
            return isLicense;
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