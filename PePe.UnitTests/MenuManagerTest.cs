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
    }
}
