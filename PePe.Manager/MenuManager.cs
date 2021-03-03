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
            if (scrapedMenu.Date.Date != todaysDate.Date) {
                logger.LogInformation("Scraped menu doesn't have same date as today's date");
                return new Menu { Date = todaysDate, Meals = new List<Meal>() };
            }

            if (scrapedMenu != null && scrapedMenu.Meals != null && scrapedMenu.Meals.Any())
                scrapedMenu = menuDao.Save(scrapedMenu);

            return scrapedMenu;
        }
    }
}
