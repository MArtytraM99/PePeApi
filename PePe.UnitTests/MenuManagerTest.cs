using Microsoft.Extensions.Logging;
using NSubstitute;
using PePe.Base;
using PePe.DAO;
using PePe.Manager;
using PePe.Service;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace PePe.UnitTests {
    public class MenuManagerTest {
        private IWebScraper GetStaticWebScraper(Menu menuToReturn) {
            var scraper = Substitute.For<IWebScraper>();
            scraper.ScrapeForMenu().Returns(menuToReturn);
            return scraper;
        }

        private IDateProvider GetStaticDateProvider(DateTime dateToReturn) {
            var dateProvider = Substitute.For<IDateProvider>();
            dateProvider.GetDate().Returns(dateToReturn);
            return dateProvider;
        }

        private ILogger<MenuManager> GetVoidLogger() {
            return new VoidLogger<MenuManager>();
        }

        [Fact]
        public void GetTodaysMenu_CallsDateProvider() {
            var todaysDate = new DateTime(1970, 12, 24).Date;
            var webScraper = GetStaticWebScraper(new Menu { Date = todaysDate });
            var dateProvider = GetStaticDateProvider(todaysDate);
            var dao = Substitute.For<IMenuDao>();
            dao.GetMenuByDate(Arg.Any<DateTime>()).Returns(ci => null);
            dao.Save(Arg.Any<Menu>()).Returns(ci => (Menu)ci.Args()[0]);
            var manager = new MenuManager(dao, dateProvider, webScraper, GetVoidLogger());

            dateProvider.ClearReceivedCalls();
            manager.GetTodaysMenu();

            dateProvider.Received().GetDate();
        }

        [Fact]
        public void GetTodaysMenu_NotFoundInDB_CallsWebScraperAndSaves() {
            var todaysDate = new DateTime(1970, 12, 24).Date;
            var menu = new Menu {
                Date = todaysDate,
                Meals = new List<Meal> {
                    new Meal { MealType = MealType.Soup, Name = "SomeSoup", Price = 10 },
                    new Meal { MealType = MealType.Main, Name = "MainName", Info = "Main info", Price = 25 }
                }
            };
            var webScraper = GetStaticWebScraper(menu);
            var dateProvider = GetStaticDateProvider(todaysDate);
            var dao = Substitute.For<IMenuDao>();
            dao.GetMenuByDate(Arg.Any<DateTime>()).Returns(ci => null);
            dao.Save(Arg.Any<Menu>()).Returns(ci => (Menu)ci.Args()[0]);
            var manager = new MenuManager(dao, dateProvider, webScraper, GetVoidLogger());

            webScraper.ClearReceivedCalls();
            dao.ClearReceivedCalls();
            var todaysMenu = manager.GetTodaysMenu();

            Assert.Equal(menu, todaysMenu);
            webScraper.Received().ScrapeForMenu();
            dao.Received().GetMenuByDate(todaysDate);
            dao.Received().Save(menu);
        }

        [Fact]
        public void GetTodaysMenu_FoundEmpty_CallsWebScraperAndSaves() {
            var todaysDate = new DateTime(1970, 12, 24).Date;
            var menu = new Menu {
                Date = todaysDate,
                Meals = new List<Meal> {
                    new Meal { MealType = MealType.Soup, Name = "SomeSoup", Price = 10 },
                    new Meal { MealType = MealType.Main, Name = "MainName", Info = "Main info", Price = 25 }
                }
            };
            var webScraper = GetStaticWebScraper(menu);
            var dateProvider = GetStaticDateProvider(todaysDate);
            var dao = Substitute.For<IMenuDao>();
            dao.GetMenuByDate(Arg.Any<DateTime>()).Returns(new Menu { Date = todaysDate, Meals = new List<Meal>() });
            dao.Save(Arg.Any<Menu>()).Returns(ci => (Menu)ci.Args()[0]);
            var manager = new MenuManager(dao, dateProvider, webScraper, GetVoidLogger());

            webScraper.ClearReceivedCalls();
            dao.ClearReceivedCalls();
            var todaysMenu = manager.GetTodaysMenu();

            Assert.Equal(menu, todaysMenu);
            webScraper.Received().ScrapeForMenu();
            dao.Received().GetMenuByDate(todaysDate);
            dao.Received().Save(menu);
        }

        [Fact]
        public void GetTodaysMenu_FoundInDB_ReturnsAndDoesntCallWebScraper() {
            var todaysDate = new DateTime(1970, 12, 24).Date;
            var menu = new Menu {
                Date = todaysDate,
                Meals = new List<Meal> {
                    new Meal { MealType = MealType.Soup, Name = "SomeSoup", Price = 10 },
                    new Meal { MealType = MealType.Main, Name = "MainName", Info = "Main info", Price = 25 }
                }
            };
            var webScraper = GetStaticWebScraper(null);
            var dateProvider = GetStaticDateProvider(todaysDate);
            var dao = Substitute.For<IMenuDao>();
            dao.GetMenuByDate(Arg.Any<DateTime>()).Returns(menu);
            dao.Save(Arg.Any<Menu>()).Returns(ci => (Menu)ci.Args()[0]);
            var manager = new MenuManager(dao, dateProvider, webScraper, GetVoidLogger());

            webScraper.ClearReceivedCalls();
            dao.ClearReceivedCalls();
            var todaysMenu = manager.GetTodaysMenu();

            Assert.Equal(menu, todaysMenu);
            webScraper.DidNotReceiveWithAnyArgs().ScrapeForMenu();
            dao.Received().GetMenuByDate(todaysDate);
            dao.DidNotReceiveWithAnyArgs().Save(Arg.Any<Menu>());
        }

        [Fact]
        public void GetTodaysMenu_ScrapesEmpty_DoesntSave() {
            var todaysDate = new DateTime(1970, 12, 24).Date;
            var emptyMenu = new Menu {
                Date = todaysDate,
                Meals = new List<Meal> {}
            };
            var webScraper = GetStaticWebScraper(emptyMenu);
            var dateProvider = GetStaticDateProvider(todaysDate);
            var dao = Substitute.For<IMenuDao>();
            dao.GetMenuByDate(Arg.Any<DateTime>()).Returns(emptyMenu);
            dao.Save(Arg.Any<Menu>()).Returns(ci => (Menu)ci.Args()[0]);
            var manager = new MenuManager(dao, dateProvider, webScraper, GetVoidLogger());

            webScraper.ClearReceivedCalls();
            dao.ClearReceivedCalls();
            var todaysMenu = manager.GetTodaysMenu();

            Assert.Equal(emptyMenu, todaysMenu);
            webScraper.Received().ScrapeForMenu();
            dao.Received().GetMenuByDate(todaysDate);
            dao.DidNotReceiveWithAnyArgs().Save(Arg.Any<Menu>());
        }

        private Menu GetMenuWithMeal(DateTime date, DayOfWeek? headingDayOfWeek) {
            return new Menu {
                Date = date,
                HeadingDayOfWeek = headingDayOfWeek,
                Meals = new List<Meal> { new Meal { MealType = MealType.Main, Name = "MainName", Price = 25 } }
            };
        }

        private (MenuManager manager, IMenuDao dao) GetManagerScraping(Menu scraped, DateTime todaysDate) {
            var dao = Substitute.For<IMenuDao>();
            dao.GetMenuByDate(Arg.Any<DateTime>()).Returns(ci => null);
            dao.Save(Arg.Any<Menu>()).Returns(ci => (Menu)ci.Args()[0]);
            var manager = new MenuManager(dao, GetStaticDateProvider(todaysDate), GetStaticWebScraper(scraped), GetVoidLogger());
            return (manager, dao);
        }

        // 2026-10-27 is Tuesday
        static readonly DateTime tuesday = new DateTime(2026, 10, 27);

        [Fact]
        public void GetTodaysMenu_DateMatchesButDayOfWeekDoesnt_SavesScrapedMenu() {
            var scraped = GetMenuWithMeal(tuesday, DayOfWeek.Monday);
            var (manager, dao) = GetManagerScraping(scraped, tuesday);

            var todaysMenu = manager.GetTodaysMenu();

            Assert.Equal(tuesday, todaysMenu.Date);
            Assert.NotEmpty(todaysMenu.Meals);
            dao.Received().Save(scraped);
        }

        [Fact]
        public void GetTodaysMenu_DayOfWeekMatchesButDateDoesnt_UsesTodaysDateAndSaves() {
            var scraped = GetMenuWithMeal(tuesday.AddDays(-1), DayOfWeek.Tuesday);
            var (manager, dao) = GetManagerScraping(scraped, tuesday);

            var todaysMenu = manager.GetTodaysMenu();

            Assert.Equal(tuesday, todaysMenu.Date);
            Assert.NotEmpty(todaysMenu.Meals);
            dao.Received().Save(Arg.Is<Menu>(m => m.Date == tuesday));
        }

        [Fact]
        public void GetTodaysMenu_NeitherDateNorDayOfWeekMatch_ReturnsEmptyAndDoesntSave() {
            var scraped = GetMenuWithMeal(tuesday.AddDays(-1), DayOfWeek.Monday);
            var (manager, dao) = GetManagerScraping(scraped, tuesday);

            var todaysMenu = manager.GetTodaysMenu();

            Assert.Equal(tuesday, todaysMenu.Date);
            Assert.Empty(todaysMenu.Meals);
            dao.DidNotReceiveWithAnyArgs().Save(Arg.Any<Menu>());
        }

        [Fact]
        public void GetTodaysMenu_DayOfWeekMatchesButDateIsWeekOld_ReturnsEmptyAndDoesntSave() {
            var scraped = GetMenuWithMeal(tuesday.AddDays(-7), DayOfWeek.Tuesday);
            var (manager, dao) = GetManagerScraping(scraped, tuesday);

            var todaysMenu = manager.GetTodaysMenu();

            Assert.Empty(todaysMenu.Meals);
            dao.DidNotReceiveWithAnyArgs().Save(Arg.Any<Menu>());
        }

        [Fact]
        public void GetTodaysMenu_UnknownDayOfWeekAndDateDoesntMatch_ReturnsEmptyAndDoesntSave() {
            var scraped = GetMenuWithMeal(tuesday.AddDays(-1), null);
            var (manager, dao) = GetManagerScraping(scraped, tuesday);

            var todaysMenu = manager.GetTodaysMenu();

            Assert.Empty(todaysMenu.Meals);
            dao.DidNotReceiveWithAnyArgs().Save(Arg.Any<Menu>());
        }
    }
}
