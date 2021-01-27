using PePe.Service;
using System;
using Xunit;
using NSubstitute;
using Microsoft.Extensions.Logging;
using PePe.Base;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PePe.UnitTests {
    public class WebScraperTest {
        readonly string htmlFileName = @"res/pepe_20_01_2021.html";
        readonly DateTime fileDate = new DateTime(2021, 1, 20);

        ILoadedHtmlDocumentProvider GetHtmlDocumentProvider(string htmlFileName) {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.Load(htmlFileName);

            var provider = Substitute.For<ILoadedHtmlDocumentProvider>();
            provider.GetLoadedHtmlDocument().Returns(doc);

            return provider;
        }

        IMonthConvertor GetMonthConvertor() {
            return new MonthConvertor();
        }

        ILogger GetVoidLogger() {
            return Substitute.For<ILogger>();
        }

        [Fact]
        public void CallsHtmlDocumentProvider() {
            var htmlDocumentProvider = GetHtmlDocumentProvider(htmlFileName);
            var monthConvertor = GetMonthConvertor();
            var scraper = new WebScraper(htmlDocumentProvider, monthConvertor, GetVoidLogger());

            htmlDocumentProvider.ClearReceivedCalls();
            scraper.ScrapeForMenu();

            htmlDocumentProvider.ReceivedWithAnyArgs();
        }

        [Fact]
        public void ParsesDateCorrectly() {
            var htmlDocumentProvider = GetHtmlDocumentProvider(htmlFileName);
            var monthConvertor = GetMonthConvertor();
            var scraper = new WebScraper(htmlDocumentProvider, monthConvertor, GetVoidLogger());
            var expectedDate = fileDate;

            var menu = scraper.ScrapeForMenu();

            Assert.NotNull(menu);
            Assert.Equal(expectedDate.Date, menu.Date.Date);
        }

        public static IEnumerable<object[]> GetExpectedMeals() {
            yield return new object[] {
                "res/pepe_20_01_2021.html",
                new List<Meal> {
                    new Meal{MealType = MealType.Soup, Name = "Tyrolská èesneèka", Info = "", Price = 30},
                    new Meal{MealType = MealType.Main, Name = "Domácí špekové knedlíky, zelí, cibulka", Info = "", Price = 98},
                    new Meal{MealType = MealType.Main, Name = "Pikantní vepøová játra pana starosty, rıe", Info = "", Price = 98},
                    new Meal{MealType = MealType.Main, Name = "Zbojnickı vepøovı závitek, rıe,", Info = "( okurka, slanina, klobása )", Price = 109},
                    new Meal{MealType = MealType.Main, Name = "Kuøecí prsíèka v sezamovém semínku, bramborová kaše, zelnı salát", Info = "", Price = 105},
                    new Meal{MealType = MealType.Main, Name = "Kuøecí prsíèka v sezamovém semínku, bramborovı salát", Info = "", Price = 105},
                    new Meal{MealType = MealType.Main, Name = "Kuøecí køidélka na medu, rozmarınové brambùrky, majonézovı dip", Info = "", Price = 109},
                    new Meal{MealType = MealType.Main, Name = "Smaenı vepøovı øízek, bramborová kaše, zelnı salát", Info = "", Price = 105},
                    new Meal{MealType = MealType.Main, Name = "Smaená TRESKA, brambor, zelnı salát, domácí tatarka", Info = "", Price = 98}
                }
            };
            yield return new object[] {
                "res/pepe_21_01_2021.html",
                new List<Meal> {
                    new Meal{MealType = MealType.Soup, Name = "Fazolová s klobásou", Info = "", Price = 30},
                    new Meal{MealType = MealType.Main, Name = "Halušky s brynzou polévané slaninou", Info = "", Price = 98},
                    new Meal{MealType = MealType.Main, Name = "Vepøovı gulášek s feferonkou, houskové knedlíky", Info = "", Price = 98},
                    new Meal{MealType = MealType.Main, Name = "Èevapèièi, brambor, domácí tatarka", Info = "", Price = 98},
                    new Meal{MealType = MealType.Main, Name = "Vepøové øízeèky, rokforová omáèka, hranolky", Info = "", Price = 109},
                    new Meal{MealType = MealType.Main, Name = "Špikovaná vepøová peèínka se švestkami, šouchané brambory", Info = "", Price = 105},
                    new Meal{MealType = MealType.Main, Name = "Špecle s høíbky, zeleninou a kuøecím masem", Info = "( smetanová omáèka )", Price = 105},
                    new Meal{MealType = MealType.Main, Name = "Smaenı kuøecí øízek, bramborová kaše, zelnı salát", Info = "", Price = 105},
                    new Meal{MealType = MealType.Main, Name = "Smaenı sır, brambor, zelnı salát, domácí tatarka", Info = "", Price = 105}
                }
            };
        }

        class MealComparer : IEqualityComparer<Meal> {
            public bool Equals([AllowNull] Meal x, [AllowNull] Meal y) {
                if (x == null && y == null)
                    return true;
                if (x == null && y != null)
                    return false;
                if (x != null && y == null)
                    return false;
                if (x.Name != y.Name)
                    return false;
                if (x.Info != y.Info)
                    return false;
                if (x.Price != y.Price)
                    return false;
                if (x.MealType != y.MealType)
                    return false;
                return true;
            }

            public int GetHashCode([DisallowNull] Meal obj) {
                return obj.GetHashCode();
            }
        }

        [Theory]
        [MemberData(nameof(GetExpectedMeals))]
        public void ParsesMealsCorrectly(string htmlFileName, IEnumerable<Meal> expectedMeals) {
            var htmlDocumentProvider = GetHtmlDocumentProvider(htmlFileName);
            var monthConvertor = GetMonthConvertor();
            var scraper = new WebScraper(htmlDocumentProvider, monthConvertor, GetVoidLogger());

            var menu = scraper.ScrapeForMenu();

            Assert.NotNull(menu);
            Assert.Equal(expectedMeals, menu.Meals, new MealComparer());
        }

        // TODO: test parseHeadingDate
    }
}
