using Microsoft.Extensions.Logging;
using PePe.Base;
using PePe.DAO;
using PePe.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PePe.Manager {
    public class MenuManager : IMenuManager {
        private const int MaxDateTypoDays = 3;

        private readonly IMenuDao menuDao;
        private readonly IDateProvider dateProvider;
        private readonly IWebScraper webScraper;
        private readonly ILogger<MenuManager> logger;

        public MenuManager(IMenuDao menuDao, IDateProvider dateProvider, IWebScraper webScraper, ILogger<MenuManager> logger) {
            this.menuDao = menuDao;
            this.dateProvider = dateProvider;
            this.webScraper = webScraper;
            this.logger = logger;
        }

        public IEnumerable<Menu> Find(MenuSearch search) {
            return menuDao.Find(search);
        }

        public Menu GetTodaysMenu() {
            var todaysDate = dateProvider.GetDate();
            var savedMenu = menuDao.GetMenuByDate(todaysDate);
            
            if (savedMenu != null && savedMenu.Meals != null && savedMenu.Meals.Any())
                return savedMenu;

            logger.LogInformation("Menu is not saved. Scraping web site");
            var scrapedMenu = webScraper.ScrapeForMenu();
            if (!MatchesToday(scrapedMenu, todaysDate)) {
                logger.LogInformation("Scraped menu doesn't have same date as today's date");
                return new Menu { Date = todaysDate, Meals = new List<Meal>() };
            }

            if (scrapedMenu != null && scrapedMenu.Meals != null && scrapedMenu.Meals.Any())
                scrapedMenu = menuDao.Save(scrapedMenu);

            return scrapedMenu;
        }

        /// <summary>
        /// The web heading sometimes has a mistyped date (i.e. "ÚTERÝ 26. října" on Tuesday 27th), so the menu is accepted
        /// when either the date or the day of week matches today. A day of week alone is accepted only when the date is
        /// close to today, otherwise a menu from the same weekday of an older week would pass.
        /// If the date is corrected, it is set to today's date.
        /// </summary>
        private bool MatchesToday(Menu scrapedMenu, DateTime todaysDate) {
            if (scrapedMenu.Date.Date == todaysDate.Date)
                return true;

            var daysApart = Math.Abs((scrapedMenu.Date.Date - todaysDate.Date).TotalDays);
            if (scrapedMenu.HeadingDayOfWeek == todaysDate.DayOfWeek && daysApart <= MaxDateTypoDays) {
                logger.LogWarning($"Scraped menu has date {scrapedMenu.Date:yyyy-MM-dd} but its day of week matches today ({todaysDate:yyyy-MM-dd}). Using today's date.");
                scrapedMenu.Date = todaysDate.Date;
                return true;
            }
            return false;
        }
    }
}
