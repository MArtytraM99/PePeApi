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

        IDateProvider GetDateProvider(DateTime date) {
            var provider = Substitute.For<IDateProvider>();
            provider.GetDate().Returns(date.Date);
            return provider;
        }

        ILogger<WebScraper> GetVoidLogger() {
            return new VoidLogger<WebScraper>();
        }

        [Fact]
        public void CallsHtmlDocumentProvider() {
            var htmlDocumentProvider = GetHtmlDocumentProvider(htmlFileName);
            var monthConvertor = GetMonthConvertor();
            var dateProvider = GetDateProvider(new DateTime(1970, 1, 1));
            var scraper = new WebScraper(htmlDocumentProvider, monthConvertor, dateProvider, GetVoidLogger());

            htmlDocumentProvider.ClearReceivedCalls();
            dateProvider.ClearReceivedCalls();
            scraper.ScrapeForMenu();

            htmlDocumentProvider.ReceivedWithAnyArgs();
            dateProvider.DidNotReceiveWithAnyArgs().GetDate();
        }

        [Fact]
        public void ParsesDateCorrectly() {
            var htmlDocumentProvider = GetHtmlDocumentProvider(htmlFileName);
            var monthConvertor = GetMonthConvertor();
            var dateProvider = GetDateProvider(new DateTime(1970, 1, 1));
            var scraper = new WebScraper(htmlDocumentProvider, monthConvertor, dateProvider, GetVoidLogger());
            var expectedDate = fileDate;

            dateProvider.ClearReceivedCalls();
            var menu = scraper.ScrapeForMenu();

            Assert.NotNull(menu);
            Assert.Equal(expectedDate.Date, menu.Date.Date);
            dateProvider.DidNotReceiveWithAnyArgs().GetDate();
        }

        public static IEnumerable<object[]> GetExpectedMeals() {
            yield return new object[] {
                "res/pepe_20_01_2021.html",
                new List<Meal> {
                    new Meal{MealType = MealType.Soup, Name = "Tyrolská česnečka", Info = "", Price = 30},
                    new Meal{MealType = MealType.Main, Name = "Domácí špekové knedlíky, zelí, cibulka", Info = "", Price = 98},
                    new Meal{MealType = MealType.Main, Name = "Pikantní vepřová játra pana starosty, rýže", Info = "", Price = 98},
                    new Meal{MealType = MealType.Main, Name = "Zbojnický vepřový závitek, rýže,", Info = "( okurka, slanina, klobása )", Price = 109},
                    new Meal{MealType = MealType.Main, Name = "Kuřecí prsíčka v sezamovém semínku, bramborová kaše, zelný salát", Info = "", Price = 105},
                    new Meal{MealType = MealType.Main, Name = "Kuřecí prsíčka v sezamovém semínku, bramborový salát", Info = "", Price = 105},
                    new Meal{MealType = MealType.Main, Name = "Kuřecí křidélka na medu, rozmarýnové brambůrky, majonézový dip", Info = "", Price = 109},
                    new Meal{MealType = MealType.Main, Name = "Smažený vepřový řízek, bramborová kaše, zelný salát", Info = "", Price = 105},
                    new Meal{MealType = MealType.Main, Name = "Smažená TRESKA, brambor, zelný salát, domácí tatarka", Info = "", Price = 98}
                }
            };
            yield return new object[] {
                "res/pepe_21_01_2021.html",
                new List<Meal> {
                    new Meal{MealType = MealType.Soup, Name = "Fazolová s klobásou", Info = "", Price = 30},
                    new Meal{MealType = MealType.Main, Name = "Halušky s brynzou polévané slaninou", Info = "", Price = 98},
                    new Meal{MealType = MealType.Main, Name = "Vepřový gulášek s feferonkou, houskové knedlíky", Info = "", Price = 98},
                    new Meal{MealType = MealType.Main, Name = "Čevapčiči, brambor, domácí tatarka", Info = "", Price = 98},
                    new Meal{MealType = MealType.Main, Name = "Vepřové řízečky, rokforová omáčka, hranolky", Info = "", Price = 109},
                    new Meal{MealType = MealType.Main, Name = "Špikovaná vepřová pečínka se švestkami, šťouchané brambory", Info = "", Price = 105},
                    new Meal{MealType = MealType.Main, Name = "Špecle s hříbky, zeleninou a kuřecím masem", Info = "( smetanová omáčka )", Price = 105},
                    new Meal{MealType = MealType.Main, Name = "Smažený kuřecí řízek, bramborová kaše, zelný salát", Info = "", Price = 105},
                    new Meal{MealType = MealType.Main, Name = "Smažený sýr, brambor, zelný salát, domácí tatarka", Info = "", Price = 105}
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
            var dateProvider = GetDateProvider(new DateTime(1970, 1, 1));
            var scraper = new WebScraper(htmlDocumentProvider, monthConvertor, dateProvider, GetVoidLogger());

            dateProvider.ClearReceivedCalls();
            var menu = scraper.ScrapeForMenu();

            Assert.NotNull(menu);
            Assert.Equal(expectedMeals, menu.Meals, new MealComparer());
            dateProvider.DidNotReceiveWithAnyArgs().GetDate();
        }

        public static IEnumerable<object[]> GetDateParsingData() {
            yield return new object[] { "Pondělí 2.ledna 2021", new DateTime(2021,1,2) };
            yield return new object[] { "Pondělí 2.LEDNA 2021", new DateTime(2021, 1, 2) };
            yield return new object[] { "Pondělí 2. ledna 2021", new DateTime(2021, 1, 2) };
            yield return new object[] { "   Pondělí   2.  LedNa    2021  ", new DateTime(2021, 1, 2) };
            yield return new object[] { "Pondělí2LedNa2021", new DateTime(2021, 1, 2) };
            yield return new object[] { "   Pondělí2LedNa2021   ", new DateTime(2021, 1, 2) };
            yield return new object[] { "Pondělí_2-LedNa_2021", new DateTime(2021, 1, 2) };
            yield return new object[] { "Pondělí - 2 - LedNa - 2021", new DateTime(2021, 1, 2) };
            yield return new object[] { "2.1.2021", new DateTime(2021, 1, 2) };
            yield return new object[] { "2-1-2021", new DateTime(2021, 1, 2) };
            yield return new object[] { "Pondělí 2.1.2021", new DateTime(2021, 1, 2) };
        }

        [Theory]
        [MemberData(nameof(GetDateParsingData))]
        public void ParseHeadingDate_ParsesDateCorrectlyWithoutUsingDefaults(string dateString, DateTime expectedDate) {
            var htmlDocumentProvider = Substitute.For<ILoadedHtmlDocumentProvider>();
            var monthConvertor = GetMonthConvertor();
            var defaultDateProvider = GetDateProvider(expectedDate);
            var scraper = new WebScraper(htmlDocumentProvider, monthConvertor, defaultDateProvider, GetVoidLogger());

            defaultDateProvider.ClearReceivedCalls();
            var parsedDate = scraper.ParseHeadingDate(dateString);

            Assert.Equal(expectedDate, parsedDate);
            defaultDateProvider.DidNotReceiveWithAnyArgs().GetDate();
        }

        public static IEnumerable<object[]> GetDateParsingDataForDefaultsUse() {
            yield return new object[] { "Pondělí 2.ledna", new DateTime(2021, 1, 1), new DateTime(2021, 1, 2) };
            yield return new object[] { "Pondělí 2.LEDNA", new DateTime(1970, 1, 1), new DateTime(1970, 1, 2) };
            yield return new object[] { "Pondělí 2.1", new DateTime(1970, 1, 1), new DateTime(1970, 1, 2) };
            yield return new object[] { "Pondělí 2.březina 2021", new DateTime(1970, 3, 1), new DateTime(2021, 3, 2) };
        }

        [Theory]
        [MemberData(nameof(GetDateParsingDataForDefaultsUse))]
        public void ParseHeadingDate_ParsesDateCorrectlyWUsingDefaults(string dateString, DateTime defaultDate, DateTime expectedDate) {
            var htmlDocumentProvider = Substitute.For<ILoadedHtmlDocumentProvider>();
            var monthConvertor = GetMonthConvertor();
            var defaultDateProvider = GetDateProvider(defaultDate);
            var scraper = new WebScraper(htmlDocumentProvider, monthConvertor, defaultDateProvider, GetVoidLogger());

            defaultDateProvider.ClearReceivedCalls();
            var parsedDate = scraper.ParseHeadingDate(dateString);

            Assert.Equal(expectedDate, parsedDate);
            defaultDateProvider.Received().GetDate();
        }
    }
}
