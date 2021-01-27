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
                    new Meal{MealType = MealType.Soup, Name = "Tyrolsk� �esne�ka", Info = "", Price = 30},
                    new Meal{MealType = MealType.Main, Name = "Dom�c� �pekov� knedl�ky, zel�, cibulka", Info = "", Price = 98},
                    new Meal{MealType = MealType.Main, Name = "Pikantn� vep�ov� j�tra pana starosty, r��e", Info = "", Price = 98},
                    new Meal{MealType = MealType.Main, Name = "Zbojnick� vep�ov� z�vitek, r��e,", Info = "( okurka, slanina, klob�sa )", Price = 109},
                    new Meal{MealType = MealType.Main, Name = "Ku�ec� prs��ka v sezamov�m sem�nku, bramborov� ka�e, zeln� sal�t", Info = "", Price = 105},
                    new Meal{MealType = MealType.Main, Name = "Ku�ec� prs��ka v sezamov�m sem�nku, bramborov� sal�t", Info = "", Price = 105},
                    new Meal{MealType = MealType.Main, Name = "Ku�ec� k�id�lka na medu, rozmar�nov� bramb�rky, majon�zov� dip", Info = "", Price = 109},
                    new Meal{MealType = MealType.Main, Name = "Sma�en� vep�ov� ��zek, bramborov� ka�e, zeln� sal�t", Info = "", Price = 105},
                    new Meal{MealType = MealType.Main, Name = "Sma�en� TRESKA, brambor, zeln� sal�t, dom�c� tatarka", Info = "", Price = 98}
                }
            };
            yield return new object[] {
                "res/pepe_21_01_2021.html",
                new List<Meal> {
                    new Meal{MealType = MealType.Soup, Name = "Fazolov� s klob�sou", Info = "", Price = 30},
                    new Meal{MealType = MealType.Main, Name = "Halu�ky s brynzou pol�van� slaninou", Info = "", Price = 98},
                    new Meal{MealType = MealType.Main, Name = "Vep�ov� gul�ek s feferonkou, houskov� knedl�ky", Info = "", Price = 98},
                    new Meal{MealType = MealType.Main, Name = "�evap�i�i, brambor, dom�c� tatarka", Info = "", Price = 98},
                    new Meal{MealType = MealType.Main, Name = "Vep�ov� ��ze�ky, rokforov� om��ka, hranolky", Info = "", Price = 109},
                    new Meal{MealType = MealType.Main, Name = "�pikovan� vep�ov� pe��nka se �vestkami, ��ouchan� brambory", Info = "", Price = 105},
                    new Meal{MealType = MealType.Main, Name = "�pecle s h��bky, zeleninou a ku�ec�m masem", Info = "( smetanov� om��ka )", Price = 105},
                    new Meal{MealType = MealType.Main, Name = "Sma�en� ku�ec� ��zek, bramborov� ka�e, zeln� sal�t", Info = "", Price = 105},
                    new Meal{MealType = MealType.Main, Name = "Sma�en� s�r, brambor, zeln� sal�t, dom�c� tatarka", Info = "", Price = 105}
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
