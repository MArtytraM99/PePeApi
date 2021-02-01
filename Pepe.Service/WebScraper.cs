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
            var tokens = LoadTokensFromString(headingDate);
            var dayName = ""; // not used at this time
            if (tokens[0].IsWord()) {
                dayName = tokens[0].representation;
            }

            int index = 0;
            while (index < tokens.Count && tokens[index].IsWord())
                index++;

            if(index == tokens.Count) {
                var msg = $"Heading date '{headingDate}' doesn't contain number of day.";
                logger.LogError(msg);
                throw new ArgumentException(msg);
            }

            var day = tokens[index].GetNumber();
            index++;
            if(index == tokens.Count) {
                var msg = $"Heading date '{headingDate}' doesn't contain month.";
                logger.LogError(msg);
                throw new ArgumentException(msg);
            }

            int month;
            if (tokens[index].IsNumber())
                month = tokens[index].GetNumber();
            else
                month = monthConvertor.convertMonth(tokens[index].representation);
            index++;

            while (index < tokens.Count && tokens[index].IsWord())
                index++;
            if (index == tokens.Count) {
                var msg = $"Heading date '{headingDate}' doesn't contain year.";
                logger.LogError(msg);
                throw new ArgumentException(msg);
            }
            int year = tokens[index].GetNumber();

            return new DateTime(year, month, day);
        }

        class Token {
            public Token(string representation) {
                this.representation = representation;
            }
            public bool IsNumber() => int.TryParse(representation, out int _);
            public int GetNumber() => int.Parse(representation);
            public bool IsWord() => !IsNumber();
            public bool IsNullOrEmpty() => string.IsNullOrEmpty(representation);

            public string representation;
        }

        enum PrevChar {
            NONE, DIGIT, LETTER
        }

        private List<Token> LoadTokensFromString(string s) {
            List<Token> tokens = new List<Token>();
            if (string.IsNullOrEmpty(s))
                return tokens;
            
            var sb = new StringBuilder();
            sb.Clear();
            var prevChar = PrevChar.NONE;
            int index = 0;
            while(index < s.Length) {
                if (char.IsDigit(s[index])) {
                    if (prevChar == PrevChar.LETTER) {
                        tokens.Add(new Token(sb.ToString()));
                        sb.Clear();
                    }
                    sb.Append(s[index]);
                    prevChar = PrevChar.DIGIT;
                } else if(char.IsLetter(s[index])) {
                    if(prevChar == PrevChar.DIGIT) {
                        tokens.Add(new Token(sb.ToString()));
                        sb.Clear();
                    }
                    sb.Append(s[index]);
                    prevChar = PrevChar.LETTER;
                } else if (sb.Length != 0) {
                    tokens.Add(new Token(sb.ToString()));
                    sb.Clear();
                    prevChar = PrevChar.NONE;
                }

                index++;
            }
            if (sb.Length != 0)
                tokens.Add(new Token(sb.ToString()));

            return tokens.Where(t => !t.IsNullOrEmpty()).ToList();
        }
    }
}
