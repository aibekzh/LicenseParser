using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using RestSharp;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
//you need to add UIAutomationTypes and UIAutomationClient to references
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Web;
using Newtonsoft.Json.Linq;


namespace license
{
    class Selenium
    {
        /*static void Main(string[] args)
        {
            ChromeOptions options = new ChromeOptions();
           
            options.AddArguments("--incognito");
            
            IWebDriver browser = new ChromeDriver(@"C:\Users\aibek\OneDrive\Рабочий стол\test", options);

            var page = GetLicense(browser,"930640000024");
            Console.WriteLine(page);

        }*/

        public static HtmlDocument GetLicense( string bin)
        {
            ChromeOptions options = new ChromeOptions();

            options.AddArguments("--incognito");
            options.AddArgument("headless");

            IWebDriver browser = new ChromeDriver(options);
            while (true)
            {
                try
                {
                    browser.Navigate().GoToUrl("http://www.elicense.kz/LicensingContent/SimpleSearchLicense");
                    break;
                }
                catch (Exception)
                {

                }
            }

            while (true)
            {
                var id = SendImage(GetImageCode(browser));
                var res = GetResult(id);
                var CaptchaField = Find(browser, "//*[@id='Captcha']");
                CaptchaField.Clear();
                CaptchaField.SendKeys(res);
                var BinField = Find(browser, "//*[@id='IinBinStr']");
                BinField.Clear();
                BinField.SendKeys(bin);
                var button = Find(browser, "//input[@type='submit']");
                button.Click();
                var html = new HtmlDocument();
                html.LoadHtml(browser.PageSource);
                if (html.DocumentNode.SelectSingleNode("//div[@style='color: red']")==null)
                {
                    return html;
                }
               
            }

        }

        private static string GetImageCode(IWebDriver browser)
        {
            var base64string = (browser as IJavaScriptExecutor).ExecuteScript(@"
                var c = document.createElement('canvas');
                var ctx = c.getContext('2d');
                var img = document.getElementById('ExampleCaptcha_CaptchaImage');
                c.height=50px;
                c.width=250px;
                ctx.drawImage(img, 0, 0,img.naturalWidth, img.naturalHeight);
                var base64String = c.toDataURL();
                return base64String;
            ") as string;
            var base64 = base64string.Split(',').Last();
           
            var base64Encoded = Encode(base64);
            return base64Encoded;
        }

        public static IWebElement Find(IWebDriver browser, string selector)
        {
            while (true)
            {
                try
                {
                    return browser.FindElement(By.XPath(selector));
                }
                catch (NoSuchElementException)
                {
                    throw new NoSuchElementException();
                }
            }
        }

        private static string SendImage(string base64)
        {
            try
            {
                var client = new RestClient("https://2captcha.com/in.php");
                var request = new RestRequest(Method.POST);
                request.AddParameter("undefined",
                    $"method=base64&key=bd5697c03356508f1fd35cd3f6dd6b85&body={base64}&json=1",
                    ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);

                var content = response.Content;
                var jobject = JObject.Parse(content);
                var id = jobject.SelectToken("request").ToString();
                return id;
            }
            catch (Exception)
            {
                throw new ArgumentException();
            }
        }

        private static string GetResult(string id)
        {
            while (true)
            {
                var client =
                    new RestClient(
                        $"https://2captcha.com/res.php?key=bd5697c03356508f1fd35cd3f6dd6b85&action=get&id={id}&json=1");
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);
                var content = response.Content;
                var jobject = JObject.Parse(content);
                var res = jobject.SelectToken("request").ToString();
                if (res.Equals("ERROR_CAPTCHA_UNSOLVABLE"))
                    throw new AuthenticationException();
                if (!res.Equals("CAPCHA_NOT_READY"))
                    return res;
                
                Thread.Sleep(2000);
            }

        }
        private static string Encode(string base64)
        {
            return HttpUtility.UrlEncode(base64);
        }
         public static JArray SendDetailsRequest(string iinBin)
        { 
            var html = GetLicense(iinBin);
            JArray license_list = new JArray();
            /*var handler = new HttpClientHandler
            {
                Proxy = new WebProxy($"{url}:{port}", false),
                UseProxy = true
            };*/
            var license_nodes = html.DocumentNode.SelectNodes("//*[@class='DefaultTablde']//a"); 
            foreach (var a_node in license_nodes)
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"),
                        $"http://www.elicense.kz{a_node.Attributes["href"].Value}?lang=ru")
                    )
                    {
                        var response = httpClient.SendAsync(request).GetAwaiter().GetResult();

                        if (response.StatusCode != HttpStatusCode.OK)
                            throw new ExternalException();

                        var htmlDoc = new HtmlDocument();
                        var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        htmlDoc.LoadHtml(content);
                        var nodes = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"licenseForm\"]/ul/li");
                        JValue documentUniqueNumber = new JValue("");
                        JValue applicationUniqueNumber = new JValue("");
                        JValue documentNikadNumber = new JValue("");
                        JValue applicationNikadNumber = new JValue("");
                        JValue licenseNumber = new JValue("");
                        JValue licensor = new JValue("");
                        JValue licensee = new JValue("");
                        JValue licenseType = new JValue("");
                        JValue status = new JValue("");
                        JValue licenseDateIssue = new JValue("");
                        JValue category = new JValue("");
                        JValue specialConditions = new JValue("");
                        JValue periodStart = new JValue("");
                        JValue periodFinish = new JValue("");
                        JValue city = new JValue("");
                        JValue activitySpecie = new JValue("");
                        JValue activitySubspecies = new JValue("");
                        JValue territory = new JValue("");
                        JValue originLicenseNumber = new JValue("");
                        JValue originLicenseDate = new JValue("");
                        JValue signature = new JValue("");
                        JValue series = new JValue("");
                        JValue authorizedBody = new JValue("");
                        JValue appropriation = new JValue("");
                        JValue originUinrd = new JValue("");
                        JValue isLicense = new JValue("false");

                        foreach (var node in nodes)
                        {
                            if (node.ChildNodes[1].InnerText.Contains("Уникальный номер документа"))
                            {
                                documentUniqueNumber.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("НИКАД документа"))
                            {
                                documentNikadNumber.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("НИКАД заявления"))
                            {
                                applicationNikadNumber.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("Уникальный номер заявления"))
                            {
                                applicationUniqueNumber.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("Серия"))
                            {
                                series.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("Номер"))
                            {
                                licenseNumber.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("Статус"))
                            {
                                status.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("Категория"))
                            {
                                category.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("Особые условия"))
                            {
                                specialConditions.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("Тип"))
                            {
                                licenseType.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("Начало периода действия"))
                            {
                                periodStart.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("Окончание периода действия"))
                            {
                                periodFinish.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("Лицензиат"))
                            {
                                licensee.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("Дата выдачи"))
                            {
                                licenseDateIssue.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("Лицензиар"))
                            {
                                licensor.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("Город"))
                            {
                                city.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("УИНРД первоначальной лицензии"))
                            {
                                originUinrd.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("Номер первоначальной лицензии"))
                            {
                                originLicenseNumber.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("Дата выдачи первоначальной лицензии"))
                            {
                                originLicenseDate.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("Предназначение"))
                            {
                                appropriation.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("Подписано"))
                            {
                                signature.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("Уполномоченный орган"))
                            {
                                authorizedBody.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("Предназначение"))
                            {
                                appropriation.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            //Территория деятельности Подвиды деятельности Вид деятельности
                            if (node.ChildNodes[1].InnerText.Contains("Территория деятельности"))
                            {
                                territory.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("Подвиды деятельности"))
                            {
                                activitySubspecies.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (node.ChildNodes[1].InnerText.Contains("Вид деятельности"))
                            {
                                activitySpecie.Value = node.ChildNodes[3].InnerText.Trim();
                            }

                            if (licenseNumber.Value != "")
                            {
                                isLicense.Value = "true";
                            }
                        }

                        JObject licence = new JObject(new JProperty("documentUniqueNumber", documentUniqueNumber),
                                new JProperty("applicationUniqueNumber", applicationUniqueNumber),
                                new JProperty("documentNikadNumber", documentNikadNumber),
                                new JProperty("applicationNikadNumber", applicationNikadNumber),
                                new JProperty("licenseNumber", licenseNumber),
                                new JProperty("licensor", licensor),
                                new JProperty("licensee", licensee),
                                new JProperty("licenseType", licenseType),
                                new JProperty("status", status),
                                new JProperty("licenseDateIssue", licenseDateIssue),
                                new JProperty("category", category),
                                new JProperty("specialConditions", specialConditions),
                                new JProperty("periodStart", periodStart),
                                new JProperty("periodFinish", periodFinish),
                                new JProperty("city", city),
                                new JProperty("activitySpecie", activitySpecie),
                                new JProperty("activitySubspecies", activitySubspecies),
                                new JProperty("territory", territory),
                                new JProperty("originLicenseNumber", originLicenseNumber),
                                new JProperty("originLicenseDate", originLicenseDate),
                                new JProperty("signature", signature),
                                new JProperty("series", series),
                                new JProperty("authorizedBody", authorizedBody),
                                new JProperty("appropriation", appropriation),
                                new JProperty("originUinrd", originUinrd),
                                new JProperty("isLicense",isLicense));
                            license_list.Add(licence);
                        


                    }
                }
            }

            return license_list;
        }

    }
}
