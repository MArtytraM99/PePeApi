using PePe.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace PePe.Service
{
    public class DummyWebScraper : IWebScraper
    {
        public Menu ScrapeForMenu() {
            return new Menu {
                Date = DateTime.Today,
                Meals = new List<Meal> {
                    new Meal { Name = "Soup", MealType = MealType.Soup, Price = 30 },
                    new Meal { Name = "Main A", MealType = MealType.Main },
                    new Meal { Name = "Main B", MealType = MealType.Main }
                }
            };
        }
    }
}
