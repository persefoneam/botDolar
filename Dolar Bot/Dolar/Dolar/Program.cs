using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Dolar
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            List<(string, decimal, decimal)> lastRound = new List<(string, decimal, decimal)>();
            lastRound.Add(("", 0, 0));
            lastRound.Add(("", 0, 0));
            lastRound.Add(("", 0, 0));
            lastRound.Add(("", 0, 0));
            lastRound.Add(("", 0, 0));
            lastRound.Add(("", 0, 0));
            lastRound.Add(("", 0, 0));
            while (true)
            {
                var html = await GetHtml();
                var elements = ParseHtml(html);

                
                var elementCount = 0;
                foreach(var e in elements)
                {
                    
                    Console.WriteLine("Values at: " + DateTime.Now);
                    Console.WriteLine(e.Item1 + "-" + e.Item2 + "-" + e.Item3);

                    if(e.Item3 != lastRound[elementCount].Item3) 
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"CAMBIO LA COTIZACION DE: {e.Item1} A {e.Item3}");
                        Console.ForegroundColor = ConsoleColor.White;
                        GenerateToast("Cambios en el dolar ARGENTINA", "CAMBIO LA COTIZACION DE", e.Item1, e.Item3.ToString());
                    }
                    elementCount++;
                   

                }
                lastRound = elements;
                Console.WriteLine("---------------------------------------------");

                Thread.Sleep(30000);

            }
        }

        private static Task<string> GetHtml()
        {
            var client = new HttpClient();
            return client.GetStringAsync("https://dolarhoy.com/");
        }
        private static List<(string, decimal, decimal)> ParseHtml(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var nodes =
                htmlDoc
                    .DocumentNode
                    .SelectNodes("//section[@class='modulo modulo__cotizaciones']/div/div/div/div/div[@class='tile dolar']/div/div");

           
            //var nodes = dolarHtml.SelectNodes("//div[@class='tile is-child']");

            List<(string,decimal,decimal)> data = new List<(string,decimal,decimal)>();

            foreach (var element in nodes)
            {
                
                var name = element.SelectSingleNode("a")?.InnerText;
              
                var compra = element.SelectSingleNode("div/div[@class='compra']/div[@class='val']")?.InnerText.Replace("$", "");
                var venta = element.SelectSingleNode("div/div[@class='venta']/div[@class='val']")?.InnerText.Replace("$", "");

                Decimal compraDecimal = 0;
                Decimal ventaDecimal = 0;
                Decimal.TryParse(compra, out compraDecimal);
                Decimal.TryParse(venta, out ventaDecimal);
                data.Add((name, compraDecimal, ventaDecimal));
            }

            return data;
        }
        public static void GenerateToast(string appid, string h1, string h2, string p1)
        {

            var template = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

            var textNodes = template.GetElementsByTagName("text");

            textNodes[0].AppendChild(template.CreateTextNode(h1));
            textNodes[1].AppendChild(template.CreateTextNode(h2));
            textNodes[2].AppendChild(template.CreateTextNode(p1));

            IXmlNode toastNode = template.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("duration", "long");

            var notifier = ToastNotificationManager.CreateToastNotifier(appid);
            var notification = new ToastNotification(template);

            notifier.Show(notification);
        }
    }
}
