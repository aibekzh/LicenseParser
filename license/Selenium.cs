using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
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

        public static HtmlDocument GetLicense(IWebDriver browser, string bin)
        {
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
                c.height=img.naturalHeight;
                c.width=img.naturalWidth;
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

    }
}
