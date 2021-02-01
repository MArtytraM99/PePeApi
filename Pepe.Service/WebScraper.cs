using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using System.Linq;
using PePe.Base;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace PePe.Service {
    public class WebScraper : IWebScraper {
        static readonly Regex spacesTrimmer = new Regex(@"\s\s+", RegexOptions.Compiled);

        private readonly ILoadedHtmlDocumentProvider htmlDocumentProvider;
        private readonly IMonthConvertor monthConvertor;
        private readonly ILogger<WebScraper> logger;

        public WebScraper(
            ILoadedHtmlDocumentProvider htmlDocumentProvider,
            IMonthConvertor monthConvertor,
            ILogger<WebScraper> logger
            ){
            this.htmlDocumentProvider = htmlDocumentProvider;
            this.monthConvertor = monthConvertor;
            this.logger = logger;
        }

        public Menu ScrapeForMenu() {
            logger.LogInformation("Requesting HtmlDocument for scraping");
            var doc = htmlDocumentProvider.GetLoadedHtmlDocument();

            logger.LogInformation("Scraping HtmlDocument");
            var dateHeader = doc.DocumentNode.SelectSingleNode("//div[@id='main']/div[contains(@class, 'container')]/h2");
            var polevkyTables = doc.DocumentNode.SelectNodes("//h3[contains(., 'HOTOVÁ JÍDLA')]/preceding-sibling::table");
            var jidlaTables = doc.DocumentNode.SelectNodes("//h3[contains(., 'HOTOVÁ JÍDLA')]/following-sibling::table[text()]");

            logger.LogInformation("Parsing menu");
            var date = ParseHeadingDate(dateHeader.InnerText);
            var meals = new List<Meal>();

            if(polevkyTables != null)
                meals.AddRange(parseMeals(polevkyTables, MealType.Soup));
            if(jidlaTables != null)
                meals.AddRange(parseMeals(jidlaTables, MealType.Main));

            return new Menu { 
                Date = date,
                Meals = meals
            };
        }

        private List<Meal> parseMeals(HtmlNodeCollection htmlNodes, MealType mealType) {
            var meals = new List<Meal>();
            foreach(HtmlNode node in htmlNodes) {
                var tbody = node.SelectSingleNode("./tbody");
                if (tbody == null) // sometimes the tbody is ommited
                    tbody = node;
                for(int i = 0; i < tbody.ChildNodes.Count; i++) {
                    if (tbody.ChildNodes[i].Name != "tr")
                        continue;
                    var tr = tbody.ChildNodes[i];
                    if (!tr.HasClass("j_radek1")) {
                        logger.LogWarning($"First row in food table didn't have class 'j_radek1'. InnerText ({tbody.InnerText}), classes ({string.Join(", ", tr.GetClasses())})");
                        continue;
                    }

                    var mealName = tr.SelectSingleNode("./td[contains(@class, 'j_jidlo')]").InnerText.Trim();
                    mealName = spacesTrimmer.Replace(mealName, " ");
                    var priceStr = tr.SelectSingleNode("./td[contains(@class, 'j_cena')]").InnerText.Trim();
                    int price = int.Parse(priceStr.Split(" ")[0]);
                    var info = "";

                    // skip weird nodes
                    while(i + 1 < tbody.ChildNodes.Count && tbody.ChildNodes[i + 1].Name != "tr")
                        i++;
                    
                    if(i + 1 < tbody.ChildNodes.Count && tbody.ChildNodes[i + 1].HasClass("j_radek2")) {
                        var infoTd = tbody.ChildNodes[i + 1].SelectSingleNode("./td[contains(@class, 'j_popis')]");
                        if(infoTd != null) {
                            info = spacesTrimmer.Replace(infoTd.InnerText.Trim(), " ");
                        }
                        i++;
                    }

                    meals.Add(new Meal { Info = info, MealType = mealType, Name = mealName, Price = price });
                }
            }
            return meals;
        }

        /// <summary>
        /// Parses date from heading. Expected format is: NAZEV_DNE CISLO_DNE.NAZEV_MESICE ROK
        /// </summary>
        /// <param name="headingDate"></param>
        /// <returns></returns>
        public DateTime ParseHeadingDate(string headingDate) {
            var splited = headingDate.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if(splited.Length < 3) {
                logger.LogError($"Heading date '{headingDate}' is in unrecognised format.");
                throw new ArgumentException($"Heading date '{headingDate}' is in unrecognised format.");
            }
            var dayName = splited[0];
            var day = int.Parse(splited[1].Split(".")[0]);
            var monthName = splited[1].Split(".")[1];
            bool skipped = false;
            if (string.IsNullOrWhiteSpace(monthName)) {
                monthName = splited[2];
                skipped = true;
            }
            var month = monthConvertor.convertMonth(monthName);
            var year = int.Parse(skipped ? splited[3] : splited[2]);

            return new DateTime(year, month, day);
        }
    }
}
